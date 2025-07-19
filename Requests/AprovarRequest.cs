using System.ComponentModel.DataAnnotations;

namespace condominio_API.Request
{
    public class AprovarRequest
    {
        public bool Aprovar { get; set; }
        public string? ComentarioSindico { get; set; }
    }

}