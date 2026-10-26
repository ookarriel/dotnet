using Asp.Versioning;
using fifo.Api.DTOs;
using fifo.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fifo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lotes")]
[Authorize(Policy = "UserOrAdmin")]
public class LotesController(ILoteService lotes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LoteResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await lotes.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoteResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await lotes.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<LoteResponse>> Create(LoteRequest request, CancellationToken cancellationToken)
    {
        var created = await lotes.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<LoteResponse>> Update(int id, LoteUpdateRequest request, CancellationToken cancellationToken) =>
        Ok(await lotes.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await lotes.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
