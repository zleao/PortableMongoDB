using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PortableMongoDb.Models
{
    public class Blog
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonRequired]
        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string Text { get; set; }
    }
}
