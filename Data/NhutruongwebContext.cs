using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using webbanhang.Models;

namespace webbanhang.Data;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Cartitem> Cartitems { get; set; }

    public virtual DbSet<Color> Colors { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderitem> Orderitems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Productcategory> Productcategories { get; set; }

    public virtual DbSet<Productimage> Productimages { get; set; }

    public virtual DbSet<Productvariant> Productvariants { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Shippinginfo> Shippinginfos { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=nhutruongweb;user=root;password=Nhut@123", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.42-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PRIMARY");

            entity.ToTable("cart");

            entity.HasIndex(e => e.UserId, "UserId").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.User).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.UserId)
                .HasConstraintName("cart_ibfk_1");
        });

        modelBuilder.Entity<Cartitem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PRIMARY");

            entity.ToTable("cartitems");

            entity.HasIndex(e => e.CartId, "CartId");

            entity.HasIndex(e => e.VariantId, "VariantId");

            entity.HasOne(d => d.Cart).WithMany(p => p.Cartitems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("cartitems_ibfk_1");

            entity.HasOne(d => d.Variant).WithMany(p => p.Cartitems)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("cartitems_ibfk_2");
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PRIMARY");

            entity.ToTable("colors");

            entity.Property(e => e.ColorName).HasMaxLength(50);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PRIMARY");

            entity.ToTable("inventory");

            entity.HasIndex(e => e.VariantId, "VariantId");

            entity.HasOne(d => d.Variant).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("inventory_ibfk_1");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.UserId, "UserId");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chờ xử lý'");
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("orders_ibfk_1");
        });

        modelBuilder.Entity<Orderitem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PRIMARY");

            entity.ToTable("orderitems");

            entity.HasIndex(e => e.OrderId, "OrderId");

            entity.HasIndex(e => e.VariantId, "VariantId");

            entity.Property(e => e.Price).HasPrecision(10, 2);

            entity.HasOne(d => d.Order).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("orderitems_ibfk_1");

            entity.HasOne(d => d.Variant).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("orderitems_ibfk_2");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity.ToTable("payments");

            entity.HasIndex(e => e.OrderId, "OrderId");

            entity.Property(e => e.PaidAt).HasColumnType("timestamp");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("payments_ibfk_1");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("products");

            entity.HasIndex(e => e.CategoryId, "CategoryId");

            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.ProductName).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("products_ibfk_1");
        });

        modelBuilder.Entity<Productcategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("productcategories");

            entity.Property(e => e.CategoryImages).HasMaxLength(255);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<Productimage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PRIMARY");

            entity.ToTable("productimages");

            entity.HasIndex(e => e.ProductId, "ProductId");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.IsThumbnail).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.Product).WithMany(p => p.Productimages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("productimages_ibfk_1");
        });

        modelBuilder.Entity<Productvariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PRIMARY");

            entity.ToTable("productvariants");

            entity.HasIndex(e => e.ColorId, "ColorId");

            entity.HasIndex(e => e.ProductId, "ProductId");

            entity.HasIndex(e => e.Sku, "SKU").IsUnique();

            entity.HasIndex(e => e.SizeId, "SizeId");

            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("SKU");

            entity.HasOne(d => d.Color).WithMany(p => p.Productvariants)
                .HasForeignKey(d => d.ColorId)
                .HasConstraintName("productvariants_ibfk_3");

            entity.HasOne(d => d.Product).WithMany(p => p.Productvariants)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("productvariants_ibfk_1");

            entity.HasOne(d => d.Size).WithMany(p => p.Productvariants)
                .HasForeignKey(d => d.SizeId)
                .HasConstraintName("productvariants_ibfk_2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Shippinginfo>(entity =>
        {
            entity.HasKey(e => e.ShippingId).HasName("PRIMARY");

            entity.ToTable("shippinginfo");

            entity.HasIndex(e => e.OrderId, "OrderId");

            entity.Property(e => e.Address).HasColumnType("text");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ReceiverName).HasMaxLength(100);
            entity.Property(e => e.ShippingStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chưa giao'");

            entity.HasOne(d => d.Order).WithMany(p => p.Shippinginfos)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("shippinginfo_ibfk_1");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PRIMARY");

            entity.ToTable("sizes");

            entity.HasIndex(e => e.CategoryId, "CategoryId");

            entity.Property(e => e.SizeName).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Sizes)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("sizes_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.RoleId, "fk_users_role");

            entity.Property(e => e.Address).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RoleId).HasDefaultValueSql("'2'");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
