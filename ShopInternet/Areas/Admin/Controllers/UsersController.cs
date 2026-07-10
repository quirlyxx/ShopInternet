using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.Areas.Admin.Models.VM;
using System.Security.Claims;

namespace ShopInternet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminAccess")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Відображення списку користувачів
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserVM>();

            foreach (var user in users)
            {
                userList.Add(new UserVM
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "N/A",
                    Email = user.Email ?? "N/A",
                    Roles = await _userManager.GetRolesAsync(user),
                    IsBlocked = await _userManager.IsLockedOutAsync(user)
                });
            }

            return View(userList);
        }

        // Перегляд даних про конкретного користувача
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new UserVM
            {
                Id = user.Id,
                UserName = user.UserName ?? "N/A",
                Email = user.Email ?? "N/A",
                Roles = await _userManager.GetRolesAsync(user)
            };

            return View(model);
        }

        // Видалення користувача
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            return BadRequest("Помилка при видаленні користувача");
        }

        // Геттер для зміни паролю
        public async Task<IActionResult> ChangePassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new ChangePasswordVM
            {
                Id = user.Id,
                UserName = user.UserName ?? "N/A"
            };

            return View(model);
        }

        // Пост для зміни паролю
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Скидання паролю через видалення та додавання нового (для простоти адміністрування)
            // Або можна використовувати GeneratePasswordResetTokenAsync
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // Блокування / розблокування користувача (через вбудований механізм Lockout)
        [HttpPost]
        public async Task<IActionResult> ToggleBlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var isBlocked = await _userManager.IsLockedOutAsync(user);
            if (isBlocked)
            {
                // Розблокувати
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                // Заблокувати назавжди
                if (!user.LockoutEnabled)
                {
                    await _userManager.SetLockoutEnabledAsync(user, true);
                }
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }

            return RedirectToAction(nameof(Index));
        }

        // Призначення ролі на основі claim та збереження в БД
        [HttpGet]
        public async Task<IActionResult> AssignRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new AssignRoleVM
            {
                UserId = user.Id,
                UserName = user.UserName ?? "N/A"
            };

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(AssignRoleVM model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(model.RoleName))
            {
                // Додаємо роль користувачу в БД (AspNetRoles, AspNetUserRoles)
                if (!await _userManager.IsInRoleAsync(user, model.RoleName))
                {
                    await _userManager.AddToRoleAsync(user, model.RoleName);

                    // Також можна додати Claim для демонстрації роботи з ними
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, model.RoleName));
                }
            }

            return RedirectToAction(nameof(Index));
        }
        }
}
