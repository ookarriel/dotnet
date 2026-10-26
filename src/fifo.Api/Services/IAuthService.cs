using fifo.Api.DTOs;

namespace fifo.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterUsuarioRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UsuarioResponse> GetProfileAsync(int id, CancellationToken cancellationToken = default);
}
