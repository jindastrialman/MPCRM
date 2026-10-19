using AuthService.Security;
using DataService.Models;
using DataService.Services.DataAccess;
using DataService.Services.DataAccess.Abstractions;
using Framework.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Controllers.Abstractions
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public abstract class BaseController<T> : ControllerBase where T : BaseLiteDbModel //mean to be abstract
    {
        private readonly ILogger<BaseController<T>> _logger;
        private readonly IDataProviderService<T> _dataProviderService;
        public BaseController(ILogger<BaseController<T>> logger, IDataProviderService<T> dataProviderService)
        {
            _logger = logger;
            _dataProviderService = dataProviderService;
        }

        public QueryResult<T> Get(QueryParameters<T> parameters)
        {
            return _dataProviderService.Get(parameters);
        }
        public QueryResult<T> Put(QueryParameters<T> parameters)
        {
            return _dataProviderService.Add(parameters);
        }
        public QueryResult<T> Post(QueryParameters<T> parameters)
        {
            return _dataProviderService.Update(parameters);
        }
        public QueryResult<T> Delete(QueryParameters<T> parameters)
        {
            return _dataProviderService.Delete(parameters);
        }
    }
}
