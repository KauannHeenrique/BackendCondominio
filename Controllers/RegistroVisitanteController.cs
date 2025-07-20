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
        public async Task<IActionResult> RegistrarEntradaVisitante([FromBody] EntradaVisitanteRequest request)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(request.QrCodeData))
                {
                    // Fluxo de QR Code
                    return await RegistrarEntradaPorQrCode(request);
                }

                // Fluxo Manual
                return await RegistrarEntradaManual(request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao processar a solicitação.", detalhes = ex.Message });
            }
        }

        // ✅ Fluxo de QR Code
        private async Task<IActionResult> RegistrarEntradaPorQrCode(EntradaVisitanteRequest request)
        {
            if (string.IsNullOrEmpty(request.QrCodeData))
            {
                return BadRequest(new { mensagem = "O QR code é obrigatório!" });
            }

            var qrCode = await _context.QRCodesTemp!
                .Include(q => q.Visitante)
                .Include(q => q.Morador)
                    .ThenInclude(m => m.Apartamento)
                .FirstOrDefaultAsync(q => q.QrCodeData == request.QrCodeData
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
                DataHoraEntrada = DateTime.Now,
                EntradaPor = "QRCode",
                RegistradoPor = request.RegistradoPor
            };

            if (!qrCode.TipoQRCode)
            {
                qrCode.Status = false; // QR code usado não pode ser reutilizado
            }

            _context.AcessoEntradaVisitantes.Add(novaEntrada);
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

        // ✅ Fluxo Manual
        private async Task<IActionResult> RegistrarEntradaManual(EntradaVisitanteRequest request)
        {
            if (request.VisitanteId == null || string.IsNullOrWhiteSpace(request.Bloco) ||
                string.IsNullOrWhiteSpace(request.Apartamento) || string.IsNullOrWhiteSpace(request.CpfMorador))
            {
                return BadRequest(new { mensagem = "Informe visitante, bloco, apartamento e CPF do morador." });
            }

            // Busca visitante
            var visitante = await _context.Visitantes.FindAsync(request.VisitanteId);
            if (visitante == null)
                return NotFound(new { mensagem = "Visitante não encontrado." });

            // Limpar CPF (somente números)
            var cpfLimpo = new string(request.CpfMorador.Where(char.IsDigit).ToArray());

            int numeroApartamento;
            if (!int.TryParse(request.Apartamento, out numeroApartamento))
            {
                return BadRequest(new { mensagem = "Número do apartamento inválido." });
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Apartamento)
                .FirstOrDefaultAsync(u =>
                    u.Documento == cpfLimpo &&
                    u.Apartamento.Bloco == request.Bloco &&
                    u.Apartamento.Numero == numeroApartamento);


            if (usuario == null)
                return NotFound(new { mensagem = "Morador não encontrado para o bloco, apartamento e CPF informados." });

            // Registrar entrada manual
            var entrada = new AcessoEntradaVisitante
            {
                VisitanteId = request.VisitanteId.Value,
                UsuarioId = usuario.UsuarioId,
                DataHoraEntrada = DateTime.Now,
                EntradaPor = "Manual",
                Observacao = request.Observacao,
                RegistradoPor = request.RegistradoPor
            };

            _context.AcessoEntradaVisitantes.Add(entrada);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Entrada manual registrada com sucesso!" });
        }

        // ✅ Listar Entradas
        [HttpGet("ListarEntradasVisitantes")]
        public async Task<ActionResult> ListarEntradasVisitantes()
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
                    dataEntrada = e.DataHoraEntrada,
                    entradaPor = e.EntradaPor
                })
                .ToListAsync();

            return Ok(entradas);
        }
    }
}
