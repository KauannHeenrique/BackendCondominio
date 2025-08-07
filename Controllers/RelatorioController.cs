using condominio_API.Services;
using Condominio_API.Requests;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace condominio_API.Controllers
{
    [ApiController]
    [Route("api/relatorios")]
    public class RelatorioController : ControllerBase
    {
        private readonly RelatorioService _relatorioService;

        public RelatorioController(RelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> GerarRelatorioUsuarios()
        {
            var dados = await _relatorioService.ObterUsuariosParaRelatorio();
            var arquivo = GeradorRelatorio.GerarRelatorioUsuarios(dados);

            return File(arquivo,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Relatorio_Usuarios_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet("visitantes")]
        public async Task<IActionResult> GerarRelatorioVisitantes()
        {
            var dados = await _relatorioService.ObterVisitantesParaRelatorio();
            var arquivo = GeradorRelatorio.GerarRelatorioVisitantes(dados);

            return File(arquivo,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Relatorio_Visitantes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet("apartamentos")]
        public async Task<IActionResult> GerarRelatorioApartamentos()
        {
            var dados = await _relatorioService.ObterApartamentosParaRelatorio();
            var arquivo = GeradorRelatorio.GerarRelatorioApartamentos(dados);

            return File(arquivo,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Relatorio_Apartamentos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet("entradas-morador")]
        public async Task<IActionResult> GerarRelatorioEntradasMorador()
        {
            var dados = await _relatorioService.ObterEntradasMoradorParaRelatorio();
            var arquivo = GeradorRelatorio.GerarRelatorioEntradasMorador(dados);

            return File(arquivo,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Relatorio_EntradasMorador_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet("entradas-visitante")]
        public async Task<IActionResult> GerarRelatorioEntradasVisitante()
        {
            var dados = await _relatorioService.ObterEntradasVisitanteParaRelatorio();
            var arquivo = GeradorRelatorio.GerarRelatorioEntradasVisitante(dados);

            return File(arquivo,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Relatorio_EntradasVisitante_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet("notificacoes-completo")]
        public async Task<IActionResult> GerarRelatorioNotificacoesCompleto()
        {
            using var package = new ExcelPackage();

            // 🟢 Aba 1 - Notificações
            var notificacoes = await _relatorioService.ObterNotificacoesParaRelatorio();
            var abaNotificacoes = package.Workbook.Worksheets.Add("Notificações");
            GeradorRelatorio.PreencherPlanilhaNotificacoes(abaNotificacoes, notificacoes);

            // 🟡 Aba 2 - Destinatários
            var destinatarios = await _relatorioService.ObterDestinatariosAba();
            var abaDestinatarios = package.Workbook.Worksheets.Add("Destinatários");
            GeradorRelatorio.PreencherPlanilhaDestinatarios(abaDestinatarios, destinatarios);

            // 🔵 Aba 3 - Histórico
            var historico = await _relatorioService.ObterHistoricoAba();
            var abaHistorico = package.Workbook.Worksheets.Add("Histórico");
            GeradorRelatorio.PreencherPlanilhaHistorico(abaHistorico, historico);

            var arquivo = package.GetAsByteArray();

            return File(arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Relatorio_NotificacoesCompleto_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }


        [HttpPost("multiplos")]
        public async Task<IActionResult> GerarRelatoriosSelecionados([FromBody] RelatorioMultiploRequest selecao)
        {
            using var package = new ExcelPackage();

            if (selecao.Usuarios)
            {
                var dados = await _relatorioService.ObterUsuariosParaRelatorio();
                var planilha = package.Workbook.Worksheets.Add("Usuários");
                GeradorRelatorio.PreencherPlanilhaUsuarios(planilha, dados);
            }

            if (selecao.Visitantes)
            {
                var dados = await _relatorioService.ObterVisitantesParaRelatorio();
                var planilha = package.Workbook.Worksheets.Add("Visitantes");
                GeradorRelatorio.PreencherPlanilhaVisitantes(planilha, dados);
            }

            if (selecao.Apartamentos)
            {
                var dados = await _relatorioService.ObterApartamentosParaRelatorio();
                var planilha = package.Workbook.Worksheets.Add("Apartamentos");
                GeradorRelatorio.PreencherPlanilhaApartamentos(planilha, dados);
            }

            if (selecao.EntradasMorador)
            {
                var dados = await _relatorioService.ObterEntradasMoradorParaRelatorio();
                var planilha = package.Workbook.Worksheets.Add("Entradas Usuario");
                GeradorRelatorio.PreencherPlanilhaEntradasMorador(planilha, dados);
            }

            if (selecao.EntradasVisitante)
            {
                var dados = await _relatorioService.ObterEntradasVisitanteParaRelatorio();
                var planilha = package.Workbook.Worksheets.Add("Entradas Visitante");
                GeradorRelatorio.PreencherPlanilhaEntradasVisitante(planilha, dados);
            }

            if (selecao.Notificacoes)
            {
                var dados = await _relatorioService.ObterNotificacoesParaRelatorio();
                var planilha = package.Workbook.Worksheets.Add("Notificações");
                GeradorRelatorio.PreencherPlanilhaNotificacoes(planilha, dados);
            }

            var arquivo = package.GetAsByteArray();

            return File(arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Relatorios_Selecionados_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
    }
}
