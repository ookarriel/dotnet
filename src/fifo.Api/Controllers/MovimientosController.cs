using System.Security.Claims;
using Asp.Versioning;
using fifo.Api.DTOs;
using fifo.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fifo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/movimientos")]
[Authorize(Policy = "UserOrAdmin")]
public class MovimientosController(IInventarioService inventario) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MovimientoResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await inventario.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovimientoResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await inventario.GetByIdAsync(id, cancellationToken));

    [HttpPost("entradas")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<MovimientoResponse>> RegistrarEntrada(EntradaRequest request, CancellationToken cancellationToken)
    {
        var movimiento = await inventario.RegistrarEntradaAsync(request, GetUserEmail(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = movimiento.Id }, movimiento);
    }

    [HttpPost("salidas")]
    public async Task<ActionResult<IReadOnlyList<MovimientoResponse>>> RegistrarSalida(SalidaRequest request, CancellationToken cancellationToken)
    {
        var movimientos = await inventario.RegistrarSalidaFifoAsync(request, GetUserEmail(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, movimientos);
    }

    private string GetUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email")
        ?? "desconocido";
}
