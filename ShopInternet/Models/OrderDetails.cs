using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopInternet.Models;

public class OrderDetails
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int OrderHeadId { get; set; }
    
    [ForeignKey("OrderHeadId")]
    public OrderHeader? OrderHeader { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
    
    public int Count { get; set; }
    
    public decimal Price { get; set; }
}