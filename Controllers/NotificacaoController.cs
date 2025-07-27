using condominio_API.Data;
using condominio_API.Models;
using condominio_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NotificacaoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly NotificacaoService _notificacaoService;
    private readonly ILogger<NotificacaoController> _logger;


    public NotificacaoController(AppDbContext context, NotificacaoService notificacaoService, ILogger<NotificacaoController> logger)
    {
        _context = context;
        _notificacaoService = notificacaoService;
        _logger = logger;  // Agora você pode usar _logger
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
        bool aprovadoAutomaticamente = false; // ✅ Declarada aqui

        // ✅ Regras para aprovação automática
        if (request.Tipo == TipoNotificacao.AvisoDeBarulho ||
            (request.Tipo == TipoNotificacao.Outro && request.CriadoPorSindico && request.UsuarioDestinoId.HasValue) ||
            request.Tipo == TipoNotificacao.ComunicadoGeral)
        {
            statusInicial = StatusNotificacao.Aprovada;
            aprovadoAutomaticamente = true; // ✅ Agora marca como automático
        }

        // ✅ Cria a notificação
        var notificacao = new Notificacao
        {
            Tipo = request.Tipo,
            Titulo = request.Titulo,
            Mensagem = request.Mensagem,
            MoradorOrigemId = request.MoradorOrigemId,
            DataCriacao = DateTime.UtcNow,
            UltimaAtualizacao = DateTime.UtcNow, 
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

            // ✅ Registro 1: Criação da notificação (sempre Status Pendente)
            await _context.NotificacaoHistoricos.AddAsync(new NotificacaoHistorico
            {
                NotificacaoId = notificacao.Id,
                UsuarioId = request.MoradorOrigemId,
                Acao = AcaoHistorico.CRIACAO,
                StatusNovo = StatusNotificacao.Pendente, // Sempre pendente na criação
                DataRegistro = DateTime.UtcNow,
                Comentario = null
            });

            // ✅ Registro 2: Aprovação automática (se aplicável)
            if (aprovadoAutomaticamente)
            {
                await _context.NotificacaoHistoricos.AddAsync(new NotificacaoHistorico
                {
                    NotificacaoId = notificacao.Id,
                    UsuarioId = null, // Sistema
                    Acao = AcaoHistorico.APROVACAO,
                    StatusNovo = StatusNotificacao.Aprovada,
                    DataRegistro = DateTime.UtcNow,
                    Comentario = "Aprovação automática do sistema"
                });
            }


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
    /// ✅ Atualizar Notificação (status, comentário, leitura)
    [HttpPut("{id}/atualizar")]
    [Authorize]
    public async Task<IActionResult> AtualizarNotificacao(int id, [FromBody] AtualizarNotificacaoRequest dto)
    {
        var usuarioId = GetUsuarioIdDoToken();
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return Unauthorized(new { mensagem = "Usuário não encontrado." });

        // ✅ Valida permissões
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
            // ✅ Prepara dados (remove comentário vazio)
            var comentarioFinal = string.IsNullOrWhiteSpace(dto.Comentario) ? null : dto.Comentario;

            // ✅ Chama serviço que faz a lógica correta (sem duplicação de histórico)
            await _notificacaoService.EditarNotificacao(
                notificacaoId: id,
                usuarioId: usuarioId,
                novoStatus: dto.Status,
                novoComentario: comentarioFinal,
                marcarComoLida: dto.MarcarComoLida
            );

            // ✅ Retorna resposta simples
            return Ok(new { mensagem = "Notificação atualizada com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao atualizar notificação {id}: {ex.Message}");
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

    [HttpGet("AlertasAtivosAdmin")]
    public async Task<IActionResult> GetTodosAlertasAtivos()
    {
        try
        {
            var alertasAtivos = await _context.Notificacoes
                .Where(n => n.Status != StatusNotificacao.Rejeitada && n.Status != StatusNotificacao.Concluida)
                .Select(n => new
                {
                    n.Id,
                    n.Titulo,
                    n.Mensagem,
                    n.Status,
                    n.Tipo,
                    n.DataCriacao,
                    n.UltimaAtualizacao
                })
                .ToListAsync();

            return Ok(alertasAtivos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao buscar alertas ativos: {ex.Message}");
        }
    }

   

    [HttpGet("AlertasRecentesAdmin")]
    public async Task<IActionResult> GetAlertasRecentesAdmin()
    {
        try
        {
            var notificacoes = await _context.Notificacoes
                .OrderByDescending(n => n.DataCriacao) // ordenar por mais recentes
                .Select(n => new
                {
                    n.Id,
                    n.Titulo,
                    n.Mensagem,
                    n.Status,
                    n.Tipo,
                    n.DataCriacao,
                    n.UltimaAtualizacao
                })
                .Take(20) // limitar para não pesar demais
                .ToListAsync();

            return Ok(notificacoes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao buscar notificações recentes: {ex.Message}");
        }
    }




    /// ✅ Detalhes com histórico
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
                .Where(h => h.Acao != AcaoHistorico.CRIACAO) // ✅ Remove ações com Acao = 0
                .OrderByDescending(h => h.DataRegistro)
                .Select(h => new
                {
                    h.Acao,
                    h.StatusNovo,
                    h.DataRegistro,
                    h.Comentario,
                    UsuarioNome = h.Usuario != null ? h.Usuario.Nome : "",
                    NivelAcesso = h.Usuario != null ? h.Usuario.NivelAcesso.ToString() : ""
                })
        };

        return Ok(dto);
    }



    /// ✅ Buscar Comentários de uma Notificação
    [HttpGet("{id}/comentarios")]
    public async Task<IActionResult> BuscarComentarios(int id)
    {
        try
        {
            var historicos = await _context.NotificacaoHistoricos
                .Include(h => h.Usuario) // Para pegar o nome do autor
                .Where(h => h.NotificacaoId == id && !string.IsNullOrEmpty(h.Comentario))
                .OrderByDescending(h => h.DataRegistro) // Mais recentes primeiro
                .Select(h => new
                {
                    Id = h.Id,
                    Autor = h.Usuario.Nome,
                    Texto = h.Comentario,
                    Data = h.DataRegistro
                })
                .ToListAsync();

            return Ok(historicos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao buscar comentários.", detalhe = ex.Message });
        }
    }

    [HttpGet("ListarTodas")]
    public async Task<IActionResult> ListarTodas()
    {
        try
        {
            var notificacoes = await _context.Notificacoes
                .Include(n => n.MoradorOrigem)
                    .ThenInclude(m => m.Apartamento)
                .OrderByDescending(n => n.DataCriacao)
                .Select(n => new
                {
                    n.Id,
                    n.Titulo,
                    n.Mensagem,
                    n.Tipo,
                    n.Status,
                    n.DataCriacao,
                    n.UltimaAtualizacao,
                    CriadoPorSindico = n.CriadoPorSindico,
                    Origem = n.MoradorOrigem != null ? new
                    {
                        UsuarioId = n.MoradorOrigem.UsuarioId,
                        Nome = n.MoradorOrigem.Nome,
                        Bloco = n.MoradorOrigem.Apartamento != null ? n.MoradorOrigem.Apartamento.Bloco.ToString() : null,
                        Apartamento = n.MoradorOrigem.Apartamento != null ? n.MoradorOrigem.Apartamento.Numero.ToString() : null
                    } : null
                })
                .ToListAsync();

            if (!notificacoes.Any())
                return NotFound(new { mensagem = "Nenhuma notificação encontrada." });

            return Ok(new
            {
                mensagem = "Notificações encontradas com sucesso.",
                total = notificacoes.Count,
                notificacoes
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao buscar notificações.", detalhes = ex.Message });
        }
    }

    [HttpGet("BuscarNotificacaoPor")]
    public async Task<IActionResult> BuscarNotificacaoPor(
    [FromQuery] int? tipo,               // Tipo da notificação
    [FromQuery] int? status,             // Status da notificação
    [FromQuery] string? bloco,           // Bloco do morador criador
    [FromQuery] string? apartamento,     // Apartamento do morador criador
    [FromQuery] DateTime? dataInicio,    // Data inicial do período
    [FromQuery] DateTime? dataFim        // Data final do período
)
    {
        try
        {
            var query = _context.Notificacoes
                .Include(n => n.MoradorOrigem)
                    .ThenInclude(m => m.Apartamento)
                .AsQueryable();

            // ✅ Aplica os filtros
            if (tipo.HasValue)
                query = query.Where(n => n.Tipo == (TipoNotificacao)tipo.Value);

            if (status.HasValue)
                query = query.Where(n => n.Status == (StatusNotificacao)status.Value);

            if (!string.IsNullOrWhiteSpace(bloco))
                query = query.Where(n => n.MoradorOrigem != null &&
                                         n.MoradorOrigem.Apartamento != null &&
                                         n.MoradorOrigem.Apartamento.Bloco.Contains(bloco));

            if (!string.IsNullOrWhiteSpace(apartamento))
                query = query.Where(n => n.MoradorOrigem != null &&
                                         n.MoradorOrigem.Apartamento != null &&
                                         n.MoradorOrigem.Apartamento.Numero.ToString().Contains(apartamento));

            if (dataInicio.HasValue)
                query = query.Where(n => n.DataCriacao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(n => n.DataCriacao <= dataFim.Value);

            // ✅ Busca os resultados
            var notificacoes = await query
                .OrderByDescending(n => n.DataCriacao)
                .Select(n => new
                {
                    n.Id,
                    n.Titulo,
                    n.Mensagem,
                    n.Tipo,
                    n.Status,
                    n.DataCriacao,
                    n.UltimaAtualizacao,
                    Origem = n.MoradorOrigem != null ? new
                    {
                        n.MoradorOrigem.UsuarioId,
                        n.MoradorOrigem.Nome,
                        Bloco = n.MoradorOrigem.Apartamento != null ? n.MoradorOrigem.Apartamento.Bloco : null,
                        Apartamento = n.MoradorOrigem.Apartamento != null ? n.MoradorOrigem.Apartamento.Numero.ToString() : null
                    } : null
                })
                .ToListAsync();

            if (!notificacoes.Any())
                return NotFound(new { mensagem = "Nenhuma notificação encontrada." });

            return Ok(new
            {
                mensagem = "Notificações encontradas com sucesso.",
                total = notificacoes.Count,
                notificacoes
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao buscar notificações.", detalhes = ex.Message });
        }
    }


    /// ✅ Helper para pegar ID do usuário logado
    private int GetUsuarioIdDoToken()
    {
        return int.Parse(User.Claims.First(c => c.Type == "id").Value);
    }
}
