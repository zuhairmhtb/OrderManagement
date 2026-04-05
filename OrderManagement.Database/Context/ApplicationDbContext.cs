using Microsoft.EntityFrameworkCore;
using OrderManagement.Database.Models;

namespace OrderManagement.Database.Context;

public class ApplicationDbContext: DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<PurchasedProduct> PurchasedProducts { get; set; } = null!;
    public DbSet<Address> CustomerAddresses { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product Table
        modelBuilder.Entity<Product>()
            .ToTable("Products");

        modelBuilder.Entity<Product>()
            .Property(p => p.Currency)
            .HasConversion<string>();
        
        modelBuilder.Entity<Product>()  
            .HasIndex(p => p.Name)
            .IsUnique();
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Price);

        // Configure Customer Table
        modelBuilder.Entity<Customer>()
        .ToTable("Customers");

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.PhoneNumber);

        // Configure Order Table
        modelBuilder.Entity<Order>()
            .ToTable("Orders");

        modelBuilder.Entity<Order>()
            .Property(o => o.OrderStatus)
            .HasConversion<string>();
        modelBuilder.Entity<Order>()
            .Property(o => o.Currency)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .ToTable("CustomerOrders")
            .HasIndex(c => c.CustomerEmail);
        modelBuilder.Entity<Order>()
            .HasIndex(c => c.CustomerContactNumber);
        modelBuilder.Entity<Order>()
            .HasIndex(c => c.CustomerId);
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderDate);
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderStatus);
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.TotalAmount);

        // Configuring Customer Address
        modelBuilder.Entity<Address>()
            .ToTable("Address");

        modelBuilder.Entity<Address>()
            .HasIndex(a => a.City);
        modelBuilder.Entity<Address>()
            .HasIndex(a => a.PostalCode);
        modelBuilder.Entity<Address>()
            .HasIndex(a => a.Country);

        modelBuilder.Entity<Address>()
            .HasOne(a => a.CustomerInfo)
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Configure PurchasedProduct Table
        modelBuilder.Entity<PurchasedProduct>()
            .ToTable("PurchasedProducts");
            
        modelBuilder.Entity<PurchasedProduct>()
            .Property(p => p.Currency)
            .HasConversion<string>();

        modelBuilder.Entity<PurchasedProduct>()
            .HasIndex(p => p.ProductId);

        modelBuilder.Entity<PurchasedProduct>()
            .HasIndex(p => p.Name);
        modelBuilder.Entity<PurchasedProduct>()
            .HasIndex(p => p.Price);

        modelBuilder.Entity<PurchasedProduct>()
            .HasOne(p => p.OrderInfo)
            .WithMany(o => o.Products)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}