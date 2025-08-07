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

        [HttpGet("{id}/detalhes")]
        public async Task<IActionResult> DetalhesNotificacao(int id)
        {
            var notificacao = await _context.Notificacoes
                .Include(n => n.MoradorOrigem)
                    .ThenInclude(m => m.Apartamento)
                .Include(n => n.Destinatarios)
                    .ThenInclude(d => d.UsuarioDestino)
                .Include(n => n.Historico)
                    .ThenInclude(h => h.Usuario)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notificacao == null)
                return NotFound(new { mensagem = "Notificação não encontrada." });

            var dto = new
            {
                Id = notificacao.Id,
                Titulo = notificacao.Titulo,
                Mensagem = notificacao.Mensagem,
                Tipo = notificacao.Tipo,
                Status = notificacao.Status,
                DataCriacao = notificacao.DataCriacao,
                UltimaAtualizacao = notificacao.UltimaAtualizacao,
                CriadoPorSindico = notificacao.CriadoPorSindico,
                MoradorOrigem = notificacao.MoradorOrigem != null ? new
                {
                    notificacao.MoradorOrigem.UsuarioId,
                    notificacao.MoradorOrigem.Nome,
                    Bloco = notificacao.MoradorOrigem.Apartamento?.Bloco,
                    Apartamento = notificacao.MoradorOrigem.Apartamento?.Numero
                } : null,
                Destinatarios = notificacao.Destinatarios.Select(d => new
                {
                    UsuarioId = d.UsuarioDestinoId,
                    Nome = d.UsuarioDestino?.Nome,
                    Lido = d.Lido
                }),
                Historico = notificacao.Historico
                    .Where(h => h.Acao != AcaoHistorico.PENDENTE) // ✅ Remove ações com Acao = 0
                    .Select(h => new
                    {
                        h.Acao,
                        h.StatusNovo,
                        h.DataRegistro,
                        h.Comentario,
                        UsuarioNome = h.Usuario != null ? h.Usuario.Nome : "Usuário não encontrado",
                        NivelAcesso = h.Usuario != null ? h.Usuario.NivelAcesso.ToString() : "Desconhecido"
                    })
            };

            return Ok(dto);
        }

        [HttpGet("Recentes/{usuarioId}")]
        public async Task<IActionResult> ObterRecentes(int usuarioId, [FromQuery] int limite = 5)
        {
            var atividades = await _context.Set<AtividadeView>()
                .Where(a => a.UsuarioId == usuarioId)
                .OrderByDescending(a => a.DataRegistro)
                .Take(limite)
                .ToListAsync();

            return Ok(atividades);
        }


    }
}
