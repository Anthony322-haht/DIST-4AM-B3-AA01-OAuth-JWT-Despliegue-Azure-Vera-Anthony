using Vehiculo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Vehiculo.Api.Data
{
    public class VehiculoDBContext : DbContext
    {
        public VehiculoDBContext(DbContextOptions<VehiculoDBContext> options) : base(options)
        {
        }
        public DbSet<Models.Vehiculo> Vehiculos { get; set; }
    }
}
