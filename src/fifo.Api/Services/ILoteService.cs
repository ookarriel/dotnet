using fifo.Api.DTOs;

namespace fifo.Api.Services;

public interface ILoteService
{
    Task<IReadOnlyList<LoteResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LoteResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LoteResponse> CreateAsync(LoteRequest request, CancellationToken cancellationToken = default);
    Task<LoteResponse> UpdateAsync(int id, LoteUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
