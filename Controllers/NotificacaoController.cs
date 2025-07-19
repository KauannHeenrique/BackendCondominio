using condominio_API.Data;
using condominio_API.Models;
using condominio_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NotificacaoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly NotificacaoService _notificacaoService;

    public NotificacaoController(AppDbContext context, NotificacaoService notificacaoService)
    {
        _context = context;
        _notificacaoService = notificacaoService;
    }

    /// ✅ Criar Notificação
    [HttpPost("CriarNotificacao")]
    public async Task<IActionResult> CriarNotificacao([FromBody] CriarNotificacaoRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Mensagem) || string.IsNullOrWhiteSpace(request.Titulo))
            return BadRequest(new { mensagem = "Dados inválidos." });

        var usuarioOrigem = await _context.Usuarios.FindAsync(request.MoradorOrigemId);
        if (usuarioOrigem == null)
            return BadRequest(new { mensagem = "Usuário não encontrado." });

        // ✅ Permissão para comunicado geral
        if (request.Tipo == TipoNotificacao.ComunicadoGeral &&
            usuarioOrigem.NivelAcesso != NivelAcessoEnum.Sindico &&
            usuarioOrigem.NivelAcesso != NivelAcessoEnum.Funcionario)
        {
            return Forbid("Você não tem permissão para comunicados gerais.");
        }

        // ✅ Define status inicial
        var statusInicial = StatusNotificacao.Pendente;

        // ✅ Regras para aprovação automática
        if (request.Tipo == TipoNotificacao.AvisoDeBarulho ||
            (request.Tipo == TipoNotificacao.Outro && request.CriadoPorSindico && request.UsuarioDestinoId.HasValue) ||
            request.Tipo == TipoNotificacao.ComunicadoGeral)
        {
            statusInicial = StatusNotificacao.Aprovada;
        }

        // ✅ Cria a notificação
        var notificacao = new Notificacao
        {
            Tipo = request.Tipo,
            Titulo = request.Titulo,
            Mensagem = request.Mensagem,
            MoradorOrigemId = request.MoradorOrigemId,
            DataCriacao = DateTime.UtcNow,
            CriadoPorSindico = request.CriadoPorSindico,
            Status = statusInicial
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync(); // Gera ID

            var destinatarios = new List<NotificacaoDestinatario>();

            // ✅ Define os destinatários
            switch (request.Tipo)
            {
                case TipoNotificacao.AvisoDeBarulho:
                    if (!request.ApartamentoDestinoId.HasValue)
                        return BadRequest(new { mensagem = "Informe o apartamento para aviso de barulho." });

                    var moradores = await _context.Usuarios
                        .Where(u => u.ApartamentoId == request.ApartamentoDestinoId.Value)
                        .ToListAsync();

                    destinatarios.AddRange(moradores.Select(u => new NotificacaoDestinatario
                    {
                        NotificacaoId = notificacao.Id,
                        UsuarioDestinoId = u.UsuarioId
                    }));
                    break;

                case TipoNotificacao.SolicitacaoDeReparo:
                case TipoNotificacao.Sugestao:
                    var sindicos = await _context.Usuarios
                        .Where(u => u.NivelAcesso == NivelAcessoEnum.Sindico)
                        .ToListAsync();

                    destinatarios.AddRange(sindicos.Select(u => new NotificacaoDestinatario
                    {
                        NotificacaoId = notificacao.Id,
                        UsuarioDestinoId = u.UsuarioId
                    }));
                    break;

                case TipoNotificacao.Outro:
                    if (request.CriadoPorSindico && request.UsuarioDestinoId.HasValue)
                    {
                        destinatarios.Add(new NotificacaoDestinatario
                        {
                            NotificacaoId = notificacao.Id,
                            UsuarioDestinoId = request.UsuarioDestinoId.Value
                        });
                    }
                    else
                    {
                        var admins = await _context.Usuarios
                            .Where(u => u.NivelAcesso == NivelAcessoEnum.Sindico || u.NivelAcesso == NivelAcessoEnum.Funcionario)
                            .ToListAsync();

                        destinatarios.AddRange(admins.Select(u => new NotificacaoDestinatario
                        {
                            NotificacaoId = notificacao.Id,
                            UsuarioDestinoId = u.UsuarioId
                        }));
                    }
                    break;

                case TipoNotificacao.ComunicadoGeral:
                    var todosUsuarios = await _context.Usuarios.ToListAsync();
                    destinatarios.AddRange(todosUsuarios.Select(u => new NotificacaoDestinatario
                    {
                        NotificacaoId = notificacao.Id,
                        UsuarioDestinoId = u.UsuarioId
                    }));
                    break;
            }

            _context.AddRange(destinatarios);
            await _context.SaveChangesAsync();

            // ✅ Registra histórico da criação
            await _context.NotificacaoHistoricos.AddAsync(new NotificacaoHistorico
            {
                NotificacaoId = notificacao.Id,
                UsuarioId = request.MoradorOrigemId,
                Acao = AcaoHistorico.CRIACAO,
                StatusNovo = statusInicial,
                DataRegistro = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { mensagem = "Notificação criada com sucesso." });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// ✅ Atualizar Notificação (status, comentário, leitura)
    [HttpPut("{id}/atualizar")]
    [Authorize]
    public async Task<IActionResult> AtualizarNotificacao(int id, [FromBody] AtualizarNotificacaoRequest dto)
    {
        var usuarioId = GetUsuarioIdDoToken();
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return Unauthorized(new { mensagem = "Usuário não encontrado." });

        // ✅ Regra de permissão
        if (usuario.NivelAcesso == NivelAcessoEnum.Morador)
        {
            // Morador só pode marcar como lida
            if (dto.MarcarComoLida != true || dto.Status.HasValue || !string.IsNullOrEmpty(dto.Comentario))
                return Forbid("Moradores só podem marcar notificações como lidas.");
        }
        else if (usuario.NivelAcesso != NivelAcessoEnum.Sindico && usuario.NivelAcesso != NivelAcessoEnum.Funcionario)
        {
            return Forbid("Você não tem permissão para alterar notificações.");
        }

        try
        {
            await _notificacaoService.EditarNotificacao(
                notificacaoId: id,
                usuarioId: usuarioId,
                novoStatus: dto.Status,
                novoComentario: dto.Comentario,
                marcarComoLida: dto.MarcarComoLida
            );

            return Ok(new { mensagem = "Notificação atualizada com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro interno ao atualizar notificação.", detalhe = ex.Message });
        }
    }


    /// ✅ Minhas Notificações
    [HttpGet("MinhasNotificacoes/{usuarioId}")]
    [Authorize]
    public async Task<IActionResult> MinhasNotificacoes(int usuarioId, int page = 1, int pageSize = 20, [FromQuery] bool criadoPorSindico = false)
    {
        var query = _context.Notificacoes
            .Where(n => n.MoradorOrigemId == usuarioId && n.CriadoPorSindico == criadoPorSindico)
            .OrderByDescending(n => n.DataCriacao);

        var total = await query.CountAsync();
        var notificacoes = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new
        {
            total,
            page,
            pageSize,
            notificacoes
        });
    }


    /// ✅ Notificações Recebidas
    [HttpGet("Recebidas/{usuarioId}")]
    public async Task<IActionResult> Recebidas(
    int usuarioId,
    int page = 1,
    int pageSize = 20,
    [FromQuery] bool criadoPorSindico = false)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        // Base da query
        var query = _context.NotificacaoDestinatarios
            .Include(d => d.Notificacao)
            .Where(d => d.UsuarioDestinoId == usuarioId && d.Notificacao.CriadoPorSindico == criadoPorSindico)
            .OrderByDescending(d => d.Notificacao.DataCriacao);

        // Conta total de notificações recebidas
        var total = await query.CountAsync();

        // Aplica paginação
        var destinatarios = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Seleciona apenas os dados necessários da notificação (para evitar payload gigante)
        var notificacoes = destinatarios.Select(d => new
        {
            d.Notificacao.Id,
            d.Notificacao.Titulo,
            d.Notificacao.Mensagem,
            d.Notificacao.Status,
            d.Notificacao.Tipo,
            d.Notificacao.DataCriacao,
            d.Notificacao.UltimaAtualizacao,
            Lido = d.Lido // Informação do destinatário
        });

        return Ok(new
        {
            total,
            page,
            pageSize,
            notificacoes
        });
    }

    [HttpGet("AlertasAtivos/{usuarioId}")]
    public async Task<IActionResult> GetAlertasAtivos(int usuarioId)
    {
        try
        {
            var alertasAtivos = await _context.Notificacoes
     .Where(n => n.Destinatarios.Any(d => d.UsuarioDestinoId == usuarioId))
     .Where(n => n.Status == StatusNotificacao.Aprovada || n.Status == StatusNotificacao.EmAndamento)
     .CountAsync();


            return Ok(alertasAtivos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao buscar alertas ativos: {ex.Message}");
        }
    }




    /// ✅ Detalhes com histórico
    [HttpGet("{id}/detalhes")]
    public async Task<IActionResult> DetalhesNotificacao(int id)
    {
        var notificacao = await _context.Notificacoes
            .Include(n => n.MoradorOrigem)
            .Include(n => n.Destinatarios)
                .ThenInclude(d => d.UsuarioDestino)
            .Include(n => n.Historico)
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
                notificacao.MoradorOrigem.Nome
            } : null,
            Destinatarios = notificacao.Destinatarios.Select(d => new
            {
                UsuarioId = d.UsuarioDestinoId,
                Nome = d.UsuarioDestino?.Nome,
                Lido = d.Lido
            }),
            Historico = notificacao.Historico.Select(h => new
            {
                h.Acao,
                h.StatusNovo,
                h.DataRegistro
            })
        };

        return Ok(dto);
    }


    /// ✅ Helper para pegar ID do usuário logado
    private int GetUsuarioIdDoToken()
    {
        return int.Parse(User.Claims.First(c => c.Type == "id").Value);
    }
}
