using System.Collections.Specialized;
using System.Web;

namespace WebApiGateway.Core.Helpers
{
    public static class UriBuilderHelper
    {
        public static UriBuilder UriBuilder(string queryString)
        {
            var uriBuilder = new UriBuilder();
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            var queryActions = new Dictionary<string, Action>()
            {
                { "decisions", () => HandleDecisionsQuery(query, queryString) },
                { "submissions", () => HandleSubmissionsQuery(query, queryString) }
            };

            uriBuilder.Query = query.ToString();
            return uriBuilder;
        }

        public static UriBuilder UriBuilder(Guid submissionId, string queryString)
        {
            var uriBuilder = new UriBuilder()
            {
                Path = $"submissions/events/events-by-type/{submissionId}"
            };
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["querystring"] = queryString;
            uriBuilder.Query = query.ToString();
            return uriBuilder;
        }

        private static void HandleSubmissionsQuery(NameValueCollection query, string queryString)
        {
            query["submissions"] = queryString;
        }

        private static void HandleDecisionsQuery(NameValueCollection query, string queryString)
        {
            query["decisions"] = queryString;
        }
    }
}
