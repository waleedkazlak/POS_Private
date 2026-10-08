namespace POS_Private.Application.DTOs;

public class FileUploadResultDto
{
    public string Url { get; set; } = null!;
    public string RelativeUrl { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = null!;
}
