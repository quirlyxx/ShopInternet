using ShopInternet.Interfaces;

namespace ShopInternet.Utility;

public class FileUploader : IUploader
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    public FileUploader(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }
    
    public async Task<string> UploadFile(IFormFile file, string path)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;
        string uploadPath = Path.Combine(webRootPath, path.TrimStart('\\', '/'));
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        string fileName = Guid.NewGuid().ToString();
        string extension = Path.GetExtension(file.FileName);
        string fullName = fileName + extension;

        using (var fileStream = new FileStream(Path.Combine(uploadPath, fullName), FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return fullName;
    }

    public void DeleteFile(string path, string fileName)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;
        string fullPath = Path.Combine(webRootPath, path.TrimStart('\\','/'), fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}