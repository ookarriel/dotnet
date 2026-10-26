using fifo.Api.DTOs;

namespace fifo.Api.Services;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoriaResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoriaResponse> CreateAsync(CategoriaRequest request, CancellationToken cancellationToken = default);
    Task<CategoriaResponse> UpdateAsync(int id, CategoriaRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
