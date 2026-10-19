using System;
using Framework.Query;

namespace Framework.Interceptors.Models;

public class InterceptResult<T>
{
    public QueryParameters<T> QueryParameters{get; init;}
    public InterceptStatus InterceptStatus{get; init;}
    public void Deconstruct(out QueryParameters<T> queryParameters, out InterceptStatus interceptStatus)
    {
        queryParameters = QueryParameters;
        interceptStatus = InterceptStatus;
    }
}
public enum InterceptStatus
{
    Allowed,
    Forbidden,
    ParametersUpdated,
    ExceptionAccured // should be never invoked
}
