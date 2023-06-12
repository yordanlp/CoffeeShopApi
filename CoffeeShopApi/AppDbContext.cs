using CoffeeShopApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShopApi {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("CoffeeShop");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.FavouriteCoffee)
                .WithMany(c => c.UsersWhoLikeThis)
                .HasForeignKey(u => u.CoffeeId);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Coffee> Coffees { get; set; }
    }
}
