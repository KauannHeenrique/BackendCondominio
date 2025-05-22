using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using condominio_API.Models;

namespace condominio_API.Services
{
    public class TokenService
    {
        private readonly string _secretKey = "PvvCX14bFPJKfA6dZib1DitiRnuhgS7uoAZw3AgIYS4=";
        private readonly string _issuer = "condominio_backend.com";
        private readonly string _audience = "condominio_backend.com";

        public string GerarJwtToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Documento),
                new Claim("id", usuario.UsuarioId.ToString()),
                new Claim("nivel_acesso", usuario.NivelAcesso.ToString()),

                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(50),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
