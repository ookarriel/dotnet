using System.ComponentModel.DataAnnotations;

namespace fifo.Api.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200)]
    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}