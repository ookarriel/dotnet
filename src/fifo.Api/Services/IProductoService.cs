using fifo.Api.DTOs;

namespace fifo.Api.Services;

public interface IProductoService
{
    Task<IReadOnlyList<ProductoResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductoResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StockProductoResponse> GetStockAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductoResponse> CreateAsync(ProductoRequest request, CancellationToken cancellationToken = default);
    Task<ProductoResponse> UpdateAsync(int id, ProductoRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
