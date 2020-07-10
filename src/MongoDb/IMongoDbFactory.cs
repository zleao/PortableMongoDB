using MongoDB.Driver;
using System.Threading.Tasks;

namespace PortableMongoDb
{
    public interface IMongoDbFactory
    {
        Task EnsureDatabaseIsCreatedAsync();

        Task ScaleDatabaseThroughputBasedOnCollectionCount();

        /// <summary>
        /// Gets the database instance.
        /// </summary>
        /// <remarks>
        /// Do not use this method directly!
        /// If you want to access the database, do it through the <see cref="IMongoDbContext"/>
        /// </remarks>
        /// <returns></returns>
        IMongoDatabase GetDatabaseInstance();
    }
}
