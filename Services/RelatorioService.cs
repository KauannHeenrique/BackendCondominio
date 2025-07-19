using condominio_API.Data;
using condominio_API.Models;
using Microsoft.EntityFrameworkCore;

namespace condominio_API.Services
{
    public class RelatorioService
    {
        private readonly AppDbContext _context;

        public RelatorioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> ObterUsuariosParaRelatorio()
        {
            return await _context.Usuarios
                .Include(u => u.Apartamento)
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }

        public async Task<List<Visitante>> ObterVisitantesParaRelatorio()
        {
            return await _context.Visitantes.OrderBy(v => v.Nome).ToListAsync();
        }

        public async Task<List<Apartamento>> ObterApartamentosParaRelatorio()
        {
            return await _context.Apartamentos
                .OrderBy(a => a.Bloco)
                .ThenBy(a => a.Numero)
                .ToListAsync();
        }

        public async Task<List<AcessoEntradaMorador>> ObterEntradasMoradorParaRelatorio()
        {
            return await _context.AcessoEntradaMoradores
                .Include(e => e.Usuario)
                .OrderByDescending(e => e.DataHoraEntrada)
                .ToListAsync();
        }

        public async Task<List<AcessoEntradaVisitante>> ObterEntradasVisitanteParaRelatorio()
        {
            return await _context.AcessoEntradaVisitantes
                .Include(e => e.Visitante)
                .Include(e => e.Usuario)
                .OrderByDescending(e => e.DataHoraEntrada)
                .ToListAsync();
        }

        public async Task<List<Notificacao>> ObterNotificacoesParaRelatorio()
        {
            return await _context.Notificacoes
                .Include(n => n.MoradorOrigem)
                .Include(n => n.Destinatarios)
                    .ThenInclude(d => d.UsuarioDestino)
                        .ThenInclude(u => u.Apartamento)
                .OrderByDescending(n => n.DataCriacao)
                .ToListAsync();
        }

    }
}
