using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public class AcessoEntradaVisitante
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int VisitanteId { get; set; }

        [ForeignKey("VisitanteId")]
        public Visitante? Visitante { get; set; }

        [Required]
        public int UsuarioId { get; set; }  // Morador responsável

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Required]
        public DateTime DataHoraEntrada { get; set; }

        [MaxLength(500)]
        public string? Observacao { get; set; } // Ex.: "Entrou pela portaria lateral"

        [MaxLength(200)]
        public string? RegistradoPor { get; set; } // Nome do operador/logado

        [Required]
        [MaxLength(50)]
        public string EntradaPor { get; set; } = "QRCode"; // QRCode ou Manual
    }
}
