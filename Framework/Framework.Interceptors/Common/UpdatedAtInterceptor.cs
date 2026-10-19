using Framework.DataAccess.Models;
using Framework.Interceptors.Abstractions;
using Framework.Interceptors.Models;
using Framework.Query;

namespace Framework.Interceptors.Common;

public class UpdatedAtInterceptor<T> : IRequestInterceptor<T> where T : IBaseEntity
{
    public InterceptResult<T> Invoke(QueryParameters<T> queryParameters)
    {
        foreach(var entity in queryParameters.CollectionParameters.Collection)
            entity.UpdatedAt = DateTime.Now;

        return new InterceptResult<T>(){QueryParameters = queryParameters, InterceptStatus = InterceptStatus.ParametersUpdated};
    }
}
