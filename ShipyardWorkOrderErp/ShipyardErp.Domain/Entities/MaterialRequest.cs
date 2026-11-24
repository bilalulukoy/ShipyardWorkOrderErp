namespace ShipyardErp.Domain.Entities;

public class MaterialRequest : BaseEntity
{
    public string MaterialCode { get; set; } = string.Empty; // ERP Stok Kodu (Örn: STL-PL-12MM)
    public string MaterialName { get; set; } = string.Empty; // Örn: Gemi İnşa Sacı 12mm Grade A
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "Adet";               // Adet, Kg, Metre, Litre
    public bool IsSyncedToErp { get; set; } = false;

    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;
}