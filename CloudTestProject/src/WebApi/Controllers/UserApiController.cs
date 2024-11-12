using CloudTestProject.ApiControllers;
using Core.DTO;
using Core.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace CloudTestProject.Controllers;

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
    
    [HttpPost]
    public async Task<Created> PostUsers(UserDto userDto)
    {
        User user = new User
        {
            Email = userDto.Email,
            Username = userDto.Username,
            PhotoAlbums = new HashSet<PhotoAlbum>(),
            Memorylands = new HashSet<Memoryland>()
        };
        
        await Context.AddAsync(user);
        await Context.SaveChangesAsync();
        return TypedResults.Created();
    }
}