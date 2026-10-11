using AuthService.Security;
using DataService.Models;
using DataService.Services.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Controllers.Abstractions
{
    public class RandomNumberController : BaseController<RandomNumber>
    {
        public RandomNumberController(ILogger<BaseController<RandomNumber>> logger) : base(logger/*, IDataProviderService<T> dataProviderService*/)
        {
        }

        [HttpGet("rand")]
        [Authorize(Policy = Permissions.ContactsWrite)]
        public string GetRandomNumber() => "67";

        [HttpGet("semirand")]
        [AllowAnonymous] 
        public string GetSemiRandomNumber() => "69";
    }

    public class RandomNumber : BaseDatabaseEntity
    {
        
    }
}