using Microsoft.Extensions.Logging;

using Cql.Middleware.Library.Util;
using CoreFtp;
using CoreFtp.Infrastructure;

namespace Cql.Middleware.Impl.Util
{
    public class FtpService : IFileService
    {
        private readonly ILogger<FtpService> _logger;
        private readonly SftpOptions _options;
        private readonly FtpClientConfiguration _clientConfig;

        public FtpService(SftpOptions options, ILogger<FtpService> logger)
        {
            _options = options;
            _clientConfig = new FtpClientConfiguration
            {
                Host = _options.Hostname,
                Username = _options.Username,
                Password = _options.Password,
                Port = _options.Port
            };
            _logger = logger;
        }

        public async Task<byte[]> DownloadFileAsync(string path, CancellationToken cancellationToken = default)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    using var ftpClient = new FtpClient(_clientConfig);
                    await ftpClient.LoginAsync();

                    using var ms = new MemoryStream();
                    using var ftpReadStream = await ftpClient.OpenFileReadStreamAsync(path);
                    await ftpReadStream.CopyToAsync(ms, cancellationToken);
                    await ms.FlushAsync(cancellationToken);
                    return ms.ToArray();
                }
                catch (FtpException ex)
                {
                    if (attempts++ >= _options.RetryCount)
                    {
                        throw;
                    }
                    _logger.LogDebug("On attempt {0} ftp execution failed with exception: {1}", attempts, ex.Message);
                }
            }
        }

        public async Task<bool> UploadFileAsync(byte[] fileStream, string path, CancellationToken cancellationToken = default)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    using var ftpClient = new FtpClient(_clientConfig);
                    await ftpClient.LoginAsync();

                    using var ms = new MemoryStream(fileStream);
                    using var writeStream = await ftpClient.OpenFileWriteStreamAsync(path);
                    await ms.CopyToAsync(writeStream, cancellationToken);
                    await writeStream.FlushAsync(cancellationToken);
                    return true;
                }
                catch (FtpException ex)
                {
                    if (attempts++ >= _options.RetryCount)
                    {
                        throw;
                    }
                    _logger.LogDebug("On attempt {0} ftp execution failed with exception: {1}", attempts, ex.Message);
                }
            }
        }

        public async Task<IEnumerable<string>> ListDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    using var ftpClient = new FtpClient(_clientConfig);
                    await ftpClient.LoginAsync();

                    await ftpClient.ChangeWorkingDirectoryAsync(path);

                    var results = new List<string>();
                    var nodes = await ftpClient.ListFilesAsync();
                    foreach (var node in nodes)
                    {
                        results.Add(path + node.Name);
                    }

                    return results;
                }
                catch (FtpException ex)
                {
                    if (attempts++ >= _options.RetryCount)
                    {
                        throw;
                    }
                    _logger.LogDebug("On attempt {0} ftp execution failed with exception: {1}", attempts, ex.Message);
                }
            }
        }

        public async Task<bool> MoveFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    using var ftpClient = new FtpClient(_clientConfig);
                    await ftpClient.LoginAsync();

                    await ftpClient.RenameAsync(path, newPath);

                    return true;
                }
                catch (FtpException ex)
                {
                    if (attempts++ >= _options.RetryCount)
                    {
                        throw;
                    }
                    _logger.LogDebug("On attempt {0} ftp execution failed with exception: {1}", attempts, ex.Message);
                }
            }
        }

        public async Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    using var ftpClient = new FtpClient(_clientConfig);
                    await ftpClient.LoginAsync();

                    await ftpClient.DeleteFileAsync(path);

                    return true;
                }
                catch (FtpException ex)
                {
                    if (attempts++ >= _options.RetryCount)
                    {
                        throw;
                    }
                    _logger.LogDebug("On attempt {0} ftp execution failed with exception: {1}", attempts, ex.Message);
                }
            }
        }
    }
}
