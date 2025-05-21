using System.ComponentModel.DataAnnotations;

namespace Condominio_API.Requests
{
    public class EntradaVisitanteRequest
    {
        [Required]
        public string QrCodeData { get; set; } = string.Empty; // aqui vai ser "lido" o texto que tem no 
                                                               // qrcode, não a imagem em sí
    }
}