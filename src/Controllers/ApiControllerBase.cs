using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected string ipAddress()
    {
        // get source ip address for the current request
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }
}