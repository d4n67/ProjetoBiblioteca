using LaeleMilVeis.Models;
using MongoDB.Driver;

namespace LaeleMilVeis.Data
{
    public class LivroRepository
    {
        private readonly IMongoCollection<Livro> _livros;

        public LivroRepository(MongoDbContext context)
        {
            _livros = context.Livros;
        }

        //Busca todos  os livros do banco
        public async Task<List<Livro>> ObterTodosAsync() =>
            await _livros.Find(_ => true).ToListAsync();

        // Busca um livro pelo ID
        public async Task<Livro?> ObterPorIdAsync(string id) =>
            await _livros.Find(l => l.Id == id).FirstOrDefaultAsync();

        // Busca todos os livros de um usuário específico
        public async Task<List<Livro>> ObterLivrosPorUsuarioAsync(string usuarioId) =>
            await _livros.Find(l => l.UsuarioId == usuarioId).ToListAsync();

        // Cria um novo livro no bd
        public async Task CriarAsync(Livro livro) =>
            await _livros.InsertOneAsync(livro);

        // Atualiza os dados de um livro
        public async Task BlacklistAtualizarAsync(string id, Livro livro) =>
            await _livros.ReplaceOneAsync(l => l.Id == id, livro);

        // Deleta um livro
        public async Task DeletarAsync(string id) =>
            await _livros.DeleteOneAsync(l => l.Id == id);
    }
}