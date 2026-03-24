using Microsoft.EntityFrameworkCore;
using VentasBackend.Data;
using VentasBackend.Application.Interface;
using VentasBackend.Infrastructure.Configuration;
using VentasBackend.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<VentasDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SalesOptions>(builder.Configuration.GetSection(SalesOptions.SectionName));
builder.Services.AddScoped<ICuentaTicketService, CuentaTicketService>();

// Register InventarioStockGateway as IStockGateway using HttpClient factory
builder.Services.AddHttpClient<IStockGateway, InventarioStockGateway>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ventas" }));

app.Run();
