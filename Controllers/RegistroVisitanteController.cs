    using Condominio_API.Requests;
using condominio_API.Data;
using condominio_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace condominio_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcessoEntradaVisitanteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AcessoEntradaVisitanteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("RegistrarEntradaVisitante")]
        public async Task<ActionResult<AcessoEntradaVisitante>> RegistrarEntradaVisitante([FromBody] EntradaVisitanteRequest entradaVisitanteReq)
        {
            try
            {
                if (entradaVisitanteReq == null || string.IsNullOrEmpty(entradaVisitanteReq.QrCodeData))
                {
                    return BadRequest(new { mensagem = "O QR code é obrigatório!" });
                }

                // Busca o QR code ativo em QRCodesTemp
                var qrCode = await _context.QRCodesTemp!
                    .Include(q => q.Visitante)
                    .Include(q => q.Morador)
                        .ThenInclude(m => m.Apartamento)
                    .FirstOrDefaultAsync(q => q.QrCodeData == entradaVisitanteReq.QrCodeData
                                           && q.Status
                                           && q.DataValidade > DateTime.Now);

                if (qrCode == null)
                {
                    return BadRequest(new { mensagem = "QR code inválido, expirado ou não encontrado!" });
                }

                var novaEntrada = new AcessoEntradaVisitante
                {
                    VisitanteId = qrCode.VisitanteId,
                    UsuarioId = qrCode.MoradorId,
                    DataHoraEntrada = DateTime.Now
                };

                if (!qrCode.TipoQRCode) // altero o status do qrcode no banco para nao ser usado novamente 
                {
                    qrCode.Status = false;
                }

                _context.AcessoEntradaVisitantes!.Add(novaEntrada);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensagem = "Entrada registrada com sucesso!",
                    entrada = new
                    {
                        id = novaEntrada.Id,
                        idVisitante = novaEntrada.VisitanteId,
                        nomeVisitante = qrCode.Visitante!.Nome,
                        idMorador = novaEntrada.UsuarioId,
                        nomeMorador = qrCode.Morador!.Nome,
                        idApartamento = qrCode.Morador!.ApartamentoId,
                        apartamento = qrCode.Morador!.Apartamento!.Numero,
                        bloco = qrCode.Morador!.Apartamento!.Bloco,
                        dataEntrada = novaEntrada.DataHoraEntrada
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao registrar entrada!", detalhes = ex.Message });
            }
        }

        [HttpGet("ListarEntradasVisitantes")]
        public async Task<ActionResult> ListarEntradasVisitantes()
        {
            try
            {
                var entradas = await _context.AcessoEntradaVisitantes!
                    .Include(e => e.Visitante)
                    .Include(e => e.Usuario)
                        .ThenInclude(u => u.Apartamento)
                    .OrderByDescending(e => e.DataHoraEntrada)
                    .Select(e => new
                    {
                        id = e.Id,
                        idVisitante = e.VisitanteId,
                        nomeVisitante = e.Visitante!.Nome,
                        documentoVisitante = e.Visitante!.Documento,
                        idMorador = e.UsuarioId,
                        nomeMorador = e.Usuario!.Nome,
                        idApartamento = e.Usuario!.ApartamentoId,
                        apartamento = e.Usuario!.Apartamento!.Numero,
                        bloco = e.Usuario!.Apartamento!.Bloco,
                        dataEntrada = e.DataHoraEntrada
                    })
                    .ToListAsync();

                return Ok(entradas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao listar entradas!", detalhes = ex.Message });
            }
        }

        [HttpGet("FiltrarEntradasVisitantesAdmin")]
        public async Task<ActionResult> FiltrarEntradasVisitantesAdmin(
    [FromQuery] string? documento = null,
    [FromQuery] int? numero = null,
    [FromQuery] string? bloco = null,
    [FromQuery] DateTime? dataInicio = null,
    [FromQuery] DateTime? dataFim = null)
        {
            try
            {
                if (string.IsNullOrEmpty(documento) && !numero.HasValue && string.IsNullOrEmpty(bloco) && !dataInicio.HasValue && !dataFim.HasValue)
                {
                    return BadRequest(new { mensagem = "Informe pelo menos um filtro!" });
                }

                var query = _context.AcessoEntradaVisitantes!
                    .Include(e => e.Visitante)
                    .Include(e => e.Usuario)
                        .ThenInclude(u => u.Apartamento)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(documento))
                {
                    query = query.Where(e => e.Visitante!.Documento == documento);
                }

                if (!string.IsNullOrEmpty(bloco))
                {
                    query = query.Where(e => e.Usuario!.Apartamento!.Bloco == bloco);
                }

                if (numero.HasValue)
                {
                    query = query.Where(e => e.Usuario!.Apartamento!.Numero == numero.Value);
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
                        id = e.Id,
                        idVisitante = e.VisitanteId,
                        nomeVisitante = e.Visitante!.Nome,
                        documentoVisitante = e.Visitante!.Documento,
                        idMorador = e.UsuarioId,
                        nomeMorador = e.Usuario!.Nome,
                        idApartamento = e.Usuario!.ApartamentoId,
                        apartamento = e.Usuario!.Apartamento!.Numero,
                        bloco = e.Usuario!.Apartamento!.Bloco,
                        dataEntrada = e.DataHoraEntrada
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


        [HttpGet("ListarEntradasPorApartamentoDoUsuario")]
        public async Task<ActionResult> ListarEntradasPorApartamentoDoUsuario([FromQuery] int usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                {
                    return BadRequest(new { mensagem = "O ID do usuário é obrigatório e deve ser válido!" });
                }

                var usuarioLogado = await _context.Usuarios!
                    .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

                if (usuarioLogado == null)
                {
                    return NotFound(new { mensagem = "Usuário não encontrado!" });
                }

                var apartamentoId = usuarioLogado.ApartamentoId;

                var entradas = await _context.AcessoEntradaVisitantes!
                    .Include(e => e.Visitante)
                    .Include(e => e.Usuario)
                        .ThenInclude(u => u.Apartamento)
                    .Where(e => e.Usuario!.ApartamentoId == apartamentoId)
                    .OrderByDescending(e => e.DataHoraEntrada)
                    .Select(e => new
                    {
                        id = e.Id,
                        idVisitante = e.VisitanteId,
                        nomeVisitante = e.Visitante!.Nome,
                        documentoVisitante = e.Visitante!.Documento,
                        idMorador = e.UsuarioId,
                        nomeMorador = e.Usuario!.Nome,
                        idApartamento = e.Usuario!.ApartamentoId,
                        apartamento = e.Usuario!.Apartamento!.Numero,
                        bloco = e.Usuario!.Apartamento!.Bloco,
                        dataEntrada = e.DataHoraEntrada
                    })
                    .ToListAsync();

                if (entradas.Count == 0)
                {
                    return NotFound(new { mensagem = "Nenhuma entrada de visitante encontrada para o apartamento deste usuário!" });
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