using System.ComponentModel.DataAnnotations;

namespace fifo.Api.DTOs;

public class ProductoRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200)]
    public string Descripcion { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoriaId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal PrecioUnitario { get; set; }

    [Range(0, 1000000)]
    public int StockMinimo { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string CodigoSku { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}

public class ProductoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int StockMinimo { get; set; }
    public string CodigoSku { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int StockTotal { get; set; }
    public bool StockBajo { get; set; }
}

public class StockProductoResponse
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int StockTotal { get; set; }
    public int StockMinimo { get; set; }
    public bool StockBajo { get; set; }
    public IReadOnlyList<LoteResponse> Lotes { get; set; } = [];
}
