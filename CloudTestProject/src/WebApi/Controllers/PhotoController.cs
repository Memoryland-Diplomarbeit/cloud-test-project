using CloudTestProject.ApiControllers;
using CloudTestProject.Service;
using Core.DTO;
using Core.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace CloudTestProject.Controllers;

public class PhotoController(
    ApplicationDbContext context,
    BlobStoragePhotoService blobStoragePhotoSvc)
    : ApiControllerBase
{
    private ApplicationDbContext Context { get; set; } = context;
    private BlobStoragePhotoService PhotoSvc { get; set; } = blobStoragePhotoSvc;

    [HttpGet]
    [Route("{albumId:int}/{photoName}")]
    public async Task<Results<NotFound, Ok<Uri>, BadRequest<string>>> GetImage(int albumId, string photoName)
    {
        var result = await GetImageWithDetails(albumId, photoName);
        
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
    public async Task<Results<NotFound, Ok<PhotoDto>, BadRequest<string>>> GetImageWithDetails(int albumId, string photoName)
    {
        if (!Context.Photos.Any()) return TypedResults.NotFound();
        
        if (!Context.PhotoAlbums.Any(pa => pa.Id == albumId))
        {
            return TypedResults.BadRequest("The photo album doesn't exist.");
        }
        
        var photo = Context.Photos
            .Include(p => p.PhotoAlbum.User)
            .FirstOrDefault(p => 
                p.PhotoAlbumId == albumId && p.Name == photoName);

        if (photo == null) return TypedResults.NotFound();
        
        var uri = await PhotoSvc.GetPhoto(
            photo.PhotoAlbum.User.Username,
            photo.PhotoAlbum.Name,
            photo.Name);
        
        if (uri == null) return TypedResults.NotFound();
        
        var photoDto = new PhotoDto(
            photo.Name,
            photo.PhotoAlbumId,
            uri);
            
        return TypedResults.Ok(photoDto);
    }
    
    [HttpPost]
    public async Task<Results<Created, BadRequest<string>>> PostImage([FromForm] PostPhotoDto<IFormFile> photoDto)
    {
        if (!Context.PhotoAlbums.Any(pa => pa.Id == photoDto.PhotoAlbumId))
        {
            return TypedResults.BadRequest("The photo album doesn't exist.");
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
        
        await Context.AddAsync(photo);
        await Context.SaveChangesAsync();
        
        var album = Context.PhotoAlbums
            .Include(photoAlbum => photoAlbum.User)
            .FirstOrDefault(pa => pa.Id == photo.PhotoAlbumId);
        
        await PhotoSvc.UploadPhoto(
            album!.User.Username,
            album.Name,
            photo.Name,
            photoData);
        
        return TypedResults.Created();
    }
}