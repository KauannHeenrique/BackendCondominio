using condominio_API.Models;

public class CriarNotificacaoRequest
{
    public string Titulo { get; set; }
    public string Mensagem { get; set; }
    public TipoNotificacao Tipo { get; set; }
    public int MoradorOrigemId { get; set; }
    public int? ApartamentoDestinoId { get; set; } // Apenas para Aviso de Barulho
    public int? UsuarioDestinoId { get; set; } // Apenas para tipo Outro, se síndico escolher
    public bool CriadoPorSindico { get; set; } // ✅ Nova flag
}

