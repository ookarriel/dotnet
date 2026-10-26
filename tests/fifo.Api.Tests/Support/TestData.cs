using fifo.Api.DTOs;

namespace fifo.Api.Tests.Support;

public static class TestData
{
    public static string Code(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 9, 50)].ToUpperInvariant();

    public static CategoriaRequest Categoria() => new()
    {
        Nombre = Code("CAT"),
        Descripcion = "Categoría creada por prueba de integración"
    };

    public static ProductoRequest Producto(int categoriaId) => new()
    {
        Nombre = "Producto de integración",
        Descripcion = "Producto creado por prueba automatizada",
        CategoriaId = categoriaId,
        PrecioUnitario = 25.50m,
        StockMinimo = 2,
        CodigoSku = Code("SKU")
    };
}
