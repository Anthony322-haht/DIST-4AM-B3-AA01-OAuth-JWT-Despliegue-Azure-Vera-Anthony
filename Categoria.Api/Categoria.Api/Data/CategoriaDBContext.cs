using Categoria.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Categoria.Api.Data
{
    public class CategoriaDBContext : DbContext
    {
        public CategoriaDBContext(DbContextOptions<CategoriaDBContext> options) : base(options)
        {
        }

        public DbSet<Models.Categoria> Categorias { get; set; }
    }
}
