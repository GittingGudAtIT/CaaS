using CaaS.Core.BusinessLogic.Common;
using Microsoft.AspNetCore.Mvc;

namespace CaaS.Api.Controllers.Common
{
    public class ControllerCaasBase : ControllerBase
    {
        protected ActionResult ResultFromRequestResult(RequestResult requestResult)
        {
            if (requestResult == RequestResult.NoPermission)
                return BadRequest();
            else if (requestResult == RequestResult.Failure)
                return NotFound();
            else if (requestResult == RequestResult.Success)
                return Ok();
            throw new NotImplementedException();
        }

        protected ActionResult<T> ResultFromRequestResult<T>(RequestResult requestResult, T valueIfSuccess)
        {
            if (requestResult == RequestResult.NoPermission)
                return BadRequest();
            else if (requestResult == RequestResult.Failure)
                return NotFound();
            else if (requestResult == RequestResult.Success)
                return valueIfSuccess;
            throw new NotImplementedException();
        }
    }
}
