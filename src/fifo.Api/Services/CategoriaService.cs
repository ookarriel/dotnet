using fifo.Api.DTOs;
using fifo.Api.Models;
using fifo.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace fifo.Api.Services;

public class CategoriaService(IRepository<Categoria> categorias) : ICategoriaService
{
    public async Task<IReadOnlyList<CategoriaResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await categorias.Query()
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaResponse
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion
            })
            .ToListAsync(cancellationToken);

    public async Task<CategoriaResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await categorias.Query()
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == id && c.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {id}.");
        return Map(categoria);
    }

    public async Task<CategoriaResponse> CreateAsync(CategoriaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateNameAsync(request.Nombre, null, cancellationToken);
        var categoria = new Categoria
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion.Trim(),
            Activo = request.Activo
        };

        await categorias.AddAsync(categoria, cancellationToken);
        await categorias.SaveChangesAsync(cancellationToken);
        return Map(categoria);
    }

    public async Task<CategoriaResponse> UpdateAsync(int id, CategoriaRequest request, CancellationToken cancellationToken = default)
    {
        var categoria = await categorias.Query()
            .SingleOrDefaultAsync(c => c.Id == id && c.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {id}.");

        await ValidateNameAsync(request.Nombre, id, cancellationToken);
        categoria.Nombre = request.Nombre.Trim();
        categoria.Descripcion = request.Descripcion.Trim();
        categoria.Activo = request.Activo;
        categorias.Update(categoria);
        await categorias.SaveChangesAsync(cancellationToken);
        return Map(categoria);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await categorias.Query()
            .Include(c => c.Productos)
            .SingleOrDefaultAsync(c => c.Id == id && c.Activo, cancellationToken)
            ?? throw new NotFoundException($"No existe la categoría {id}.");

        if (categoria.Productos.Any(p => p.Activo))
        {
            throw new ConflictException("No se puede eliminar una categoría con productos activos.");
        }

        categoria.Activo = false;
        await categorias.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateNameAsync(string nombre, int? excludedId, CancellationToken cancellationToken)
    {
        var normalized = nombre.Trim().ToLowerInvariant();
        var duplicated = await categorias.Query().AnyAsync(
            c => c.Nombre.ToLower() == normalized && c.Id != excludedId,
            cancellationToken);
        if (duplicated)
        {
            throw new ConflictException($"La categoría '{nombre}' ya existe.");
        }
    }

    private static CategoriaResponse Map(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nombre = categoria.Nombre,
        Descripcion = categoria.Descripcion,
        Activo = categoria.Activo,
        FechaCreacion = categoria.FechaCreacion
    };
}
