using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using fifo.Api.DTOs;
using fifo.Api.Tests.Infrastructure;

namespace fifo.Api.Tests.Support;

public abstract class ApiTestBase : IDisposable
{
    protected CustomWebApplicationFactory Factory { get; } = new();
    protected HttpClient Client { get; }

    protected ApiTestBase()
    {
        Client = Factory.CreateClient();
    }

    protected async Task<string> UseAdminAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = CustomWebApplicationFactory.AdminEmail,
            Password = CustomWebApplicationFactory.AdminPassword
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await Json.ReadAsync<AuthResponse>(response);
        UseToken(auth.Token);
        return auth.Token;
    }

    protected async Task<AuthResponse> UseNewUserAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", new RegisterUsuarioRequest
        {
            NombreCompleto = "Usuario de integración",
            Email = $"user-{Guid.NewGuid():N}@integration.local",
            Password = "User-Integration-123!"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await Json.ReadAsync<AuthResponse>(response);
        UseToken(auth.Token);
        return auth;
    }

    protected async Task<CategoriaResponse> CreateCategoriaAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/categorias", TestData.Categoria());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Json.ReadAsync<CategoriaResponse>(response);
    }

    protected async Task<ProductoResponse> CreateProductoAsync(ProductoRequest? request = null)
    {
        request ??= TestData.Producto((await CreateCategoriaAsync()).Id);
        var response = await Client.PostAsJsonAsync("/api/v1/productos", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Json.ReadAsync<ProductoResponse>(response);
    }

    protected async Task<LoteResponse> CreateLoteAsync(
        int productoId,
        int cantidad,
        DateTime fechaIngreso)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/lotes", new LoteRequest
        {
            ProductoId = productoId,
            CodigoLote = TestData.Code("LOTE"),
            Cantidad = cantidad,
            PrecioCompra = 10,
            FechaIngreso = fechaIngreso
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Json.ReadAsync<LoteResponse>(response);
    }

    private void UseToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
