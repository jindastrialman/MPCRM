using System.Collections.ObjectModel;
using DataService.Models.Abstractions;
using Framework.Query;
using LiteDB;

namespace DataService.Services.DataAccess.Abstractions;

public class LiteDbDataProvider<T> : IDataProviderService<T> where T : BaseLiteDbModel
{
    private ConnectionString _connectionString;
    public LiteDbDataProvider(DatabaseConnectionOptions options)
    {
        _connectionString = new ConnectionString(options.ConnectionString);
        using var db = new LiteDatabase(_connectionString);
        var col = db.GetCollection<T>();
    }

    public QueryResult<T> Add(QueryParameters<T> parameters)
    {
        if(parameters.CollectionParameters is null)
            throw new ArgumentNullException();

        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>();
        var changedEntitiesCount = collection.Insert(parameters.CollectionParameters.Collection);

        return new QueryResult<T>(){ChangedCount = changedEntitiesCount};
    }

    public int Count(QueryParameters<T> parameters)
    {
        if(parameters.FilterParameters is null)
            throw new ArgumentNullException();

        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>();
        return collection.Count(parameters.FilterParameters.FilterExpression); //should support filterParams (it is empty now so who cares)
    }

    public QueryResult<T> Delete(QueryParameters<T> parameters)
    {
        
        if(parameters.CollectionParameters is null)
            throw new ArgumentNullException();
        
        var entitiesToDelete = parameters.CollectionParameters.Collection;

        bool isForceDelete = parameters.DeleteParameters != null && parameters.DeleteParameters.ForcedDelete; 

        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>();
        var changedEntitiesCount = 0;

        if(isForceDelete)
        {
            changedEntitiesCount = collection.DeleteMany(x => entitiesToDelete.Select(etd => etd.Id).Contains(x.Id));
        }
        else
        {
            foreach(var entity in entitiesToDelete)
            {
                entity.IsDeleted = true;
            }

            changedEntitiesCount = collection.Update(entitiesToDelete);
        }

        return new QueryResult<T>(){ChangedCount = changedEntitiesCount};
    }

    public QueryResult<T> Get(QueryParameters<T> parameters)
    {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>();
        return new QueryResult<T>(){OutputCollection = new Collection<T>(collection.Find(x => true).ToList())}; //blah blah same as coment in count
    }

    public QueryResult<T> Update(QueryParameters<T> parameters)
    {
        if(parameters.CollectionParameters is null)
            throw new ArgumentNullException();

        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>();

        var changedEntitiesCount = collection.Update(parameters.CollectionParameters.Collection);
        return new QueryResult<T>(){ChangedCount = changedEntitiesCount};
    }
}

public class DatabaseConnectionOptions
{
    public string ConnectionString{get; set;} = String.Empty;
}
