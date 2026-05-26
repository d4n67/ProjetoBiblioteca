using LaeleMilVeis.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LaeleMilVeis.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(Usuario usuario)
        {
            // Lê a chave
            var secret = _configuration.GetSection("JwtSettings:Secret").Value;
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenHandler = new JwtSecurityTokenHandler();

            // Claims e roles do usuário
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id ?? ""),
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Perfil) // Marca como Admin ou Usuario
                }),
                Expires = DateTime.UtcNow.AddHours(3), // Token expira em 3 horas
                Issuer = _configuration.GetSection("JwtSettings:Issuer").Value,
                Audience = _configuration.GetSection("JwtSettings:Audience").Value,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); // Retorna o Token criptografado em formato de texto
        }
    }
}