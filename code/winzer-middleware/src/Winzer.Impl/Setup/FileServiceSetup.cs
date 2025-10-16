using Amazon.S3;
using Winzer.Impl.Util;
using Winzer.Library.Util;
using Cql.Middleware.Impl.Util;
using Cql.Middleware.Library.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Winzer.Impl.Setup
{
    public static class FileServiceSetup
    {
        public static void ConfigureFileService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            var S3ConfigSection = configuration.GetSection("S3");
            if (S3ConfigSection.Exists() && S3ConfigSection.GetChildren().Any(x => x.Key == "BucketName"))
            {
                services.AddTransient<IFileService, S3Service>();
                services.AddSingleton<S3Options>(S3ConfigSection.Get<S3Options>());
                services.AddAWSService<IAmazonS3>();
            }
            else
            {
                services.AddTransient<IFileService, FtpService>();
                services.AddSingleton<SftpOptions>(configuration.GetSection("Sftp").Get<SftpOptions>());
            }
            services.AddSingleton<IFileServiceFactory, FileServiceFactory>();
        }
    }
}
