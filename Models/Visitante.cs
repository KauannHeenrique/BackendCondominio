using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public class Visitante
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VisitanteId { get; set; }


        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string Nome { get; set; }


        [Required]
        [StringLength(14, MinimumLength = 8)]
        public required string Documento { get; set; }


        [Required]
        [StringLength(15)]
        public string? Telefone { get; set; }


        [StringLength(14)]
        public string? Cnpj { get; set; }


        [StringLength(100)]
        public string? NomeEmpresa { get; set; }

        public bool Status { get; set; } = true;

        public bool PrestadorServico { get; set; } = false;


    }
}