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
            var result = hasher.VerifyHashedPassword(null, hash, senha);
            return result == PasswordVerificationResult.Success;
        }
    }
}
