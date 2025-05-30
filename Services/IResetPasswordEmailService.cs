namespace condominio_API.Services
{
    public interface IResetPasswordEmailService
    {
        Task SendResetPasswordEmailAsync(string toEmail, string senhaPadrao);
    }
}
