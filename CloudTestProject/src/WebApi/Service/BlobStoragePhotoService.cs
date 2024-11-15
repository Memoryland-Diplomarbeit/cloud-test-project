using Azure.Storage.Blobs;

namespace CloudTestProject.Service;

public class BlobStoragePhotoService(BlobServiceClient blobServiceClient)
{
    private BlobServiceClient BlobSvcClient { get; set; } = blobServiceClient;

    
    
}