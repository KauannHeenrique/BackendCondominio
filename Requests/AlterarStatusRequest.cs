using condominio_API.Models;

public class AlterarStatusRequest
{
    public StatusNotificacao NovoStatus { get; set; }
    public string? ComentarioSindico { get; set; } // Comentário opcional para qualquer mudança
}
