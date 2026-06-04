using LaeleMilVeis.Models;
using LaeleMilVeis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LaeleMilVeis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LivrosController : ControllerBase
    {
        private readonly LivroService _service;

        public LivrosController(LivroService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            var livros = await _service.ListarTodosLivrosAsync();
            return Ok(livros);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(string id)
        {
            var livro = await _service.BuscarPorIdAsync(id);
            if (livro == null) return NotFound(new { mensagem = "Livro não encontrado." });
            return Ok(livro);
        }

        [HttpGet("vinculos")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListarVinculos()
        {
            var livros = await _service.ListarLivrosComVinculoUsuarioAsync();
            return Ok(livros);
        }

        [HttpGet("meus-emprestimos")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> MeusEmprestimos()
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized(new { mensagem = "Usuário não identificado." });

            var livros = await _service.ListarPorUsuarioAsync(usuarioId);
            return Ok(livros);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Criar([FromBody] Livro novoLivro)
        {
            try
            {
                var livroCriado = await _service.CadastrarLivroAsync(novoLivro);
                return CreatedAtAction(nameof(ObterPorId), new { id = livroCriado.Id }, livroCriado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Atualizar(string id, [FromBody] Livro livroAtualizado)
        {
            try
            {
                var sucesso = await _service.AtualizarLivroAsync(id, livroAtualizado);
                if (!sucesso) return NotFound(new { mensagem = "Livro não encontrado." });
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPost("{id}/emprestar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> Emprestar(string id)
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized(new { mensagem = "Usuário não identificado." });

            try
            {
                var sucesso = await _service.EmprestarLivroAsync(id, usuarioId);
                if (!sucesso) return BadRequest(new { erro = "Livro indisponível ou não encontrado." });

                return Ok(new { mensagem = "Empréstimo realizado com sucesso!" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        [HttpPost("{id}/devolver")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> Devolver(string id)
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized(new { mensagem = "Usuário não identificado." });

            try
            {
                var sucesso = await _service.DevolverLivroAsync(id, usuarioId);
                if (!sucesso) return BadRequest(new { erro = "Livro não encontrado ou já se encontra disponível." });

                return Ok(new { mensagem = "Livro devolvido com sucesso!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPost("{id}/emprestar-admin/{usuarioId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EmprestarAdmin(string id, string usuarioId)
        {
            try
            {
                var sucesso = await _service.EmprestarLivroAdminAsync(id, usuarioId);
                if (!sucesso) return BadRequest(new { erro = "Livro indisponível ou não encontrado." });

                return Ok(new { mensagem = "Empréstimo realizado com sucesso pelo administrador!" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        [HttpPost("{id}/devolver-admin/{usuarioId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DevolverAdmin(string id, string usuarioId)
        {
            try
            {
                var sucesso = await _service.DevolverLivroAdminAsync(id, usuarioId);
                if (!sucesso) return BadRequest(new { erro = "Livro não encontrado ou já se encontra disponível." });

                return Ok(new { mensagem = "Livro devolvido com sucesso pelo administrador!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deletar(string id)
        {
            try
            {
                var sucesso = await _service.DeletarLivroAsync(id);
                if (!sucesso) return NotFound(new { mensagem = "Livro não encontrado." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
