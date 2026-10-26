using System.ComponentModel.DataAnnotations;

namespace fifo.Api.Models;

public enum TipoMovimiento
{
    Entrada = 1,
    Salida = 2,
    Ajuste = 3
}

public class Movimiento
{
    public int Id { get; set; }

    [Required]
    public TipoMovimiento Tipo { get; set; }

    [Required]
    public int LoteId { get; set; }

    [Required]
    public int ProductoId { get; set; }

    [Range(1, 1000000)]
    public int Cantidad { get; set; }

    [Range(0, 1000000)]
    public decimal PrecioUnitario { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string Observaciones { get; set; } = string.Empty;

    public string UsuarioResponsable { get; set; } = string.Empty;

    // Navegación
    public virtual Lote? Lote { get; set; }
    public virtual Producto? Producto { get; set; }
}