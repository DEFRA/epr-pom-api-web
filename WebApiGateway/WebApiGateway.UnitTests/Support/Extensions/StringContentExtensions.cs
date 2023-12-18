namespace WebApiGateway.UnitTests.Support.Extensions;

using System.Text;
using Newtonsoft.Json;

public static class StringContentExtensions
{
    public static StringContent ToJsonContent(this object parameters)
    {
        var jsonContent = JsonConvert.SerializeObject(parameters);
        return new StringContent(jsonContent, Encoding.UTF8, "application/json");
    }
}