using EPR.Common.Logging.Constants;
using EPR.Common.Logging.Models;
using EPR.Common.Logging.Services;
using Microsoft.Extensions.Options;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Helpers;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionStatusClient _submissionStatusClient;
    private readonly ILoggingService _loggingService;
    private readonly ILogger<SubmissionService> _logger;
    private readonly StorageAccountOptions _options;

    public SubmissionService(
        ISubmissionStatusClient submissionStatusClient,
        ILoggingService loggingService,
        ILogger<SubmissionService> logger,
        IOptions<StorageAccountOptions> options)
    {
        _submissionStatusClient = submissionStatusClient;
        _loggingService = loggingService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Guid> CreateSubmissionAsync(SubmissionType submissionType, string submissionPeriod, Guid? complianceSchemeId)
    {
        var submission = new CreateSubmission
        {
            Id = Guid.NewGuid(),
            DataSourceType = DataSourceType.File,
            SubmissionType = submissionType,
            SubmissionPeriod = submissionPeriod,
            ComplianceSchemeId = complianceSchemeId
        };

        await _submissionStatusClient.CreateSubmissionAsync(submission);

        return submission.Id;
    }

    public async Task<Guid> CreateAntivirusCheckEventAsync(string fileName, FileType fileType, Guid submissionId, Guid? registrationSetId)
    {
        var @event = new AntivirusCheckEvent
        {
            FileName = fileName,
            FileType = fileType,
            FileId = Guid.NewGuid(),
            BlobContainerName = fileType == FileType.Pom ? _options.PomContainer : _options.RegistrationContainer,
            RegistrationSetId = registrationSetId
        };

        await _submissionStatusClient.CreateEventAsync(@event, submissionId);

        try
        {
            await _loggingService.SendEventAsync(new ProtectiveMonitoringEvent(
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
            _logger.LogError(exception, "An error occurred creating the protective monitoring event");
        }

        return @event.FileId;
    }

    public async Task<HttpResponseMessage> GetSubmissionAsync(Guid submissionId)
    {
        return await _submissionStatusClient.GetSubmissionAsync(submissionId);
    }

    public async Task<List<AbstractSubmission>> GetSubmissionsAsync(string queryString)
    {
        return await _submissionStatusClient.GetSubmissionsAsync(queryString);
    }

    public async Task<List<ProducerValidationIssueRow>> GetProducerValidationIssuesAsync(Guid submissionId)
    {
        var errorsTask = _submissionStatusClient.GetProducerValidationErrorRowsAsync(submissionId);
        var warningsTask = _submissionStatusClient.GetProducerValidationWarningRowsAsync(submissionId);

        var issues = await Task.WhenAll(errorsTask, warningsTask);
        var result = issues
            .SelectMany(x => x)
            .OrderBy(x => x.RowNumber)
            .ToList();

        return result;
    }

    public async Task SubmitAsync(Guid submissionId, SubmissionPayload submissionPayload)
    {
        await _submissionStatusClient.SubmitAsync(submissionId, submissionPayload);
    }
}