using Framework.DataAccess.Models;
using Framework.Interceptors.Abstractions;
using Framework.Interceptors.Models;
using Framework.Query;

namespace Framework.Interceptors.Common;

public class CreatedAtInterceptor<T> : IRequestInterceptor<T> where T : IBaseEntity
{
    public InterceptResult<T> Invoke(QueryParameters<T> queryParameters)
    {
        foreach(var entity in queryParameters.CollectionParameters.Collection)
        {
            if (entity.CreatedAt is null) entity.CreatedAt = DateTime.Now;
            if (entity.Version is null) entity.Version = 0;
        }
        return new InterceptResult<T>(){QueryParameters = queryParameters, InterceptStatus = InterceptStatus.ParametersUpdated};
    }
}
