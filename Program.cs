using Microsoft.EntityFrameworkCore;
using VentasBackend.Infrastructure.Data;
using VentasBackend.Application.Interface;
using VentasBackend.Infrastructure.Configuration;
using VentasBackend.Application.Services;
using VentasBackend.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<VentasDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SalesOptions>(builder.Configuration.GetSection(SalesOptions.SectionName));
builder.Services.AddScoped<ICuentaTicketService, CuentaTicketService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IKdsService, KdsService>();

// Register InventarioStockGateway as IStockGateway using HttpClient factory
builder.Services.AddHttpClient<IStockGateway, InventarioStockGateway>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ventas" }));

app.Run();
