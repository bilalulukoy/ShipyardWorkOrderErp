using ShipyardErp.Domain.Enums;

namespace ShipyardErp.Domain.Entities;

public class WorkOrder : BaseEntity
{
    public string WorkOrderNumber { get; set; } = string.Empty; // Örn: WO-2026-001
    public string BlockOrZoneCode { get; set; } = string.Empty; // Örn: BL-104, Cargo Center Tank 2
    public string Department { get; set; } = string.Empty;      // Örn: Çelik/Boru/Boya/Elektrik
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedManHours { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Planned;
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }

    public Guid ShipProjectId { get; set; }
    public ShipProject ShipProject { get; set; } = null!;

    public ICollection<MaterialRequest> MaterialRequests { get; set; } = new List<MaterialRequest>();
}