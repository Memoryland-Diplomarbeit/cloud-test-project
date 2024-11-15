using Azure.Storage.Blobs;

namespace CloudTestProject.Service;

public class BlobStoragePhotoService
{
    public BlobStoragePhotoService(BlobServiceClient blobServiceClient, 
        IConfiguration config)
    {
        BlobSvcClient = blobServiceClient;
        var accessKey = config.GetValue<string>("ConnectionStrings:BlobStorageDefault");
        
        if (string.IsNullOrEmpty(accessKey))
            throw new NullReferenceException(nameof(accessKey));
        
        var values = accessKey.Split(';');
        
        foreach (var value in values)
        {
            if (value.Contains("AccountKey"))
            {
                AccessKey = value.Replace("AccountKey=", "");
                return;
            }
        }
        
        // null-reference exception since the access key isn't set in the config
        throw new NullReferenceException(nameof(accessKey));
    }

    private BlobServiceClient BlobSvcClient { get; set; }
    private string AccessKey { get; }

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

    public async Task<Uri?> GetPhoto(
        string username, 
        string albumName, 
        string photoName)
    {
        var containerClient = BlobSvcClient.GetBlobContainerClient(
            username);

        if (!await containerClient.ExistsAsync()) return null;
        
        var blobClient = containerClient.GetBlobClient(
            $"{albumName}/{photoName}");

        if (await blobClient.ExistsAsync())
        {
            return SasTokenGeneratorService
                .CreateUserDelegationSasBlob(
                    blobClient, 
                    BlobSvcClient,
                    AccessKey);
        }

        return null;
    }
}