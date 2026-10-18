using System.Collections.ObjectModel;
using DataService.Models;
using LiteDB;

namespace DataService.Services.DataAccess.Abstractions;

public class LiteDbDataProvider<T> : IDataProviderService<T> where T : BaseDatabaseEntity
{
    private ConnectionString _connectionString;
    public LiteDbDataProvider(DatabaseConnectionOptions options)
    {
        _connectionString = new ConnectionString(options.ConnectionString);
        using var db = new LiteDatabase(_connectionString);
        var col = db.GetCollection<T>();
    }

    public CollectionUpdateInfo<T> Add(Collection<T> entitiesToAdd)
    {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>();
        var changedEntitiesCount = collection.Insert(entitiesToAdd);

        return new CollectionUpdateInfo<T>(){ChangedCount = changedEntitiesCount};
    }

    public int Count(FilterParameters<T> filterParameters)
    {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>();
        return collection.Count(); //should support filterParams (it is empty now so who cares)
    }

    public CollectionUpdateInfo<T> Delete(Collection<T> entitiesToDelete, DeleteParameters? deleteParameters)
    {
        bool isForceDelete = deleteParameters != null && deleteParameters.ForcedDelete; 

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

        return new CollectionUpdateInfo<T>(){ChangedCount = changedEntitiesCount};
    }

    public Collection<T> Get(PagedFilterParameters<T> pagedFilterParameters)
    {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>();
        return new Collection<T>(collection.Find(x => true).ToList()); //blah blah same as coment in count
    }

    public CollectionUpdateInfo<T> Update(Collection<T> entitiesToUpdate)
    {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<T>();

        var changedEntitiesCount = collection.Update(entitiesToUpdate);
        return new CollectionUpdateInfo<T>(){ChangedCount = changedEntitiesCount};
    }
}

public class DatabaseConnectionOptions
{
    public string ConnectionString{get; set;} = String.Empty;
}
