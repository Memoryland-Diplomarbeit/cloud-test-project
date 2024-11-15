using CloudTestProject.ApiControllers;
using Core.DTO;
using Core.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace CloudTestProject.Controllers;

public class UserController : ApiControllerBase
{
    #region Properties and Constructors

    private ApplicationDbContext Context { get; set; }

    public UserController(ApplicationDbContext context)
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
    public async Task<Created> PostUser(UserDto userDto)
    {
        //TODO: (From: AZURE)
        // This name may only contain lowercase letters,
        // numbers, and hyphens, and must begin with a letter
        // or a number. Each hyphen must be preceded and followed
        // by a non-hyphen character. The name must also be
        // between 3 and 63 characters long.
        
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