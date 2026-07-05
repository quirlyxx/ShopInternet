using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.Data;
using ShopInternet.Helpers;

namespace ShopInternet.Controllers;

[Route("api/product")]
[ApiController]
public class ProductApiController : Controller
{
    private readonly ShopDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ProductApiController> _logger;

    public ProductApiController(ShopDbContext db, UserManager<IdentityUser> userManager, ILogger<ProductApiController> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<bool> IsValidKey(string key)
    {
        var users = await _userManager.GetUsersForClaimAsync(new Claim("ApiKey", key));
        var user = users.FirstOrDefault();
        if (user != null)
        {
            _logger.LogInformation($"[DEBUG_LOG] API Request: {user.UserName}, Controller: {nameof(ProductApiController)}");
            Console.WriteLine($"{user.UserName}, Controller: {nameof(ProductApiController)}");
            return true;
        }

        return false;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(string apiKey, string? productName = null, string? categoryName = null)
    {
        if (!await IsValidKey(apiKey)) return Unauthorized("Invalid API key");
        var query = _db.Product.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrEmpty(productName))
        {
            query = query.Where(p => p.Name.Contains(productName));
        }
        if (!string.IsNullOrEmpty(categoryName))
        {
            query = query.Where(p => p.Category.Name.Contains(categoryName));
        }

        var products = await query.ToListAsync();
        AppHelper.ConverImagePathToURL(products, Request);
        return Ok(products);
    }

    [HttpGet("category/{id:int}")]
    public async Task<IActionResult> GetCategoryById(int id, string apiKey)
    {
        if(!await IsValidKey(apiKey)) return Unauthorized("Invalid API key");
        var category = await _db.Category.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();
        var products = await _db.Product.Where(x => x.CategoryId == id).ToListAsync();
        AppHelper.ConverImagePathToURL(products, Request);
        return Ok(products);
    }

}