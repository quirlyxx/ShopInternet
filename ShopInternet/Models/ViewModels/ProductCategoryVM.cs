namespace ShopInternet.Models.ViewModels;

public class ProductCategoryVM
{
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public IEnumerable<Product> Products { get; set; } = new List<Product>();
}