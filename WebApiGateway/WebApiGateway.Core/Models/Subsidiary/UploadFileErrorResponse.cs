namespace WebApiGateway.Core.Models.Subsidiary;

public class UploadFileErrorResponse
{
    public string Status { get; set; }

    public int? RowsAdded { get; set; }

    public List<UploadFileErrorModel> Errors { get; set; }
}
