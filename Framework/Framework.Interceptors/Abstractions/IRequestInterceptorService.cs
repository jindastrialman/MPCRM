using Framework.Interceptors.Models;
using Framework.Query;

namespace Framework.Interceptors.Abstractions;

public interface IRequestInterceptorService<T> 
{
    public InterceptResult<T> InvokeAll(QueryParameters<T> queryParameters);
}
