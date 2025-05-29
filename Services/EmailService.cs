using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using condominio_API.Models;

namespace condominio_API.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string senhaPadrao);
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
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Bem-vindo ao condomínio JK";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h1 style='color: #26c9a8;'>Bem-vindo ao sistema!</h1>
                    <p>Sua conta foi criada com sucesso. Sua senha padrão é: <strong>{senhaPadrao}</strong></p>
                    <p>Por motivos de segurança, recomendamos que você altere sua senha no primeiro acesso.</p>
                    <p><a href='http://localhost:3000/recuperar-senha' style='color: #26c9a8;'>Clique aqui para alterar sua senha</a></p>
                    <p>Se você não solicitou esta conta, entre em contato com o suporte.</p>"
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