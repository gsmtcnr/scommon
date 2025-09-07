using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace scommon.FileUploads.AzureStorage;

public class AzureStorageFileUpload : IFileUploader
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureStorageFileUpload(string connectionString)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    private static string GetContentType(ByteFileUploadRequest data)
    {
        var contentType = data.FileExtension;
        if (contentType == "pdf")
            contentType = "application/pdf";
        if (contentType == "svg")
        {
            contentType = "image/svg+xml";
        }

        return contentType;
    }

    public async Task UploadAsyncFile(ByteFileUploadRequest req)
    {
        // Blob container client
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(req.Container);
        var filePath = $"{(!string.IsNullOrEmpty(req.Path) ? string.Format("{0}/", req.Path) : string.Empty)}{req.FileName}.{req.FileExtension}";
        // Blob client
        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = GetContentType(req)
        };
        var blobClient = blobContainerClient.GetBlobClient(filePath);
        await using var memoryStream = new MemoryStream(req.FileData!);
        await blobClient.UploadAsync(memoryStream, httpHeaders: blobHttpHeaders);
        await memoryStream.DisposeAsync();
    }

    public Task<string> CreateSignedUrl(SignedUrlRequest req)
    {
        // URL'den container ve blobName'i çıkarıyoruz
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(req.Container);

        // Container client oluştur
        var blobClient = blobContainerClient.GetBlobClient(req.FilePath);

        if (!blobClient.CanGenerateSasUri)
            throw new InvalidOperationException("SAS URL oluşturulamıyor. Container client SAS URI üretmeye yetkili değil.");

        // SAS token ayarları
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = req.Container,
            BlobName = req.FilePath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(req.Expiry)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        // SAS URL oluştur
        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return Task.FromResult(sasUri.ToString());
    }
}
