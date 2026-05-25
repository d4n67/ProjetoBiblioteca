using LaeleMilVeis.Models;
using LaeleMilVeis.Services;
using Microsoft.AspNetCore.Mvc;

namespace LaeleMilVeis.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Rota REST padrão: /api/usuarios
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuariosController(UsuarioService service)
        {
            _service = service;
        }

        [HttpGet] // GET /api/usuarios
        public async Task<IActionResult> ObterTodos()
        {
            var usuarios = await _service.ListarTodosUsuariosAsync();
            return Ok(usuarios); // Retorna HTTP 200
        }

        [HttpGet("{id}")] // GET /api/usuarios/{id}
        public async Task<IActionResult> ObterPorId(string id)
        {
            var usuario = await _service.BuscarPorIdAsync(id);
            if (usuario == null) return NotFound(new { mensagem = "Usuário não encontrado." }); // Retorna HTTP 404

            return Ok(usuario);
        }

        [HttpPost] // POST /api/usuarios
        public async Task<IActionResult> Criar([FromBody] Usuario novoUsuario)
        {
            try
            {
                var usuarioCriado = await _service.CadastrarUsuarioAsync(novoUsuario);
                // Retorna HTTP 201 chamando a rota de detalhes do usuário criado
                return CreatedAtAction(nameof(ObterPorId), new { id = usuarioCriado.Id }, usuarioCriado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message }); // Retorna HTTP 400
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { erro = ex.Message }); // Retorna HTTP 409 (Conflito de dados)
            }
        }

        [HttpPut("{id}")] // PUT /api/usuarios/{id}
        public async Task<IActionResult> Atualizar(string id, [FromBody] Usuario usuarioAtualizado)
        {
            var sucesso = await _service.AtualizarUsuarioAsync(id, usuarioAtualizado);
            if (!sucesso) return NotFound(new { mensagem = "Usuário não encontrado para atualização." });

            return NoContent(); // Retorna HTTP 204 (Sucesso mas nao retorna nada pq n tem usuário)
        }

        [HttpDelete("{id}")] // DELETE /api/usuarios/{id}
        public async Task<IActionResult> Deletar(string id)
        {
            var sucesso = await _service.DeletarUsuarioAsync(id);
            if (!sucesso) return NotFound(new { mensagem = "Usuário não encontrado para exclusão." });

            return NoContent(); // Retorna HTTP 204 (Usuário deletado mas não retorna nada)
        }
    }
}