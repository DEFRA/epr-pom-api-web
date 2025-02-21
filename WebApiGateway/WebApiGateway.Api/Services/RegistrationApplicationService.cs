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
            result.RegistrationFeeCalculationDetails = await registrationFeeCalculationDetailsClient.GetRegistrationFeeCalculationDetails(result.LastSubmittedFile.FileId.Value);
        }

        return result;
    }
}