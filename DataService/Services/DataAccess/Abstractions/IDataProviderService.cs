using System.Collections.ObjectModel;
using DataService.Models;

namespace DataService.Services.DataAccess.Abstractions;

public interface IDataProviderService<T> where T : BaseDatabaseEntity
{
    public Collection<T> Get(PagedFilterParameters<T> pagedFilterParameters); //should add filterParams and overloads
    public CollectionUpdateInfo<T> Add(Collection<T> entitiesToUpdate);
    public CollectionUpdateInfo<T> Update(Collection<T> entitiesToUpdate);
    public CollectionUpdateInfo<T> Delete(Collection<T> entitiesToDelete, DeleteParameters? deleteParameters = null);
    public int Count(FilterParameters<T> filterParameters);
}
