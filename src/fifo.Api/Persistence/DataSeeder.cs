using fifo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace fifo.Api.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(FifoDbContext context, IConfiguration configuration)
    {
        await SeedUserAsync(context, configuration, Roles.Admin, "Admin");
        await SeedUserAsync(context, configuration, Roles.User, "User");
    }

    private static async Task SeedUserAsync(
        FifoDbContext context,
        IConfiguration configuration,
        string role,
        string prefix)
    {
        var email = configuration[$"SeedData:{prefix}Email"]?.Trim().ToLowerInvariant();
        var password = configuration[$"SeedData:{prefix}Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (password.Length < 8)
        {
            throw new InvalidOperationException($"La contraseña inicial de {prefix} debe tener al menos 8 caracteres.");
        }

        if (await context.Usuarios.AnyAsync(u => u.Email == email))
        {
            return;
        }

        await context.Usuarios.AddAsync(new Usuario
        {
            NombreCompleto = $"{role} de prueba",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Rol = role
        });

        await context.SaveChangesAsync();
    }
}
