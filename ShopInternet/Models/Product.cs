using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopInternet.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Вкажіть назву товару")]
    [DisplayName("Назва товару")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Вкажіть короткий опис товару")]
    [DisplayName("Короткий опис товару")]
    [MaxLength(100, ErrorMessage = "Опис не може бути довшим за 100 символів")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Вкажіть ціну товару")]
    [DisplayName("Ціна товару")]
    [Range(0.01, 1000000, ErrorMessage = "Введіть нормальну ціну")]
    [Precision(18, 2)]
    public decimal Price { get; set; } 
    
    [DisplayName("Зображення товару")]
    public string? Image { get; set; }
    
    [Required(ErrorMessage = "Оберіть категорію")]
    [DisplayName("Категорія товару")]
    public int CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    [DisplayName("Категорія товару")]
    public virtual Category? Category { get; set; }
    
    [NotMapped]
    public int TempCount { get; set; }
}