using System.Collections.ObjectModel;
using DataService.Models;

namespace DataService.Services.DataAccess;

public interface IDataProviderService<T> where T : BaseDatabaseEntity
{
    public Collection<T> Get();
    public Collection<T> Add();
    public Collection<T> Update();
    public Collection<T> Delete();
    public int Count();
}
