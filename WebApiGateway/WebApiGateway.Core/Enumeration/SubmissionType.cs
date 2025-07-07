namespace WebApiGateway.Core.Enumeration;

using System.ComponentModel.DataAnnotations;

public enum SubmissionType
{
    [Display(Name = "pom")]
    Producer = 1,
    [Display(Name = "registration")]
    Registration = 2,
    [Display(Name = "subsidiary")]
    Subsidiary = 3,
    [Display(Name = "companies house")]
    CompaniesHouse = 4,
    [Display(Name = "registration fee payment")]
    RegistrationFeePayment = 5,
    [Display(Name = "registration application submitted")]
    RegistrationApplicationSubmitted = 6,
    [Display(Name = "accreditation")]
    Accreditation = 7,
}