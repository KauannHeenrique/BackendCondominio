    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

    namespace condominio_API.Models
    {
    [Keyless]
    public class AtividadeView
    {
        public int ReferenciaId { get; set; }  // ✅ ADICIONE ISSO
        public string? Tipo { get; set; }
        public string? Descricao { get; set; }
        public DateTime? DataRegistro { get; set; } // <-- Aqui
        public string? Status { get; set; }
        public int UsuarioId { get; set; }
    }


}
