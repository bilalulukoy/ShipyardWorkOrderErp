using Microsoft.AspNetCore.Mvc;
using ShipyardErp.Application.Interfaces;
using ShipyardErp.Domain.Enums;

namespace ShipyardErp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErpIntegrationController : ControllerBase
{
    private readonly IShipyardWorkOrderService _service;

    public ErpIntegrationController(IShipyardWorkOrderService service)
    {
        _service = service;
    }

    [HttpGet("outbox")]
    public async Task<IActionResult> GetOutboxLogs([FromQuery] ErpSyncStatus? status, CancellationToken ct)
    {
        var logs = await _service.GetOutboxLogsAsync(status, ct);
        return Ok(logs);
    }

    [HttpPost("outbox/{outboxId:guid}/retry")]
    public async Task<IActionResult> RetryOutboxItem(Guid outboxId, CancellationToken ct)
    {
        var result = await _service.RetryFailedOutboxItemAsync(outboxId, ct);
        if (!result) return NotFound("Outbox kaydı bulunamadı.");
        return Ok(new { Message = "Kayıt yeniden senkronizasyon kuyruğuna alındı." });
    }
}