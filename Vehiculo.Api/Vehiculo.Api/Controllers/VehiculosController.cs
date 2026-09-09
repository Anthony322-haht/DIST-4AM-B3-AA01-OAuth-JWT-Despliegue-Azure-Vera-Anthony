using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehiculo.Api.Data;
using Vehiculo.Api.Models;

namespace Vehiculo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiculosController : ControllerBase
    {
        private readonly VehiculoDBContext _context;

        public VehiculosController(VehiculoDBContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Vehiculo>>> GetVehiculos()
        {
            return await _context.Vehiculos.AsNoTracking().ToListAsync();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Vehiculo>> GetVehiculo(int id)
        {
            var vehiculo = await _context.Vehiculos.AsNoTracking().FirstOrDefaultAsync(v => v.IdVehiculo == id);
            if (vehiculo == null)
            {
                return NotFound();
            }
            return Ok(vehiculo);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<ActionResult<Models.Vehiculo>> CrearVehiculo(Models.Vehiculo vehiculo)
        {
            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.IdVehiculo }, vehiculo);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarVehiculo(int id, Models.Vehiculo vehiculo)
        {
            if (vehiculo.IdVehiculo != id || id == 0)
            {
                return BadRequest();
            }
            _context.Entry(vehiculo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarVehiculo(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }
            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
