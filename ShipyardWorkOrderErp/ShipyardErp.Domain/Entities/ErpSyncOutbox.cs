using ShipyardErp.Domain.Enums;

namespace ShipyardErp.Domain.Entities;

public class ErpSyncOutbox : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // "WorkOrder" veya "MaterialRequest"
    public Guid EntityId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public ErpSyncStatus Status { get; set; } = ErpSyncStatus.Pending;
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 5;
    public DateTime NextRetryAt { get; set; } = DateTime.UtcNow;
    public DateTime? SyncedAt { get; set; }
    public string? LastErrorMessage { get; set; }
}