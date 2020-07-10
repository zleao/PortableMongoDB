using MongoDB.Driver;
using System.Threading.Tasks;

namespace PortableMongoDb.MongoDb.Configuration
{
    public interface IMongoDbConfiguration
    {
        /// <summary>
        /// Database name
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Location of the database (Local, Azure)
        /// This will determine the logic for creation and resources management (throughput)
        /// </summary>
        DbLocation DatabaseLocation { get; }

        /// <summary>
        /// MongoDb connection string
        /// </summary>
        string ConnectionString { get; }

        // -- Azure Cosmos DB related configurations --

        /// <summary>
        /// Throughput that should be assigned when creating a new database
        /// Defaults to 2000
        /// </summary>
        int AzureDatabaseInitialThroughput { get; }

        /// <summary>
        /// Azure Cosmos DB management port.
        /// Defaults to 443
        /// </summary>
        int AzureDatabaseManagementPort { get; }

    }
}
