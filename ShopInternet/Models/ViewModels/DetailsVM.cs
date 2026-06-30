namespace ShopInternet.Models.ViewModels;

public class DetailsVM
{
    public Product Product { get; set; } = new Product();
    public bool ExistsInCart { get; set; }
}