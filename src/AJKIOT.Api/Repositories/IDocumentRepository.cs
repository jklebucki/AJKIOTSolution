using MongoDB.Bson;

namespace AJKIOT.Api.Repositories
{
    public interface IDocumentRepository
    {
        Task<List<BsonDocument>> GetAllAsync();
        Task<BsonDocument> GetByIdAsync(string id);
        Task CreateOrUpdateAsync(BsonDocument document);
        Task DeleteAsync(string id);
    }
}
