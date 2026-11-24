using Microsoft.AspNetCore.Mvc;
using ShipyardErp.Application.DTOs;
using ShipyardErp.Application.Interfaces;
using ShipyardErp.Domain.Enums;

namespace ShipyardErp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly IShipyardWorkOrderService _service;

    public WorkOrdersController(IShipyardWorkOrderService service)
    {
        _service = service;
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetByProject(Guid projectId, CancellationToken ct)
    {
        var list = await _service.GetWorkOrdersByProjectAsync(projectId, ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderDto dto, CancellationToken ct)
    {
        var created = await _service.CreateWorkOrderAsync(dto, ct);
        return Ok(created);
    }

    [HttpPatch("{workOrderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid workOrderId, [FromQuery] WorkOrderStatus status, CancellationToken ct)
    {
        var updated = await _service.UpdateWorkOrderStatusAsync(workOrderId, status, ct);
        return Ok(updated);
    }

    [HttpPost("material-requests")]
    public async Task<IActionResult> AddMaterialRequest([FromBody] CreateMaterialRequestDto dto, CancellationToken ct)
    {
        var request = await _service.AddMaterialRequestAsync(dto, ct);
        return Ok(request);
    }
}