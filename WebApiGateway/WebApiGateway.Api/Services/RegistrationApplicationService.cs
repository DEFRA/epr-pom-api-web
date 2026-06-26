using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.RegistrationFeeCalculation;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.Api.Services;

public class RegistrationApplicationService(
    ISubmissionStatusClient submissionStatusClient,
    IRegistrationFeeCalculationDetailsClient registrationFeeCalculationDetailsClient,
    IPaymentServiceClient paymentServiceClient)
    : IRegistrationApplicationService
{
    public async Task<RegistrationApplicationDetails?> GetRegistrationApplicationDetails(string request)
    {
        var result = await submissionStatusClient.GetRegistrationApplicationDetails(request);

        if (result?.LastSubmittedFile?.FileId is not null && result.IsSubmitted && result.SubmissionId is not null)
        {
            RegistrationFeeCalculationDetails[]? registrationFeeCalculationDetails;
            
            // call payments to get non-legacy fees
            registrationFeeCalculationDetails = await paymentServiceClient.GetRegistrationFeeCalculationDetails(result.SubmissionId.Value);

            if (registrationFeeCalculationDetails is null)
            {
                // get legacy fees
                registrationFeeCalculationDetails = await registrationFeeCalculationDetailsClient.GetRegistrationFeeCalculationDetails(result.LastSubmittedFile.FileId.Value);
            }
            
            result.RegistrationFeeCalculationDetails = registrationFeeCalculationDetails;
        }

        return result;
    }
}