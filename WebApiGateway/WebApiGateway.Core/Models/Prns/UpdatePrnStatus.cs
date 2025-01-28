namespace WebApiGateway.Core.Models.Prns;

public class UpdatePrnStatus
{
    public Guid PrnId { get; set; }

    public string Status { get; set; }
}