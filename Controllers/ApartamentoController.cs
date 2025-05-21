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
    public class ApartamentoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApartamentoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ExibirTodosApartamentos")]
        public async Task<ActionResult<IEnumerable<Apartamento>>> GetTodosApartamentos()
        {
            return await _context.Apartamentos.ToListAsync();
        }

        [HttpGet("BuscarApartamentoPor")]   
        public async Task<ActionResult<IEnumerable<Apartamento>>> GetApartamentos([FromQuery] string? bloco, [FromQuery] int? numero, [FromQuery] string? proprietario)
        {
            var query = _context.Apartamentos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(bloco))
            {
                query = query.Where(ap => ap.Bloco.Contains(bloco));
            }

            if (numero.HasValue)
            {
                query = query.Where(ap => ap.Numero == numero.Value);
            }

            if (!string.IsNullOrWhiteSpace(proprietario))
            {
                query = query.Where(ap => ap.Proprietario.Contains(proprietario));
            }

            var apartamentos = await query.ToListAsync();

            if (apartamentos.Count == 0)
            {
                return NotFound(new { mensagem = "Nenhum apartamento encontrado." });
            }

            return Ok(apartamentos);
        }

        [HttpGet("BuscarApartamentoPorId/{id}")]
        public IActionResult BuscarPorId(int id)
        {
            var apartamento = _context.Apartamentos.Find(id);
            if (apartamento == null)
                return NotFound();

            return Ok(apartamento);
        }

        [HttpPost("CadastrarApartamento")] 
        public async Task<ActionResult<Apartamento>> PostApartamento(Apartamento novoApartamento)
        {

            try
            {
                if (novoApartamento == null)
                {
                    return BadRequest(new { mensagem = "Por favor, preencha todos os campos" });
                }

                var apartamentoFirst = await _context.Apartamentos.FirstOrDefaultAsync(ap => ap.Bloco == novoApartamento.Bloco &&
                ap.Numero == novoApartamento.Numero);

                if (apartamentoFirst != null)
                {
                    return BadRequest(new { mensagem = "Este apartamento já está cadastrado!" });
                }

                _context.Apartamentos.Add(novoApartamento);
                await _context.SaveChangesAsync();

                return Ok(new { mensagem = "Apartamento cadastrado com sucesso", novoApartamento });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao cadastrar apartamento", detalhes = ex.Message });
            }
        }

        [HttpPut("AtualizarApartamento/{id}")]
        public async Task<IActionResult> PutApartamento(int id, [FromBody] Apartamento apartamento)
        {
            if (id <= 0)
            {
                return BadRequest("Apartamento inválido.");
            }

            var apartamentoTemp = await _context.Apartamentos.FindAsync(id);

            if (apartamentoTemp == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(apartamento.Bloco) && apartamento.Bloco != "string" && apartamentoTemp.Bloco != apartamento.Bloco)
            {
                apartamentoTemp.Bloco = apartamento.Bloco;
                _context.Entry(apartamentoTemp).Property(a => a.Bloco).IsModified = true;
            }

            if (apartamento.Numero > 0 && apartamentoTemp.Numero != apartamento.Numero)
            {
                apartamentoTemp.Numero = apartamento.Numero;
                _context.Entry(apartamentoTemp).Property(a => a.Numero).IsModified = true;
            }

            if (!string.IsNullOrEmpty(apartamento.Proprietario) && apartamento.Proprietario != "string" && apartamentoTemp.Proprietario != apartamento.Proprietario)
            {
                apartamentoTemp.Proprietario = apartamento.Proprietario;
                _context.Entry(apartamentoTemp).Property(a => a.Proprietario).IsModified = true;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("ExcluirApartamento/{id}")]
        public async Task<IActionResult> DeletarApartamento(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Apartamento inválido.");
            }

            var apartamento = await _context.Apartamentos.FindAsync(id);
            if (apartamento == null)
            {
                return NotFound();
            }

            _context.Apartamentos.Remove(apartamento);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
