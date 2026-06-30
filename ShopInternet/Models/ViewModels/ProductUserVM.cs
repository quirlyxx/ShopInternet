namespace ShopInternet.Models.ViewModels;

public class ProductUserVM
{
    public List<Product> ProductList { get; set; }
    public OrderHeader OrderHeader { get; set; } = new OrderHeader();

    public ProductUserVM()
    {
        ProductList = new List<Product>();
    }
}