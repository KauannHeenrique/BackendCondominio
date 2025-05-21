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
        public int UsuarioId { get; set; }  

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Required]
        public DateTime DataHoraEntrada { get; set; }
    }
}
