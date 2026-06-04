using LaeleMilVeis.Data;
using LaeleMilVeis.Models;

namespace LaeleMilVeis.Services
{
    public class LivroService
    {
        private readonly LivroRepository _livroRepository;
        private readonly UsuarioRepository _usuarioRepository;

        // uso o repositório de user pra validar se o usuário existe na hora de emprestar um livro
        public LivroService(LivroRepository livroRepository, UsuarioRepository usuarioRepository)
        {
            _livroRepository = livroRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<List<Livro>> ListarTodosLivrosAsync() => await _livroRepository.ObterTodosAsync();

        public async Task<List<LivroComUsuarioDto>> ListarLivrosComVinculoUsuarioAsync()
        {
            var livros = await _livroRepository.ObterTodosAsync();
            var usuarios = await _usuarioRepository.ObterTodosAsync();
            var usuariosPorId = usuarios
                .Where(u => u.Id != null)
                .ToDictionary(u => u.Id!);

            return livros.Select(livro =>
            {
                Usuario? usuario = null;
                if (!string.IsNullOrEmpty(livro.UsuarioId))
                    usuariosPorId.TryGetValue(livro.UsuarioId, out usuario);

                return new LivroComUsuarioDto
                {
                    Id = livro.Id,
                    Titulo = livro.Titulo,
                    Autor = livro.Autor,
                    Ano = livro.Ano,
                    Disponivel = livro.Disponivel,
                    UsuarioId = livro.UsuarioId,
                    UsuarioNome = usuario?.Nome,
                    UsuarioEmail = usuario?.Email
                };
            }).ToList();
        }

        public async Task<Livro?> BuscarPorIdAsync(string id) => await _livroRepository.ObterPorIdAsync(id);

        public async Task<Livro> CadastrarLivroAsync(Livro livro)
        {
            if (string.IsNullOrEmpty(livro.Titulo) || string.IsNullOrEmpty(livro.Autor))
                throw new ArgumentException("Título e Autor são obrigatórios para o cadastro.");

            livro.Disponivel = true;
            livro.UsuarioId = null; // Livro novo começa sem vinculo com user 

            await _livroRepository.CriarAsync(livro);
            return livro;
        }

        public async Task<List<Livro>> ListarPorUsuarioAsync(string usuarioId) =>
            await _livroRepository.ObterLivrosPorUsuarioAsync(usuarioId);

        public async Task<bool> AtualizarLivroAsync(string id, Livro dadosAtualizados)
        {
            var livro = await _livroRepository.ObterPorIdAsync(id);
            if (livro == null) return false;

            if (string.IsNullOrEmpty(dadosAtualizados.Titulo) || string.IsNullOrEmpty(dadosAtualizados.Autor))
                throw new ArgumentException("Título e Autor são obrigatórios.");

            livro.Titulo = dadosAtualizados.Titulo;
            livro.Autor = dadosAtualizados.Autor;
            livro.Ano = dadosAtualizados.Ano;

            await _livroRepository.BlacklistAtualizarAsync(id, livro);
            return true;
        }

        // Realizar emprestimo de livro pra algum user e validar se o livro existe, se tá disponível e se o usuário existe
        public async Task<bool> EmprestarLivroAsync(string livroId, string usuarioId)
        {
            var livro = await _livroRepository.ObterPorIdAsync(livroId);
            if (livro == null || !livro.Disponivel) return false; // Livro não existe ou já está emprestado

            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            if (usuario == null) throw new InvalidOperationException("Usuário informado não existe.");

            //  guardando o ID do usuário no documento do livro
            livro.Disponivel = false;
            livro.UsuarioId = usuarioId;

            await _livroRepository.BlacklistAtualizarAsync(livroId, livro);
            return true;
        }

        //Devolver Livro (apenas quem emprestou pode devolver)
        public async Task<bool> DevolverLivroAsync(string livroId, string usuarioId)
        {
            var livro = await _livroRepository.ObterPorIdAsync(livroId);
            if (livro == null || livro.Disponivel) return false;

            if (livro.UsuarioId != usuarioId)
                throw new InvalidOperationException("Este livro não está emprestado para você.");

            livro.Disponivel = true;
            livro.UsuarioId = null;

            await _livroRepository.BlacklistAtualizarAsync(livroId, livro);
            return true;
        }

        // Admin empresta um livro para um usuário específico
        public async Task<bool> EmprestarLivroAdminAsync(string livroId, string usuarioId)
        {
            var livro = await _livroRepository.ObterPorIdAsync(livroId);
            if (livro == null || !livro.Disponivel) return false; // Livro não existe ou já está emprestado

            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            if (usuario == null) throw new InvalidOperationException("Usuário informado não existe.");

            livro.Disponivel = false;
            livro.UsuarioId = usuarioId;

            await _livroRepository.BlacklistAtualizarAsync(livroId, livro);
            return true;
        }

        // Admin devolve um livro emprestado para um usuário específico
        public async Task<bool> DevolverLivroAdminAsync(string livroId, string usuarioId)
        {
            var livro = await _livroRepository.ObterPorIdAsync(livroId);
            if (livro == null || livro.Disponivel) return false; // Livro não existe ou já está disponível

            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            if (usuario == null) throw new InvalidOperationException("Usuário informado não existe.");

            if (livro.UsuarioId != usuarioId)
                throw new InvalidOperationException($"Este livro não está emprestado para o usuário {usuario.Nome}.");

            livro.Disponivel = true;
            livro.UsuarioId = null;

            await _livroRepository.BlacklistAtualizarAsync(livroId, livro);
            return true;
        }

        public async Task<bool> DeletarLivroAsync(string id)
        {
            var livro = await _livroRepository.ObterPorIdAsync(id);
            if (livro == null) return false;

            if (!livro.Disponivel) throw new InvalidOperationException("Não é possível deletar um livro que está emprestado.");

            await _livroRepository.DeletarAsync(id);
            return true;
        }
    }
}