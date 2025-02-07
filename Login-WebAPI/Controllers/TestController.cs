using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Login_WebAPI.Controllers
{
    [Route("api/test-session")]
    [ApiController]
    public class TestController : ControllerBase
    {

        [HttpGet]
        public IActionResult GetSession()
        {
            HttpContext.Session.SetString("TestKey", "Session is working!");
            var sessionValue = HttpContext.Session.GetString("TestKey");
            return Ok(new { message = sessionValue });
        }
    }
}
