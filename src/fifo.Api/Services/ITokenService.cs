using fifo.Api.Models;

namespace fifo.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Usuario usuario);
}
