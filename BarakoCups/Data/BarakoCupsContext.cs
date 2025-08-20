using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarakoCups.Models;

namespace BarakoCups.Data
{
    public class BarakoCupsContext : DbContext
    {
        public BarakoCupsContext (DbContextOptions<BarakoCupsContext> options)
            : base(options)
        {

        }
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Barako Beans", Price = 200, ImageUrl = "/images/1.jpg", Stock = 50 },
                new Product { ProductId = 2, Name = "Cold Brew", Price = 130, ImageUrl = "/images/cofeee.jpg", Stock = 60 },
                new Product { ProductId = 3, Name = "Espresso", Price = 100, ImageUrl = "/images/coff33.jpg", Stock = 80 },
                new Product { ProductId = 4, Name = "Latte", Price = 150, ImageUrl = "/images/latte.jpg", Stock = 70 }
            );
        }
    }
}
