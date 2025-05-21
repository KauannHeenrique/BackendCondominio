using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace condominio_API.Models
{
    public class QRCodeTemp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MoradorId { get; set; }

        [ForeignKey("MoradorId")]
        public Usuario? Morador { get; set; }

        [Required]
        public int VisitanteId { get; set; }

        [ForeignKey("VisitanteId")]
        public Visitante? Visitante { get; set; }

        [Required]
        public bool TipoQRCode { get; set; } //se for true = 24h, se for false = unico 

        [Required]
        public DateTime DataCriacao { get; set; }

        [Required]
        public DateTime DataValidade { get; set; }

        [Required]
        public bool Status { get; set; }

        [Required]
        public byte[] QrCodeImagem { get; set; }

        public string QrCodeData { get; set; } = string.Empty; // campo onde eu vou salvar o texto que 
                                                               // vai ser escrito no qrcode para verificar 
                                                               // se ele é valido 
    }
}
