using condominio_API.Data;
using condominio_API.Models;
using Condominio_API.Utilitarios; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCoder; // gera os qrcodes
using System.Drawing; // usado no bitmap

namespace condominio_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRCodeTempController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QRCodeTempController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("CriarQRCode")]
        public async Task<ActionResult<QRCodeTemp>> CriarQRCode([FromBody] QRCodeRequest qrCodeReq)
        {
            try
            {
                if (qrCodeReq == null || qrCodeReq.MoradorId <= 0 || qrCodeReq.VisitanteId <= 0)
                {
                    return BadRequest(new { mensagem = "Morador e visitante são campos obrigatórios!" });
                }

                var morador = await _context.Usuarios!.FindAsync(qrCodeReq.MoradorId);
                var visitante = await _context.Visitantes!.FindAsync(qrCodeReq.VisitanteId);

                if (morador == null || visitante == null)
                {
                    return BadRequest(new { mensagem = "Morador ou Visitante não encontrado!" });
                }

                var novoQRCode = new QRCodeTemp
                {
                    MoradorId = qrCodeReq.MoradorId,
                    VisitanteId = qrCodeReq.VisitanteId,
                    TipoQRCode = qrCodeReq.TipoQRCode,
                    DataCriacao = DateTime.Now,
                    DataValidade = qrCodeReq.TipoQRCode ? DateTime.Now.AddHours(24) : DateTime.Now.AddHours(4),
                    Status = true,
                    QrCodeImagem = Array.Empty<byte>()
                };

                string qrCodeData = $"Visitante - {visitante.Nome ?? "Desconhecido"}, Validade: {novoQRCode.DataValidade:dd/MM/yyyy HH:mm:ss}";
                Console.WriteLine($"QR Code Data: {qrCodeData}");

                var qrGenerator = new QRCodeGenerator();
                var qrCode = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
                var qrCodeImage = new QRCode(qrCode).GetGraphic(20);
                novoQRCode.QrCodeImagem = qrCodeImage.ToByteArray();
                novoQRCode.QrCodeData = qrCodeData;

                _context.QRCodesTemp!.Add(novoQRCode);
                await _context.SaveChangesAsync();

                var qrCodeBase64 = Convert.ToBase64String(novoQRCode.QrCodeImagem);
                return Ok(new
                {
                    mensagem = "QR Code criado com sucesso",
                    novoQRCode = new
                    {
                        novoQRCode.Id,
                        novoQRCode.MoradorId,
                        novoQRCode.VisitanteId,
                        novoQRCode.TipoQRCode,
                        novoQRCode.DataCriacao,
                        novoQRCode.DataValidade,
                        novoQRCode.Status,
                        QrCodeImagem = qrCodeBase64,
                        QrCodeData = qrCodeData
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao criar QR Code!", detalhes = ex.Message });
            }
        }

        [HttpGet("ExibirQRCodesPorMorador/{moradorId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetQRCodesPorMorador(int moradorId)
        {
            try
            {
                if (moradorId <= 0)
                {
                    return BadRequest(new { mensagem = "MoradorId inválido!" });
                }

                var qrCodes = await _context.QRCodesTemp
                    .Where(q => q.MoradorId == moradorId)
                    .Include(q => q.Morador)
                    .Include(q => q.Visitante)
                    .OrderByDescending(q => q.DataCriacao)
                    .Select(q => new
                    {
                        q.Id,
                        q.MoradorId,
                        q.VisitanteId,
                        q.TipoQRCode,
                        q.DataCriacao,
                        q.DataValidade,
                        q.Status,
                        QrCodeImagem = Convert.ToBase64String(q.QrCodeImagem), // converte byte pra base 64 no retorno
                        Morador = new { q.Morador.UsuarioId, q.Morador.Nome }, 
                        Visitante = new { q.Visitante.VisitanteId, q.Visitante.Nome, q.Visitante.Documento, q.Visitante.Telefone },
                        IsValid = q.Status && q.DataValidade >= DateTime.Now
                    })
                    .ToListAsync();

                if (qrCodes.Count == 0)
                {
                    return NotFound(new { mensagem = "Nenhum QR Code encontrado para este morador." });
                }

                return Ok(qrCodes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao buscar QR Codes!", detalhes = ex.Message });
            }
        }

        [HttpGet("BuscarQRCodePorVisitante")]
        public async Task<ActionResult<IEnumerable<object>>> BuscarQRCodePorVisitante([FromQuery] int moradorId,
            [FromQuery] string? nomeVisitante = null, [FromQuery] string? documento = null, [FromQuery] string? telefone = null)
        {
            try
            {
                if (moradorId <= 0)
                {
                    return BadRequest(new { mensagem = "Morador inválido!" });
                }

                if (string.IsNullOrEmpty(nomeVisitante) && string.IsNullOrEmpty(documento) && string.IsNullOrEmpty(telefone))
                {
                    return BadRequest(new { mensagem = "Informe pelo menos um filtro: nome, documento ou telefone!" });
                }

                var query = _context.QRCodesTemp
                    .Where(q => q.MoradorId == moradorId)
                    .Include(q => q.Morador)
                    .Include(q => q.Visitante)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(nomeVisitante))
                {
                    query = query.Where(q => q.Visitante.Nome.Contains(nomeVisitante));
                }
                if (!string.IsNullOrEmpty(documento))
                {
                    query = query.Where(q => q.Visitante.Documento.Contains(documento));
                }
                if (!string.IsNullOrEmpty(telefone))
                {
                    query = query.Where(q => q.Visitante.Telefone.Contains(telefone));
                }

                var qrCodes = await query
                    .Select(q => new
                    {
                        q.Id,
                        q.MoradorId,
                        q.VisitanteId,
                        q.TipoQRCode,
                        q.DataCriacao,
                        q.DataValidade,
                        q.Status,
                        QrCodeImagem = Convert.ToBase64String(q.QrCodeImagem), 
                        Morador = new { q.Morador.UsuarioId, q.Morador.Nome }, 
                        Visitante = new { q.Visitante.VisitanteId, q.Visitante.Nome, q.Visitante.Documento, q.Visitante.Telefone },
                        IsValid = q.Status && q.DataValidade >= DateTime.Now
                    })
                    .ToListAsync();

                if (qrCodes.Count == 0)
                {
                    return NotFound(new { mensagem = "Nenhum QR Code encontrado!" });
                }

                return Ok(qrCodes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao buscar QR Code!", detalhes = ex.Message });
            }
        }

        [HttpPost("InativarQRCode/{id}")]
        public async Task<IActionResult> InativarQRCode(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { mensagem = "QR Code  inválido!" });
                }

                var qrCode = await _context.QRCodesTemp.FindAsync(id);
                if (qrCode == null)
                {
                    return NotFound(new { mensagem = "QR Code não encontrado!" });
                }

                if (!qrCode.Status)
                {
                    return BadRequest(new { mensagem = "O QR Code já está inativo!" });
                }

                qrCode.Status = false;
                await _context.SaveChangesAsync();

                return Ok(new { mensagem = "QR Code inativado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao inativar QR Code!", detalhes = ex.Message });
            }
        }

        [HttpPost("ValidarQRCode/{id}")]
        public async Task<IActionResult> ValidarQRCode(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { mensagem = "QR Code inválido!" });
                }

                var qrCode = await _context.QRCodesTemp.FindAsync(id);
                if (qrCode == null)
                {
                    return NotFound(new { mensagem = "QR Code não encontrado!" });
                }

                if (!qrCode.Status || qrCode.DataValidade < DateTime.Now)
                {
                    return BadRequest(new { mensagem = "QR Code expirado ou inativado!" });
                }

                if (!qrCode.TipoQRCode) // falso será uso único
                {
                    qrCode.Status = false;
                    await _context.SaveChangesAsync();
                    return Ok(new { mensagem = "QR Code válido e inativado!" });
                }

                return Ok(new { mensagem = "QR Code válido!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao validar QR Code!", detalhes = ex.Message });
            }
        }
    }
}