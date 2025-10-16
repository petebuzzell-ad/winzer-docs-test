
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Cql.Middleware.Library.Util;

namespace Cql.Middleware.Impl.Util
{
    public class S3Service : IFileService
    {
        IAmazonS3 _awsS3Client { get; set; }
        S3Options _options { get; set; }

        public S3Service(IAmazonS3 s3Client, S3Options options)
        {
            _awsS3Client = s3Client;
            _options = options;
        }

        public async Task<byte[]> DownloadFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var key = CleanS3Key(path);
            MemoryStream? ms = null;
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key
            };
            using var response = await _awsS3Client.GetObjectAsync(getObjectRequest, cancellationToken);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                using (ms = new MemoryStream())
                {
                    await response.ResponseStream.CopyToAsync(ms, cancellationToken);
                }
            }
            if (ms == null || ms.ToArray().Length < 1)
                throw new FileNotFoundException(string.Format("The document '{0}' is not found", key));

            return ms.ToArray();
        }

        public async Task<bool> UploadFileAsync(byte[] fileStream, string path, CancellationToken cancellationToken = default)
        {
            var key = CleanS3Key(path);
            using var ms = new MemoryStream(fileStream);
            await _awsS3Client.UploadObjectFromStreamAsync(_options.BucketName, key, ms, new Dictionary<string, object>(), cancellationToken);
            return true;
        }

        public async Task<IEnumerable<string>> ListDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            var key = CleanS3Key(path);
            return await _awsS3Client.GetAllObjectKeysAsync(_options.BucketName, key, new Dictionary<string, object>());
        }

        public async Task<bool> MoveFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            var key = CleanS3Key(path);
            var newKey = CleanS3Key(newPath);
            var response = await _awsS3Client.CopyObjectAsync(_options.BucketName, key, _options.BucketName, newKey, cancellationToken);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return await DeleteFileAsync(path, cancellationToken);
            }
            return false;
        }

        public async Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var key = CleanS3Key(path);
            var response = await _awsS3Client.DeleteObjectAsync(_options.BucketName, key, cancellationToken);
            return response.HttpStatusCode == HttpStatusCode.NoContent;
        }

        private string CleanS3Key(string key)
        {
            return key.TrimStart('/');
        }
    }
}
