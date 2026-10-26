using System.ComponentModel.DataAnnotations;

namespace fifo.Api.Models;

public class Lote
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string CodigoLote { get; set; } = string.Empty;

    [Required]
    public int ProductoId { get; set; }

    [Range(0, 1000000)]
    public int CantidadInicial { get; set; }

    [Range(0, 1000000)]
    public int CantidadDisponible { get; set; }

    [Range(0, 1000000)]
    public decimal PrecioCompra { get; set; }

    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;

    public DateTime? FechaVencimiento { get; set; }

    public bool Activo { get; set; } = true;

    // Navegación
    public virtual Producto? Producto { get; set; }
    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}