using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ShipyardErp.API.Middleware;
using ShipyardErp.Application.Interfaces;
using ShipyardErp.Application.Services;
using ShipyardErp.Infrastructure.BackgroundJobs;
using ShipyardErp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabanı Yapılandırması: PostgreSQL bağlantı cümlesi varsa PostgreSQL, yoksa lokal SQLite kullanır
var pgConnection = builder.Configuration.GetConnectionString("PostgreSql");
if (!string.IsNullOrWhiteSpace(pgConnection))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(pgConnection));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=shipyard_erp.db"));
}

builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IShipyardWorkOrderService, ShipyardWorkOrderService>();
builder.Services.AddHostedService<ErpSyncBackgroundWorker>();

var app = builder.Build();

// Uygulama başlarken veritabanı tablolarını otomatik oluştur
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shipyard Work Order & ERP Integration API v1");
    c.RoutePrefix = string.Empty; // Ana sayfada doğrudan Swagger açılsın
});

app.MapControllers();

app.Run();