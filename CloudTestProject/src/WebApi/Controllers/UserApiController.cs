using Core.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace CloudTestProject.ApiControllers;

[Route("users")]
public class UserApiController : ApiControllerBase
{
    #region Properties and Constructors

    public ApplicationDbContext Context { get; set; }

    public UserApiController(ApplicationDbContext context)
    {
        Context = context;
    }

    #endregion
    
    [HttpGet]
    public Results<NotFound, Ok<IEnumerable<User>>> GetUsers()
    {
        if (!Context.Users.Any())
        {
            return TypedResults.NotFound();
        }
        
        return TypedResults.Ok(Context.Users.AsEnumerable());
    }
}