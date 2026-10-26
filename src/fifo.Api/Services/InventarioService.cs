using fifo.Api.DTOs;
using fifo.Api.Models;
using fifo.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace fifo.Api.Services;

public class InventarioService(
    IRepository<Movimiento> movimientos,
    IRepository<Lote> lotes,
    IRepository<Producto> productos) : IInventarioService
{
    public async Task<IReadOnlyList<MovimientoResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await BaseQuery()
            .AsNoTracking()
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.Id)
            .ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<MovimientoResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var movimiento = await BaseQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No existe el movimiento {id}.");
        return Map(movimiento);
    }

    public async Task<MovimientoResponse> RegistrarEntradaAsync(
        EntradaRequest request,
        string usuario,
        CancellationToken cancellationToken = default)
    {
        var lote = await lotes.Query()
            .Include(l => l.Producto)
            .SingleOrDefaultAsync(l => l.Id == request.LoteId && l.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe el lote {request.LoteId}.");

        if (lote.Producto is null || !lote.Producto.Activo)
        {
            throw new BusinessRuleException("No se puede registrar entrada para un producto inactivo.");
        }

        lote.CantidadDisponible += request.Cantidad;
        var movimiento = new Movimiento
        {
            Tipo = TipoMovimiento.Entrada,
            ProductoId = lote.ProductoId,
            LoteId = lote.Id,
            Cantidad = request.Cantidad,
            PrecioUnitario = request.PrecioUnitario,
            Observaciones = request.Observaciones.Trim(),
            UsuarioResponsable = usuario
        };

        await movimientos.AddAsync(movimiento, cancellationToken);
        await movimientos.SaveChangesAsync(cancellationToken);
        movimiento.Lote = lote;
        movimiento.Producto = lote.Producto;
        return Map(movimiento);
    }

    public async Task<IReadOnlyList<MovimientoResponse>> RegistrarSalidaFifoAsync(
        SalidaRequest request,
        string usuario,
        CancellationToken cancellationToken = default)
    {
        var producto = await productos.Query()
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == request.ProductoId && p.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe el producto {request.ProductoId}.");

        // El orden por ingreso e Id implementa First-In-First-Out de forma determinista.
        var lotesDisponibles = await lotes.Query()
            .Where(l => l.ProductoId == producto.Id && l.Activo && l.CantidadDisponible > 0)
            .OrderBy(l => l.FechaIngreso)
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);

        var disponible = lotesDisponibles.Sum(l => l.CantidadDisponible);
        if (disponible < request.Cantidad)
        {
            throw new ConflictException(
                $"Stock insuficiente. Disponible: {disponible}; solicitado: {request.Cantidad}.");
        }

        var restante = request.Cantidad;
        var fechaMovimiento = DateTime.UtcNow;
        var creados = new List<Movimiento>();

        foreach (var lote in lotesDisponibles)
        {
            if (restante == 0)
            {
                break;
            }

            var cantidad = Math.Min(restante, lote.CantidadDisponible);
            lote.CantidadDisponible -= cantidad;
            restante -= cantidad;

            var movimiento = new Movimiento
            {
                Tipo = TipoMovimiento.Salida,
                ProductoId = producto.Id,
                LoteId = lote.Id,
                Cantidad = cantidad,
                PrecioUnitario = request.PrecioUnitario,
                Fecha = fechaMovimiento,
                Observaciones = request.Observaciones.Trim(),
                UsuarioResponsable = usuario
            };

            await movimientos.AddAsync(movimiento, cancellationToken);
            creados.Add(movimiento);
        }

        // Una sola escritura garantiza que movimientos y stock queden sincronizados.
        await movimientos.SaveChangesAsync(cancellationToken);

        foreach (var movimiento in creados)
        {
            movimiento.Producto = producto;
            movimiento.Lote = lotesDisponibles.Single(l => l.Id == movimiento.LoteId);
        }

        return creados.Select(Map).ToList();
    }

    private IQueryable<Movimiento> BaseQuery() => movimientos.Query()
        .Include(m => m.Producto)
        .Include(m => m.Lote);

    private static MovimientoResponse Map(Movimiento movimiento) => new()
    {
        Id = movimiento.Id,
        Tipo = movimiento.Tipo.ToString(),
        ProductoId = movimiento.ProductoId,
        ProductoNombre = movimiento.Producto?.Nombre ?? string.Empty,
        LoteId = movimiento.LoteId,
        CodigoLote = movimiento.Lote?.CodigoLote ?? string.Empty,
        Cantidad = movimiento.Cantidad,
        PrecioUnitario = movimiento.PrecioUnitario,
        Fecha = movimiento.Fecha,
        Observaciones = movimiento.Observaciones,
        UsuarioResponsable = movimiento.UsuarioResponsable
    };
}
