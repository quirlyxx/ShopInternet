using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.Data;
using ShopInternet.Models;
using ShopInternet.Models.ViewModels;
using ShopInternet.Utility;

namespace ShopInternet.Controllers;

public class OrderController : Controller
{
    private readonly ShopDbContext _db;
    [BindProperty]
    public ProductUserVM ProductUserVM { get; set; }
    
    public OrderController(ShopDbContext db)
    {
        _db = db;
    }

    // GET
    public async Task<IActionResult> Summary()
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }
        List<int> productInCart = shoppingCartList.Select(x => x.ProductId).ToList();
        IEnumerable<Product> productList = await _db.Product.Where(x => productInCart.Contains(x.Id)).ToListAsync();
        ProductUserVM = new ProductUserVM()
        {
            OrderHeader = new OrderHeader()
        };
        foreach (var cart in shoppingCartList)
        {
            Product prodTemp = productList.FirstOrDefault(x => x.Id == cart.ProductId);
            prodTemp.TempCount = cart.Count;
            ProductUserVM.ProductList.Add(prodTemp);
        }
        
        return View(ProductUserVM);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Summary")]
    public async Task<IActionResult> SummaryPost(ProductUserVM productUserVm)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }
        productUserVm.OrderHeader.OrderDate = DateTime.Now;
        productUserVm.OrderHeader.OrderStatus = WC.StatusInProgress;
        
        _db.OrderHeader.Add(productUserVm.OrderHeader);
        await _db.SaveChangesAsync();

        decimal orderTotal = 0;
        foreach (var cart in shoppingCartList)
        {
            Product product = await _db.Product.FindAsync(cart.ProductId);
            OrderDetails orderDetails = new OrderDetails()
            {
                OrderHeadId = productUserVm.OrderHeader.Id,
                ProductId = cart.ProductId,
                Count = cart.Count,
                Price = product.Price
            };
            orderTotal += cart.Count * product.Price;
            _db.OrderDetails.Add(orderDetails);
        }
        
        productUserVm.OrderHeader.OrderTotal = orderTotal;
        await _db.SaveChangesAsync();
        
        HttpContext.Session.Clear();
        
        return RedirectToAction(nameof(Confirmation), new { id = productUserVm.OrderHeader.Id });
    }
    
    public IActionResult Confirmation(int id)
    {
        ViewData["Id"] = id.ToString();
        return View();
    }
}