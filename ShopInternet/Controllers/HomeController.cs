using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.Data;
using ShopInternet.Models;
using ShopInternet.Models.ViewModels;
using ShopInternet.Utility;

namespace ShopInternet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ShopDbContext _db;

    public HomeController(ILogger<HomeController> logger, ShopDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        var query = _db.Product.Include(p => p.Category).AsQueryable();
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        ProductCategoryVM modelVM = new ProductCategoryVM()
        {
            Products = await query.ToListAsync(),
            Categories = await _db.Category.OrderBy(c => c.Order).ToListAsync()
        };
        ViewBag.SelectedCategoryId = categoryId;
        return View(modelVM);
    }

    public async Task<IActionResult> Details(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }

        DetailsVM detailsVM = new DetailsVM()
        {
            Product = await _db.Product.Include(c => c.Category).FirstOrDefaultAsync(x => x.Id == id) ?? new Product(),
            ExistsInCart = false
        };

        var itemInCart = shoppingCartList.FirstOrDefault(x => x.ProductId == id);
        if (itemInCart != null)
        {
            detailsVM.ExistsInCart = true;
            detailsVM.Product.TempCount = itemInCart.Count;
        }
        else
        {
            detailsVM.Product.TempCount = 1;
        }

        return View(detailsVM);
    }

    [HttpPost, ActionName("Details")]
    public IActionResult DetailsPost(int id, DetailsVM detailsVM)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }

        int count = detailsVM.Product.TempCount;
        if (count < 1)
        {
            count = 1;
        }

        shoppingCartList.Add(new ShoppingCart { ProductId = id, Count = count });
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult RemoveFromCart(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }
        
        var itemToRemove = shoppingCartList.SingleOrDefault(x => x.ProductId == id);
        if (itemToRemove != null)
        {
            shoppingCartList.Remove(itemToRemove);
        }
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}