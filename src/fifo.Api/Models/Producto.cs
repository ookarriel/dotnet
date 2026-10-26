using System.ComponentModel.DataAnnotations;

namespace fifo.Api.Models;

public class Producto
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public int CategoriaId { get; set; }

    [Range(0, 1000000)]
    public decimal PrecioUnitario { get; set; }

    [Range(0, 1000000)]
    public int StockMinimo { get; set; } = 0;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string CodigoSku { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual Categoria? Categoria { get; set; }
    public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}