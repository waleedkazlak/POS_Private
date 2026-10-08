using POS_Private.Application.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

public class ImageUploadRequest
{
    /// <summary>
    /// The image file to upload
    /// </summary>
    public IFormFile File { get; set; } = null!;
}

/// <summary>
/// Handles media and image uploads for POS_Private catalog, products, and categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadsController> _logger;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg", ".bmp", ".jfif"
    };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public UploadsController(IWebHostEnvironment environment, ILogger<UploadsController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Upload an image file and receive the hosted server URL to store in the database
    /// </summary>
    /// <param name="request">The image file to upload (Max 10MB)</param>
    /// <returns>FileUploadResultDto containing both absolute and relative URLs</returns>
    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(APIBaseResponse<FileUploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIBaseResponse<FileUploadResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage([FromForm] ImageUploadRequest request)
    {
        var file = request?.File;
        if (file == null || file.Length == 0)
        {
            return BadRequest(APIBaseResponse<FileUploadResultDto>.Failed(StatusCodes.Status400BadRequest, "No image file was provided for upload."));
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(APIBaseResponse<FileUploadResultDto>.Failed(StatusCodes.Status400BadRequest, "Image file size exceeds the 10MB limit."));
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            return BadRequest(APIBaseResponse<FileUploadResultDto>.Failed(
                StatusCodes.Status400BadRequest,
                $"Invalid image format. Supported formats: {string.Join(", ", AllowedExtensions)}"));
        }

        try
        {
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var uploadsFolder = Path.Combine(webRootPath, "uploads", "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var destinationFilePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(destinationFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var httpRequest = HttpContext.Request;
            var baseUrl = $"{httpRequest.Scheme}://{httpRequest.Host}";
            var relativeUrl = $"/uploads/images/{uniqueFileName}";
            var absoluteUrl = $"{baseUrl}{relativeUrl}";

            var result = new FileUploadResultDto
            {
                Url = absoluteUrl,
                RelativeUrl = relativeUrl,
                FileName = file.FileName,
                FileSize = file.Length,
                ContentType = file.ContentType
            };

            _logger.LogInformation("Image '{OriginalName}' successfully saved as '{UniqueName}'", file.FileName, uniqueFileName);

            return Ok(APIBaseResponse<FileUploadResultDto>.Success(result, "Image uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image file '{OriginalName}'", file.FileName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                APIBaseResponse<FileUploadResultDto>.Failed(StatusCodes.Status500InternalServerError, "Failed to upload image file to server."));
        }
    }
}
