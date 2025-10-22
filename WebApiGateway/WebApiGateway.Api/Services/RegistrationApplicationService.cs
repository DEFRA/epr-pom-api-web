using System.Globalization;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.Api.Services;

public class RegistrationApplicationService(
    ISubmissionStatusClient submissionStatusClient,
    IRegistrationFeeCalculationDetailsClient registrationFeeCalculationDetailsClient)
    : IRegistrationApplicationService
{
    public async Task<RegistrationApplicationDetails?> GetRegistrationApplicationDetails(string request)
    {
        var result = await submissionStatusClient.GetRegistrationApplicationDetails(request);

        if (result?.LastSubmittedFile?.FileId is not null && result.IsSubmitted)
        {
            var requestParams = System.Web.HttpUtility.ParseQueryString(request);

            DateTime? largeProducerLateFeeDeadLine = null;
            if (DateTime.TryParse(requestParams["LargeProducerLateFeeDeadLine"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedLargeProducerLateFeeDeadLine))
            {
                largeProducerLateFeeDeadLine = parsedLargeProducerLateFeeDeadLine;
            }

            DateTime? smallProducerLateFeeDeadLine = null;
            if (DateTime.TryParse(requestParams["SmallProducerLateFeeDeadLine"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedSmallProducerLateFeeDeadLine))
            {
                smallProducerLateFeeDeadLine = parsedSmallProducerLateFeeDeadLine;
            }

            result.RegistrationFeeCalculationDetails = await registrationFeeCalculationDetailsClient.GetRegistrationFeeCalculationDetails(result.LastSubmittedFile.FileId.Value, largeProducerLateFeeDeadLine, smallProducerLateFeeDeadLine);
        }

        return result;
    }
}