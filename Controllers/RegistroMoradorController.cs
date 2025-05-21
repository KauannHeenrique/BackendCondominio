using condominio_API.Request;
using condominio_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using condominio_API.Models;

namespace condominio_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcessoEntradaMoradorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AcessoEntradaMoradorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("RegistrarEntrada")]
        public async Task<ActionResult<AcessoEntradaMorador>> RegistrarEntrada([FromBody] EntradaMoradorRequest entradaMoradorReq)
        {
            try
            {
                if (entradaMoradorReq == null || string.IsNullOrEmpty(entradaMoradorReq.CodigoRFID))
                {
                    return BadRequest(new { mensagem = "O codigo RFID é obrigatório!" });
                }

                var usuario = await _context.Usuarios!
                    .Include(u => u.Apartamento) 
                    .FirstOrDefaultAsync(u => u.CodigoRFID == entradaMoradorReq.CodigoRFID);

                if (usuario == null)
                {
                    return BadRequest(new { mensagem = "TAG não cadastrada!" });
                }

                var novaEntrada = new AcessoEntradaMorador
                {
                    UsuarioId = usuario.UsuarioId,
                    DataHoraEntrada = DateTime.Now
                };

                _context.AcessoEntradaMoradores!.Add(novaEntrada);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensagem = "Entrada registrada com sucesso!",
                    entrada = new
                    {
                        novaEntrada.Id,
                        novaEntrada.UsuarioId,
                        UsuarioNome = usuario.Nome,
                        usuario.ApartamentoId,
                        ApartamentoNumero = usuario.Apartamento?.Numero, 
                        bloco = usuario.Apartamento?.Bloco,
                        novaEntrada.DataHoraEntrada
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao registrar entrada!", detalhes = ex.Message });
            }
        }

        [HttpGet("ListarEntradas")]
        public async Task<ActionResult> ListarEntradas()
        {
            try
            {
                var entradas = await _context.AcessoEntradaMoradores!
                    .Include(e => e.Usuario)
                        .ThenInclude(u => u.Apartamento) 
                    .OrderByDescending(e => e.DataHoraEntrada)
                    .Select(e => new
                    {
                        e.Id,
                        e.UsuarioId,
                        Nome = e.Usuario!.Nome,
                        e.Usuario!.ApartamentoId,
                        Apartamento = e.Usuario!.Apartamento!.Numero,
                        bloco = e.Usuario!.Apartamento!.Bloco,
                        e.DataHoraEntrada
                    })
                    .ToListAsync();

                return Ok(entradas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao listar acessos!", detalhes = ex.Message });
            }
        }

        [HttpGet("FiltrarEntradasAdmin")]
        public async Task<ActionResult> FiltrarEntradasAdmin ([FromQuery] string? documento = null, [FromQuery] int? apartamentoId = null,
            [FromQuery] string? bloco = null, [FromQuery] DateTime? dataInicio = null, [FromQuery] DateTime? dataFim = null)
        {
            try
            {
                if (string.IsNullOrEmpty(documento) && !apartamentoId.HasValue && string.IsNullOrEmpty(bloco) && !dataInicio.HasValue && !dataFim.HasValue)
                {
                    return BadRequest(new { mensagem = "Informe pelo menos um filtro!" });
                }

                var query = _context.AcessoEntradaMoradores!
                    .Include(e => e.Usuario)
                        .ThenInclude(u => u.Apartamento)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(documento))
                {
                    query = query.Where(e => e.Usuario!.Documento == documento);
                }

                if (apartamentoId.HasValue)
                {
                    query = query.Where(e => e.Usuario!.ApartamentoId == apartamentoId.Value); 
                }

                if (dataInicio.HasValue)
                {
                    query = query.Where(e => e.DataHoraEntrada >= dataInicio.Value);
                }

                if (dataFim.HasValue)
                {
                    query = query.Where(e => e.DataHoraEntrada <= dataFim.Value);
                }

                var entradas = await query
                    .OrderByDescending(e => e.DataHoraEntrada)
                    .Select(e => new
                    {
                        e.Id,
                        e.UsuarioId,
                        Nome = e.Usuario!.Nome,
                        e.Usuario!.ApartamentoId,
                        Apartamento = e.Usuario!.Apartamento!.Numero,
                        bloco = e.Usuario!.Apartamento!.Bloco,
                        e.DataHoraEntrada
                    })
                    .ToListAsync();

                if (entradas.Count == 0)
                {
                    return NotFound(new { mensagem = "Nenhuma entrada encontrada!" });
                }

                return Ok(entradas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao filtrar entradas!", detalhes = ex.Message });
            }
        }

        [HttpGet("FiltrarEntradasUsuario")]
        public async Task<ActionResult> FiltrarEntradasUsuario([FromQuery] int usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                {
                    return BadRequest(new { mensagem = "O usuário não é valido!" });
                }

                var usuarioLogado = await _context.Usuarios!.FirstOrDefaultAsync(user => user.UsuarioId == usuarioId);

                if (usuarioLogado == null)
                {
                    return NotFound(new { mensagem = "Usuário não encontrado!" });
                }

                var apartamentoId = usuarioLogado.ApartamentoId;

                var entradas = await _context.AcessoEntradaMoradores!
                    .Include(e => e.Usuario)
                        .ThenInclude(user => user.Apartamento)
                    .Where(e => e.Usuario!.ApartamentoId == apartamentoId)
                    .OrderByDescending(e => e.DataHoraEntrada)
                    .Select(e => new
                    {
                        id = e.Id,
                        idUsuario = e.UsuarioId,
                        nome = e.Usuario!.Nome,
                        idApartamento = e.Usuario!.ApartamentoId,
                        apartamento = e.Usuario!.Apartamento!.Numero,
                        bloco = e.Usuario!.Apartamento!.Bloco,
                        dataEntrada = e.DataHoraEntrada
                    })
                    .ToListAsync();

                if (entradas.Count == 0)
                {
                    return NotFound(new { mensagem = "Nenhuma entrada encontrada para o apartamento deste usuário!" });
                }

                return Ok(entradas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao listar entradas do apartamento!", detalhes = ex.Message });
            }
        }
    }
}