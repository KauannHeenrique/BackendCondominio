using condominio_API.Data;
using condominio_API.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using condominio_API.Services;
using Condominio_API.Requests;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Crypto.Generators;


namespace condominio_API.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public UsuarioController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        [HttpGet("ExibirTodosUsuarios")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetTodosUsuarios()
        {
            return await _context.Usuarios
                .Include(user => user.Apartamento)
                .Where(u => u.NivelAcesso != (NivelAcessoEnum)1) // Remove Administrador
                .ToListAsync();
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Documento == request.CPF);

            if (usuario == null || !HashHelper.VerificarHash(usuario.Senha, request.Senha))
            {
                return Unauthorized(new { mensagem = "CPF ou senha inválidos." });
            }

            var token = new TokenService().GerarJwtToken(usuario);

            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // ok para dev, mas use true em produção com HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(1),
                Path = "/" // este é o valor padrão, mas vale explicitar
            });

            if (usuario.IsTemporaryPassword)
            {
                return Ok(new
                {
                    mensagem = "Por favor, altere sua senha.",
                    redirectTo = "/changePassword"
                });
            }

            return Ok(new { mensagem = "Login realizado com sucesso." });
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth_token", new CookieOptions
            {
                HttpOnly = true, // não obrigatório aqui, mas não atrapalha
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/" // MESMO PATH usado na criação
            });

            return Ok(new { mensagem = "Logout realizado com sucesso." });
        }




        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> GetPerfil()
        {
            var userId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var usuario = await _context.Usuarios
                .Include(u => u.Apartamento)
                .FirstOrDefaultAsync(u => u.UsuarioId == int.Parse(userId));

            if (usuario == null) return NotFound();

            return Ok(new
            {
                usuario.UsuarioId,
                usuario.Documento,
                usuario.NivelAcesso,
                usuario.Nome,
                usuario.Email,
                usuario.ApartamentoId,
                usuario.Telefone,
                Bloco = usuario.Apartamento?.Bloco,
                Apartamento = usuario.Apartamento?.Numero,
                usuario.DataCadastro
            });
        }



        [HttpGet("BuscarUsuarioPor")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuario(
    [FromQuery] int? id,
    [FromQuery] string? nome,
    [FromQuery] string? documento,
    [FromQuery] string? bloco,
    [FromQuery] string? apartamento,
    [FromQuery] int? nivelAcesso,
    [FromQuery] bool? status)
        {
            var query = _context.Usuarios
            .Include(u => u.Apartamento)
            .Where(u => u.NivelAcesso != (NivelAcessoEnum)1 ) // Excluir administrador
            .AsQueryable();

            if (id.HasValue)
                query = query.Where(user => user.UsuarioId == id.Value);

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(user => user.Nome.Contains(nome));

            if (!string.IsNullOrWhiteSpace(documento))
                query = query.Where(user => user.Documento.Contains(documento));

            if (!string.IsNullOrWhiteSpace(bloco))
                query = query.Where(user => user.Apartamento != null && user.Apartamento.Bloco.Contains(bloco));

            if (!string.IsNullOrWhiteSpace(apartamento))
                query = query.Where(user => user.Apartamento != null && user.Apartamento.Numero.ToString().Contains(apartamento));

            if (nivelAcesso.HasValue)
            {
                var nivelEnum = (NivelAcessoEnum)nivelAcesso.Value;
                query = query.Where(user => user.NivelAcesso == nivelEnum);
            }

            if (status.HasValue)
                query = query.Where(user => user.Status == status.Value);

            var usuarios = await query.ToListAsync();

            if (usuarios.Count == 0)
                return NotFound(new { mensagem = "Nenhum usuário encontrado." });

            return Ok(usuarios);
        }

        public static bool IsValidCPF(string cpf)
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
            if (remainder != numbers[9])
                return false;

            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += numbers[i] * (11 - i);
            remainder = (sum * 10) % 11;
            if (remainder == 10) remainder = 0;
            if (remainder != numbers[10])
                return false;

            return true;
        }

        [HttpPost("CadastrarUsuario")]
        public async Task<IActionResult> CadastrarUsuario([FromBody] Usuario novoUsuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(novoUsuario.Email))
                {
                    return BadRequest(new { mensagem = "É necessário informar um e-mail válido." });
                }

                if (string.IsNullOrWhiteSpace(novoUsuario.Documento))
                {
                    return BadRequest(new { mensagem = "É necessário informar um CPF." });
                }

                // Normaliza e-mail e documento
                var emailNormalizado = novoUsuario.Email.Trim().ToLower();
                var documentoNormalizado = new string(novoUsuario.Documento.Where(char.IsDigit).ToArray());

                // ✅ Verifica se CPF tem 11 dígitos
                if (documentoNormalizado.Length != 11)
                {
                    return BadRequest(new { mensagem = "CPF deve conter 11 dígitos." });
                }

                // ✅ Valida CPF
                if (!ValidarCPF(documentoNormalizado))
                {
                    return BadRequest(new { mensagem = "CPF inválido." });
                }

                // Verifica se o e-mail já está em uso
                var emailExistente = _context.Usuarios.Any(u => u.Email.ToLower() == emailNormalizado);
                if (emailExistente)
                {
                    return BadRequest(new { mensagem = "E-mail já cadastrado." });
                }

                // Verifica se o documento (CPF) já está em uso
                var documentoExistente = _context.Usuarios.Any(u => u.Documento == documentoNormalizado);
                if (documentoExistente)
                {
                    return BadRequest(new { mensagem = "Documento (CPF) já cadastrado." });
                }

                // Define senha padrão
                string senhaPadrao = "Condominio123";
                novoUsuario.Senha = HashHelper.GerarHash(senhaPadrao);
                novoUsuario.IsTemporaryPassword = true;
                novoUsuario.DataCadastro = DateTime.UtcNow;
                novoUsuario.Documento = documentoNormalizado; // ✅ Salva CPF sem máscara
                novoUsuario.Email = emailNormalizado; // ✅ Salva email normalizado

                _context.Usuarios.Add(novoUsuario);
                await _context.SaveChangesAsync();

                // Envia e-mail de boas-vindas de forma assíncrona
                _ = Task.Run(() => _emailService.SendWelcomeEmailAsync(novoUsuario.Email, senhaPadrao));

                return Ok(new { mensagem = "Usuário cadastrado com sucesso." });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { mensagem = "Erro: dados obrigatórios não informados corretamente.", detalhes = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao cadastrar usuário ou enviar e-mail.", detalhes = ex.Message });
            }
        }

        /// <summary>
        /// Valida CPF pelo cálculo dos dígitos verificadores.
        /// </summary>
        private bool ValidarCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            // Remove caracteres não numéricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11) return false;

            // Verifica se todos os dígitos são iguais
            if (cpf.All(c => c == cpf[0])) return false;

            // Calcula dígito 1
            int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * mult1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            string digito = resto.ToString();
            tempCpf += digito;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * mult2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }




        [HttpPut("AtualizarUsuario/{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] AtualizarUsuarioRequest usuario)
        {
            if (id <= 0)
                return BadRequest("Usuário inválido.");

            var usuarioTemp = await _context.Usuarios.FindAsync(id);
            if (usuarioTemp == null)
                return NotFound(new { mensagem = "Usuário não encontrado." });

            // ✅ Verificar duplicidade de CPF
            if (!string.IsNullOrEmpty(usuario.Documento) && usuario.Documento != "string" && usuarioTemp.Documento != usuario.Documento)
            {
                bool cpfExistente = await _context.Usuarios
                    .AnyAsync(u => u.Documento == usuario.Documento && u.UsuarioId != id);

                if (cpfExistente)
                    return BadRequest(new { mensagem = "Já existe um usuário cadastrado com este CPF." });
            }

            // ✅ Verificar duplicidade de Email
            if (!string.IsNullOrEmpty(usuario.Email) && usuario.Email != "string" && usuarioTemp.Email != usuario.Email)
            {
                bool emailExistente = await _context.Usuarios
                    .AnyAsync(u => u.Email == usuario.Email && u.UsuarioId != id);

                if (emailExistente)
                    return BadRequest(new { mensagem = "Já existe um usuário cadastrado com este e-mail." });
            }

            // ✅ Nome
            if (!string.IsNullOrEmpty(usuario.Nome) && usuario.Nome != "string" && usuarioTemp.Nome != usuario.Nome)
            {
                usuarioTemp.Nome = usuario.Nome;
                _context.Entry(usuarioTemp).Property(u => u.Nome).IsModified = true;
            }

            // ✅ Documento
            if (!string.IsNullOrEmpty(usuario.Documento) && usuario.Documento != "string" && usuarioTemp.Documento != usuario.Documento)
            {
                usuarioTemp.Documento = usuario.Documento;
                _context.Entry(usuarioTemp).Property(u => u.Documento).IsModified = true;
            }

            // ✅ Email
            if (!string.IsNullOrEmpty(usuario.Email) && usuario.Email != "string" && usuarioTemp.Email != usuario.Email)
            {
                usuarioTemp.Email = usuario.Email;
                _context.Entry(usuarioTemp).Property(u => u.Email).IsModified = true;
            }

            // ✅ Telefone
            if (!string.IsNullOrEmpty(usuario.Telefone) && usuario.Telefone != "string" && usuarioTemp.Telefone != usuario.Telefone)
            {
                usuarioTemp.Telefone = usuario.Telefone;
                _context.Entry(usuarioTemp).Property(u => u.Telefone).IsModified = true;
            }

            // ✅ Nível de Acesso
            if (usuario.NivelAcesso.HasValue && usuarioTemp.NivelAcesso != usuario.NivelAcesso.Value)
            {
                usuarioTemp.NivelAcesso = usuario.NivelAcesso.Value;
                _context.Entry(usuarioTemp).Property(u => u.NivelAcesso).IsModified = true;

                if (usuario.NivelAcesso.Value == NivelAcessoEnum.Funcionario)
                {
                    usuarioTemp.ApartamentoId = null;
                    _context.Entry(usuarioTemp).Property(u => u.ApartamentoId).IsModified = true;
                }
            }

            // ✅ Validar e atualizar apartamento conforme novo nível de acesso
            if (usuario.NivelAcesso.HasValue)
            {
                if (usuario.NivelAcesso == NivelAcessoEnum.Funcionario)
                {
                    // Funcionário → remove vínculo com apartamento
                    usuarioTemp.ApartamentoId = null;
                    _context.Entry(usuarioTemp).Property(u => u.ApartamentoId).IsModified = true;
                }
                else if (usuario.NivelAcesso == NivelAcessoEnum.Morador || usuario.NivelAcesso == NivelAcessoEnum.Sindico)
                {
                    // Morador/Síndico → precisa ter apartamento válido
                    if (!usuario.ApartamentoId.HasValue)
                    {
                        return BadRequest(new { mensagem = "Moradores e Síndicos precisam ter um apartamento associado." });
                    }

                    if (usuarioTemp.ApartamentoId != usuario.ApartamentoId)
                    {
                        usuarioTemp.ApartamentoId = usuario.ApartamentoId;
                        _context.Entry(usuarioTemp).Property(u => u.ApartamentoId).IsModified = true;
                    }
                }
            }


            // ✅ Código RFID
            if (!string.IsNullOrEmpty(usuario.CodigoRFID) && usuario.CodigoRFID != "string" && usuarioTemp.CodigoRFID != usuario.CodigoRFID)
            {
                usuarioTemp.CodigoRFID = usuario.CodigoRFID;
                _context.Entry(usuarioTemp).Property(u => u.CodigoRFID).IsModified = true;
            }

            // ✅ Status
            if (usuarioTemp.Status != usuario.Status)
            {
                usuarioTemp.Status = usuario.Status;
                _context.Entry(usuarioTemp).Property(u => u.Status).IsModified = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Usuário atualizado com sucesso." });
        }



        [HttpGet("BuscarPorRFID")]  // rota de verificar se a tag esta cadastrada
        public async Task<IActionResult> BuscarPorRFID([FromQuery] string rfid)
        {
            if (string.IsNullOrWhiteSpace(rfid))
            {
                return BadRequest(new { mensagem = "Código RFID inválido." });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.CodigoRFID == rfid);

            if (usuario == null)
            {
                return Ok(new { authorized = false });
            }

            return Ok(new { authorized = true });
        }

        [Authorize]
        [HttpPut("AlterarSenha")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request)
        {
            if (string.IsNullOrEmpty(request.SenhaAtual) || string.IsNullOrEmpty(request.NovaSenha))
            {
                return BadRequest(new { mensagem = "Preencha todos os campos." });
            }

            var userId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var usuario = await _context.Usuarios.FindAsync(int.Parse(userId));

            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado." });
            }

            // ve se a senha atual ta certa (comparacao de hasheada com hasheada)
            if (!HashHelper.VerificarHash(usuario.Senha, request.SenhaAtual))
            {
                return BadRequest(new { mensagem = "Senha atual incorreta." });
            }

            // altera a senha
            usuario.Senha = HashHelper.GerarHash(request.NovaSenha);
            usuario.IsTemporaryPassword = false;

            _context.Entry(usuario).Property(u => u.Senha).IsModified = true;
            _context.Entry(usuario).Property(u => u.IsTemporaryPassword).IsModified = true;

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Senha alterada com sucesso." });
        }


        [HttpPut("ResetarSenha/{UsuarioId}")]
        public async Task<IActionResult> ResetarSenha(int UsuarioId)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(UsuarioId);

                if (usuario == null)
                {
                    Console.WriteLine($"[ERRO] Usuário com ID {UsuarioId} não encontrado.");
                    return NotFound(new { mensagem = "Usuário não encontrado." });
                }

                Console.WriteLine($"[INFO] Usuário encontrado: {usuario.Nome}");
                Console.WriteLine($"[INFO] Email encontrado: {usuario.Email}");

                if (string.IsNullOrWhiteSpace(usuario.Email))
                {
                    Console.WriteLine("[ERRO] Campo de e-mail está vazio ou nulo.");
                    return BadRequest(new { mensagem = "Este usuário não possui e-mail cadastrado." });
                }

                string novaSenha = "Condominio123";
                usuario.Senha = HashHelper.GerarHash(novaSenha);
                usuario.IsTemporaryPassword = true;

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();


                _ = Task.Run(() => _emailService.SendResetPasswordEmailAsync(usuario.Email, novaSenha));



                return Ok(new { mensagem = "Senha redefinida e e-mail enviado com sucesso." });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { mensagem = "Erro: e-mail não informado corretamente.", detalhes = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao redefinir senha ou enviar e-mail.", detalhes = ex.Message });
            }
        }

        [HttpDelete("ExcluirUsuario/{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Usuário inválido.");
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
