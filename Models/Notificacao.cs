using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public enum StatusNotificacao
    {
        Pendente = 1,
        Aprovada = 2,
        Rejeitada = 3,
        EmAndamento = 4,
        Concluida = 5
    }

    public enum TipoNotificacao
    {
        AvisoDeBarulho = 1,
        SolicitacaoDeReparo = 2,
        Sugestao = 3,
        Outro = 4,
        ComunicadoGeral = 5 
    }


    public class Notificacao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public TipoNotificacao Tipo { get; set; }

        [Required, MaxLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Mensagem { get; set; } = string.Empty;

        [Required]
        public StatusNotificacao Status { get; set; } = StatusNotificacao.Pendente;

        public string? ComentarioSindico { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? UltimaAtualizacao { get; set; }

        [Required]
        public int MoradorOrigemId { get; set; }
        public Usuario? MoradorOrigem { get; set; }

        public bool CriadoPorSindico { get; set; } = false;

        // Relacionamento com destinatários
        public ICollection<NotificacaoDestinatario> Destinatarios { get; set; } = new List<NotificacaoDestinatario>();

        // ✅ Novo: Histórico da Notificação
        public ICollection<NotificacaoHistorico> Historico { get; set; } = new List<NotificacaoHistorico>();

    }


}
