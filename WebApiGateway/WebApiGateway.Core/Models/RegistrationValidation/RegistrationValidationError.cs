namespace WebApiGateway.Core.Models.RegistrationValidation;

public class RegistrationValidationError
{
    public List<ColumnValidationError> ColumnErrors { get; set; }

    public string OrganisationId { get; set; }

    public string SubsidiaryId { get; set; }

    public int RowNumber { get; set; }
}