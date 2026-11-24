using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShipyardErp.Application.DTOs;
using ShipyardErp.Application.Interfaces;
using ShipyardErp.Domain.Entities;
using ShipyardErp.Domain.Enums;

namespace ShipyardErp.Application.Services;

public class ShipyardWorkOrderService : IShipyardWorkOrderService
{
    private readonly IAppDbContext _context;

    public ShipyardWorkOrderService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ShipProject> CreateProjectAsync(CreateShipProjectDto dto, CancellationToken ct = default)
    {
        var project = new ShipProject
        {
            ProjectCode = dto.ProjectCode,
            VesselName = dto.VesselName,
            ClientCompany = dto.ClientCompany,
            AssignedDock = dto.AssignedDock,
            DockEntryDate = DateTime.SpecifyKind(dto.DockEntryDate, DateTimeKind.Utc),
            PlannedUndockingDate = DateTime.SpecifyKind(dto.PlannedUndockingDate, DateTimeKind.Utc)
        };

        _context.ShipProjects.Add(project);
        await _context.SaveChangesAsync(ct);
        return project;
    }

    public async Task<List<ShipProject>> GetAllProjectsAsync(CancellationToken ct = default)
    {
        return await _context.ShipProjects
            .Include(p => p.WorkOrders)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<WorkOrderResponseDto> CreateWorkOrderAsync(CreateWorkOrderDto dto, CancellationToken ct = default)
    {
        var project = await _context.ShipProjects.FindAsync(new object[] { dto.ShipProjectId }, ct)
            ?? throw new KeyNotFoundException("Belirtilen gemi projesi bulunamadı.");

        var workOrder = new WorkOrder
        {
            ShipProjectId = dto.ShipProjectId,
            WorkOrderNumber = dto.WorkOrderNumber,
            BlockOrZoneCode = dto.BlockOrZoneCode,
            Department = dto.Department,
            Description = dto.Description,
            EstimatedManHours = dto.EstimatedManHours,
            PlannedStartDate = DateTime.SpecifyKind(dto.PlannedStartDate, DateTimeKind.Utc),
            PlannedEndDate = DateTime.SpecifyKind(dto.PlannedEndDate, DateTimeKind.Utc),
            Status = WorkOrderStatus.Planned
        };

        // Transactional Outbox: İş emri ile aynı transaction içinde ERP kuyruğuna kayıt eklenir
        var outboxEntry = new ErpSyncOutbox
        {
            EntityType = "WorkOrder",
            EntityId = workOrder.Id,
            PayloadJson = JsonSerializer.Serialize(new
            {
                project.ProjectCode,
                workOrder.WorkOrderNumber,
                workOrder.BlockOrZoneCode,
                workOrder.Department,
                workOrder.EstimatedManHours,
                Status = workOrder.Status.ToString()
            })
        };

        _context.WorkOrders.Add(workOrder);
        _context.ErpSyncOutboxes.Add(outboxEntry);
        await _context.SaveChangesAsync(ct);

        return new WorkOrderResponseDto(
            workOrder.Id,
            workOrder.WorkOrderNumber,
            project.ProjectCode,
            workOrder.BlockOrZoneCode,
            workOrder.Department,
            workOrder.Description,
            workOrder.EstimatedManHours,
            workOrder.Status.ToString(),
            0
        );
    }

    public async Task<List<WorkOrderResponseDto>> GetWorkOrdersByProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.WorkOrders
            .Include(w => w.ShipProject)
            .Include(w => w.MaterialRequests)
            .Where(w => w.ShipProjectId == projectId)
            .Select(w => new WorkOrderResponseDto(
                w.Id,
                w.WorkOrderNumber,
                w.ShipProject.ProjectCode,
                w.BlockOrZoneCode,
                w.Department,
                w.Description,
                w.EstimatedManHours,
                w.Status.ToString(),
                w.MaterialRequests.Count
            ))
            .ToListAsync(ct);
    }

    public async Task<WorkOrderResponseDto> UpdateWorkOrderStatusAsync(Guid workOrderId, WorkOrderStatus newStatus, CancellationToken ct = default)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.ShipProject)
            .Include(w => w.MaterialRequests)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, ct)
            ?? throw new KeyNotFoundException("İş emri bulunamadı.");

        workOrder.Status = newStatus;
        workOrder.UpdatedAt = DateTime.UtcNow;

        _context.ErpSyncOutboxes.Add(new ErpSyncOutbox
        {
            EntityType = "WorkOrderStatusUpdate",
            EntityId = workOrder.Id,
            PayloadJson = JsonSerializer.Serialize(new
            {
                workOrder.WorkOrderNumber,
                NewStatus = newStatus.ToString(),
                UpdatedAt = workOrder.UpdatedAt
            })
        });

        await _context.SaveChangesAsync(ct);

        return new WorkOrderResponseDto(
            workOrder.Id,
            workOrder.WorkOrderNumber,
            workOrder.ShipProject.ProjectCode,
            workOrder.BlockOrZoneCode,
            workOrder.Department,
            workOrder.Description,
            workOrder.EstimatedManHours,
            workOrder.Status.ToString(),
            workOrder.MaterialRequests.Count
        );
    }

    public async Task<MaterialRequest> AddMaterialRequestAsync(CreateMaterialRequestDto dto, CancellationToken ct = default)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.ShipProject)
            .FirstOrDefaultAsync(w => w.Id == dto.WorkOrderId, ct)
            ?? throw new KeyNotFoundException("İlgili iş emri bulunamadı.");

        var materialRequest = new MaterialRequest
        {
            WorkOrderId = dto.WorkOrderId,
            MaterialCode = dto.MaterialCode,
            MaterialName = dto.MaterialName,
            Quantity = dto.Quantity,
            Unit = dto.Unit
        };

        var outboxEntry = new ErpSyncOutbox
        {
            EntityType = "MaterialRequest",
            EntityId = materialRequest.Id,
            PayloadJson = JsonSerializer.Serialize(new
            {
                workOrder.ShipProject.ProjectCode,
                workOrder.WorkOrderNumber,
                materialRequest.MaterialCode,
                materialRequest.MaterialName,
                materialRequest.Quantity,
                materialRequest.Unit
            })
        };

        _context.MaterialRequests.Add(materialRequest);
        _context.ErpSyncOutboxes.Add(outboxEntry);
        await _context.SaveChangesAsync(ct);

        return materialRequest;
    }

    public async Task<List<ErpSyncOutbox>> GetOutboxLogsAsync(ErpSyncStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.ErpSyncOutboxes.AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        return await query.OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
    }

    public async Task<bool> RetryFailedOutboxItemAsync(Guid outboxId, CancellationToken ct = default)
    {
        var item = await _context.ErpSyncOutboxes.FindAsync(new object[] { outboxId }, ct);
        if (item == null) return false;

        item.Status = ErpSyncStatus.Pending;
        item.RetryCount = 0;
        item.NextRetryAt = DateTime.UtcNow;
        item.LastErrorMessage = null;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}