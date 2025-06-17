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
    public class VisitanteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VisitanteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ExibirTodosVisitantes")]
        public async Task<ActionResult<IEnumerable<Visitante>>> GetTodosVisitantes()
        {
            return await _context.Visitantes.ToListAsync();
        }

        [HttpGet("BuscarVisitantePor")]
        public async Task<ActionResult<IEnumerable<Visitante>>> GetVisitante([FromQuery] string? nomeVisitante, [FromQuery] string? documento)
        {
            var query = _context.Visitantes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nomeVisitante))
            {
                query = query.Where(visit => visit.Nome.Contains(nomeVisitante));
            }

            if (!string.IsNullOrWhiteSpace(documento))
            {
                query = query.Where(visit => visit.Documento.Contains(documento));
            }

            var visitantes = await query.ToListAsync();

            if (visitantes.Count == 0)
            {
                return NotFound(new { mensagem = "Nenhum visitante encontrado." });
            }

            return Ok(visitantes);
        }

        [HttpPost("CadastrarVisitante")]
        public async Task<ActionResult<Visitante>> PostVisitante(Visitante NovoVisitante)
        {
            try
            {
                if (string.IsNullOrEmpty(NovoVisitante.Nome) || NovoVisitante.Nome == "string" ||
                    string.IsNullOrEmpty(NovoVisitante.Documento) || NovoVisitante.Documento == "string" ||
                    string.IsNullOrEmpty(NovoVisitante.Telefone) || NovoVisitante.Telefone == "string")
                {
                    return BadRequest(new { mensagem = "Nome, Documento e Telefone são obrigatórios!" });
                }

                var visitanteExistente = await _context.Visitantes.FirstOrDefaultAsync(visit => visit.Documento == NovoVisitante.Documento && visit.Cnpj == NovoVisitante.Cnpj);

                if (visitanteExistente != null)
                {
                    return Ok(new { mensagem = "Visitante já cadastrado. Deseja gerar um QR code para esta visita?", visitante = visitanteExistente, gerarQrCode = true });
                }

                _context.Visitantes.Add(NovoVisitante);
                await _context.SaveChangesAsync();

                return Ok(new { mensagem = "Visitante cadastrado com sucesso", NovoVisitante });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao cadastrar visitante!", detalhes = ex.Message });
            }
        }

        [HttpPut("AtualizarVisitante/{id}")]
        public async Task<IActionResult> PutVisitante(int id, [FromBody] Visitante visitante)
        {
            if (id <= 0)
            {
                return BadRequest("Visitante inválido.");
            }

            var visitanteTemp = await _context.Visitantes.FindAsync(id);

            if (visitanteTemp == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(visitante.Nome) && visitante.Nome != "string" && visitanteTemp.Nome != visitante.Nome)
            {
                visitanteTemp.Nome = visitante.Nome;
                _context.Entry(visitanteTemp).Property(v => v.Nome).IsModified = true;
            }

            if (!string.IsNullOrEmpty(visitante.Documento) && visitante.Documento != "string" && visitanteTemp.Documento != visitante.Documento)
            {
                visitanteTemp.Documento = visitante.Documento;
                _context.Entry(visitanteTemp).Property(v => v.Documento).IsModified = true;
            }

            if (!string.IsNullOrEmpty(visitante.Telefone) && visitante.Telefone != "string" && visitanteTemp.Telefone != visitante.Telefone)
            {
                visitanteTemp.Telefone = visitante.Telefone;
                _context.Entry(visitanteTemp).Property(v => v.Telefone).IsModified = true;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("ExcluirVisitante/{id}")]
        public async Task<IActionResult> DeletarVisitante(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Visitante inválido.");
            }

            var visitante = await _context.Visitantes.FindAsync(id);
            if (visitante == null)
            {
                return NotFound();
            }

            _context.Visitantes.Remove(visitante);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}