using ShopInternet.Models;

namespace ShopInternet.Helpers;

public static class AppHelper
{
    public static List<Product> ConverImagePathToURL(List<Product> products, HttpRequest request)
    {
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        var imagePath = WC.ImagePath.Replace("\\", "/");
        if (!imagePath.StartsWith("/"))
        {
            imagePath = "/" + imagePath;
        }
        if (!imagePath.EndsWith("/"))
        {
            imagePath = imagePath + "/";
        }

        foreach (var product in products)
        {
            if (!string.IsNullOrEmpty(product.Image))
            {
                var imageName = product.Image.Replace("\\", "/").TrimStart('/');
                product.Image = $"{baseUrl}{imagePath}{imageName}";
            }
        }
        return products;
    }
}