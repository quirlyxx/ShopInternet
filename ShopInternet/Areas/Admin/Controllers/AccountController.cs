using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopInternet.Areas.Admin.Models.VM;
using ShopInternet;

namespace ShopInternet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RegisterUserAccess")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var apiKeyClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "ApiKey");
            ViewBag.ApiKey = apiKeyClaim?.Value;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GenerateApiKey()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var claims = await _userManager.GetClaimsAsync(user);
            var apiKey = claims.FirstOrDefault(c => c.Type == "ApiKey");
            if (apiKey != null)
            {
                await _userManager.RemoveClaimAsync(user, apiKey);
            }

            var newKey = Guid.NewGuid().ToString();
            await _userManager.AddClaimAsync(user, new Claim("ApiKey", newKey));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            RegisterViewModel regModel = new RegisterViewModel();
            return View(regModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // TODO: тимчасово для тестування - видати роль адміністратора при реєстрації.
                    // Перед продом обов'язково повернути "Користувач" (WC.CustomerRole)!
                    await _userManager.AddToRoleAsync(user, WC.AdminRole);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            LoginViewModel lModel = new LoginViewModel();
            lModel.RememberMe = true;
            return View(lModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                ModelState.AddModelError(string.Empty, "Невірний логін чи пароль");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}