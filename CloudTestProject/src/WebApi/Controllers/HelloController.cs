using CloudTestProject.ApiControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace CloudTestProject.Controllers;

public class HelloController : ApiControllerBase
{
    private readonly IHttpContextAccessor _contextAccessor;
    
    public HelloController(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }
    
    [HttpGet]
    public Ok<string> GetHello()
    {
        return TypedResults.Ok("Hello!");
    }
    
    [HttpGet("loggedin")]
    [Authorize]
    [RequiredScope("backend.read")]
    public Ok<string> GetHelloLogin()
    {
        return TypedResults.Ok($"Hello, {User.Claims.FirstOrDefault(c => c.Type.ToLower() == "name")?.Value}!");
    }
    
}