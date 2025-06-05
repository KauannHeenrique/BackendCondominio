public class AtualizarUsuarioRequest
{
    public string? Nome { get; set; }
    public string? Documento { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public int? ApartamentoId { get; set; }
    public string? CodigoRFID { get; set; }
    public bool Status { get; set; }
}