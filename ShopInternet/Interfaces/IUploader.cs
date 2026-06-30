namespace ShopInternet.Interfaces;

public interface IUploader
{
    Task<string> UploadFile(IFormFile file, string path);
    void DeleteFile(string path, string fileName);
}