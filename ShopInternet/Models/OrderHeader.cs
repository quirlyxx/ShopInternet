using System.ComponentModel.DataAnnotations;

namespace ShopInternet.Models;

public class OrderHeader
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [Display(Name = "ПІБ замовника")]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Номер телефона")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Адреса доставки")]
    public string ShippingAddress { get; set; } = string.Empty;
    
    public DateTime OrderDate { get; set; }
    public decimal OrderTotal { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
}