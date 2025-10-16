using Microsoft.Extensions.Logging;

using Cql.Middleware.Library.Util;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Cql.Middleware.Impl.Util
{
    public class SftpService : IFileService
    {
        private readonly ILogger<SftpService> _logger;
        private readonly SftpOptions _options;
        private readonly ConnectionInfo _connectionInfo;

        public SftpService(SftpOptions options, ILogger<SftpService> logger)
        {
            _options = options;
            _logger = logger;
            _connectionInfo = new ConnectionInfo(options.Hostname, options.Port, options.Username, new PasswordAuthenticationMethod(options.Username, options.Password));
            _connectionInfo.Timeout = _options.Timeout;
        }

        public async Task<byte[]> DownloadFileAsync(string path, CancellationToken cancellationToken = default)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    using var client = new SftpClient(_connectionInfo);
                    client.Connect();

                    using var ms = new MemoryStream();
                    await Task.Factory.FromAsync(client.BeginDownloadFile(path, ms), client.EndDownloadFile);

                    return ms.ToArray();
                }

                catch (Renci.SshNet.Common.SshException ex)
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
                    using var client = new SftpClient(_connectionInfo);
                    client.Connect();

                    using var ms = new MemoryStream(fileStream);
                    await Task.Factory.FromAsync(client.BeginUploadFile(ms, path), client.EndUploadFile);
                    return true;
                }

                catch (Renci.SshNet.Common.SshException ex)
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
                    using var client = new SftpClient(_connectionInfo);
                    client.Connect();

                    var files = await Task.Factory.FromAsync<IEnumerable<SftpFile>>((callback, obj) => client.BeginListDirectory(path, callback, obj), client.EndListDirectory, null);

                    return files.Select(f => f.FullName).ToList();
                }

                catch (Renci.SshNet.Common.SshException ex)
                {
                    if (attempts++ >= _options.RetryCount)
                    {
                        throw;
                    }
                    _logger.LogDebug("On attempt {0} ftp execution failed with exception: {1}", attempts, ex.Message);
                }
            }
        }

        public bool MoveFile(string path, string newPath)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    using var client = new SftpClient(_connectionInfo);
                    client.Connect();

                    client.RenameFile(path, newPath);

                    return true;
                }

                catch (Renci.SshNet.Common.SshException ex)
                {
                    if (attempts++ >= _options.RetryCount)
                    {
                        throw;
                    }
                    _logger.LogDebug("On attempt {0} ftp execution failed with exception: {1}", attempts, ex.Message);
                }
            }
        }

        public Task<bool> MoveFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            // There isn't an Async version of the SFTP service
            return Task.FromResult(MoveFile(path, newPath));
        }

        public bool DeleteFile(string path)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    using var client = new SftpClient(_connectionInfo);
                    client.Connect();

                    client.DeleteFile(path);

                    return true;
                }

                catch (Renci.SshNet.Common.SshException ex)
                {
                    if (attempts++ >= _options.RetryCount)
                    {
                        throw;
                    }
                    _logger.LogDebug("On attempt {0} ftp execution failed with exception: {1}", attempts, ex.Message);
                }
            }
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            // There isn't an Async version of the SFTP service
            return Task.FromResult(DeleteFile(path));
        }
    }
}
