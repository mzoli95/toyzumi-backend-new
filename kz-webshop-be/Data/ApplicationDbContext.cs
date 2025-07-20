using kz_webshop_be.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Address> Addresses { get; set; }

    public DbSet<FunkoPop> FunkoPops { get; set; }
    public DbSet<Labubu> Labubus { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<ProductComment> ProductComments { get; set; }
    public DbSet<RelatedProduct> RelatedProducts { get; set; }

    public DbSet<FunkoPopTag> FunkoPopTags { get; set; }

    public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<LikedItem> LikedItems { get; set; }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<DiscountCode> DiscountCodes { get; set; }
    public DbSet<UserDiscountCode> UserDiscountCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<Product>()
            .HasDiscriminator<string>("ProductType")
            .HasValue<FunkoPop>("FunkoPop")
            .HasValue<Labubu>("Labubu");

        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId);

        modelBuilder.Entity<ProductReview>()
            .HasOne(pr => pr.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(pr => pr.ProductId);

        modelBuilder.Entity<ProductReview>()
            .HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId);

        modelBuilder.Entity<ProductComment>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.Comments)
            .HasForeignKey(pc => pc.ProductId);

        modelBuilder.Entity<ProductComment>()
            .HasOne(pc => pc.User)
            .WithMany()
            .HasForeignKey(pc => pc.UserId);

        modelBuilder.Entity<RelatedProduct>()
            .HasKey(rp => new { rp.ProductId, rp.RelatedToId });

        modelBuilder.Entity<RelatedProduct>()
            .HasOne(rp => rp.Product)
            .WithMany(p => p.RelatedProducts)
            .HasForeignKey(rp => rp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RelatedProduct>()
            .HasOne(rp => rp.RelatedTo)
            .WithMany()
            .HasForeignKey(rp => rp.RelatedToId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FunkoPop>()
            .HasIndex(fp => fp.Name);

        modelBuilder.Entity<FunkoPop>()
            .HasIndex(fp => fp.Franchise);

        modelBuilder.Entity<FunkoPop>()
            .HasIndex(fp => fp.Category);

        modelBuilder.Entity<Labubu>()
            .HasIndex(l => l.Name);

        modelBuilder.Entity<Labubu>()
            .HasIndex(l => l.Series);

        modelBuilder.Entity<Labubu>()
            .HasIndex(l => l.Edition);

        modelBuilder.Entity<FunkoPopTag>()
            .HasKey(ft => ft.Id);

        modelBuilder.Entity<FunkoPopTag>()
            .HasOne(ft => ft.FunkoPop)
            .WithMany(fp => fp.FunkoPopTags)
            .HasForeignKey(ft => ft.FunkoPopId);

        modelBuilder.Entity<ShoppingCartItem>()
            .HasKey(sci => sci.Id);

        modelBuilder.Entity<ShoppingCartItem>()
            .HasOne(sci => sci.User)
            .WithMany(u => u.ShoppingCartItems)
            .HasForeignKey(sci => sci.UserId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Invoice)
            .WithOne(i => i.Order)
            .HasForeignKey<Invoice>(i => i.OrderId);
        
        modelBuilder.Entity<UserDiscountCode>()
            .HasKey(udc => new { udc.UserId, udc.DiscountCodeId });

        modelBuilder.Entity<UserDiscountCode>()
            .HasOne(udc => udc.User)
            .WithMany(u => u.UserDiscountCodes)
            .HasForeignKey(udc => udc.UserId);

        modelBuilder.Entity<UserDiscountCode>()
            .HasOne(udc => udc.DiscountCode)
            .WithMany(dc => dc.UserDiscountCodes)
            .HasForeignKey(udc => udc.DiscountCodeId);

        modelBuilder.Entity<LikedItem>()
            .HasKey(li => li.Id);

        modelBuilder.Entity<LikedItem>()
            .HasOne(li => li.User)
            .WithMany(u => u.LikedItems)
            .HasForeignKey(li => li.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}