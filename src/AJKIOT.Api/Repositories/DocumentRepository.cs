using AJKIOT.Api.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AJKIOT.Api.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public DocumentRepository(IMongoDatabase database, IOptions<MongoDBSettings> settings)
        {
            _collection = database.GetCollection<BsonDocument>(settings.Value.CollectionName);
        }

        public async Task<List<BsonDocument>> GetAllAsync()
        {
            return await _collection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<BsonDocument> GetByIdAsync(string id)
        {
            return await _collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }

        public async Task CreateOrUpdateAsync(BsonDocument document)
        {
            var idValue = document["_id"];
            var filter = Builders<BsonDocument>.Filter.Eq("_id", idValue);
            await _collection.UpdateOneAsync(filter, new BsonDocument("$set", document), new UpdateOptions { IsUpsert = true });
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", id));
        }
    }

}
