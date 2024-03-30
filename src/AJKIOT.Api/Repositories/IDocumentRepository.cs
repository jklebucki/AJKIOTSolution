using MongoDB.Bson;

namespace AJKIOT.Api.Repositories
{
    public interface IDocumentRepository
    {
        Task<List<BsonDocument>> GetAllAsync();
        Task<BsonDocument> GetByIdAsync(string id);
        Task CreateAsync(BsonDocument document);
        Task UpdateAsync(string id, BsonDocument document);
        Task DeleteAsync(string id);
    }
}
