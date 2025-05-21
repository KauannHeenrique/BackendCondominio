using System.ComponentModel.DataAnnotations;

namespace condominio_API.Request
{
    public class EntradaMoradorRequest
    {
        [Required]
        public string CodigoRFID { get; set; } = string.Empty; 
    }
}