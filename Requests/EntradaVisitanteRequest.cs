public class EntradaVisitanteRequest
{
    public string? QrCodeData { get; set; } // Para QR Code
    public int? VisitanteId { get; set; }   // Para manual
    public string? Bloco { get; set; }
    public string? Apartamento { get; set; }
    public string? CpfMorador { get; set; }
    public string? Observacao { get; set; }
    public string? RegistradoPor { get; set; }
}
