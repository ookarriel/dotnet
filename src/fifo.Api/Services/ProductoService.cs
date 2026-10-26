using fifo.Api.DTOs;
using fifo.Api.Models;
using fifo.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace fifo.Api.Services;

public class ProductoService(
    IRepository<Producto> productos,
    IRepository<Categoria> categorias) : IProductoService
{
    public async Task<IReadOnlyList<ProductoResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await productos.Query()
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Lotes)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    public async Task<ProductoResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await GetActiveWithDetailsAsync(id, cancellationToken);
        return Map(producto);
    }

    public async Task<StockProductoResponse> GetStockAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await productos.Query()
            .AsNoTracking()
            .Include(p => p.Lotes)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No existe el producto {id}.");

        var lotes = producto.Lotes
            .Where(l => l.Activo)
            .OrderBy(l => l.FechaIngreso)
            .ThenBy(l => l.Id)
            .ToList();

        var total = lotes.Sum(l => l.CantidadDisponible);
        return new StockProductoResponse
        {
            ProductoId = producto.Id,
            ProductoNombre = producto.Nombre,
            StockTotal = total,
            StockMinimo = producto.StockMinimo,
            StockBajo = total <= producto.StockMinimo,
            Lotes = lotes.Select(MapLote).ToList()
        };
    }

    public async Task<ProductoResponse> CreateAsync(ProductoRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateCategoryAsync(request.CategoriaId, cancellationToken);
        await ValidateSkuAsync(request.CodigoSku, null, cancellationToken);

        var producto = new Producto
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion.Trim(),
            CategoriaId = request.CategoriaId,
            PrecioUnitario = request.PrecioUnitario,
            StockMinimo = request.StockMinimo,
            CodigoSku = request.CodigoSku.Trim(),
            Activo = request.Activo
        };

        await productos.AddAsync(producto, cancellationToken);
        await productos.SaveChangesAsync(cancellationToken);

        return Map(await GetWithDetailsAsync(producto.Id, cancellationToken));
    }

    public async Task<ProductoResponse> UpdateAsync(int id, ProductoRequest request, CancellationToken cancellationToken = default)
    {
        var producto = await GetActiveWithDetailsAsync(id, cancellationToken);
        await ValidateCategoryAsync(request.CategoriaId, cancellationToken);
        await ValidateSkuAsync(request.CodigoSku, id, cancellationToken);

        producto.Nombre = request.Nombre.Trim();
        producto.Descripcion = request.Descripcion.Trim();
        producto.CategoriaId = request.CategoriaId;
        producto.PrecioUnitario = request.PrecioUnitario;
        producto.StockMinimo = request.StockMinimo;
        producto.CodigoSku = request.CodigoSku.Trim();
        producto.Activo = request.Activo;

        productos.Update(producto);
        await productos.SaveChangesAsync(cancellationToken);
        return Map(producto);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await productos.Query()
            .SingleOrDefaultAsync(p => p.Id == id && p.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe el producto {id}.");

        producto.Activo = false;
        await productos.SaveChangesAsync(cancellationToken);
    }

    private async Task<Producto> GetActiveWithDetailsAsync(int id, CancellationToken cancellationToken) =>
        await productos.Query()
            .Include(p => p.Categoria)
            .Include(p => p.Lotes)
            .SingleOrDefaultAsync(p => p.Id == id && p.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe el producto {id}.");

    private async Task<Producto> GetWithDetailsAsync(int id, CancellationToken cancellationToken) =>
        await productos.Query()
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Lotes)
            .SingleAsync(p => p.Id == id, cancellationToken);

    private async Task ValidateCategoryAsync(int categoriaId, CancellationToken cancellationToken)
    {
        var exists = await categorias.Query()
            .AnyAsync(c => c.Id == categoriaId && c.Activo, cancellationToken);
        if (!exists)
        {
            throw new BusinessRuleException("La categoría indicada no existe o está inactiva.");
        }
    }

    private async Task ValidateSkuAsync(string sku, int? excludedId, CancellationToken cancellationToken)
    {
        var normalized = sku.Trim().ToLowerInvariant();
        var duplicated = await productos.Query().AnyAsync(
            p => p.CodigoSku.ToLower() == normalized && p.Id != excludedId,
            cancellationToken);

        if (duplicated)
        {
            throw new ConflictException($"El SKU '{sku}' ya está registrado.");
        }
    }

    private static ProductoResponse Map(Producto producto)
    {
        var stock = producto.Lotes.Where(l => l.Activo).Sum(l => l.CantidadDisponible);
        return new ProductoResponse
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            CategoriaId = producto.CategoriaId,
            CategoriaNombre = producto.Categoria?.Nombre ?? string.Empty,
            PrecioUnitario = producto.PrecioUnitario,
            StockMinimo = producto.StockMinimo,
            CodigoSku = producto.CodigoSku,
            Activo = producto.Activo,
            FechaCreacion = producto.FechaCreacion,
            StockTotal = stock,
            StockBajo = stock <= producto.StockMinimo
        };
    }

    private static LoteResponse MapLote(Lote lote) => new()
    {
        Id = lote.Id,
        CodigoLote = lote.CodigoLote,
        ProductoId = lote.ProductoId,
        CantidadInicial = lote.CantidadInicial,
        CantidadDisponible = lote.CantidadDisponible,
        PrecioCompra = lote.PrecioCompra,
        FechaIngreso = lote.FechaIngreso,
        FechaVencimiento = lote.FechaVencimiento,
        Activo = lote.Activo
    };
}
