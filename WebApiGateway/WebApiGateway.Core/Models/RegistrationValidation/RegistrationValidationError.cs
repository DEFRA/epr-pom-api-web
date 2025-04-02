namespace WebApiGateway.Core.Models.RegistrationValidation;

public class RegistrationValidationError
{
    public List<ColumnValidationError> ColumnErrors { get; set; }

    public string OrganisationId { get; set; }

    public string SubsidiaryId { get; set; }

    public int RowNumber { get; set; }

    /// <summary>
    /// Gets or sets the IssueType.
    /// </summary>
    /// <value>can be Error or Warning.</value>
    public string? IssueType { get; set; }
}