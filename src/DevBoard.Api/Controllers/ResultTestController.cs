using DevBoard.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultTestController : ControllerBase
    {
        [HttpGet("test")]
        public ActionResult<Result<string>> Test()
        {
            var result = Result<string>.Success("Hello From result Controller"); // ✅ Clean and readable

            return Ok(result);
        }
    }
}
