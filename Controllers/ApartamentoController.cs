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

                // Verificar se a situação do apartamento é válida
                if (!Enum.IsDefined(typeof(SituacaoApartamento), novoApartamento.Situacao))
                {
                    return BadRequest(new { mensagem = "Situação do apartamento inválida." });
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
                return BadRequest(new { mensagem = "Apartamento inválido." });
            }

            var apartamentoTemp = await _context.Apartamentos.FindAsync(id);

            if (apartamentoTemp == null)
            {
                return NotFound(new { mensagem = "Apartamento não encontrado." });
            }

            // Verifica duplicidade: mesmo bloco e número, mas com ID diferente
            var duplicado = await _context.Apartamentos
                .AnyAsync(a => a.Id != id && a.Bloco == apartamento.Bloco && a.Numero == apartamento.Numero);

            if (duplicado)
            {
                return BadRequest(new { mensagem = "Já existe um apartamento com esse bloco e número." });
            }

            // Atualiza os campos
            apartamentoTemp.Bloco = apartamento.Bloco;
            apartamentoTemp.Numero = apartamento.Numero;
            apartamentoTemp.Proprietario = apartamento.Proprietario;
            apartamentoTemp.Situacao = apartamento.Situacao;
            apartamentoTemp.Observacoes = apartamento.Observacoes;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Apartamento atualizado com sucesso!" });
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
