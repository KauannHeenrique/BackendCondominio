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
    public class VisitanteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VisitanteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ExibirTodosVisitantes")]
        public async Task<ActionResult<IEnumerable<Visitante>>> GetTodosVisitantes()
        {
            return await _context.Visitantes.ToListAsync();
        }

        [HttpGet("BuscarVisitantePor")]
        public async Task<ActionResult<IEnumerable<Visitante>>> GetVisitante(
    [FromQuery] int? id,
    [FromQuery] string? nomeVisitante,
    [FromQuery] string? nomeEmpresa,
    [FromQuery] string? documento,
    [FromQuery] string? cnpj,
    [FromQuery] string? telefone,
    [FromQuery] bool? status,
    [FromQuery] bool? prestadorServico)
        {
            // Se ID for informado, ignora os outros filtros e retorna apenas esse visitante
            if (id.HasValue)
            {
                var visitante = await _context.Visitantes.FirstOrDefaultAsync(v => v.VisitanteId == id.Value);
                if (visitante == null)
                    return NotFound(new { mensagem = "Visitante não encontrado." });

                return Ok(visitante); // Retorna apenas um objeto
            }

            var query = _context.Visitantes.AsQueryable();

            // Buscar por nome do visitante
            if (!string.IsNullOrWhiteSpace(nomeVisitante))
            {
                query = query.Where(v => v.Nome.Contains(nomeVisitante));
            }

            // Buscar por nome da empresa
            if (!string.IsNullOrWhiteSpace(nomeEmpresa))
            {
                query = query.Where(v => v.NomeEmpresa.Contains(nomeEmpresa));
            }

            // Buscar por CPF ou CNPJ usando "documento"
            if (!string.IsNullOrWhiteSpace(documento))
            {
                var docLimpo = new string(documento.Where(char.IsDigit).ToArray());

                if (docLimpo.Length == 11)
                {
                    // CPF
                    query = query.Where(v => v.Documento == docLimpo);
                }
                else if (docLimpo.Length == 14)
                {
                    // CNPJ
                    query = query.Where(v => v.Cnpj == docLimpo);
                }
                else
                {
                    return BadRequest(new { mensagem = "Documento inválido! Informe um CPF (11 dígitos) ou CNPJ (14 dígitos)." });
                }
            }

            // Buscar por CNPJ explicitamente
            if (!string.IsNullOrWhiteSpace(cnpj))
            {
                var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());
                query = query.Where(v => v.Cnpj == cnpjLimpo);
            }

            // Buscar por telefone
            if (!string.IsNullOrWhiteSpace(telefone))
            {
                query = query.Where(v => v.Telefone.Contains(telefone));
            }

            // Status
            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }

            // Prestador de serviço
            if (prestadorServico.HasValue)
            {
                query = query.Where(v => v.PrestadorServico == prestadorServico.Value);
            }

            var visitantes = await query.ToListAsync();

            if (!visitantes.Any())
            {
                return NotFound(new { mensagem = "Nenhum visitante encontrado." });
            }

            return Ok(visitantes);
        }

        private static bool IsValidCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
                return false;

            var numbers = cpf.Select(c => int.Parse(c.ToString())).ToArray();

            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += numbers[i] * (10 - i);
            int remainder = (sum * 10) % 11;
            if (remainder == 10) remainder = 0;
            if (remainder != numbers[9]) return false;

            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += numbers[i] * (11 - i);
            remainder = (sum * 10) % 11;
            if (remainder == 10) remainder = 0;
            if (remainder != numbers[10]) return false;

            return true;
        }

        private static bool IsValidCNPJ(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

            if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
                return false;

            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int sum = 0;

            for (int i = 0; i < 12; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int remainder = (sum % 11);
            remainder = remainder < 2 ? 0 : 11 - remainder;

            string digit = remainder.ToString();
            tempCnpj += digit;
            sum = 0;

            for (int i = 0; i < 13; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            remainder = (sum % 11);
            remainder = remainder < 2 ? 0 : 11 - remainder;

            digit += remainder.ToString();

            return cnpj.EndsWith(digit);
        }

        [HttpPost("CadastrarVisitante")]
        public async Task<ActionResult<Visitante>> PostVisitante(Visitante NovoVisitante)
        {
            try
            {
                if (string.IsNullOrEmpty(NovoVisitante.Nome) || NovoVisitante.Nome == "string" ||
                    string.IsNullOrEmpty(NovoVisitante.Documento) || NovoVisitante.Documento == "string" ||
                    string.IsNullOrEmpty(NovoVisitante.Telefone) || NovoVisitante.Telefone == "string")
                {
                    return BadRequest(new { mensagem = "Nome, Documento e Telefone são obrigatórios!" });
                }

                string documentoNumeros = new string(NovoVisitante.Documento.Where(char.IsDigit).ToArray());

                bool documentoValido =
                    documentoNumeros.Length == 11 ? IsValidCPF(documentoNumeros) :
                    documentoNumeros.Length == 14 ? IsValidCNPJ(documentoNumeros) :
                    false;

                if (!documentoValido)
                {
                    return BadRequest(new { mensagem = "CPF ou CNPJ inválido!" });
                }

                var visitanteExistente = await _context.Visitantes.FirstOrDefaultAsync(
                    visit => visit.Documento == NovoVisitante.Documento && visit.Cnpj == NovoVisitante.Cnpj);

                if (visitanteExistente != null)
                {
                    return Ok(new
                    {
                        mensagem = "Visitante já cadastrado. Deseja gerar um QR code para esta visita?",
                        visitante = visitanteExistente,
                        gerarQrCode = true
                    });
                }

                _context.Visitantes.Add(NovoVisitante);
                await _context.SaveChangesAsync();

                return Ok(new { mensagem = "Visitante cadastrado com sucesso", NovoVisitante });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao cadastrar visitante!", detalhes = ex.Message });
            }
        }

        [HttpPut("AtualizarVisitante/{id}")]
        public async Task<IActionResult> PutVisitante(int id, [FromBody] Visitante visitante)
        {
            if (id <= 0)
            {
                return BadRequest("Visitante inválido.");
            }

            var visitanteTemp = await _context.Visitantes.FindAsync(id);

            if (visitanteTemp == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(visitante.Nome) && visitante.Nome != "string" && visitanteTemp.Nome != visitante.Nome)
            {
                visitanteTemp.Nome = visitante.Nome;
                _context.Entry(visitanteTemp).Property(v => v.Nome).IsModified = true;
            }

            if (!string.IsNullOrEmpty(visitante.Documento) && visitante.Documento != "string" && visitanteTemp.Documento != visitante.Documento)
            {
                visitanteTemp.Documento = visitante.Documento;
                _context.Entry(visitanteTemp).Property(v => v.Documento).IsModified = true;
            }

            if (!string.IsNullOrEmpty(visitante.Telefone) && visitante.Telefone != "string" && visitanteTemp.Telefone != visitante.Telefone)
            {
                visitanteTemp.Telefone = visitante.Telefone;
                _context.Entry(visitanteTemp).Property(v => v.Telefone).IsModified = true;
            }

            if (visitante.Status != visitanteTemp.Status)
            {
                visitanteTemp.Status = visitante.Status;
                _context.Entry(visitanteTemp).Property(v => v.Status).IsModified = true;
            }


            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("ExcluirVisitante/{id}")]
        public async Task<IActionResult> DeletarVisitante(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Visitante inválido.");
            }

            var visitante = await _context.Visitantes.FindAsync(id);
            if (visitante == null)
            {
                return NotFound();
            }

            _context.Visitantes.Remove(visitante);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}