using System.ComponentModel.DataAnnotations;

namespace ShopInternet.Models.Api;

// Модель відповіді з даними користувача
public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}

// Запит на створення користувача
public class CreateUserRequest
{
    [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обов'язковий")]
    [MinLength(3, ErrorMessage = "Мінімальна довжина паролю - 3 символи")]
    public string Password { get; set; } = string.Empty;

    // Список ролей, які потрібно призначити новому користувачу (опційно)
    public List<string>? Roles { get; set; }
}

// Запит на редагування користувача. Усі поля опційні - оновлюється лише те, що передано
public class UpdateUserRequest
{
    public string? UserName { get; set; }

    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    public string? Email { get; set; }

    [MinLength(3, ErrorMessage = "Мінімальна довжина паролю - 3 символи")]
    public string? NewPassword { get; set; }

    // Якщо передано - повний новий список ролей користувача (замінює поточний)
    public List<string>? Roles { get; set; }
}
