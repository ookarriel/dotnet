using System.Net;
using System.Net.Http.Json;
using fifo.Api.DTOs;
using fifo.Api.Tests.Infrastructure;
using fifo.Api.Tests.Support;

namespace fifo.Api.Tests.Integration;

public class ApiIntegrationTests : ApiTestBase
{
    [Fact]
    public async Task Productos_SinToken_Retorna401()
    {
        var response = await Client.GetAsync("/api/v1/productos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_Administrador_RetornaJwtConRolAdmin()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = CustomWebApplicationFactory.AdminEmail,
            Password = CustomWebApplicationFactory.AdminPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await Json.ReadAsync<AuthResponse>(response);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        Assert.Equal("Admin", auth.Usuario.Rol);
        Assert.True(auth.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Productos_CrudCompleto_FuncionaConAdministrador()
    {
        await UseAdminAsync();
        var categoria = await CreateCategoriaAsync();
        var request = TestData.Producto(categoria.Id);
        var producto = await CreateProductoAsync(request);

        Assert.Equal(request.CodigoSku, producto.CodigoSku);
        Assert.Equal(categoria.Nombre, producto.CategoriaNombre);
        Assert.Equal(0, producto.StockTotal);

        var getResponse = await Client.GetAsync($"/api/v1/productos/{producto.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        request.Nombre = "Producto actualizado";
        request.PrecioUnitario = 99.90m;
        var updateResponse = await Client.PutAsJsonAsync($"/api/v1/productos/{producto.Id}", request);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await Json.ReadAsync<ProductoResponse>(updateResponse);
        Assert.Equal("Producto actualizado", updated.Nombre);
        Assert.Equal(99.90m, updated.PrecioUnitario);

        var listResponse = await Client.GetAsync("/api/v1/productos");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await Json.ReadAsync<List<ProductoResponse>>(listResponse);
        Assert.Contains(list, p => p.Id == producto.Id);

        var deleteResponse = await Client.DeleteAsync($"/api/v1/productos/{producto.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var notFoundResponse = await Client.GetAsync($"/api/v1/productos/{producto.Id}");
        Assert.Equal(HttpStatusCode.NotFound, notFoundResponse.StatusCode);
    }

    [Fact]
    public async Task Producto_DatosInvalidos_Retorna400()
    {
        await UseAdminAsync();
        var response = await Client.PostAsJsonAsync("/api/v1/productos", new ProductoRequest
        {
            Nombre = "x",
            CategoriaId = 1,
            PrecioUnitario = -1,
            CodigoSku = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UsuarioBasico_PuedeConsultarPeroNoCrearCategoria()
    {
        await UseNewUserAsync();

        var getResponse = await Client.GetAsync("/api/v1/productos");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var createResponse = await Client.PostAsJsonAsync("/api/v1/categorias", TestData.Categoria());
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task Salida_ConsumeLotesEnOrdenFifo()
    {
        await UseAdminAsync();
        var producto = await CreateProductoAsync();
        var antiguo = await CreateLoteAsync(
            producto.Id, 5, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var reciente = await CreateLoteAsync(
            producto.Id, 10, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        var response = await Client.PostAsJsonAsync("/api/v1/movimientos/salidas", new SalidaRequest
        {
            ProductoId = producto.Id,
            Cantidad = 7,
            PrecioUnitario = 20,
            Observaciones = "Salida FIFO de integración"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var movimientos = await Json.ReadAsync<List<MovimientoResponse>>(response);
        Assert.Equal(2, movimientos.Count);
        Assert.Equal(antiguo.Id, movimientos[0].LoteId);
        Assert.Equal(5, movimientos[0].Cantidad);
        Assert.Equal(reciente.Id, movimientos[1].LoteId);
        Assert.Equal(2, movimientos[1].Cantidad);

        var loteAntiguo = await Json.ReadAsync<LoteResponse>(
            await Client.GetAsync($"/api/v1/lotes/{antiguo.Id}"));
        var loteReciente = await Json.ReadAsync<LoteResponse>(
            await Client.GetAsync($"/api/v1/lotes/{reciente.Id}"));
        Assert.Equal(0, loteAntiguo.CantidadDisponible);
        Assert.Equal(8, loteReciente.CantidadDisponible);

        var stock = await Json.ReadAsync<StockProductoResponse>(
            await Client.GetAsync($"/api/v1/productos/{producto.Id}/stock"));
        Assert.Equal(8, stock.StockTotal);
    }

    [Fact]
    public async Task Salida_SinStockSuficiente_Retorna409()
    {
        await UseAdminAsync();
        var producto = await CreateProductoAsync();
        await CreateLoteAsync(producto.Id, 2, DateTime.UtcNow);

        var response = await Client.PostAsJsonAsync("/api/v1/movimientos/salidas", new SalidaRequest
        {
            ProductoId = producto.Id,
            Cantidad = 5
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Entrada_ActualizaStockDelLote()
    {
        await UseAdminAsync();
        var producto = await CreateProductoAsync();
        var lote = await CreateLoteAsync(producto.Id, 1, DateTime.UtcNow);

        var response = await Client.PostAsJsonAsync("/api/v1/movimientos/entradas", new EntradaRequest
        {
            LoteId = lote.Id,
            Cantidad = 4,
            PrecioUnitario = 12
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var movimiento = await Json.ReadAsync<MovimientoResponse>(response);
        Assert.Equal("Entrada", movimiento.Tipo);

        var loteActualizado = await Json.ReadAsync<LoteResponse>(
            await Client.GetAsync($"/api/v1/lotes/{lote.Id}"));
        Assert.Equal(5, loteActualizado.CantidadDisponible);
    }

    [Fact]
    public async Task Swagger_ExponeDocumentoOpenApiVersionado()
    {
        var response = await Client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadAsStringAsync();
        Assert.Contains("Inventario FIFO", document);
        Assert.Contains("/api/v1/productos", document);
    }
}
