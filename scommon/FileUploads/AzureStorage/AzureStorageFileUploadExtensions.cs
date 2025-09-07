using Microsoft.Extensions.DependencyInjection;

namespace scommon.FileUploads.AzureStorage;

public static class AzureStorageFileUploadExtensions
{
    public static void AddAzureStorage(this IServiceCollection serviceCollection, string? connectionString)
    {
        var azureStorageFileUpload = new AzureStorageFileUpload(connectionString!);

        serviceCollection.AddSingleton<IFileUploader>(azureStorageFileUpload);
    }
}
