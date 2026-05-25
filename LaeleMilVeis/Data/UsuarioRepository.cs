using LaeleMilVeis.Models;
using MongoDB.Driver;

namespace LaeleMilVeis.Data
{
    public class UsuarioRepository
    {
        private readonly IMongoCollection<Usuario> _usuarios;

       
        public UsuarioRepository(MongoDbContext context)
        {
            _usuarios = context.Usuarios;
        }

        // Busca todos os usuários do banco
        public async Task<List<Usuario>> ObterTodosAsync() =>
            await _usuarios.Find(_ => true).ToListAsync();

        // Busca um usuário por ID específico
        public async Task<Usuario?> ObterPorIdAsync(string id) =>
            await _usuarios.Find(u => u.Id == id).FirstOrDefaultAsync();

        // Busca por e-mail (bom pra ver se tem 2 usuarios iguais ou com o mesmo email)
        public async Task<Usuario?> ObterPorEmailAsync(string email) =>
            await _usuarios.Find(u => u.Email == email).FirstOrDefaultAsync();

        // Coloca o documento do user no db
        public async Task CriarAsync(Usuario usuario) =>
            await _usuarios.InsertOneAsync(usuario);

        // Atualiza os dados do usuário, exceto o id 
        public async Task AtualizarAsync(string id, Usuario usuario) =>
            await _usuarios.ReplaceOneAsync(u => u.Id == id, usuario);

        // Remove o usuário do banco de dados usando o id
        public async Task DeletarAsync(string id) =>
            await _usuarios.DeleteOneAsync(u => u.Id == id);
    }
}
