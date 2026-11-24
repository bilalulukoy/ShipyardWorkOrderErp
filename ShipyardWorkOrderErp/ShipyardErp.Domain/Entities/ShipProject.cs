using ShipyardErp.Domain.Enums;

namespace ShipyardErp.Domain.Entities;

public class ShipProject : BaseEntity
{
    public string ProjectCode { get; set; } = string.Empty; // Örn: NB-105 veya RPR-2026-04
    public string VesselName { get; set; } = string.Empty;  // Gemi Adı
    public string ClientCompany { get; set; } = string.Empty;
    public DockLocation AssignedDock { get; set; }
    public DateTime DockEntryDate { get; set; }
    public DateTime PlannedUndockingDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}