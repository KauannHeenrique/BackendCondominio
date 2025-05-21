/*using condominio_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace condominio_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacaoController : ControllerBase
    {
        private static List<Notificacao> notificacoes = new List<Notificacao>();

        [HttpGet]
        public ActionResult<IEnumerable<Notificacao>> GetNotificacoes()
        {
            return Ok(notificacoes);
        }

        [HttpGet("{id}")]
        public ActionResult<Notificacao> GetNotificacao(int id)
        {
            var notificacao = notificacoes.FirstOrDefault(n => n.Id == id);
            if (notificacao == null)
            {
                return NotFound(new { mensagem = "Notificação não encontrada." });
            }
            return Ok(notificacao);
        }

        [HttpPost]
        public ActionResult<Notificacao> PostNotificacao(Notificacao novaNotificacao)
        {
            if (string.IsNullOrEmpty(novaNotificacao.Mensagem))
            {
                return BadRequest(new { mensagem = "Mensagem é obrigatória." });
            }

            var moradorOrigem = UsuarioController.usuarios.FirstOrDefault(u => u.UsuarioId == novaNotificacao.MoradorOrigemId);
            if (moradorOrigem == null)
            {
                return BadRequest(new { mensagem = "Morador de origem não encontrado." });
            }

            var apartamentoDestino = ApartamentoController.apartamentos.FirstOrDefault(a => a.Id == novaNotificacao.ApartamentoDestinoId);
            if (apartamentoDestino == null)
            {
                return BadRequest(new { mensagem = "Apartamento de destino não encontrado." });
            }

            int novoId = notificacoes.Count > 0 ? notificacoes.Max(n => n.Id) + 1 : 1;
            novaNotificacao.Id = novoId;
            novaNotificacao.DataHora = DateTime.Now;

            notificacoes.Add(novaNotificacao);

            return CreatedAtAction(nameof(GetNotificacao), new { id = novaNotificacao.Id }, novaNotificacao);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteNotificacao(int id)
        {
            var notificacao = notificacoes.FirstOrDefault(n => n.Id == id);
            if (notificacao == null)
            {
                return NotFound(new { mensagem = "Notificação não encontrada." });
            }

            notificacoes.Remove(notificacao);
            return Ok(new { mensagem = "Notificação removida com sucesso." });
        }
    }
}*/