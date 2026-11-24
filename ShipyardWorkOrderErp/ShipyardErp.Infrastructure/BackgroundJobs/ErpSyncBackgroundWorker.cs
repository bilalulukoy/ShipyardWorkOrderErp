using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShipyardErp.Domain.Enums;
using ShipyardErp.Infrastructure.Persistence;

namespace ShipyardErp.Infrastructure.BackgroundJobs;

public class ErpSyncBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ErpSyncBackgroundWorker> _logger;

    public ErpSyncBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<ErpSyncBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ERP Senkronizasyon & Retry Worker başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingOutboxItemsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERP Outbox işlenirken beklenmeyen bir hata oluştu.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessPendingOutboxItemsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var pendingItems = await dbContext.ErpSyncOutboxes
            .Where(x => x.Status == ErpSyncStatus.Pending && x.NextRetryAt <= now)
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (!pendingItems.Any()) return;

        foreach (var item in pendingItems)
        {
            try
            {
                // Gerçek ERP API entegrasyon noktası (Simülasyon: %85 başarı, %15 geçici ağ/ERP hatası)
                bool isErpAvailable = Random.Shared.Next(1, 101) <= 85;
                if (!isErpAvailable)
                {
                    throw new HttpRequestException("ERP Sunucusu geçici olarak yanıt vermiyor (503 Service Unavailable).");
                }

                item.Status = ErpSyncStatus.Synced;
                item.SyncedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
                item.LastErrorMessage = null;

                // Eğer kayıt bir MaterialRequest ise ilgili kaydı da ERP'ye aktarıldı olarak işaretle
                if (item.EntityType == "MaterialRequest")
                {
                    var matReq = await dbContext.MaterialRequests.FindAsync(new object[] { item.EntityId }, ct);
                    if (matReq != null) matReq.IsSyncedToErp = true;
                }

                _logger.LogInformation("[ERP SYNC BAŞARILI] Tip: {Type} | EntityId: {Id}", item.EntityType, item.EntityId);
            }
            catch (Exception ex)
            {
                item.RetryCount++;
                item.UpdatedAt = DateTime.UtcNow;
                item.LastErrorMessage = ex.Message;

                if (item.RetryCount >= item.MaxRetries)
                {
                    item.Status = ErpSyncStatus.Failed;
                    _logger.LogWarning("[ERP SYNC İPTAL - MAX RETRY] OutboxId: {Id} | Hata: {Err}", item.Id, ex.Message);
                }
                else
                {
                    // Exponential Backoff: 15sn, 30sn, 60sn...
                    int delaySeconds = 15 * (int)Math.Pow(2, item.RetryCount - 1);
                    item.NextRetryAt = DateTime.UtcNow.AddSeconds(delaySeconds);
                    _logger.LogWarning("[ERP SYNC RETRY PLANLANDI] OutboxId: {Id} | Deneme: {Retry}/{Max} | Sonraki: {Next}",
                        item.Id, item.RetryCount, item.MaxRetries, item.NextRetryAt);
                }
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }
}