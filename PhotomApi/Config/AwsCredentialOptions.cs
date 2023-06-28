namespace PhotomApi.Config
{
    public class AwsCredentialOptions
    {
        public string? AccessKey { get; set; }
        public string? SecretKey { get; set; }

        public override string ToString()
        {
            return $"\n AccessKey: {AccessKey} \n SecretKey: {SecretKey}";
        }
    }
}
