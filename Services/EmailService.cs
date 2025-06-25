using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using condominio_API.Models;
using static QRCoder.PayloadGenerator.SwissQrCode;

namespace condominio_API.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string senhaPadrao);
        Task SendResetPasswordEmailAsync(string toEmail, string senhaPadrao);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string senhaPadrao)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentNullException(nameof(toEmail), "O endereço de e-mail não pode ser nulo ou vazio.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail)); // Substitui a versão antiga com ""
            message.Subject = "Bem-vindo ao condomínio JK";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h1 style='color: #26c9a8;'>Bem-vindo ao sistema!</h1>
                    <p>Sua conta foi criada com sucesso. Sua senha padrão é: <strong>{senhaPadrao}</strong></p>
                    <p>Por motivos de segurança, recomendamos que você altere sua senha no primeiro acesso.</p>
                    <p><a href='http://172.20.10.2:3000/changePassword' style='color: #26c9a8;'>Clique aqui para alterar sua senha</a></p>
                    <p>Se você não solicitou esta conta, entre em contato com o suporte.</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendResetPasswordEmailAsync(string toEmail, string senhaPadrao)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentNullException(nameof(toEmail), "O endereço de e-mail não pode ser nulo ou vazio.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Senha redefinida - Sistema Condomínio JK";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2 style='color: #26c9a8;'>Sua senha foi redefinida</h2>
                    <p>Olá, sua nova senha temporária é: <strong>{senhaPadrao}</strong></p>
                    <p>Recomendamos que você altere sua senha assim que possível.</p>
                    <<p><a href='http://172.20.10.2:3000/changePassword' style='color: #26c9a8;'>Clique aqui para alterar sua senha</a></p>
                    <p>Se você não solicitou esta alteração, entre em contato com o suporte.</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
