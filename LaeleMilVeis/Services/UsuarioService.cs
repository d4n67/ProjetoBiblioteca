using LaeleMilVeis.Data;
using LaeleMilVeis.Models;
using BCrypt.Net;

namespace LaeleMilVeis.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly ILivroRepository _livroRepository;

        // O .NET injeta os repositórios aq
        public UsuarioService(IUsuarioRepository repository, ILivroRepository livroRepository)
        {
            _repository = repository;
            _livroRepository = livroRepository;
        }

        public async Task<List<Usuario>> ListarTodosUsuariosAsync() =>
            await _repository.ObterTodosAsync();

        public async Task<Usuario?> BuscarPorIdAsync(string id) =>
            await _repository.ObterPorIdAsync(id);

        // regra de negócio/tratamento de erros.
        public async Task<Usuario> CadastrarUsuarioAsync(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentException("Dados do usuário são obrigatórios.");

            if (string.IsNullOrWhiteSpace(usuario.Nome))
                throw new ArgumentException("Nome é obrigatório.");

            if (string.IsNullOrEmpty(usuario.Email) || string.IsNullOrEmpty(usuario.Senha))
                throw new ArgumentException("E-mail e Senha são obrigatórios.");

            if (usuario.Senha.Length < 6)
                throw new ArgumentException("A senha deve ter pelo menos 6 caracteres.");

            if (!usuario.Email.Contains("@"))
                throw new ArgumentException("E-mail inválido.");

            // Verifica se já existe um email igual no bd
            var usuarioExistente = await _repository.ObterPorEmailAsync(usuario.Email);
            if (usuarioExistente != null)
                throw new InvalidOperationException("Este e-mail já está em uso por outro usuário.");

            // Hash da senha antes de salvar
            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);

            await _repository.CriarAsync(usuario);
            return usuario;
        }

        public async Task<bool> AtualizarUsuarioAsync(string id, Usuario dadosAtualizados)
        {
            var usuarioExistente = await _repository.ObterPorIdAsync(id);
            if (usuarioExistente == null) return false; // Retorna falso se o ID não existir

            // Atualiza as propriedades do usuário no bd
            usuarioExistente.Nome = dadosAtualizados.Nome;
            usuarioExistente.Email = dadosAtualizados.Email;
            usuarioExistente.Perfil = dadosAtualizados.Perfil;

            if (!string.IsNullOrEmpty(dadosAtualizados.Senha))
                usuarioExistente.Senha = BCrypt.Net.BCrypt.HashPassword(dadosAtualizados.Senha);

            await _repository.AtualizarAsync(id, usuarioExistente);
            return true;
        }

        public async Task<bool> DeletarUsuarioAsync(string id)
        {
            var usuarioExistente = await _repository.ObterPorIdAsync(id);
            if (usuarioExistente == null) return false;

            // Libera todos os livros deste usuário antes de deletá-lo
            await _livroRepository.LiberarLivrosUsuarioAsync(id);

            await _repository.DeletarAsync(id);
            return true;
        }
        // Valida as credenciais e retorna o usuário se tiver tudo certo ou nulo se não
        public async Task<Usuario?> AutenticarAsync(string email, string senha)
        {
            // Validação de entrada
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
                return null;

            var usuario = await _repository.ObterPorEmailAsync(email);

            if (usuario == null || string.IsNullOrEmpty(usuario.Senha))
                return null;

            try
            {
                // Verifica a senha com BCrypt
                if (!BCrypt.Net.BCrypt.Verify(senha, usuario.Senha))
                    return null;
            }
            catch (FormatException)
            {
                // Hash inválido no banco de dados
                return null;
            }

            return usuario;
        }
    }
}