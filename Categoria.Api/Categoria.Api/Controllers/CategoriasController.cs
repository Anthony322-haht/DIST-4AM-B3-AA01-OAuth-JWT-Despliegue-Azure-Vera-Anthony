using Categoria.Api.Data;
using Categoria.Api.Models;
using Categoria.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Categoria.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly CategoriaDBContext _context;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public CategoriasController(CategoriaDBContext context, RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Categoria>>> GetCategorias()
        {
            return await _context.Categorias.AsNoTracking().ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Categoria>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.IdCategoria == id);
            if (categoria == null)
            {
                return NotFound();
            }
            return Ok(categoria);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<ActionResult<Models.Categoria>> CrearCategoria(Models.Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            // Publicamos a RabbitMQ
            await _rabbitMQPublisher.PublicarCategoriaCreadaAsync(categoria);

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.IdCategoria }, categoria);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCategoria(int id, Models.Categoria categoria)
        {
            if (categoria.IdCategoria != id || id == 0)
            {
                return BadRequest();
            }

            _context.Entry(categoria).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}