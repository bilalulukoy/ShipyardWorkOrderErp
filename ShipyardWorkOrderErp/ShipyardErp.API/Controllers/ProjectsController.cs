using Microsoft.AspNetCore.Mvc;
using ShipyardErp.Application.DTOs;
using ShipyardErp.Application.Interfaces;

namespace ShipyardErp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IShipyardWorkOrderService _service;

    public ProjectsController(IShipyardWorkOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var projects = await _service.GetAllProjectsAsync(ct);
        return Ok(projects);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShipProjectDto dto, CancellationToken ct)
    {
        var created = await _service.CreateProjectAsync(dto, ct);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}