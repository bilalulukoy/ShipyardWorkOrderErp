using ShipyardErp.Application.DTOs;
using ShipyardErp.Domain.Entities;
using ShipyardErp.Domain.Enums;

namespace ShipyardErp.Application.Interfaces;

public interface IShipyardWorkOrderService
{
    Task<ShipProject> CreateProjectAsync(CreateShipProjectDto dto, CancellationToken ct = default);
    Task<List<ShipProject>> GetAllProjectsAsync(CancellationToken ct = default);
    Task<WorkOrderResponseDto> CreateWorkOrderAsync(CreateWorkOrderDto dto, CancellationToken ct = default);
    Task<List<WorkOrderResponseDto>> GetWorkOrdersByProjectAsync(Guid projectId, CancellationToken ct = default);
    Task<WorkOrderResponseDto> UpdateWorkOrderStatusAsync(Guid workOrderId, WorkOrderStatus newStatus, CancellationToken ct = default);
    Task<MaterialRequest> AddMaterialRequestAsync(CreateMaterialRequestDto dto, CancellationToken ct = default);
    Task<List<ErpSyncOutbox>> GetOutboxLogsAsync(ErpSyncStatus? status = null, CancellationToken ct = default);
    Task<bool> RetryFailedOutboxItemAsync(Guid outboxId, CancellationToken ct = default);
}