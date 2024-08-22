namespace WebApiGateway.Core.Enumeration;

using System.ComponentModel.DataAnnotations;

public enum SubmissionType
{
    [Display(Name = "pom")]
    Producer = 1,
    [Display(Name = "registration")]
    Registration = 2,
    [Display(Name = "subsidiary")]
    Subsidiary = 3
}