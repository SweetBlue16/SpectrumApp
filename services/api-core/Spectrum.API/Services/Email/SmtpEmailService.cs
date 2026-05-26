using Microsoft.Extensions.Options;
using Spectrum.API.Configuration;
using Spectrum.API.Exceptions;
using Spectrum.API.Utilities;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Spectrum.API.Services.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public Task SendRegistrationVerificationAsync(string email, string code)
        {
            return SendCodeAsync(
                email,
                "Verifica tu cuenta de Spectrum",
                "Verificación de cuenta",
                "Usa este código para activar tu cuenta en Spectrum:",
                code
            );
        }

        public Task SendPasswordResetAsync(string email, string code)
        {
            return SendCodeAsync(
                email,
                "Recupera tu contraseña de Spectrum",
                "Recuperación de contraseña",
                "Usa este código para continuar con la recuperación de tu contraseña:",
                code
            );
        }

        public Task SendPasswordChangeAsync(string email, string code)
        {
            return SendCodeAsync(
                email,
                "Confirma tu cambio de contraseña",
                "Cambio de contraseña",
                "Usa este código para confirmar el cambio de contraseña de tu cuenta:",
                code
            );
        }

        private async Task SendCodeAsync(string email, string subject, string title, string intro, string code)
        {
            EnsureConfigured();

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = BuildHtmlTemplate(title, intro, code),
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };
            message.To.Add(email);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseTls,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            try
            {
                await client.SendMailAsync(message);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP delivery failed for purpose {Subject}", subject);
                throw new SpectrumServiceUnavailableException("emailDeliveryFailed");
            }
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(_options.Host) ||
                string.IsNullOrWhiteSpace(_options.Username) ||
                string.IsNullOrWhiteSpace(_options.Password) ||
                string.IsNullOrWhiteSpace(_options.FromEmail))
            {
                throw new SpectrumServiceUnavailableException(Constants.ErrorMessages.SmtpConfigurationInvalid);
            }
        }

        private static string BuildHtmlTemplate(string title, string intro, string code)
        {
            return $"""
                <!doctype html>
                <html lang="es">
                <body style="margin:0;padding:0;background:#111;color:#fff;font-family:Arial,sans-serif;">
                  <div style="max-width:560px;margin:0 auto;padding:32px;">
                    <h1 style="color:#ffffff;margin:0 0 16px;">Spectrum</h1>
                    <div style="background:#1b1b1f;border:1px solid #3b2d68;border-radius:8px;padding:24px;">
                      <h2 style="margin:0 0 12px;color:#ffffff;">{WebUtility.HtmlEncode(title)}</h2>
                      <p style="line-height:1.5;color:#d8d8e2;">{WebUtility.HtmlEncode(intro)}</p>
                      <p style="font-size:28px;letter-spacing:6px;font-weight:700;color:#a987ff;margin:24px 0;">{WebUtility.HtmlEncode(code)}</p>
                      <p style="line-height:1.5;color:#aaa;">Este código expira pronto. Si no solicitaste este correo, puedes ignorarlo.</p>
                    </div>
                  </div>
                </body>
                </html>
                """;
        }
    }
}
