using Microsoft.AspNetCore.Identity;

namespace condominio_API.Services
{
    public static class HashHelper
    {
        private static PasswordHasher<object> hasher = new PasswordHasher<object>();

        public static string GerarHash(string senha)
        {
            return hasher.HashPassword(null, senha);
        }

        public static bool VerificarHash(string hash, string senha)
        {
            if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(senha))
                return false;

            try
            {
                var hasher = new PasswordHasher<string>();
                var result = hasher.VerifyHashedPassword(null, hash, senha);
                return result == PasswordVerificationResult.Success;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Erro ao verificar hash: {ex.Message}");
                return false;
            }
        }
    }
}
