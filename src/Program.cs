using MongoDB.Driver;
using Newtonsoft.Json;
using PortableMongoDb.Models;
using PortableMongoDb.MongoDb;
using PortableMongoDb.MongoDb.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PortableMongoDb
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            try
            {
                //var config = new MongoDbConfiguration(
                //        "MyBlogsDatabase",
                //        DbLocation.Local,
                //        "<connection string to an on premisse MongoDB instance>");

                //var config = new MongoDbConfiguration(
                //        "MyBlogsDatabase",
                //        DbLocation.Azure,
                //        "<Connection string from azure cosmos db>");

                //Initialize the services
                Console.WriteLine($"Initializing services with {config.DatabaseLocation} configuration...");
                var factory = new MongoDbFactory(config);
                var service = new MongoDbService(factory);
                Console.WriteLine("Services initialized.");

                //Ensure the database is created
                Console.WriteLine("Ensuring database is created...");
                await factory.EnsureDatabaseIsCreatedAsync();

                //Create some collections
                Console.WriteLine("Ensuring that database has some collections...");
                await service.CreateCollectionAsync<Blog>();

                //Set the Throughput based on the number of colections we have.
                //This is only applicable to Azure Cosmos Db instances
                Console.WriteLine("Scaling the throughput, based on the number of collections...");
                await factory.ScaleDatabaseThroughputBasedOnCollectionCount();
                
                Console.WriteLine("Inserting some data in the collections...");
                var col = await service.GetCollectionAsync<Blog>();
                await col.InsertOneAsync(new Blog
                {
                    Title = "My First Blog",
                    SubTitle = "How to insert data in MongoDb",
                    Text = "InsertOneAsync"
                });

                Console.WriteLine("Get data from the collections...");
                var blogs = await (await col.FindAsync(_ => true)).ToListAsync();
                Console.WriteLine(JsonConvert.SerializeObject(blogs, Newtonsoft.Json.Formatting.Indented));

                Console.WriteLine("Delete data from the collections...");
                foreach (var blog in blogs)
                {
                    await col.DeleteOneAsync(b => b.Id == blog.Id);
                }
                
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                Console.WriteLine(ex.Message);
            }
        }
    }
}
