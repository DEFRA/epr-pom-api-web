namespace WebApiGateway.Core.Models.Subsidiary;

public class UploadFileErrorModel
{
    public int FileLineNumber { get; set; }

    public string FileContent { get; set; }

    public string Message { get; set; }

    public bool IsError { get; set; }

    public int ErrorNumber { get; set; }
}
