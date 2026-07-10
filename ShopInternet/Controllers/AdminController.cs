using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.Data;

namespace ShopInternet.Controllers;

public class AdminController : Controller
{
    private readonly ShopDbContext _db;

    public AdminController(ShopDbContext db)
    {
        _db = db;
    }

    // GET: /Admin/Users
    public async Task<IActionResult> Users()
    {
        var users = await _db.User.OrderBy(u => u.FullName).ToListAsync();
        return View(users);
    }

    // GET: /Admin/CreateUser
    public IActionResult CreateUser()
    {
        return View();
    }

    // POST: /Admin/CreateUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(ShopInternet.Models.User user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        user.RegisteredDate = DateTime.Now;
        _db.User.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Users));
    }

    // POST: /Admin/ToggleBlock
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int id)
    {
        var user = await _db.User.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.IsBlocked = !user.IsBlocked;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Users));
        
    }
}
