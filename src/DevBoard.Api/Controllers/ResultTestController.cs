using DevBoard.Application.Dtos;
using DevBoard.Domain.Common;
using DevBoard.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevBoard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultTestController : ControllerBase
    {
        //[HttpGet("test")]
        //public ActionResult<Result<string>> Test()
        //{
        //    var result = Result<string>.Success("Hello From result Controller"); // ✅ Clean and readable

        //    return Ok(result);
        //}
    }
}
