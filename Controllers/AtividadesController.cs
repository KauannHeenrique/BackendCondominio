using condominio_API.Data;
using condominio_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace condominio_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AtividadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AtividadesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Recentes/{usuarioId}")]
        public IActionResult GetAtividadesRecentes(int usuarioId, int limite = 5)
        {
            var atividades = _context.AtividadesRecentes
                .Where(a => a.UsuarioId == usuarioId)
                .OrderByDescending(a => a.DataRegistro)
                .Take(limite)
                .ToList();

            return Ok(atividades);
        }




    }
}
