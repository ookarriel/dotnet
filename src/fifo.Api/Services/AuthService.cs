using fifo.Api.DTOs;
using fifo.Api.Models;
using fifo.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace fifo.Api.Services;

public class AuthService(
    IRepository<Usuario> usuarios,
    ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await usuarios.Query().AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new ConflictException("El correo ya está registrado.");
        }

        // El registro público siempre crea usuarios básicos; Admin se asigna por seed o administración.
        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = Roles.User
        };

        await usuarios.AddAsync(usuario, cancellationToken);
        await usuarios.SaveChangesAsync(cancellationToken);
        return CreateAuthResponse(usuario);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await usuarios.Query()
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (usuario is null || !usuario.Activo || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return CreateAuthResponse(usuario);
    }

    public async Task<UsuarioResponse> GetProfileAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarios.Query()
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id && u.Activo, cancellationToken)
            ?? throw new NotFoundException("Usuario no encontrado.");
        return MapUsuario(usuario);
    }

    private AuthResponse CreateAuthResponse(Usuario usuario)
    {
        var token = tokenService.CreateToken(usuario);
        return new AuthResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            Usuario = MapUsuario(usuario)
        };
    }

    private static UsuarioResponse MapUsuario(Usuario usuario) => new()
    {
        Id = usuario.Id,
        NombreCompleto = usuario.NombreCompleto,
        Email = usuario.Email,
        Rol = usuario.Rol
    };
}
