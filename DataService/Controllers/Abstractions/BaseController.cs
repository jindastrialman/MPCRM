using DataService.Models;
using DataService.Services.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Controllers.Abstractions
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<T> : ControllerBase where T : BaseDatabaseEntity //mean to be abstract
    {
        private readonly ILogger<BaseController<T>> _logger;
        private readonly IDataProviderService<T> _dataProviderService;
        public BaseController(ILogger<BaseController<T>> logger, IDataProviderService<T> dataProviderService)
        {
            _logger = logger;
            _dataProviderService = dataProviderService;
        }
    }
}
