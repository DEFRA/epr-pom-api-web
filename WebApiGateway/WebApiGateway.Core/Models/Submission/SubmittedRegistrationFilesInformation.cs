﻿using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
public class SubmittedRegistrationFilesInformation
{
    public string CompanyDetailsFileName { get; set; }

    public string? BrandsFileName { get; set; }

    public string? PartnersFileName { get; set; }

    public DateTime SubmittedDateTime { get; set; }

    public Guid SubmittedBy { get; set; }
}