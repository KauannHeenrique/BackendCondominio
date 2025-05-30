using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using condominio_API.Models;

namespace condominio_API.Services
{
    public class ResetPasswordEmailService : IResetPasswordEmailService
    {
        private readonly EmailSettings _emailSettings;

        // Construtor que recebe a configuração do e-mail via injeção de dependência
        public ResetPasswordEmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        // Método que envia o e-mail de reset de senha
        public async Task SendResetPasswordEmailAsync(string toEmail, string senhaPadrao)
        {
            // Criação da mensagem de e-mail
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Alteração de senha bem-sucedida!";

            // Corpo do e-mail em HTML
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h1 style='color: #26c9a8;'>Sua senha foi resetada!</h1>
                    <p>Atendendo sua solicitação, sua senha foi redefinida.</p>
                    <p>Sua nova senha é: <strong>{senhaPadrao}</strong></p>
                    <p>Recomendamos que você altere sua senha assim que possível.</p>
                    <p><a href='http://172.20.10.2:3000/changePassword' style='color: #26c9a8;'>Clique aqui para alterar sua senha</a></p>
                    <p>Se você não solicitou esta alteração, entre em contato com o suporte.</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            // Envio do e-mail utilizando MailKit
            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
