using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Core.Entities;

namespace CloudTestProject.Service;

public class BlobStoragePhotoService(BlobServiceClient blobServiceClient)
{
    private BlobServiceClient BlobSvcClient { get; set; } = blobServiceClient;

    public async Task UploadPhoto(
        string username, 
        string albumName, 
        string photoName, 
        byte[] photoBytes)
    {
        var containerClient = BlobSvcClient.GetBlobContainerClient(
            username);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(
            $"{albumName}/{photoName}");

        using var stream = new MemoryStream(photoBytes);
        await blobClient.UploadAsync(stream, overwrite: true);
    }
}