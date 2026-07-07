using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Logging;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.Api.Services;

public class RegistrationApplicationService(
    ISubmissionStatusClient submissionStatusClient,
    IRegistrationFeeCalculationDetailsClient registrationFeeCalculationDetailsClient,
    IPaymentServiceClient paymentServiceClient,
    ILogger<RegistrationApplicationService> logger)
    : IRegistrationApplicationService
{
    public async Task<RegistrationApplicationDetails?> GetRegistrationApplicationDetails(string request)
    {
        logger.LogInformation("Get registration application details from submissions api");
        var result = await submissionStatusClient.GetRegistrationApplicationDetails(request);

        if (result?.LastSubmittedFile?.FileId is not null && result.IsSubmitted && result.SubmissionId is not null)
        {

            using (logger.AddScopedData(new Dictionary<string, object>
                   {
                       ["FileId"] = result.LastSubmittedFile.FileId,
                       ["SubmissionId"] = result.SubmissionId
                   }))
            {
                logger.LogInformation("Registration file has been submitted.");

                // call payments to get non-legacy fees
                var registrationFeeCalculationDetails = await paymentServiceClient.GetRegistrationFeeCalculationDetails(result.SubmissionId.Value);

                if (registrationFeeCalculationDetails is null)
                {
                    // get legacy fees
                    registrationFeeCalculationDetails =
                        await registrationFeeCalculationDetailsClient.GetRegistrationFeeCalculationDetails(
                            result.LastSubmittedFile.FileId.Value);
                }

                result.RegistrationFeeCalculationDetails = registrationFeeCalculationDetails;
            }
        }

        return result;
    }
}