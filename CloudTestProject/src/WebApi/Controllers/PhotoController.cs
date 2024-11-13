using CloudTestProject.ApiControllers;
using Core.DTO;
using Core.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace CloudTestProject.Controllers;

public class PhotoController : ApiControllerBase
{
    #region Properties and Constructors

    private ApplicationDbContext Context { get; set; }

    public PhotoController(ApplicationDbContext context)
    {
        Context = context;
    }

    #endregion
    
    [HttpGet]
    [Route("{albumId:int}/{photoName}")]
    public Results<NotFound, Ok<byte[]>, BadRequest<string>> GetImage(int albumId, string photoName)
    {
        var result = GetImageWithDetails(albumId, photoName);
        
        if (result.Result is BadRequest<string> badRequest) return badRequest;
        
        if (result.Result is not Ok<PhotoDto> ok) return TypedResults.NotFound();
        
        var photo = ok.Value;

        if (photo != null)
        {
            return TypedResults.Ok(photo.Photo);
        }

        return TypedResults.NotFound();
    }
    
    [HttpGet]
    [Route("{albumId:int}/{photoName}/details")]
    public Results<NotFound, Ok<PhotoDto>, BadRequest<string>> GetImageWithDetails(int albumId, string photoName)
    {
        if (!Context.Photos.Any()) return TypedResults.NotFound();
        
        if (!Context.PhotoAlbums.Any(pa => pa.Id == albumId))
        {
            return TypedResults.BadRequest("No photo album found");
        }
        
        var photo = Context.Photos
            .FirstOrDefault(p => 
                p.PhotoAlbumId == albumId && p.Name == photoName);

        if (photo == null) return TypedResults.NotFound();
        
        var photoDto = new PhotoDto(
            photo.Name,
            photo.PhotoAlbumId,
            [0]);
            
        return TypedResults.Ok(photoDto);

    }
    
    [HttpPost]
    public async Task<Results<Created, BadRequest<string>>> PostImage([FromForm] PostPhotoDto<IFormFile> photoDto)
    {
        if (!Context.PhotoAlbums.Any(pa => pa.Id == photoDto.PhotoAlbumId))
        {
            return TypedResults.BadRequest("No photo album found");
        }
        
        if (photoDto.Photo.Length == 0)
        {
            return TypedResults.BadRequest("No image file provided.");
        }
        
        byte[] photoData;
        using (var memoryStream = new MemoryStream())
        {
            await photoDto.Photo.CopyToAsync(memoryStream);
            photoData = memoryStream.ToArray();
        }
        
        var photo = new Photo
        {
            Name = photoDto.FileName,
            PhotoAlbumId = photoDto.PhotoAlbumId
        };
        //TODO: save photoData
        
        await Context.AddAsync(photo);
        await Context.SaveChangesAsync();
        return TypedResults.Created();
    }
}