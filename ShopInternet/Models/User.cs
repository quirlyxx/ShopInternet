using System.ComponentModel.DataAnnotations;

namespace ShopInternet.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "ПІБ")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Номер телефона")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Дата реєстрації")]
    public DateTime RegisteredDate { get; set; } = DateTime.Now;

    [Display(Name = "Блокований")]
    public bool IsBlocked { get; set; }
}
