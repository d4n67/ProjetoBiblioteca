using LaeleMilVeis.Data;
using LaeleMilVeis.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace LaeleMilVeis.Controllers
{
    [ApiController]
    [Route("api/[controller]")] //cria a rota 
    public class LivrosController : ControllerBase
    {
        private readonly MongoDbContext _context;

       
        public LivrosController(MongoDbContext context)
        {
            _context = context;
        }

        // Endpoint: listar os livros.
        [HttpGet]
        public async Task<ActionResult<List<Livro>>> ListarTodos()
        {
            try
            {
                // Busca todos os documentos sem  filtro
                var livros = await _context.Livros.Find(_ => true).ToListAsync();
                return Ok(livros); // Retorna HTTP Status 200 (válido) com a lista de livros ou 500 caso de erro.
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Erro interno no servidor: {ex.Message}");
            }
        }
    }
}