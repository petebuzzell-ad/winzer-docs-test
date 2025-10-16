using Microsoft.Extensions.Configuration;
using Winzer.Library.Util;
using Cql.Middleware.Library.Util;
using Cql.Middleware.Impl.Util;
using Microsoft.Extensions.Logging;

namespace Winzer.Impl.Util;

public class FileServiceFactory : IFileServiceFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _logger;
    public FileServiceFactory(IConfiguration configuration, ILoggerFactory logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public IFileService GetFileService(string name)
    {
        if (name == "KWI")
            return KWIFileService();
        else
            return DefaultFileService();
    }

    public IFileService DefaultFileService()
    {
        var sftpOptions = _configuration.GetSection("Sftp").Get<SftpOptions>();
        if (sftpOptions.Port == 21)
        {
            return new FtpService(sftpOptions, _logger.CreateLogger<FtpService>());
        }
        else
        {
            return new SftpService(sftpOptions, _logger.CreateLogger<SftpService>());
        }
    }

    private IFileService KWIFileService()
    {
        var sftpOptions = _configuration.GetSection("KWI:Sftp").Get<SftpOptions>();
        if (sftpOptions.Port == 21)
        {
            return new FtpService(sftpOptions, _logger.CreateLogger<FtpService>());
        }
        else
        {
            return new SftpService(sftpOptions, _logger.CreateLogger<SftpService>());
        }
    }
}
