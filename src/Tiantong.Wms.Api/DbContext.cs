using Tiantong.Wms.DB;
using Microsoft.EntityFrameworkCore;

namespace Tiantong.Wms.Api
{
  public class DbContext : PostgresContext, IUnitOfWork
  {
    public DbSet<User> Users { get; set; }

    public DbSet<Item> Items { get; set; }

    public DbSet<Area> Areas { get; set; }

    public DbSet<Stock> stocks { get; set; }

    public DbSet<Keeper> Keepers { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<Location> Locations { get; set; }

    public DbSet<Warehouse> Warehouses { get; set; }

    public DbSet<ItemCategory> ItemCategories { get; set; }

    public DbSet<OrderCategory> OrderCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Keeper>()
        .HasKey(keeper => new {
          keeper.role,
          keeper.user_id,
          keeper.warehouse_id
        });
    }
  }
}
