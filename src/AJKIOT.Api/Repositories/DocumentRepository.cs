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
            return await _collection.Find(Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(BsonDocument document)
        {
            await _collection.InsertOneAsync(document);
        }

        public async Task UpdateAsync(string id, BsonDocument document)
        {
            await _collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id)), document);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id)));
        }
    }

}
