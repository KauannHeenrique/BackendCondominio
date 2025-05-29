namespace Condominio_API.Requests
{
    public class AuthRequest
    {
        public required string CPF { get; set; }
        public required string Senha { get; set; }
    }
}
