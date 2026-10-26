using fifo.Api.DTOs;

namespace fifo.Api.Services;

public interface IInventarioService
{
    Task<IReadOnlyList<MovimientoResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MovimientoResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MovimientoResponse> RegistrarEntradaAsync(EntradaRequest request, string usuario, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MovimientoResponse>> RegistrarSalidaFifoAsync(SalidaRequest request, string usuario, CancellationToken cancellationToken = default);
}
