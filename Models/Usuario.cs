using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string Nome { get; set; }

        [Required]
        [StringLength(14, MinimumLength = 8)]
        public required string Documento { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }

        [Required]
        [StringLength(255)]
        public required string Senha { get; set; }

        [Required]
        public NivelAcessoEnum NivelAcesso { get; set; }

        [Required]
        [StringLength(15)]
        public string? Telefone { get; set; }

        public int? ApartamentoId { get; set; }

        [ForeignKey("ApartamentoId")]
        public Apartamento? Apartamento { get; set; }

        [Required]
        [StringLength(50)]
        public required string CodigoRFID { get; set; }

        [Required]
        public bool Status { get; set; }

        public bool IsTemporaryPassword { get; set; }

        [Required]
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }

    public enum NivelAcessoEnum
    {
        Admin = 1,
        Sindico = 2,
        Funcionario = 3,
        Morador = 4
    }
}
