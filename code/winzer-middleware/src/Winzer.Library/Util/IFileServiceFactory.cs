using Cql.Middleware.Library.Util;
namespace Winzer.Library.Util;

public interface IFileServiceFactory
{
    IFileService GetFileService(string name);
    IFileService DefaultFileService();
}
