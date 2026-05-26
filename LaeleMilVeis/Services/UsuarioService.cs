using LaeleMilVeis.Data;
using LaeleMilVeis.Models;

namespace LaeleMilVeis.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _repository;

        // O .NET injeta o repositório aq
        public UsuarioService(UsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Usuario>> ListarTodosUsuariosAsync() =>
            await _repository.ObterTodosAsync();

        public async Task<Usuario?> BuscarPorIdAsync(string id) =>
            await _repository.ObterPorIdAsync(id);

        // regra de negócio/tratamento de erros.
        public async Task<Usuario> CadastrarUsuarioAsync(Usuario usuario)
        {
            if (string.IsNullOrEmpty(usuario.Email) || string.IsNullOrEmpty(usuario.Senha))
                throw new ArgumentException("E-mail e Senha são obrigatórios.");

            // Verifica se já existe um email igual no bd
            var usuarioExistente = await _repository.ObterPorEmailAsync(usuario.Email);
            if (usuarioExistente != null)
                throw new InvalidOperationException("Este e-mail já está em uso por outro usuário.");

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
                usuarioExistente.Senha = dadosAtualizados.Senha;

            await _repository.AtualizarAsync(id, usuarioExistente);
            return true;
        }

        public async Task<bool> DeletarUsuarioAsync(string id)
        {
            var usuarioExistente = await _repository.ObterPorIdAsync(id);
            if (usuarioExistente == null) return false;

            await _repository.DeletarAsync(id);
            return true;
        }
        // Valida as credenciais e retorna o usuário se tiver tudo certo ou nulo se não
        public async Task<Usuario?> AutenticarAsync(string email, string senha)
        {
            var usuario = await _repository.ObterPorEmailAsync(email);

            if (usuario == null || usuario.Senha != senha)
                return null;

            return usuario;
        }
    }
}