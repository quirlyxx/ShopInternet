using Microsoft.EntityFrameworkCore;
using ShopInternet.Models;

namespace ShopInternet.Data;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {

    }
    
    public DbSet<Category> Category { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<OrderHeader> OrderHeader { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }
}