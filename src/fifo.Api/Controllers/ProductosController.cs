using Asp.Versioning;
using fifo.Api.DTOs;
using fifo.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fifo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/productos")]
[Authorize(Policy = "UserOrAdmin")]
public class ProductosController(IProductoService productos) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductoResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await productos.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await productos.GetByIdAsync(id, cancellationToken));

    [HttpGet("{id:int}/stock")]
    [ProducesResponseType(typeof(StockProductoResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StockProductoResponse>> GetStock(int id, CancellationToken cancellationToken) =>
        Ok(await productos.GetStockAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoResponse>> Create(ProductoRequest request, CancellationToken cancellationToken)
    {
        var created = await productos.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoResponse>> Update(int id, ProductoRequest request, CancellationToken cancellationToken) =>
        Ok(await productos.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await productos.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
