using Framework.Interceptors.Models;
using Framework.Query;

namespace Framework.Interceptors.Abstractions;

public interface IRequestInterceptor<T> 
{
    public InterceptResult<T> Invoke(QueryParameters<T> queryParameters);
}
