using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options
{
    [ExcludeFromCodeCoverage]
    public class PrnServiceApiOptions
    {
        public const string Section = "PrnServiceApi";

        public string BaseUrl { get; set; }

        public int Timeout { get; set; }

        public string ClientId { get; set; }
    }
}
