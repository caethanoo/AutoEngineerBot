using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AutoEngineerBot.Models
{
    public class ProdutoMonitorado
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public decimal PrecoAlvo { get; set; }
        public decimal UltimoPreco { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}