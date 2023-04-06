using MongoDB.Bson.Serialization.Attributes;

namespace FishieTransactions.Helper
{
    public interface IDocument
    {
        [BsonId]
        Guid Id { get; set; }
    }

    public abstract class Document : IDocument
    {
        public Guid Id { get; set; }
    }
}
