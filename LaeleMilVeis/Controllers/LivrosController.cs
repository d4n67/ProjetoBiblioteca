using LaeleMilVeis.Models;
using LaeleMilVeis.Services;
using Microsoft.AspNetCore.Mvc;

namespace LaeleMilVeis.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Rota REST: /api/livros
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

        [HttpPost]
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

        // Efetuar empréstimo -> POST /api/livros/{id}/emprestar/{usuarioId}
        [HttpPost("{id}/emprestar/{usuarioId}")]
        public async Task<IActionResult> Emprestar(string id, string usuarioId)
        {
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

        //  Devolver livro -> POST /api/livros/{id}/devolver
        [HttpPost("{id}/devolver")]
        public async Task<IActionResult> Devolver(string id)
        {
            var sucesso = await _service.DevolverLivroAsync(id);
            if (!sucesso) return BadRequest(new { erro = "Livro não encontrado ou já se encontra disponível." });

            return Ok(new { mensagem = "Livro devolvido com sucesso!" });
        }

        [HttpDelete("{id}")]
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