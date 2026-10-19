using System;
using Framework.Interceptors.Abstractions;
using Framework.Interceptors.Models;
using Framework.Query;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace Framework.Interceptors.Services;

public class RequestInterceptorService<T> : IRequestInterceptorService<T>
{
    private IEnumerable<IRequestInterceptor<T>> _interceptors;
    private ILogger<RequestInterceptorService<T>> _logger;
    public RequestInterceptorService(IEnumerable<IRequestInterceptor<T>> interceptors, ILogger<RequestInterceptorService<T>> logger)
    {
        _interceptors = interceptors;
        _logger = logger;   
    }

    public InterceptResult<T> InvokeAll(QueryParameters<T> queryParameters)
    {
        var interceptStatus = InterceptStatus.Allowed;
        var lastInterceptStatus = InterceptStatus.Allowed;

        foreach (var interceptor in _interceptors)
        {
            try
            {
                (queryParameters, lastInterceptStatus) = interceptor.Invoke(queryParameters);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception accured during interceptor {interceptor.GetType()}");
                lastInterceptStatus = InterceptStatus.ExceptionAccured;
            }

            _logger.LogDebug($"Last interceptor status result was {lastInterceptStatus}, current overall status is {interceptStatus}");

            switch (lastInterceptStatus)
            {
                case InterceptStatus.Allowed:
                    break;

                case InterceptStatus.ParametersUpdated:
                    interceptStatus = lastInterceptStatus;
                    break;

                case InterceptStatus.Forbidden or InterceptStatus.ExceptionAccured:
                    _logger.LogDebug($"Request should be denied. Resulting status is {lastInterceptStatus}");
                    return new InterceptResult<T>(){QueryParameters = queryParameters, InterceptStatus = lastInterceptStatus};
            }
        }

        _logger.LogDebug($"Request passed interceptors. Resulting status is {interceptStatus}");
                    return new InterceptResult<T>(){QueryParameters = queryParameters, InterceptStatus = interceptStatus};
    }
}
