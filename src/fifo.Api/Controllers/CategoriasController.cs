using Asp.Versioning;
using fifo.Api.DTOs;
using fifo.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fifo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categorias")]
[Authorize(Policy = "UserOrAdmin")]
public class CategoriasController(ICategoriaService categorias) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await categorias.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await categorias.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CategoriaResponse>> Create(CategoriaRequest request, CancellationToken cancellationToken)
    {
        var created = await categorias.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CategoriaResponse>> Update(int id, CategoriaRequest request, CancellationToken cancellationToken) =>
        Ok(await categorias.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await categorias.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
