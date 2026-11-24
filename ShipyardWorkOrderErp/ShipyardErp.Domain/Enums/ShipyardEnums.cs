namespace ShipyardErp.Domain.Enums;

public enum DockLocation
{
    DryDock1 = 1,       // Kuru Havuz 1
    DryDock2 = 2,       // Kuru Havuz 2
    FloatingDock = 3,   // Yüzer Havuz
    PanelShop = 4,      // Pano Sahası
    OutfittingQuay = 5  // Donatım Rıhtımı
}

public enum WorkOrderStatus
{
    Planned = 1,
    InProgress = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}

public enum ErpSyncStatus
{
    Pending = 1,
    Processing = 2,
    Synced = 3,
    Failed = 4
}