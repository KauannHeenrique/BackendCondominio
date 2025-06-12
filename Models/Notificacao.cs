using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public enum StatusNotificacao
    {
        Pendente = 1,
        Aprovada = 2,
        Rejeitada = 3,
        EmAndamento = 4
    }

    public enum TipoNotificacao
    {
        AvisoDeBarulho = 1,
        SolicitacaoDeReparo = 2,
        Sugestao = 3,
        Outro = 4
    }

    public class Notificacao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public TipoNotificacao Tipo { get; set; }

        [Required]
        [MaxLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Mensagem { get; set; } = string.Empty;

        [Required]
        public StatusNotificacao Status { get; set; } = StatusNotificacao.Pendente;

        public string? ComentarioSindico { get; set; }

        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? UltimaAtualizacao { get; set; }

        // Morador que criou a notificação
        [Required]
        public int MoradorOrigemId { get; set; }

        [ForeignKey("MoradorOrigemId")]
        public Usuario? MoradorOrigem { get; set; }

        // Morador de destino (opcional)
        public int? ApartamentoDestinoId { get; set; }

        [ForeignKey("ApartamentoDestinoId")]
        public Apartamento? ApartamentoDestino { get; set; }
    }
}
