    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace condominio_API.Models
    {
        public class AcessoEntradaMorador
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            [Required]
            public int UsuarioId { get; set; }  

            [ForeignKey("UsuarioId")]
            public Usuario? Usuario { get; set; } 

            [Required]
            public DateTime DataHoraEntrada { get; set; }
        }
    }
