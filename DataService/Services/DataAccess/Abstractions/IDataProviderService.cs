using DataService.Models;
using Framework.Query;

namespace DataService.Services.DataAccess.Abstractions;

public interface IDataProviderService<T> where T : BaseDatabaseEntity
{
    public QueryResult<T> Get(QueryParameters<T> parameters); //should add filterParams and overloads
    public QueryResult<T> Add(QueryParameters<T> parameters);
    public QueryResult<T> Update(QueryParameters<T> parameters);
    public QueryResult<T> Delete(QueryParameters<T> parameters);
    public int Count(QueryParameters<T> parameters);
}
