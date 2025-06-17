using condominio_API.Data;
using condominio_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace condominio_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificacaoController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Criar notificação
        [HttpPost("CriarNotificacao")]
        public async Task<IActionResult> CriarNotificacao([FromBody] Notificacao nova)
        {
            if (nova == null || string.IsNullOrWhiteSpace(nova.Mensagem))
                return BadRequest(new { mensagem = "Dados da notificação inválidos." });

            nova.DataCriacao = DateTime.UtcNow;
            nova.Status = StatusNotificacao.Pendente;

            _context.Notificacoes.Add(nova);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Notificação criada com sucesso.", nova });
        }

        // 2. Notificações do morador
        [HttpGet("MinhasNotificacoes/{usuarioId}")]
        public async Task<IActionResult> GetMinhasNotificacoes(int usuarioId)
        {
            var lista = await _context.Notificacoes
                .Include(n => n.ApartamentoDestino)
                .Where(n => n.MoradorOrigemId == usuarioId)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();

            return Ok(lista);
        }

        // 3. Notificações pendentes para aprovação
        [HttpGet("PendentesParaAprovacao")]
        public async Task<IActionResult> GetPendentes()
        {
            var pendentes = await _context.Notificacoes
                .Include(n => n.MoradorOrigem)
                .Include(n => n.ApartamentoDestino)
                .Where(n => n.Status == StatusNotificacao.Pendente)
                .OrderBy(n => n.DataCriacao)
                .ToListAsync();

            return Ok(pendentes);
        }

        // 4. Aprovar notificação
        [HttpPut("Aprovar/{id}")]
        public async Task<IActionResult> Aprovar(int id)
        {
            var notificacao = await _context.Notificacoes.FindAsync(id);
            if (notificacao == null)
                return NotFound(new { mensagem = "Notificação não encontrada." });

            notificacao.Status = StatusNotificacao.Aprovada;
            notificacao.UltimaAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Notificação aprovada." });
        }

        // 5. Rejeitar notificação com comentário
        [HttpPut("Rejeitar/{id}")]
        public async Task<IActionResult> Rejeitar(int id, [FromBody] string comentario)
        {
            var notificacao = await _context.Notificacoes.FindAsync(id);
            if (notificacao == null)
                return NotFound(new { mensagem = "Notificação não encontrada." });

            notificacao.Status = StatusNotificacao.Rejeitada;
            notificacao.ComentarioSindico = comentario;
            notificacao.UltimaAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Notificação rejeitada com comentário." });
        }

        // 6. Notificações recebidas por morador (caso tenha destino)
        [HttpGet("Recebidas/{apartamentoId}")]
        public async Task<IActionResult> GetRecebidas(int apartamentoId)
        {
            var recebidas = await _context.Notificacoes
                .Include(n => n.MoradorOrigem)
                .Where(n => n.ApartamentoDestinoId == apartamentoId && n.Status == StatusNotificacao.Aprovada)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();

            return Ok(recebidas);
        }
    }
}
