using condominio_API.Models;

public class AtualizarNotificacaoRequest
{
    public StatusNotificacao? Status { get; set; }
    public string? Comentario { get; set; }
    public bool? MarcarComoLida { get; set; }
}
