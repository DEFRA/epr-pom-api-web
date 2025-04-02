using EPR.Common.Logging.Constants;
using EPR.Common.Logging.Models;
using EPR.Common.Logging.Services;
using Microsoft.Extensions.Options;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Constants;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Models.SubmissionHistory;
using WebApiGateway.Core.Models.Submissions;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.Services;

public class SubmissionService(
    ISubmissionStatusClient submissionStatusClient,
    ILoggingService loggingService,
    ILogger<SubmissionService> logger,
    IOptions<StorageAccountOptions> options)
    : ISubmissionService
{
    private readonly StorageAccountOptions _options = options.Value;

    public async Task<Guid> CreateSubmissionAsync(SubmissionType submissionType, string submissionPeriod, Guid? complianceSchemeId, bool? isResubmission)
    {
        var submission = new CreateSubmission
        {
            Id = Guid.NewGuid(),
            DataSourceType = DataSourceType.File,
            SubmissionType = submissionType,
            SubmissionPeriod = submissionPeriod,
            ComplianceSchemeId = complianceSchemeId,
            IsResubmission = isResubmission
        };

        await submissionStatusClient.CreateSubmissionAsync(submission);

        return submission.Id;
    }

    public async Task<Guid> CreateAntivirusCheckEventAsync(string fileName, FileType fileType, Guid submissionId, Guid? registrationSetId)
    {
        var blobContainer = fileType switch
        {
            FileType.Pom => _options.PomContainer,
            FileType.Subsidiaries => _options.SubsidiaryContainer,
            _ => _options.RegistrationContainer
        };

        var @event = new AntivirusCheckEvent
        {
            FileName = fileName,
            FileType = fileType,
            FileId = Guid.NewGuid(),
            BlobContainerName = blobContainer,
            RegistrationSetId = registrationSetId
        };

        await submissionStatusClient.CreateEventAsync(@event, submissionId);

        try
        {
            await loggingService.SendEventAsync(new ProtectiveMonitoringEvent(
                submissionId,
                "epr_pom_api_web",
                PmcCodes.Code0212,
                Priorities.NormalEvent,
                TransactionCodes.Uploaded,
                fileName,
                $"FileId: {@event.FileId}"));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred creating the protective monitoring event");
        }

        return @event.FileId;
    }

    public async Task CreateRegistrationEventAsync(Guid submissionId, RegistrationApplicationPayload applicationPayload)
    {
        if (applicationPayload.SubmissionType == SubmissionType.RegistrationFeePayment)
        {
            var feePaymentEvent = new RegistrationFeePaymentEvent
            {
                ApplicationReferenceNumber = applicationPayload.ApplicationReferenceNumber,
                ComplianceSchemeId = applicationPayload.ComplianceSchemeId,
                PaidAmount = applicationPayload.PaidAmount,
                PaymentMethod = applicationPayload.PaymentMethod,
                PaymentStatus = applicationPayload.PaymentStatus,
                IsResubmission = applicationPayload.IsResubmission
            };

            await submissionStatusClient.CreateRegistrationFeePaymentEventAsync(feePaymentEvent, submissionId);
        }

        if (applicationPayload.SubmissionType == SubmissionType.RegistrationApplicationSubmitted)
        {
            var submittedEvent = new RegistrationApplicationSubmittedEvent
            {
                Comments = applicationPayload.Comments,
                ApplicationReferenceNumber = applicationPayload.ApplicationReferenceNumber,
                ComplianceSchemeId = applicationPayload.ComplianceSchemeId,
                SubmissionDate = DateTime.UtcNow,
                IsResubmission = applicationPayload.IsResubmission
            };

            await submissionStatusClient.CreateApplicationSubmittedEventAsync(submittedEvent, submissionId);
        }
    }

    public async Task<HttpResponseMessage> GetSubmissionAsync(Guid submissionId)
    {
        return await submissionStatusClient.GetSubmissionAsync(submissionId);
    }

    public async Task<List<AbstractSubmission>> GetSubmissionsAsync(string queryString)
    {
        return await submissionStatusClient.GetSubmissionsAsync(queryString);
    }

    public async Task<List<SubmissionHistoryResponse>> GetSubmissionPeriodHistory(Guid submissionId, string queryString)
    {
        var submissionHistory = await submissionStatusClient.GetSubmissionPeriodHistory(submissionId, queryString);

        return FormatSubmissionHistoryData(submissionHistory);
    }

    public async Task<List<SubmissionGetResponse>> GetSubmissionsByFilter(Guid organisationId, Guid? complianceSchemeId, int? year, SubmissionType submissionType)
    {
        return await submissionStatusClient.GetSubmissionsByFilter(organisationId, complianceSchemeId, year, submissionType);
    }

    /// <summary>
    /// Gets registration validation errors & warnings.
    /// </summary>
    /// <param name="submissionId">submission id.</param>
    /// <returns>returns list of errors & warnings.</returns>
    public async Task<List<RegistrationValidationError>> GetRegistrationValidationErrorsAsync(Guid submissionId)
    {
        var errors = submissionStatusClient.GetRegistrationValidationErrorsAsync(submissionId);
        var warnings = submissionStatusClient.GetRegistrationValidationWarningsAsync(submissionId);

        var issues = await Task.WhenAll(errors, warnings);
        var result = issues.SelectMany(x => x)
                            .OrderBy(x => x.RowNumber)
                            .ToList();

        return result;
    }

    public async Task<List<ProducerValidationIssueRow>> GetProducerValidationIssuesAsync(Guid submissionId)
    {
        var errorsTask = submissionStatusClient.GetProducerValidationErrorRowsAsync(submissionId);
        var warningsTask = submissionStatusClient.GetProducerValidationWarningRowsAsync(submissionId);

        var issues = await Task.WhenAll(errorsTask, warningsTask);
        var result = issues
            .SelectMany(x => x)
            .OrderBy(x => x.RowNumber)
            .ToList();

        return result;
    }

    public async Task SubmitAsync(Guid submissionId, SubmissionPayload submissionPayload)
    {
        await submissionStatusClient.SubmitAsync(submissionId, submissionPayload);
    }

    public async Task SubmitAsync(CreateSubmission submission)
    {
        await submissionStatusClient.CreateSubmissionAsync(submission);
    }

    public async Task CreateFileDownloadCheckEventAsync(Guid submissionId, FileDownloadCheckEvent fileDownloadCheckEvent)
    {
        await submissionStatusClient.CreateFileDownloadEventAsync(fileDownloadCheckEvent, submissionId);
        var transactionCode = fileDownloadCheckEvent.ContentScan == ContentScan.Clean ? TransactionCodes.DownloadAllowed : TransactionCodes.AntivirusThreatDetected;

        try
        {
            await loggingService.SendEventAsync(new ProtectiveMonitoringEvent(
                submissionId,
                "epr_pom_api_web",
                PmcCodes.Code0212,
                Priorities.NormalEvent,
                transactionCode,
                fileDownloadCheckEvent.FileName,
                $"FileId: {fileDownloadCheckEvent.FileId}"));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred creating the protective monitoring event");
        }
    }

    public async Task<string> GetFileBlobNameAsync(Guid submissionId, Guid fileId)
    {
        var antivirusResultEvent = await submissionStatusClient.GetFileScanResultAsync(submissionId, fileId);

        return antivirusResultEvent.BlobName;
    }

    private static List<SubmissionHistoryResponse> FormatSubmissionHistoryData(SubmissionHistoryEventsResponse submissionHistoryEventsResponse)
    {
        var response = submissionHistoryEventsResponse.SubmittedEvents.Select(x =>
            new SubmissionHistoryResponse
            {
                SubmissionId = x.SubmissionId,
                FileName = x.FileName,
                UserName = x.SubmittedBy,
                SubmissionDate = x.Created,
                FileId = x.FileId
            }).ToList();

        var regulatorDecisions = submissionHistoryEventsResponse.RegulatorDecisionEvents;

        response.ForEach(m =>
        {
            var latestRelevantDecision = regulatorDecisions
                .Where(x => x.SubmissionId == m.SubmissionId && x.FileId == m.FileId)
                .MaxBy(x => x.Created);

            if (latestRelevantDecision != null)
            {
                m.Status = latestRelevantDecision.Decision;
                m.DateofLatestStatusChange = latestRelevantDecision.Created;
            }
            else
            {
                m.Status = "Submitted";
                m.DateofLatestStatusChange = m.SubmissionDate;
            }
        });

        return response;
    }
}