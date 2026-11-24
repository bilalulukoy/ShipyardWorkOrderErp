using ShipyardErp.Domain.Enums;

namespace ShipyardErp.Application.DTOs;

public record CreateShipProjectDto(
    string ProjectCode,
    string VesselName,
    string ClientCompany,
    DockLocation AssignedDock,
    DateTime DockEntryDate,
    DateTime PlannedUndockingDate
);

public record CreateWorkOrderDto(
    Guid ShipProjectId,
    string WorkOrderNumber,
    string BlockOrZoneCode,
    string Department,
    string Description,
    decimal EstimatedManHours,
    DateTime PlannedStartDate,
    DateTime PlannedEndDate
);

public record CreateMaterialRequestDto(
    Guid WorkOrderId,
    string MaterialCode,
    string MaterialName,
    decimal Quantity,
    string Unit
);

public record WorkOrderResponseDto(
    Guid Id,
    string WorkOrderNumber,
    string ProjectCode,
    string BlockOrZoneCode,
    string Department,
    string Description,
    decimal EstimatedManHours,
    string Status,
    int MaterialRequestCount
);