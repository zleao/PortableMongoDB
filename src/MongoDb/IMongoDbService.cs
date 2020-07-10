using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PortableMongoDb.MongoDb
{
    public interface IMongoDbService
    {
        /// <summary>
        /// Checks in the database has any collection
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> AnyCollectionAsync(CancellationToken token = default);

        /// <summary>
        /// Gets a list with the existing collection names.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        Task<IList<string>> GetCollectionNamesAsync(CancellationToken token = default);

        /// <summary>
        /// Drops a collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        Task DropCollectionAsync(string collectionName, CancellationToken token = default);

        /// <summary>
        /// Checks if the given collection exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<bool> CollectionExistsAsync<T>() where T : class;

        /// <summary>
        /// Creates the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task CreateCollectionAsync<T>() where T : class;

        /// <summary>
        /// Gets the collection with the given type name trimming the prefix "Db" from it. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IMongoCollection<T>> GetCollectionAsync<T>() where T : class;
    }
}
