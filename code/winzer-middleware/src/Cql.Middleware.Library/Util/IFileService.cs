namespace Cql.Middleware.Library.Util;

public interface IFileService
{
    public Task<byte[]> DownloadFileAsync(string path, CancellationToken cancellationToken = default);
    public Task<bool> UploadFileAsync(byte[] fileStream, string path, CancellationToken cancellationToken = default);
    public Task<IEnumerable<string>> ListDirectoryAsync(string path, CancellationToken cancellationToken = default);
    public Task<bool> MoveFileAsync(string path, string newPath, CancellationToken cancellationToken = default);
    public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default);
}

public class S3Options
{
    public String BucketName { get; set; } = String.Empty;
}

public class SftpOptions
{
    public String Hostname { get; set; } = String.Empty;
    public String Username { get; set; } = String.Empty;
    public String Password { get; set; } = String.Empty;
    public int Port { get; set; } = 22;
    public int RetryCount { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
}
