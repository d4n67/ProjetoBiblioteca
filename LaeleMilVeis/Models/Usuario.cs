using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LaeleMilVeis.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Perfil { get; set; } = "Usuario"; // "Admin" ou "Usuario"
    }
}
