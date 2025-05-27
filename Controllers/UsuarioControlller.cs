using condominio_API.Data;
using condominio_API.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using condominio_API.Services;
using Condominio_API.Requests;
using Microsoft.IdentityModel.Tokens;
namespace condominio_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ExibirTodosUsuarios")]

        public async Task<ActionResult<IEnumerable<Usuario>>> GetTodosUsuarios()
        {
            return await _context.Usuarios.Include(user => user.Apartamento).ToListAsync();
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Documento == request.CPF);

            if (usuario == null || !HashHelper.VerificarHash(usuario.Senha, request.Senha))
            {
                return Unauthorized(new { mensagem = "Email ou senha inválidos." });
            }

            var newToken = new TokenService();

            var token = newToken.GerarJwtToken(usuario);
            return Ok(new
            {
                token,
                usuario = new
                {
                    usuario.UsuarioId,
                    usuario.Nome,
                    usuario.Documento,
                    usuario.NivelAcesso
                }
            });
        }

        [HttpGet("BuscarUsuarioPor")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuario([FromQuery] string? nomeUsuario, [FromQuery] string? documento, [FromQuery]  string? emailUsuario)
        {
            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nomeUsuario))
            {
                query = query.Where(user => user.Nome.Contains(nomeUsuario));
            }

            if (!string.IsNullOrWhiteSpace(documento))
            {
                query = query.Where(user => user.Documento.Contains(documento));
            }

            if (!string.IsNullOrWhiteSpace(emailUsuario))
            {
                query = query.Where(user => user.Email.Contains(emailUsuario));
            }

            var usuarios = await query.ToListAsync();

            if (usuarios.Count == 0)
            {
                return NotFound(new { mensagem = "Nenhum usuário encontrado." });
            }

            return Ok(usuarios);
        }

        [HttpPost("AdicionarUsuario")]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario novoUsuario)
        {
            try
            {
                if (novoUsuario == null)
                {
                        return BadRequest(new { mensagem = "Por favor, preencha todos os campos" });
                    }

                    var moradorFirst = await _context.Usuarios.FirstOrDefaultAsync(user => user.Documento == novoUsuario.Documento
                   || user.Email == novoUsuario.Email);

                    if (moradorFirst != null)
                    {
                        return BadRequest(new { mensagem = "Documento ou e-mail já cadastrado. Por favor, tente novamente." });
                    }

                    if (((int)novoUsuario.NivelAcesso == 2 || (int)novoUsuario.NivelAcesso == 4) && novoUsuario.ApartamentoId <= 0)
                    {
                        return BadRequest(new { mensagem = "Síndicos e moradores devem ter um apartamento válido." });
                    }

                    var usuarioRetornado = new  // usado pra retornar na tela os dados "required"(?)
                    {
                        novoUsuario.UsuarioId,
                        novoUsuario.Nome,
                        novoUsuario.Email,
                        novoUsuario.NivelAcesso,
                        novoUsuario.ApartamentoId
                    };

                novoUsuario.Senha = HashHelper.GerarHash("Condominio123");

                _context.Usuarios.Add(novoUsuario);
                await _context.SaveChangesAsync();

                return Ok(new { mensagem = "Usuário cadastrado com sucesso!", usuarioRetornado });
                }

            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao cadastrar usuário!", detalhes = ex.Message });
            }
        }

        [HttpPut("AtualizarUsuario/{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] Usuario usuario)
        {
            if (id <= 0)
            {
                return BadRequest("Usuário inválido.");
            }

            var usuarioTemp = await _context.Usuarios.FindAsync(id);

            if (usuarioTemp == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(usuario.Nome) && usuario.Nome != "string" && usuarioTemp.Nome != usuario.Nome)
            {
                usuarioTemp.Nome = usuario.Nome;
                _context.Entry(usuarioTemp).Property(user => user.Nome).IsModified = true;
            }

            if (!string.IsNullOrEmpty(usuario.Documento) && usuario.Documento != "string" && usuarioTemp.Documento != usuario.Documento)
            {
                usuarioTemp.Documento = usuario.Documento;
                _context.Entry(usuarioTemp).Property(user => user.Documento).IsModified = true;
            }

            if (!string.IsNullOrEmpty(usuario.Email) && usuario.Email != "string" && usuarioTemp.Email != usuario.Email)
            {
                usuarioTemp.Email = usuario.Email;
                _context.Entry(usuarioTemp).Property(user => user.Email).IsModified = true;
            }

            if (!string.IsNullOrEmpty(usuario.Telefone) && usuario.Telefone != "string" && usuarioTemp.Telefone != usuario.Telefone)
            {
                usuarioTemp.Telefone = usuario.Telefone;
                _context.Entry(usuarioTemp).Property(user => user.Telefone).IsModified = true;
            }

            if (usuario.ApartamentoId > 0 && usuarioTemp.ApartamentoId != usuario.ApartamentoId)
            {
                usuarioTemp.ApartamentoId = usuario.ApartamentoId;
                _context.Entry(usuarioTemp).Property(user => user.ApartamentoId).IsModified = true;
            }

            if (!string.IsNullOrEmpty(usuario.CodigoRFID) && usuario.CodigoRFID != "string" && usuarioTemp.CodigoRFID != usuario.CodigoRFID)
            {
                usuarioTemp.CodigoRFID = usuario.CodigoRFID;
                _context.Entry(usuarioTemp).Property(user => user.CodigoRFID).IsModified = true;
            }

            if (usuarioTemp.Status != usuario.Status)
            {
                usuarioTemp.Status = usuario.Status;
                _context.Entry(usuarioTemp).Property(user => user.Status).IsModified = true;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("ResetarSenha/{id}")]
        public async Task<IActionResult> ResetarSenha(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Usuário inválido.");
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            // senha padrão do condominio
            usuario.Senha = HashHelper.GerarHash("Condominio123");

            _context.Entry(usuario).Property(u => u.Senha).IsModified = true;
            await _context.SaveChangesAsync();

            return Ok("Senha resetada com sucesso.");
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
