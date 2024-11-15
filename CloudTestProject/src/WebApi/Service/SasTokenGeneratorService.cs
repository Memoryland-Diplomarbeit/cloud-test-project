using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace CloudTestProject.Service;

public class SasTokenGeneratorService
{
    private static double TokenLifeTimeInHours { get; } = 0.25; //TODO: what length should this have?

    private static async Task<UserDelegationKey> RequestUserDelegationKey(BlobServiceClient blobServiceClient)
    {
        return await blobServiceClient.GetUserDelegationKeyAsync(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(TokenLifeTimeInHours));
    }
    
    public static async Task<Uri> CreateUserDelegationSasBlob(
        BlobClient blobClient,
        BlobServiceClient blobServiceClient)
    {
        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b", // Blob resource
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(TokenLifeTimeInHours)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

        // Add the SAS token to the blob URI
        var uriBuilder = new BlobUriBuilder(blobClient.Uri)
        {
            // Specify the user delegation key
            Sas = sasBuilder.ToSasQueryParameters(
                await RequestUserDelegationKey(blobServiceClient),
                blobClient
                    .GetParentBlobContainerClient()
                    .GetParentBlobServiceClient()
                    .AccountName)
        };

        return uriBuilder.ToUri();
    }
    
    public static async Task<Uri> CreateUserDelegationSasContainer(
        BlobContainerClient containerClient,
        BlobServiceClient blobServiceClient)
    {
        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = containerClient.Name,
            Resource = "c", // container resource
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(TokenLifeTimeInHours)
        };

        // Specify the necessary permissions
        sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

        // Add the SAS token to the blob URI
        var uriBuilder = new BlobUriBuilder(containerClient.Uri)
        {
            // Specify the user delegation key
            Sas = sasBuilder.ToSasQueryParameters(
                await RequestUserDelegationKey(blobServiceClient),
                containerClient.GetParentBlobServiceClient().AccountName)
        };

        return uriBuilder.ToUri();
    }
}