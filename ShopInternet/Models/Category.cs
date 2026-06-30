using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ShopInternet.Models;

public class Category
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Вкажіть ім'я категорії")]
    [DisplayName("Категорія")]
    public string Name { get; set; } = string.Empty;

    [DisplayName("Порядок відображення")]
    [Required(ErrorMessage = "Вкажіть порядок")]
    [Range(1, int.MaxValue, ErrorMessage = "Значення {0} повинно бути між {1} та {2}")]
    public int Order { get; set; } = 1;
}