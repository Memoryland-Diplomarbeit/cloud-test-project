using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace CloudTestProject.Service;

public static class SasTokenGeneratorService
{
    private static double TokenLifeTimeInHours { get; } = 1; //TODO: what length should this have?

    private static async Task<UserDelegationKey> RequestUserDelegationKey(
        BlobServiceClient blobServiceClient)
    {
        //TODO: manage token lifetime and don't always get new token
        return await blobServiceClient.GetUserDelegationKeyAsync(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(TokenLifeTimeInHours));
    }
    
    public static Uri CreateUserDelegationSasBlob(
        BlobClient blobClient,
        BlobServiceClient blobServiceClient,
        string accessKey)
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

        // var userDelegationKey = await RequestUserDelegationKey(blobServiceClient);
        //
        // // Add the SAS token to the blob URI
        // var uriBuilder = new BlobUriBuilder(blobClient.Uri)
        // {
        //     // Specify the user delegation key
        //     Sas = sasBuilder.ToSasQueryParameters(
        //         userDelegationKey,
        //         blobClient
        //             .GetParentBlobContainerClient()
        //             .GetParentBlobServiceClient()
        //             .AccountName)
        // };

        var blobSvcClient = blobClient
            .GetParentBlobContainerClient()
            .GetParentBlobServiceClient();
        
        var storageSharedKeyCredential = new StorageSharedKeyCredential(
            blobSvcClient.AccountName,
            accessKey);

        var uriBuilder = new BlobUriBuilder(blobClient.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(storageSharedKeyCredential)
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
        
        var userDelegationKey = await RequestUserDelegationKey(blobServiceClient);

        // Add the SAS token to the blob URI
        var uriBuilder = new BlobUriBuilder(containerClient.Uri)
        {
            // Specify the user delegation key
            Sas = sasBuilder.ToSasQueryParameters(
                userDelegationKey,
                containerClient.GetParentBlobServiceClient().AccountName)
        };

        return uriBuilder.ToUri();
    }
}