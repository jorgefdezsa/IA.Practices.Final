namespace CQRS.Shared.Models
{
    public class AzureOpenAIOptions
    {
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
        public string Deployment { get; set; }
        public string ApiVersion { get; set; }
    }
}
