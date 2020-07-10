using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using MongoDB.Driver;
using PortableMongoDb.MongoDb.Configuration;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace PortableMongoDb.MongoDb
{
    public class MongoDbFactory : IMongoDbFactory
    {
        protected IMongoDbConfiguration Configuration { get; }

        public MongoDbFactory(IMongoDbConfiguration configuration)
        {
            Configuration = configuration;
        }


        public async Task EnsureDatabaseIsCreatedAsync()
        {
            //The data base initialization depends on the configured DbLocation
            switch (Configuration.DatabaseLocation)
            {
                case DbLocation.Local:
                    //A local DB means that we're using a 'pure' mongoDb database.
                    //For these cases, there is no method to create a database. 
                    // We can just call the 'GetDatabase' method that will automatically create the database for us.
                    //https://www.codementor.io/pmbanugo/working-with-mongodb-in-net-1-basics-g4frivcvz
                    GetDatabaseInstance();
                    break;

                case DbLocation.Azure:
                    //Using a Azure Cosmos Db 
                    //In this case we need to create a database with a specified throughput.
                    //https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-provision-database-throughput
                    //https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.client.documentclient.createdatabaseifnotexistsasync?view=azure-dotnet
                    using (var client = GetDocumentClient())
                    {
                        //set the throughput for the database
                        var options = new RequestOptions { OfferThroughput = Configuration.AzureDatabaseInitialThroughput };

                        //create the database without overriding any existing one
                        await client.CreateDatabaseIfNotExistsAsync(new Microsoft.Azure.Documents.Database { Id = Configuration.DatabaseName }, options);
                    }

                    break;

                default:
                    throw new NotSupportedException($"Database location not supported ({Configuration.DatabaseLocation})");
            }
        }

        public async Task ScaleDatabaseThroughputBasedOnCollectionCount()
        {
            if (Configuration.DatabaseLocation != DbLocation.Azure)
            {
                //Throughput scaling is only applied when we're dealing with Azure Cosmos DB
                return;
            }

            //https://docs.microsoft.com/en-us/azure/cosmos-db/set-throughput
            using (var client = GetDocumentClient())
            {
                var database = client.CreateDatabaseQuery().Where(db => db.Id == Configuration.DatabaseName).ToList().FirstOrDefault();
                if (database == null)
                {
                    //Database not found
                    return;
                }

                //The throughput value will be based on the number of collection we have.
                //Each collection needs 100 units and we cannot have less than 400 units.
                //The algorithm to calculate the throughput should be: MAX(400, (numberOfCollections * 100))
                var feedResult = await client.ReadDocumentCollectionFeedAsync(UriFactory.CreateDatabaseUri(database.Id));
                if (feedResult == null)
                {
                    return;
                }

                var offer = client.CreateOfferQuery().Where(r => r.ResourceLink == database.SelfLink).ToList().FirstOrDefault();
                if(offer != null)
                {
                    offer = new OfferV2(offer, Math.Max(400, (feedResult.Count * 100)));
                    await client.ReplaceOfferAsync(offer);
                }
            }
        }

        /// <summary>
        /// Gets the database instance.
        /// </summary>
        /// <remarks>
        /// Do not use this method directly!
        /// If you want to access the database, do it through the <see cref="IMongoDbContext"/>
        /// </remarks>
        /// <returns></returns>
        public IMongoDatabase GetDatabaseInstance()
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(Configuration.ConnectionString));

            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            var client = new MongoClient(settings);

            return client.GetDatabase(Configuration.DatabaseName);
        }

        private DocumentClient GetDocumentClient()
        {
            
            var mongoUrl = new MongoUrl(Configuration.ConnectionString);
            var managementPort = Configuration.AzureDatabaseManagementPort;

            return new DocumentClient(new Uri($"https://{mongoUrl.Server.Host}:{managementPort}/"), mongoUrl.Password);
        }
    }
}