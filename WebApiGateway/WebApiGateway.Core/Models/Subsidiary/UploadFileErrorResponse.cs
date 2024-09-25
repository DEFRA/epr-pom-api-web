namespace WebApiGateway.Core.Models.Subsidiary;

public class UploadFileErrorResponse
{
    public string Status { get; set; }

    public List<UploadFileErrorModel> Errors { get; set; }
}
