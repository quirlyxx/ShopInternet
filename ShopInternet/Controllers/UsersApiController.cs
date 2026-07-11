using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.Models.Api;

namespace ShopInternet.Controllers;

// API контроллер для керування користувачами.
// Доступ дозволено виключно адміністратору (політика "AdminAccess").
[Route("api/users")]
[ApiController]
[Authorize(Policy = "AdminAccess")]
public class UsersApiController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UsersApiController> _logger;

    public UsersApiController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UsersApiController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    // GET api/users
    // Перегляд списку всіх користувачів
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<UserResponse>();

        foreach (var user in users)
        {
            result.Add(await ToResponseAsync(user));
        }

        return Ok(result);
    }

    // GET api/users/{id}
    // Перегляд конкретного користувача за Id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"Користувача з Id={id} не знайдено");

        return Ok(await ToResponseAsync(user));
    }

    // POST api/users
    // Створення нового користувача
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _userManager.FindByNameAsync(request.UserName);
        if (existing != null)
            return Conflict("Користувач з таким ім'ям вже існує");

        var user = new IdentityUser
        {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return BadRequest(createResult.Errors.Select(e => e.Description));

        if (request.Roles is { Count: > 0 })
        {
            var rolesToAssign = new List<string>();
            foreach (var role in request.Roles.Distinct())
            {
                if (await _roleManager.RoleExistsAsync(role))
                    rolesToAssign.Add(role);
            }

            if (rolesToAssign.Count > 0)
                await _userManager.AddToRolesAsync(user, rolesToAssign);
        }

        var response = await ToResponseAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, response);
    }

    // PUT api/users/{id}
    // Редагування наявного користувача. Оновлюються лише передані поля
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"Користувача з Id={id} не знайдено");

        if (!string.IsNullOrWhiteSpace(request.UserName) && request.UserName != user.UserName)
        {
            var setNameResult = await _userManager.SetUserNameAsync(user, request.UserName);
            if (!setNameResult.Succeeded)
                return BadRequest(setNameResult.Errors.Select(e => e.Description));
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, request.Email);
            if (!setEmailResult.Succeeded)
                return BadRequest(setEmailResult.Errors.Select(e => e.Description));
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!resetResult.Succeeded)
                return BadRequest(resetResult.Errors.Select(e => e.Description));
        }

        if (request.Roles != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Except(request.Roles).ToList();
            if (rolesToRemove.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            var rolesToAdd = new List<string>();
            foreach (var role in request.Roles.Except(currentRoles).Distinct())
            {
                if (await _roleManager.RoleExistsAsync(role))
                    rolesToAdd.Add(role);
            }

            if (rolesToAdd.Count > 0)
                await _userManager.AddToRolesAsync(user, rolesToAdd);
        }

        return Ok(await ToResponseAsync(user));
    }

    private async Task<UserResponse> ToResponseAsync(IdentityUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? "N/A",
            Email = user.Email ?? "N/A",
            Roles = await _userManager.GetRolesAsync(user)
        };
    }
}
