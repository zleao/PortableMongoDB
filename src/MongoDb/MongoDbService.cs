using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PortableMongoDb.MongoDb
{
    public class MongoDbService : IMongoDbService
    {
        protected IMongoDbFactory DatabaseFactory { get; }

        protected IMongoDatabase Database => _database ??= DatabaseFactory.GetDatabaseInstance();
        private IMongoDatabase _database;

        public MongoDbService(IMongoDbFactory databaseFactory)
        {
            DatabaseFactory = databaseFactory;
        }

        public async Task<bool> AnyCollectionAsync(CancellationToken token = default(CancellationToken))
        {
            return await (await Database.ListCollectionNamesAsync(cancellationToken: token)).AnyAsync(token);
        }

        public async Task<IList<string>> GetCollectionNamesAsync(CancellationToken token = default(CancellationToken))
        {
            return await (await Database.ListCollectionNamesAsync(cancellationToken: token)).ToListAsync(token);
        }

        public Task DropCollectionAsync(string collectionName, CancellationToken token = default(CancellationToken))
        {
            return Database.DropCollectionAsync(collectionName, token);
        }

        public async Task<bool> CollectionExistsAsync<T>() where T : class
        {
            //filter by collection name
            var filter = new BsonDocument("name", GetCollectionName<T>());
            var collections = await Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });

            //check for existence
            return await collections.AnyAsync();
        }

        public async Task CreateCollectionAsync<T>() where T : class
        {
            if (await CollectionExistsAsync<T>())
            {
                //no point in trying to create a collection that already exists
                return;
            }

            await TryCreateCollection<T>(GetCollectionName<T>());
        }

        public async Task<IMongoCollection<T>> GetCollectionAsync<T>() where T : class
        {
            if (!await CollectionExistsAsync<T>())
            {
                return null;
            }

            //make sure collection names do not use Db prefix
            return Database.GetCollection<T>(GetCollectionName<T>());
        }

        private async Task TryCreateCollection<T>(string collectionName) where T : class
        {
            //Because we're using a shared throughput, we're forced to use partition keys
            //https://blog.jeremylikness.com/what-the-shard-cc29623503ad

            //get the partition key, based on type
            var partitionKey = GetPartitionKey<T>();

            //Create the collection with a partition key
            var partition = new BsonDocument {
                {"shardCollection", $"{Database.DatabaseNamespace.DatabaseName}.{collectionName}"},
                {"key", new BsonDocument {{partitionKey, "hashed"}}}
            };
            var command = new BsonDocumentCommand<BsonDocument>(partition);

            try
            {
                await Database.RunCommandAsync(command);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to create a shard collection. going to try it with normal collection: { e.Message }");
            }

            await Database.CreateCollectionAsync(collectionName);
        }

        private static string GetCollectionName<T>() where T : class
        {
            //Some logic to generate the names for the collections....
            return typeof(T).Name;
        }

        private static string GetPartitionKey<T>() where T : class
        {
            //Some logic to generate the partition keys...
            return "_id";
        }
    }
}
