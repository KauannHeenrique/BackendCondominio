namespace Condominio_API.Requests
{
    public class RelatorioMultiploRequest
    {
        public bool Usuarios { get; set; }
        public bool Visitantes { get; set; }
        public bool Apartamentos { get; set; }
        public bool EntradasMorador { get; set; }
        public bool EntradasVisitante { get; set; }
        public bool Notificacoes { get; set; }
    }

}
