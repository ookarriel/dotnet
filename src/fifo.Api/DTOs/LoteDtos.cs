using System.ComponentModel.DataAnnotations;

namespace fifo.Api.DTOs;

public class LoteRequest
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string CodigoLote { get; set; } = string.Empty;

    [Range(1, 1000000)]
    public int Cantidad { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal PrecioCompra { get; set; }

    public DateTime? FechaIngreso { get; set; }

    public DateTime? FechaVencimiento { get; set; }
}

public class LoteUpdateRequest
{
    [Range(typeof(decimal), "0", "1000000")]
    public decimal PrecioCompra { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public bool Activo { get; set; }
}

public class LoteResponse
{
    public int Id { get; set; }
    public string CodigoLote { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int CantidadInicial { get; set; }
    public int CantidadDisponible { get; set; }
    public decimal PrecioCompra { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool Activo { get; set; }
}
