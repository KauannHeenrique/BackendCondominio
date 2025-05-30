using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public class Apartamento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string Bloco { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "O número do apartamento deve ser maior que zero.")]
        public int Numero { get; set; }

        [Required]
        public required string Proprietario { get; set; }

        [Required]
        public SituacaoApartamento Situacao { get; set; } 

        public string? Observacoes { get; set; }
    }
    public enum SituacaoApartamento
    {
        Disponivel = 1,
        Ocupado = 2,
        EmManutencao = 3
    }

}
