using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDbWrapper.Domain;

namespace MongoDbWrapper.Client
{
    public interface IMongoDbClient<T, TId> where T : BaseEntity<TId>
    {
        IClientSessionHandle StartSession();
        List<T> GetAll();
        List<T> FindAll(Expression<Func<T, bool>> predicate);
        T Find(Expression<Func<T, bool>> predicate);
        T Insert(T data);
        List<T> InsertMany(List<T> data);
        bool Update(TId id, T input);
        bool Remove(TId id);
    }
}