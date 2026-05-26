# Spectrum SMTP setup

Spectrum sends registration verification, password recovery, and profile password-change codes through SMTP.

For Gmail:

1. Enable two-step verification on the Google account.
2. Generate an app password at https://myaccount.google.com/apppasswords.
3. Store the app password only in local secrets or environment variables.

Required variables:

```env
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=spectrum.noreplay@gmail.com
SMTP_PASSWORD=<APP_PASSWORD_GENERATED_FROM_GOOGLE>
SMTP_FROM_EMAIL=spectrum.noreplay@gmail.com
SMTP_FROM_NAME=Spectrum
SMTP_USE_TLS=true
```

For local .NET development, prefer User Secrets:

```powershell
dotnet user-secrets set "Smtp:Password" "<APP_PASSWORD_GENERATED_FROM_GOOGLE>" --project services/api-core/Spectrum.API/Spectrum.API.csproj
```

Do not commit real SMTP passwords in `appsettings*.json`, `.env`, README files, or frontend environment files.
