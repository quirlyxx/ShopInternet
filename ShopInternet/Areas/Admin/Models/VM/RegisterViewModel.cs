using System.ComponentModel.DataAnnotations;

namespace ShopInternet.Areas.Admin.Models.VM
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль єобов'язковим")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження пароля")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
