using fifo.Api.DTOs;
using fifo.Api.Models;
using fifo.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace fifo.Api.Services;

public class LoteService(
    IRepository<Lote> lotes,
    IRepository<Producto> productos) : ILoteService
{
    public async Task<IReadOnlyList<LoteResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await lotes.Query()
            .AsNoTracking()
            .Include(l => l.Producto)
            .Where(l => l.Activo)
            .OrderBy(l => l.FechaIngreso)
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<LoteResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var lote = await lotes.Query()
            .AsNoTracking()
            .Include(l => l.Producto)
            .SingleOrDefaultAsync(l => l.Id == id && l.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe el lote {id}.");
        return Map(lote);
    }

    public async Task<LoteResponse> CreateAsync(LoteRequest request, CancellationToken cancellationToken = default)
    {
        var producto = await productos.Query()
            .SingleOrDefaultAsync(p => p.Id == request.ProductoId && p.Activo, cancellationToken)
            ?? throw new BusinessRuleException("El producto indicado no existe o está inactivo.");

        await ValidateCodeAsync(request.CodigoLote, cancellationToken);
        ValidateDates(request.FechaIngreso, request.FechaVencimiento);

        var lote = new Lote
        {
            ProductoId = producto.Id,
            CodigoLote = request.CodigoLote.Trim(),
            CantidadInicial = request.Cantidad,
            CantidadDisponible = request.Cantidad,
            PrecioCompra = request.PrecioCompra,
            FechaIngreso = request.FechaIngreso ?? DateTime.UtcNow,
            FechaVencimiento = request.FechaVencimiento
        };

        await lotes.AddAsync(lote, cancellationToken);
        await lotes.SaveChangesAsync(cancellationToken);
        lote.Producto = producto;
        return Map(lote);
    }

    public async Task<LoteResponse> UpdateAsync(int id, LoteUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var lote = await lotes.Query()
            .Include(l => l.Producto)
            .SingleOrDefaultAsync(l => l.Id == id && l.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe el lote {id}.");

        ValidateDates(lote.FechaIngreso, request.FechaVencimiento);
        lote.PrecioCompra = request.PrecioCompra;
        lote.FechaVencimiento = request.FechaVencimiento;
        lote.Activo = request.Activo;
        lotes.Update(lote);
        await lotes.SaveChangesAsync(cancellationToken);
        return Map(lote);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var lote = await lotes.Query()
            .SingleOrDefaultAsync(l => l.Id == id && l.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe el lote {id}.");

        lote.Activo = false;
        await lotes.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateCodeAsync(string codigo, CancellationToken cancellationToken)
    {
        var normalized = codigo.Trim().ToLowerInvariant();
        if (await lotes.Query().AnyAsync(l => l.CodigoLote.ToLower() == normalized, cancellationToken))
        {
            throw new ConflictException($"El código de lote '{codigo}' ya existe.");
        }
    }

    private static void ValidateDates(DateTime? fechaIngreso, DateTime? fechaVencimiento)
    {
        if (fechaIngreso.HasValue && fechaVencimiento.HasValue && fechaVencimiento <= fechaIngreso)
        {
            throw new BusinessRuleException("La fecha de vencimiento debe ser posterior a la fecha de ingreso.");
        }
    }

    private static LoteResponse Map(Lote lote) => new()
    {
        Id = lote.Id,
        CodigoLote = lote.CodigoLote,
        ProductoId = lote.ProductoId,
        ProductoNombre = lote.Producto?.Nombre ?? string.Empty,
        CantidadInicial = lote.CantidadInicial,
        CantidadDisponible = lote.CantidadDisponible,
        PrecioCompra = lote.PrecioCompra,
        FechaIngreso = lote.FechaIngreso,
        FechaVencimiento = lote.FechaVencimiento,
        Activo = lote.Activo
    };
}
