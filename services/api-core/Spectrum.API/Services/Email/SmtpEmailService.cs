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
                "Verificacion de cuenta",
                "Usa este codigo para activar tu cuenta en Spectrum:",
                code
            );
        }

        public Task SendPasswordResetAsync(string email, string code)
        {
            return SendCodeAsync(
                email,
                "Recupera tu contrasena de Spectrum",
                "Recuperacion de contrasena",
                "Usa este codigo para continuar con la recuperacion de tu contrasena:",
                code
            );
        }

        public Task SendPasswordChangeAsync(string email, string code)
        {
            return SendCodeAsync(
                email,
                "Confirma tu cambio de contrasena",
                "Cambio de contrasena",
                "Usa este codigo para confirmar el cambio de contrasena de tu cuenta:",
                code
            );
        }

        public Task SendRewardAsync(string email, string eventTitle, string rewardCode)
        {
            return SendCustomAsync(
                email,
                $"Tu recompensa de Spectrum: {eventTitle}",
                "Ganaste un sorteo en Spectrum",
                $"Felicidades. Ganaste el sorteo {eventTitle}. Tienes 24 horas para canjear este codigo:",
                rewardCode,
                "No compartas este codigo. Si no reconoces este premio, contacta al soporte de Spectrum."
            );
        }

        private Task SendCodeAsync(string email, string subject, string title, string intro, string code)
        {
            return SendCustomAsync(
                email,
                subject,
                title,
                intro,
                code,
                "Este codigo expira pronto. Si no solicitaste este correo, puedes ignorarlo."
            );
        }

        private async Task SendCustomAsync(string email, string subject, string title, string intro, string code, string footer)
        {
            EnsureConfigured();

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = BuildHtmlTemplate(title, intro, code, footer),
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

        private static string BuildHtmlTemplate(string title, string intro, string code, string footer)
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
                      <p style="line-height:1.5;color:#aaa;">{WebUtility.HtmlEncode(footer)}</p>
                    </div>
                  </div>
                </body>
                </html>
                """;
        }
    }
}
