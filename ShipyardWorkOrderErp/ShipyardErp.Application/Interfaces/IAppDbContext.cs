using Microsoft.EntityFrameworkCore;
using ShipyardErp.Domain.Entities;

namespace ShipyardErp.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<ShipProject> ShipProjects { get; }
    DbSet<WorkOrder> WorkOrders { get; }
    DbSet<MaterialRequest> MaterialRequests { get; }
    DbSet<ErpSyncOutbox> ErpSyncOutboxes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}