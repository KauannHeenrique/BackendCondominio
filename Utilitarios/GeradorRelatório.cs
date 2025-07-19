using condominio_API.Models;
using OfficeOpenXml;
using System.Globalization;
using System.Linq;

public static class GeradorRelatorio
{
    // ====================== USUÁRIOS ======================
    public static byte[] GerarRelatorioUsuarios(List<Usuario> usuarios)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Usuários");
        PreencherPlanilhaUsuarios(sheet, usuarios);
        return package.GetAsByteArray();
    }

    public static void PreencherPlanilhaUsuarios(ExcelWorksheet sheet, List<Usuario> usuarios)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "Nome";
        sheet.Cells[1, 3].Value = "Documento";
        sheet.Cells[1, 4].Value = "Email";
        sheet.Cells[1, 5].Value = "Nível de Acesso";
        sheet.Cells[1, 6].Value = "Telefone";
        sheet.Cells[1, 7].Value = "Status";
        sheet.Cells[1, 8].Value = "Data de Cadastro";

        for (int i = 0; i < usuarios.Count; i++)
        {
            var u = usuarios[i];
            sheet.Cells[i + 2, 1].Value = u.UsuarioId;
            sheet.Cells[i + 2, 2].Value = u.Nome;
            sheet.Cells[i + 2, 3].Value = u.Documento;
            sheet.Cells[i + 2, 4].Value = u.Email;
            sheet.Cells[i + 2, 5].Value = u.NivelAcesso.ToString();
            sheet.Cells[i + 2, 6].Value = u.Telefone;
            sheet.Cells[i + 2, 7].Value = u.Status ? "Ativo" : "Inativo";
            sheet.Cells[i + 2, 8].Value = u.DataCadastro.ToString("dd/MM/yyyy HH:mm", new CultureInfo("pt-BR"));
        }

        sheet.Cells.AutoFitColumns();
    }

    // ====================== VISITANTES ======================
    public static byte[] GerarRelatorioVisitantes(List<Visitante> visitantes)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Visitantes");
        PreencherPlanilhaVisitantes(sheet, visitantes);
        return package.GetAsByteArray();
    }

    public static void PreencherPlanilhaVisitantes(ExcelWorksheet sheet, List<Visitante> visitantes)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "Nome";
        sheet.Cells[1, 3].Value = "Documento";
        sheet.Cells[1, 4].Value = "Telefone";
        sheet.Cells[1, 5].Value = "Empresa";
        sheet.Cells[1, 6].Value = "CNPJ";

        for (int i = 0; i < visitantes.Count; i++)
        {
            var v = visitantes[i];
            sheet.Cells[i + 2, 1].Value = v.VisitanteId;
            sheet.Cells[i + 2, 2].Value = v.Nome;
            sheet.Cells[i + 2, 3].Value = v.Documento;
            sheet.Cells[i + 2, 4].Value = v.Telefone;
            sheet.Cells[i + 2, 5].Value = v.NomeEmpresa;
            sheet.Cells[i + 2, 6].Value = v.Cnpj;
        }

        sheet.Cells.AutoFitColumns();
    }

    // ====================== APARTAMENTOS ======================
    public static byte[] GerarRelatorioApartamentos(List<Apartamento> apartamentos)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Apartamentos");
        PreencherPlanilhaApartamentos(sheet, apartamentos);
        return package.GetAsByteArray();
    }

    public static void PreencherPlanilhaApartamentos(ExcelWorksheet sheet, List<Apartamento> apartamentos)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "Bloco";
        sheet.Cells[1, 3].Value = "Número";
        sheet.Cells[1, 4].Value = "Proprietário";
        sheet.Cells[1, 5].Value = "Situação";
        sheet.Cells[1, 6].Value = "Observações";

        for (int i = 0; i < apartamentos.Count; i++)
        {
            var a = apartamentos[i];
            sheet.Cells[i + 2, 1].Value = a.Id;
            sheet.Cells[i + 2, 2].Value = a.Bloco;
            sheet.Cells[i + 2, 3].Value = a.Numero;
            sheet.Cells[i + 2, 4].Value = a.Proprietario;
            sheet.Cells[i + 2, 5].Value = a.Situacao.ToString();
            sheet.Cells[i + 2, 6].Value = a.Observacoes;
        }

        sheet.Cells.AutoFitColumns();
    }

    // ====================== ENTRADAS MORADOR ======================
    public static byte[] GerarRelatorioEntradasMorador(List<AcessoEntradaMorador> entradas)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Entradas Morador");
        PreencherPlanilhaEntradasMorador(sheet, entradas);
        return package.GetAsByteArray();
    }

    public static void PreencherPlanilhaEntradasMorador(ExcelWorksheet sheet, List<AcessoEntradaMorador> entradas)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "Morador";
        sheet.Cells[1, 3].Value = "Data/Hora Entrada";
        sheet.Cells[1, 4].Value = "Entrada Por";
        sheet.Cells[1, 5].Value = "Observação";
        sheet.Cells[1, 6].Value = "Registrado Por";

        for (int i = 0; i < entradas.Count; i++)
        {
            var e = entradas[i];
            sheet.Cells[i + 2, 1].Value = e.Id;
            sheet.Cells[i + 2, 2].Value = e.Usuario?.Nome ?? "Desconhecido";
            sheet.Cells[i + 2, 3].Value = e.DataHoraEntrada.ToString("dd/MM/yyyy HH:mm");
            sheet.Cells[i + 2, 4].Value = e.EntradaPor == "1" ? "TAG" :
                                           e.EntradaPor == "2" ? "Manual" : "Desconhecido";
            sheet.Cells[i + 2, 5].Value = e.Observacao;
            sheet.Cells[i + 2, 6].Value = e.RegistradoPor;
        }

        sheet.Cells.AutoFitColumns();
    }

    // ====================== ENTRADAS VISITANTE ======================
    public static byte[] GerarRelatorioEntradasVisitante(List<AcessoEntradaVisitante> entradas)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Entradas Visitante");
        PreencherPlanilhaEntradasVisitante(sheet, entradas);
        return package.GetAsByteArray();
    }

    public static void PreencherPlanilhaEntradasVisitante(ExcelWorksheet sheet, List<AcessoEntradaVisitante> entradas)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "Visitante";
        sheet.Cells[1, 3].Value = "Documento";
        sheet.Cells[1, 4].Value = "Data/Hora Entrada";
        sheet.Cells[1, 5].Value = "Registrado Por";

        for (int i = 0; i < entradas.Count; i++)
        {
            var e = entradas[i];
            sheet.Cells[i + 2, 1].Value = e.Id;
            sheet.Cells[i + 2, 2].Value = e.Visitante?.Nome ?? "Desconhecido";
            sheet.Cells[i + 2, 3].Value = e.Visitante?.Documento;
            sheet.Cells[i + 2, 4].Value = e.DataHoraEntrada.ToString("dd/MM/yyyy HH:mm");
            sheet.Cells[i + 2, 5].Value = e.Usuario?.Nome ?? "Desconhecido";
        }

        sheet.Cells.AutoFitColumns();
    }

    // ====================== NOTIFICAÇÕES ======================
    public static byte[] GerarRelatorioNotificacoes(List<Notificacao> notificacoes)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Notificações");
        PreencherPlanilhaNotificacoes(sheet, notificacoes);
        return package.GetAsByteArray();
    }

    public static void PreencherPlanilhaNotificacoes(ExcelWorksheet sheet, List<Notificacao> notificacoes)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "Título";
        sheet.Cells[1, 3].Value = "Tipo";
        sheet.Cells[1, 4].Value = "Mensagem";
        sheet.Cells[1, 5].Value = "Status";
        sheet.Cells[1, 6].Value = "Data Criação";
        sheet.Cells[1, 7].Value = "Origem";
        sheet.Cells[1, 8].Value = "Destinos";

        for (int i = 0; i < notificacoes.Count; i++)
        {
            var n = notificacoes[i];
            sheet.Cells[i + 2, 1].Value = n.Id;
            sheet.Cells[i + 2, 2].Value = n.Titulo;
            sheet.Cells[i + 2, 3].Value = n.Tipo.ToString();
            sheet.Cells[i + 2, 4].Value = n.Mensagem;
            sheet.Cells[i + 2, 5].Value = n.Status.ToString();
            sheet.Cells[i + 2, 6].Value = n.DataCriacao.ToString("dd/MM/yyyy HH:mm");
            sheet.Cells[i + 2, 7].Value = n.MoradorOrigem?.Nome ?? "Desconhecido";

            // ✅ Monta a lista de destinos com Bloco-Número
            var destinos = n.Destinatarios
                .Where(d => d.UsuarioDestino?.Apartamento != null)
                .Select(d => $"{d.UsuarioDestino.Apartamento.Bloco}-{d.UsuarioDestino.Apartamento.Numero}")
                .ToList();

            sheet.Cells[i + 2, 8].Value = destinos.Any() ? string.Join(", ", destinos) : "-";
        }

        sheet.Cells.AutoFitColumns();
    }
}
