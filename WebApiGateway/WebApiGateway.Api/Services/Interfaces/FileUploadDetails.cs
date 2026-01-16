using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Api.Services.Interfaces;

public class FileUploadDetails
{
    public FileUploadDetails(
        SubmissionType submissionType, 
        SubmissionSubType? submissionSubType,
        string fileName, 
        string submissionPeriod,
        Guid? originalSubmissionId,
        Guid? registrationSetId,
        Guid? complianceSchemeId)
    {
        FileName = fileName;
        SubmissionType = submissionType;
        SubmissionSubType = submissionSubType;
        RegistrationSetId = registrationSetId;
        SubmissionPeriod = submissionPeriod;
        OriginalSubmissionId = originalSubmissionId;
        ComplianceSchemeId = complianceSchemeId;
    }

    public FileUploadDetails()
    {
    }
    
    public string FileName { get; set; }
    public SubmissionType SubmissionType { get; set; }
    public SubmissionSubType? SubmissionSubType { get; set; }
    public Guid? RegistrationSetId { get; set; }
    public string SubmissionPeriod { get; set; }
    public Guid? OriginalSubmissionId { get; set; }
    public Guid? ComplianceSchemeId { get; set; }
    public bool? IsResubmission { get; set; }
    public string? RegistrationJourney { get; set; }
}