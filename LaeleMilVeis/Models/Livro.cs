using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LaeleMilVeis.Models
{
    public class Livro
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public int Ano { get; set; }
        public bool Disponivel { get; set; } = true;

        // Pega o id de quem pegou o livro
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UsuarioId { get; set; }
    }
}