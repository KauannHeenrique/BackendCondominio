using System.ComponentModel.DataAnnotations;

public class EntradaMoradorManualRequest
{
    [Required]
    public int UsuarioId { get; set; }

    [StringLength(255)]
    public string? Observacao { get; set; }

    [StringLength(100)]
    public string? RegistradoPor { get; set; }
}
