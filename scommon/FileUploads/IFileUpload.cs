namespace scommon.FileUploads;

public interface IFileUploader
{
    Task UploadAsyncFile(ByteFileUploadRequest req);
    Task<string> CreateSignedUrl(SignedUrlRequest req);
}

public class ByteFileUploadRequest
{
    public required string Container { get; set; }
    public string Path { get; set; }
    public byte[] FileData { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
}

public class SignedUrlRequest
{
    public required string Container { get; set; }
    public string FilePath { get; set; }
    public TimeSpan Expiry { get; set; }
}
