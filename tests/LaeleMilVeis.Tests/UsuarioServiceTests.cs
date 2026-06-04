using LaeleMilVeis.Data;
using LaeleMilVeis.Models;
using LaeleMilVeis.Services;
using Moq;
using Xunit;

namespace LaeleMilVeis.Tests
{
    public class UsuarioServiceTests
    {
        [Fact]
        public async Task CadastrarUsuario_Sucesso()
        {
            var repoMock = new Mock<IUsuarioRepository>();
            var livroMock = new Mock<ILivroRepository>();

            repoMock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
            repoMock.Setup(r => r.CriarAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);

            var service = new UsuarioService(repoMock.Object, livroMock.Object);

            var usuario = new Usuario { Nome = "Teste", Email = "teste@ex.com", Senha = "123456" };

            var result = await service.CadastrarUsuarioAsync(usuario);

            Assert.NotNull(result);
            Assert.NotEqual("123456", result.Senha); // senha deve estar hasheada
            repoMock.Verify(r => r.CriarAsync(It.IsAny<Usuario>()), Times.Once);
        }

        [Theory]
        [InlineData(null, "nome", "email@ex.com", "123456")]
        [InlineData(null, "", "email@ex.com", "123456")]
        [InlineData(null, "    ", "email@ex.com", "123456")]
        public async Task CadastrarUsuario_Falha_Nome(string id, string nome, string email, string senha)
        {
            var repoMock = new Mock<IUsuarioRepository>();
            var livroMock = new Mock<ILivroRepository>();
            var service = new UsuarioService(repoMock.Object, livroMock.Object);

            var usuario = new Usuario { Id = id, Nome = nome, Email = email, Senha = senha };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CadastrarUsuarioAsync(usuario));
        }

        [Fact]
        public async Task Autenticar_Sucesso()
        {
            var repoMock = new Mock<IUsuarioRepository>();
            var livroMock = new Mock<ILivroRepository>();

            var hashed = BCrypt.Net.BCrypt.HashPassword("senha123");
            var user = new Usuario { Id = "1", Nome = "U", Email = "u@e.com", Senha = hashed };

            repoMock.Setup(r => r.ObterPorEmailAsync("u@e.com")).ReturnsAsync(user);

            var service = new UsuarioService(repoMock.Object, livroMock.Object);

            var result = await service.AutenticarAsync("u@e.com", "senha123");

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("u@e.com", "wrongpass")]
        public async Task Autenticar_Falha(string email, string senha)
        {
            var repoMock = new Mock<IUsuarioRepository>();
            var livroMock = new Mock<ILivroRepository>();

            repoMock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);

            var service = new UsuarioService(repoMock.Object, livroMock.Object);

            var result = await service.AutenticarAsync(email, senha);

            Assert.Null(result);
        }
    }
}
