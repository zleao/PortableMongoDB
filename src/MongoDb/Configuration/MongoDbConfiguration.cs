namespace PortableMongoDb.MongoDb.Configuration
{
    public class MongoDbConfiguration : IMongoDbConfiguration
    {
        public string DatabaseName { get; }

        public DbLocation DatabaseLocation { get; }

        public string ConnectionString { get; }

        public int AzureDatabaseInitialThroughput { get; }

        public int AzureDatabaseManagementPort { get; }

        public MongoDbConfiguration(string databaseName,
                                    DbLocation databaseLocation,
                                    string connectionString,
                                    int azureDatabaseInitialThroughput = 2000,
                                    int azureDatabaseManagementPort = 443)
        {
            DatabaseName = databaseName;
            DatabaseLocation = databaseLocation;
            ConnectionString = connectionString;
            AzureDatabaseInitialThroughput = azureDatabaseInitialThroughput;
            AzureDatabaseManagementPort = azureDatabaseManagementPort;
        }
    }
}
