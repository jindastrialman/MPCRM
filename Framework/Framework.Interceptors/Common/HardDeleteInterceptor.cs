using Framework.DataAccess.Models;
using Framework.Interceptors.Abstractions;
using Framework.Interceptors.Models;
using Framework.Query;

namespace Framework.Interceptors.Common;

public class HardDeleteInterceptor<T> : IRequestInterceptor<T> where T : IBaseEntity
{
    public InterceptResult<T> Invoke(QueryParameters<T> queryParameters)
    {
        if (queryParameters.DeleteParameters.ForcedDelete && !queryParameters.AuthParameters.Permissions.Contains("harddelete"))
            return new InterceptResult<T>(){QueryParameters = queryParameters, InterceptStatus = InterceptStatus.Forbidden};

        return new InterceptResult<T>(){QueryParameters = queryParameters, InterceptStatus = InterceptStatus.ParametersUpdated};
    }
}
