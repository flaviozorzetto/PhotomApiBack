using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Options;
using PhotomApi.Config;

namespace PhotomApi.Context
{
    public class AmazonContext
    {
        private readonly AwsCredentialOptions _options;
        public readonly string bucketName = "photom-image-upload-bucket";
        public readonly string rootPrefix = "posts";
        public AmazonContext(IOptions<AwsCredentialOptions> options)
        {
            _options = options.Value;
        }

        public AmazonS3Client GetConnection()
        {
            return new AmazonS3Client(_options.AccessKey, _options.SecretKey, RegionEndpoint.USEast1);
        }
    }
}
