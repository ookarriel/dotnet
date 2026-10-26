using System.ComponentModel.DataAnnotations;

namespace fifo.Api.DTOs;

public class CategoriaRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200)]
    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}

public class CategoriaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
