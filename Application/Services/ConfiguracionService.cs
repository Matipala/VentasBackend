using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VentasBackend.Application.Interface;
using VentasBackend.Domain.Entities;
using VentasBackend.Infrastructure.Data;

namespace VentasBackend.Application.Services
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly VentasDbContext _context;

        public ConfiguracionService(VentasDbContext context)
        {
            _context = context;
        }

        public async Task<ConfiguracionVentas> ObtenerConfiguracionAsync(int empresaId)
        {
            var config = await _context.Configuracion
                .FirstOrDefaultAsync(c => c.IdEmpresa == empresaId);

            if (config == null)
            {
                // Retornar valores por defecto si no existe
                return new ConfiguracionVentas 
                { 
                    IdEmpresa = empresaId,
                    NombreImpuesto = "IVA", 
                    PorcentajeImpuesto = 0m 
                };
            }

            return config;
        }

        public async Task<ConfiguracionVentas> ActualizarConfiguracionAsync(int empresaId, ConfiguracionVentas request)
        {
            var config = await _context.Configuracion
                .FirstOrDefaultAsync(c => c.IdEmpresa == empresaId);

            if (config == null)
            {
                config = new ConfiguracionVentas
                {
                    IdEmpresa = empresaId,
                    NombreImpuesto = request.NombreImpuesto,
                    PorcentajeImpuesto = request.PorcentajeImpuesto
                };
                _context.Configuracion.Add(config);
            }
            else
            {
                config.NombreImpuesto = request.NombreImpuesto;
                config.PorcentajeImpuesto = request.PorcentajeImpuesto;
                _context.Configuracion.Update(config);
            }

            await _context.SaveChangesAsync();
            return config;
        }
    }
}
