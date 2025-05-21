using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public class Notificacao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MoradorOrigemId { get; set; }

        [ForeignKey("MoradorOrigemId")]
        public Usuario? MoradorOrigem { get; set; }

        [Required]
        public int ApartamentoDestinoId { get; set; }

        [ForeignKey("ApartamentoDestinoId")]
        public Apartamento? ApartamentoDestino { get; set; }

        [Required]
        public required string Mensagem { get; set; }

        [Required]
        public DateTime DataHora { get; set; }
    }
}