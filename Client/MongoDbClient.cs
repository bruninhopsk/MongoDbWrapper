using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDbWrapper.Domain;
using SkyApm.Diagnostics.MongoDB;
using SkyApm.Logging;

namespace MongoDbWrapper.Client
{
    public class MongoDbClient<T, TId> : IMongoDbClient<T, TId> where T : BaseEntity<TId>
    {
        private MongoClient Client { get; }
        public IMongoCollection<T> Collection { get; }

        public MongoDbClient(IOptions<MongoConfig> config, ILogger logger)
        {
            try
            {
                BsonClassMap.RegisterClassMap<T>(cm => cm.AutoMap());

                var name = typeof(T).Name;

                MongoUrl mongoUrl = new MongoUrl(config.Value.ConnectionString);
                var mongoClientSettings = MongoClientSettings.FromUrl(mongoUrl);
                mongoClientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
                Client = new MongoClient(mongoClientSettings);
                Collection = Client.GetDatabase(config.Value.Database).GetCollection<T>(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: ", ex);
                return;
            }
        }

        public IClientSessionHandle StartSession()
        {
            return Client.StartSession();
        }

        public List<T> GetAll()
        {
            return Collection.Find(col => true).ToList();
        }

        public List<T> FindAll(Expression<Func<T, bool>> predicate)
        {
            return Collection.Find(predicate).ToList();
        }

        public T Find(Expression<Func<T, bool>> predicate)
        {
            return Collection.Find(predicate).FirstOrDefault();
        }

        public T Insert(T data)
        {
            Collection.InsertOne(data);
            return data;
        }

        public List<T> InsertMany(List<T> data)
        {
            Collection.InsertMany(data);
            return data;
        }

        public bool Update(TId id, T input)
        {
            input.Id = id;
            var changesDocument = BsonDocument.Parse(input.ToJson());
            var filter = Builders<T>.Filter.Eq("_id", id);
            var firstElement = changesDocument.FirstOrDefault();

            if (Find(i => i.Id.Equals(id)) != null)
            {
                var update = Builders<T>.Update.Set(firstElement.Name, firstElement.Value);

                foreach (var item in changesDocument)
                {
                    if (!item.Value.IsBsonNull && item.Name != "_id")
                    {
                        update = update.Set(item.Name, item.Value);
                    }
                }

                var result = Collection.UpdateOne(filter, update);
                return result.IsAcknowledged;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(TId id)
        {
            return Collection.DeleteOne(data => data.Id.Equals(id)).DeletedCount > 0;
        }
    }
}