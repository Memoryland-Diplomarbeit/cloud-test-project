using CloudTestProject.ApiControllers;
using Core.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace CloudTestProject.Controllers;

[Route("photos")]
public class PhotoApiController : ApiControllerBase
{
    #region Properties and Constructors

    public ApplicationDbContext Context { get; set; }

    public PhotoApiController(ApplicationDbContext context)
    {
        Context = context;
    }

    #endregion
    
    [HttpGet]
    [Route("{albumId:int}/{photoName}")]
    public Results<NotFound, Ok<PhotoDto>> GetImage(int albumId, string photoName)
    {
        if (!Context.Photos.Any()) return TypedResults.NotFound();
        
        var photo = Context.Photos
            .FirstOrDefault(p => 
                p.PhotoAlbumId == albumId && p.Name == photoName);
            
        if (photo != null)
        {
            return TypedResults.Ok(
                new PhotoDto(
                    photo.Name,
                    photo.PhotoAlbumId,
                    [0]));
        }

        return TypedResults.NotFound();
    }
}