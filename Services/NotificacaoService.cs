using condominio_API.Data;
using condominio_API.Models;
using Microsoft.EntityFrameworkCore;

namespace condominio_API.Services
{
    public class NotificacaoService
    {
        private readonly AppDbContext _context;

        public NotificacaoService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Edita uma notificação (status, comentário, leitura) e registra histórico.
        /// </summary>
        public async Task EditarNotificacao(
    int notificacaoId,
    int usuarioId,
    StatusNotificacao? novoStatus = null,
    string? novoComentario = null,
    bool? marcarComoLida = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var notificacao = await _context.Notificacoes.FindAsync(notificacaoId);
                if (notificacao == null)
                    throw new Exception("Notificação não encontrada.");

                var statusAnterior = notificacao.Status;
                bool statusAlterado = false;

                // Atualiza status
                if (novoStatus.HasValue && novoStatus.Value != notificacao.Status)
                {
                    notificacao.Status = novoStatus.Value;
                    statusAlterado = true;

                    await RegistrarHistorico(
                        notificacao.Id,
                        usuarioId,
                        acao: novoStatus switch
                        {
                            StatusNotificacao.Aprovada => AcaoHistorico.APROVACAO,
                            StatusNotificacao.Rejeitada => AcaoHistorico.REJEICAO,
                            StatusNotificacao.EmAndamento => AcaoHistorico.EM_ANDAMENTO,
                            StatusNotificacao.Concluida => AcaoHistorico.CONCLUIDA,
                            _ => AcaoHistorico.COMENTARIO
                        },
                        statusAnterior: statusAnterior,
                        statusNovo: novoStatus.Value,
                        comentario: novoComentario // Comentário opcional no mesmo histórico
                    );
                }

                // Só salva comentário se NÃO foi salvo junto com status
                if (!string.IsNullOrEmpty(novoComentario) && !statusAlterado)
                {
                    await RegistrarHistorico(notificacao.Id, usuarioId, AcaoHistorico.COMENTARIO, comentario: novoComentario);
                }

                // Marca como lida
                if (marcarComoLida == true)
                {
                    var destinatario = await _context.NotificacaoDestinatarios
                        .FirstOrDefaultAsync(d => d.NotificacaoId == notificacao.Id && d.UsuarioDestinoId == usuarioId);

                    if (destinatario != null && !destinatario.Lido)
                    {
                        destinatario.Lido = true;
                        _context.Entry(destinatario).Property(d => d.Lido).IsModified = true;
                    }

                    await RegistrarHistorico(notificacao.Id, usuarioId, AcaoHistorico.LEITURA);
                }

                notificacao.UltimaAtualizacao = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        /// <summary>
        /// Registra uma ação no histórico da notificação.
        /// </summary>
        private async Task RegistrarHistorico(
            int notificacaoId,
            int usuarioId,
            AcaoHistorico acao,
            StatusNotificacao? statusAnterior = null,
            StatusNotificacao? statusNovo = null,
            string? comentario = null)
        {
            var historico = new NotificacaoHistorico
            {
                NotificacaoId = notificacaoId,
                UsuarioId = usuarioId,
                Acao = acao,
                StatusAnterior = statusAnterior,
                StatusNovo = statusNovo,
                Comentario = comentario,
                DataRegistro = DateTime.UtcNow
            };

            _context.NotificacaoHistoricos.Add(historico);
        }
    }

}
