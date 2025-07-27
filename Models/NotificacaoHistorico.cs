using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public class NotificacaoHistorico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Relacionamento com a Notificação
        [Required]
        public int NotificacaoId { get; set; }
        public Notificacao Notificacao { get; set; }

        // Quem executou a ação
        public int? UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        // Tipo da ação
        [Required]
        public AcaoHistorico Acao { get; set; }

        // Status anterior e novo (quando aplicável)
        public StatusNotificacao? StatusAnterior { get; set; }
        public StatusNotificacao? StatusNovo { get; set; }

        // Comentário opcional
        public string? Comentario { get; set; }

        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    }

    public enum AcaoHistorico
    {
        CRIACAO,  // comeca em 0
        APROVACAO, // 1 
        REJEICAO,  // 2
        COMENTARIO,
        LEITURA,
        PENDENTE,
        EM_ANDAMENTO,
        CONCLUIDA  //  7
    }
}
