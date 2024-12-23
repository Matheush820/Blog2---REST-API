using Blog2.Attributes;
using Microsoft.AspNetCore.Mvc;

//Health check
namespace Blog2.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet("")]
    //[ApiKey]
    public IActionResult Get([FromServices]IConfiguration config)
    {
        var env = config.GetValue<string>("Env");
        return Ok(new
        {
           environment = env
        });
    }
}

