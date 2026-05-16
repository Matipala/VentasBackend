using Microsoft.EntityFrameworkCore;
using VentasBackend.Infrastructure.Data;
using VentasBackend.Application.Interface;
using VentasBackend.Infrastructure.Configuration;
using VentasBackend.Application.Services;
using VentasBackend.Infrastructure.Middlewares;
using VentasBackend.Presentation.Hubs;
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddDbContext<VentasDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SalesOptions>(builder.Configuration.GetSection(SalesOptions.SectionName));
builder.Services.AddScoped<ICuentaTicketService, CuentaTicketService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IKdsService, KdsService>();

builder.Services.AddControllers();
builder.Services.AddSignalR();
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

app.UseMiddleware<ExceptionMiddleware>();

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
app.MapHub<KdsHub>("/api/ventas/hubs/kds");
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ventas" }));

app.Run();
