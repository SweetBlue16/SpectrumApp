using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Spectrum.API.Configuration;
using Spectrum.API.Data;
using Spectrum.API.Grpc.Drops;
using Spectrum.API.Grpc.Social;
using Spectrum.API.Middlewares;
using Spectrum.API.Repositories;
using Spectrum.API.Services.Auth;
using Spectrum.API.Services.Drops;
using Spectrum.API.Services.Email;
using Spectrum.API.Services.External;
using Spectrum.API.Services.Profile;
using Spectrum.API.Services.Reports;
using Spectrum.API.Services.Reviews;
using Spectrum.API.Services.Votes;
using Spectrum.API.Services.Storage;
using Spectrum.API.Services.Clips;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\n[SPECTRUM API] Starting server configuration...");
Console.ResetColor();

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("[SPECTRUM API] Configuring exceptions and controllers...");
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

builder.Services.AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection(SmtpOptions.SectionName))
    .Configure(options =>
    {
        options.Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? options.Host;
        options.Username = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? options.Username;
        options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? options.Password;
        options.FromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? options.FromEmail;
        options.FromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? options.FromName;

        if (int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var smtpPort))
        {
            options.Port = smtpPort;
        }

        if (bool.TryParse(Environment.GetEnvironmentVariable("SMTP_USE_TLS"), out var useTls))
        {
            options.UseTls = useTls;
        }
    });
builder.Services.AddOptions<VerificationCodeOptions>()
    .Bind(builder.Configuration.GetSection(VerificationCodeOptions.SectionName));

Console.WriteLine("[SPECTRUM API] Configuring CORS policy (AllowFrontend)...");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("SensitiveAuth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

Console.WriteLine("[SPECTRUM API] Configuring PostgreSQL database context...");
builder.Services.AddDbContext<SpectrumDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

Console.WriteLine("[SPECTRUM API] Registering repositories and services...");
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAdminDetailRepository, AdminDetailRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IReviewCommentService, ReviewCommentService>();
builder.Services.AddScoped<IVoteService, VoteServiceClient>();
builder.Services.AddScoped<IReportService, ReportsService>();
builder.Services.AddScoped<IUserModerationService, UserModerationService>();
builder.Services.AddScoped<IDropsService, DropsService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
builder.Services.AddScoped<IVideoStorageService, VideoStorageService>();
builder.Services.AddScoped<IGameClipService, GameClipService>();
builder.Services.AddScoped<IGameClipRepository, GameClipRepository>(); 

Console.WriteLine("[SPECTRUM API] Registering Game Catalog Services (Memory Cache & Sync)...");
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddHttpClient<IRawgSyncService, RawgSyncService>();
builder.Services.AddScoped<IGameService, GameService>();

Console.WriteLine("[SPECTRUM API] Configuring JWT Authentication...");
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

Console.WriteLine("[SPECTRUM API] Generating Swagger documentation...");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Spectrum API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT token prefixed with ‘Bearer ’. Example: Bearer abcdef12345"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

Console.WriteLine("[SPECTRUM API] Configuring gRPC clients for services...");
builder.Services.AddGrpcClient<DropService.DropServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DropsServiceUrl"]!);
});

builder.Services.AddGrpcClient<VoteService.VoteServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:SocialServiceUrl"]!);
});

builder.Services.AddGrpcClient<CommentService.CommentServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:SocialServiceUrl"]!);
});

builder.Services.AddGrpcClient<ReportService.ReportServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:SocialServiceUrl"]!);
});

Console.WriteLine("[SPECTRUM API] Building the application...");
var app = builder.Build();

Console.WriteLine("[SPECTRUM API] Setting up the HTTP request pipeline...");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Console.WriteLine("[SPECTRUM API] Initializing in-memory game catalog...\n");

var repository = app.Services.GetRequiredService<IGameRepository>();
var totalGames = repository.GetAll().Count();

if (totalGames > 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[SPECTRUM API] Success: {totalGames} high-quality games loaded into RAM.");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("[SPECTRUM API] Warning: Snapshot not found or empty. Run sync via AdminGamesController.");
    Console.ResetColor();
}

app.UseExceptionHandler();

app.UseCors("AllowFrontend");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("[SPECTRUM API] Boot sequence complete. Server is now listening to requests!\n");
Console.ResetColor();

await app.RunAsync();
