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
                    return BadRequest(new { mensagem = "O código RFID é obrigatório!" });
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
                    DataHoraEntrada = DateTime.UtcNow,
                    EntradaPor = entradaMoradorReq.EntradaPor ?? "1"
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
                        nome = usuario.Nome,
                        documento = usuario.Documento,
                        nivelAcesso = usuario.NivelAcesso.ToString(),
                        apartamentoId = usuario.ApartamentoId,
                        apartamento = usuario.Apartamento?.Numero,
                        bloco = usuario.Apartamento?.Bloco,
                        dataHoraEntrada = novaEntrada.DataHoraEntrada
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao registrar entrada!", detalhes = ex.Message });
            }
        }

        [HttpGet("ListarEntradasMorador")]
        public async Task<ActionResult> ListarEntradas()
        {
            try
            {
                var entradas = await _context.AcessoEntradaMoradores!
                    .Include(e => e.Usuario)
                        .ThenInclude(u => u.Apartamento)
                    .OrderByDescending(e => e.DataHoraEntrada)
                    .ToListAsync(); 

                var resultado = entradas.Select(e => new
                {
                    e.Id,
                    e.UsuarioId,
                    nome = e.Usuario?.Nome,
                    documento = e.Usuario?.Documento,
                    nivelAcesso = e.Usuario?.NivelAcesso.ToString(),
                    apartamentoId = e.Usuario?.ApartamentoId,
                    apartamento = e.Usuario?.Apartamento?.Numero,
                    bloco = e.Usuario?.Apartamento?.Bloco,
                    dataHoraEntrada = e.DataHoraEntrada,
                    entradaPor = e.EntradaPor
                });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao listar acessos!", detalhes = ex.Message });
            }
        }


        [HttpGet("FiltrarEntradasAdmin")]
        public async Task<ActionResult> FiltrarEntradasAdmin(
    [FromQuery] string? nome = null,
    [FromQuery] string? documento = null,
    [FromQuery] int? numero = null,
    [FromQuery] string? bloco = null,
    [FromQuery] string? nivelAcesso = null,
    [FromQuery] DateTime? dataInicio = null,
    [FromQuery] DateTime? dataFim = null,
    [FromQuery] int? apartamentoId = null) // ✅ Novo filtro
        {
            try
            {
                if (string.IsNullOrEmpty(nome) && string.IsNullOrEmpty(documento) && !numero.HasValue &&
                    string.IsNullOrEmpty(bloco) && string.IsNullOrEmpty(nivelAcesso) &&
                    !dataInicio.HasValue && !dataFim.HasValue && !apartamentoId.HasValue)
                {
                    return BadRequest(new { mensagem = "Informe pelo menos um filtro!" });
                }

                var query = _context.AcessoEntradaMoradores!
                    .Include(e => e.Usuario)
                        .ThenInclude(u => u.Apartamento)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(nome))
                {
                    var nomeLower = nome.ToLower();
                    query = query.Where(e => e.Usuario!.Nome.ToLower().Contains(nomeLower));
                }

                if (!string.IsNullOrEmpty(documento))
                {
                    query = query.Where(e => e.Usuario!.Documento == documento);
                }

                if (!string.IsNullOrEmpty(nivelAcesso))
                {
                    if (Enum.TryParse<NivelAcessoEnum>(nivelAcesso, ignoreCase: true, out var nivelEnum))
                    {
                        query = query.Where(e => e.Usuario!.NivelAcesso == nivelEnum);
                    }
                    else
                    {
                        return BadRequest(new { mensagem = $"Nível de acesso inválido: {nivelAcesso}" });
                    }
                }

                if (numero.HasValue)
                {
                    query = query.Where(e => e.Usuario!.Apartamento != null && e.Usuario.Apartamento.Numero == numero.Value);
                }

                if (!string.IsNullOrEmpty(bloco))
                {
                    query = query.Where(e => e.Usuario!.Apartamento != null && e.Usuario.Apartamento.Bloco == bloco);
                }

                // ✅ Novo filtro por ID do Apartamento
                if (apartamentoId.HasValue)
                {
                    query = query.Where(e => e.Usuario!.ApartamentoId == apartamentoId.Value);
                }

                if (dataInicio.HasValue && dataFim.HasValue)
                {
                    var inicio = dataInicio.Value.Date;
                    var fim = dataFim.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(e => e.DataHoraEntrada >= inicio && e.DataHoraEntrada <= fim);
                }
                else if (dataInicio.HasValue)
                {
                    var inicio = dataInicio.Value.Date;
                    query = query.Where(e => e.DataHoraEntrada >= inicio);
                }
                else if (dataFim.HasValue)
                {
                    var fim = dataFim.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(e => e.DataHoraEntrada <= fim);
                }

                var entradas = await query
                    .OrderByDescending(e => e.DataHoraEntrada)
                    .Select(e => new
                    {
                        e.Id,
                        e.UsuarioId,
                        Nome = e.Usuario!.Nome,
                        e.Usuario!.ApartamentoId,
                        Apartamento = e.Usuario!.Apartamento != null ? (int?)e.Usuario.Apartamento.Numero : null,
                        Bloco = e.Usuario!.Apartamento != null ? e.Usuario.Apartamento.Bloco : null,
                        NivelAcesso = e.Usuario!.NivelAcesso.ToString(),
                        e.DataHoraEntrada,
                        entradaPor = e.EntradaPor
                    })
                    .ToListAsync();

                if (entradas.Count == 0)
                {
                    return Ok(new List<object>()); // Retorna lista vazia
                }


                return Ok(entradas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao filtrar entradas!", detalhes = ex.Message });
            }
        }

        [HttpGet("AcessosHojeAdmin")]
        public async Task<IActionResult> GetAcessosHojeAdmin()
        {
            try
            {
                var hoje = DateTime.Now.Date; // 00:00 do dia atual
                var amanha = hoje.AddDays(1).AddTicks(-1); // 23:59:59 do dia atual

                // ✅ Log do intervalo
                Console.WriteLine($"[DEBUG] Intervalo de hoje: {hoje} até {amanha}");

                // ✅ Log dos primeiros registros no banco
                var registros = await _context.AcessoEntradaMoradores
                    .OrderByDescending(e => e.DataHoraEntrada)
                    .Select(e => new { e.Id, e.DataHoraEntrada })
                    .Take(5)
                    .ToListAsync();

                Console.WriteLine("[DEBUG] Primeiros registros no banco:");
                foreach (var r in registros)
                {
                    Console.WriteLine($"ID: {r.Id}, Data: {r.DataHoraEntrada}");
                }

                // ✅ Query original
                var acessosHoje = await _context.AcessoEntradaMoradores
                    .Include(e => e.Usuario)
                        .ThenInclude(u => u.Apartamento)
                    .Where(e => e.DataHoraEntrada >= hoje && e.DataHoraEntrada <= amanha)
                    .OrderByDescending(e => e.DataHoraEntrada)
                    .Select(e => new
                    {
                        e.Id,
                        e.UsuarioId,
                        Nome = e.Usuario!.Nome,
                        e.Usuario!.ApartamentoId,
                        Apartamento = e.Usuario!.Apartamento != null ? (int?)e.Usuario.Apartamento.Numero : null,
                        Bloco = e.Usuario!.Apartamento != null ? e.Usuario.Apartamento.Bloco : null,
                        NivelAcesso = e.Usuario!.NivelAcesso.ToString(),
                        e.DataHoraEntrada,
                        EntradaPor = e.EntradaPor
                    })
                    .ToListAsync();

                Console.WriteLine($"[DEBUG] Total encontrado hoje: {acessosHoje.Count}");

                return Ok(acessosHoje);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao buscar acessos de hoje!", detalhes = ex.Message });
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
                        dataEntrada = e.DataHoraEntrada,
                        entradaPor = e.EntradaPor
                    })
                    .ToListAsync();

                if (entradas.Count == 0)
                {
                    return Ok(new List<object>()); // Retorna lista vazia
                }

                return Ok(entradas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao listar entradas do apartamento!", detalhes = ex.Message });
            }
        }


        [HttpGet("BuscarEntradaPorId")]
        public async Task<ActionResult> BuscarEntradaPorId([FromQuery] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { mensagem = "ID da entrada inválido!" });
                }

                var entrada = await _context.AcessoEntradaMoradores!
                    .Include(e => e.Usuario)
                        .ThenInclude(u => u.Apartamento)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (entrada == null)
                {
                    return NotFound(new { mensagem = "Entrada não encontrada!" });
                }

                var resultado = new
                {
                    id = entrada.Id,
                    dataHoraEntrada = entrada.DataHoraEntrada,
                    entradaPor = entrada.EntradaPor,
                    codigoRFID = entrada.Usuario?.CodigoRFID,
                    observacao = entrada.Observacao,
                    registradoPor = entrada.RegistradoPor,
                    usuario = new
                    {
                        nome = entrada.Usuario?.Nome,
                        documento = entrada.Usuario?.Documento,
                        nivelAcesso = entrada.Usuario?.NivelAcesso.ToString().ToLower(),
                        fotoUrl = entrada.Usuario?.FotoUrl,
                        apartamento = new
                        {
                            numero = entrada.Usuario?.Apartamento?.Numero,
                            bloco = entrada.Usuario?.Apartamento?.Bloco
                        }
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao buscar entrada!", detalhes = ex.Message });
            }
        }

        [HttpPost("RegistrarEntradaManual")]
        public async Task<IActionResult> RegistrarEntradaManual([FromBody] EntradaMoradorManualRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _context.Usuarios.FindAsync(request.UsuarioId);
            if (usuario == null)
                return NotFound(new { mensagem = "Usuário não encontrado" });

            var novaEntrada = new AcessoEntradaMorador
            {
                UsuarioId = request.UsuarioId,
                DataHoraEntrada = DateTime.UtcNow, 
                EntradaPor = "2", 
                Observacao = request.Observacao,
                RegistradoPor = request.RegistradoPor
            };

            _context.AcessoEntradaMoradores.Add(novaEntrada);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Entrada registrada com sucesso", id = novaEntrada.Id });
        }



    }
}