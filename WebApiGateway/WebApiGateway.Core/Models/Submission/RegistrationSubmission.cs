﻿using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
public class RegistrationSubmission : AbstractSubmission
{
    public override SubmissionType SubmissionType => SubmissionType.Registration;

    public bool RequiresBrandsFile { get; set; }

    public bool RequiresPartnershipsFile { get; set; }

    public string CompanyDetailsFileName { get; set; }

    public bool CompanyDetailsDataComplete { get; set; }

    public Guid? CompanyDetailsUploadedBy { get; set; }

    public DateTime? CompanyDetailsUploadedDate { get; set; }

    public string? BrandsFileName { get; set; }

    public bool BrandsDataComplete { get; set; }

    public Guid? BrandsUploadedBy { get; set; }

    public DateTime? BrandsUploadedDate { get; set; }

    public string? PartnershipsFileName { get; set; }

    public bool PartnershipsDataComplete { get; set; }

    public Guid? PartnershipsUploadedBy { get; set; }

    public DateTime? PartnershipsUploadedDate { get; set; }

    public UploadedRegistrationFilesInformation? LastUploadedValidFiles { get; set; }

    public SubmittedRegistrationFilesInformation? LastSubmittedFiles { get; set; }
}