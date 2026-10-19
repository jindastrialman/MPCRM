using System;
using Framework.Query;

namespace Framework.Interceptors.Models;

public class InterceptResult<T>
{
    public QueryParameters<T>? queryParameters{get; set;}
    public InterceptStatus InterceptStatus{get; set;}
}
public enum InterceptStatus
{
    Allowed,
    Forbidden,
    ParameterUpdated,
    ExceptionAccured // should be never invoked
}
