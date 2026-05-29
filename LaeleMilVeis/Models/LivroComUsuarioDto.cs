namespace LaeleMilVeis.Models
{
    public class LivroComUsuarioDto
    {
        public string? Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public int Ano { get; set; }
        public bool Disponivel { get; set; }
        public string? UsuarioId { get; set; }
        public string? UsuarioNome { get; set; }
        public string? UsuarioEmail { get; set; }
    }
}
