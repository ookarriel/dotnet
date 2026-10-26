using System.ComponentModel.DataAnnotations;

namespace fifo.Api.DTOs;

public class EntradaRequest
{
    [Range(1, int.MaxValue)]
    public int LoteId { get; set; }

    [Range(1, 1000000)]
    public int Cantidad { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal PrecioUnitario { get; set; }

    [StringLength(200)]
    public string Observaciones { get; set; } = string.Empty;
}

public class SalidaRequest
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(1, 1000000)]
    public int Cantidad { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal PrecioUnitario { get; set; }

    [StringLength(200)]
    public string Observaciones { get; set; } = string.Empty;
}

public class MovimientoResponse
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int LoteId { get; set; }
    public string CodigoLote { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public DateTime Fecha { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public string UsuarioResponsable { get; set; } = string.Empty;
}
