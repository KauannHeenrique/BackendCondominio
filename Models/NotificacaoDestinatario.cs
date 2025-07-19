using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public class NotificacaoDestinatario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int NotificacaoId { get; set; }
        public Notificacao Notificacao { get; set; }

        [Required]
        public int UsuarioDestinoId { get; set; }
        public Usuario UsuarioDestino { get; set; }

        public bool Lido { get; set; } = false;
    }


}
