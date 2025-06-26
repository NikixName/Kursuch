
using Microsoft.EntityFrameworkCore;
using Kurs_HTML.Models;

namespace Kurs_HTML.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts)
            : base(opts) { }

        public DbSet<Client>       Clients       { get; set; } = null!;
        public DbSet<Administrator> Administrators{ get; set; } = null!;
        public DbSet<Mechanic>     Mechanics     { get; set; } = null!;
        public DbSet<CarWasher>    CarWashers    { get; set; } = null!;

        public DbSet<Service>      Services      { get; set; } = null!;
        public DbSet<Order>        Orders        { get; set; } = null!;

        public DbSet<OrderStatus>  OrderStatuses { get; set; } = null!;

        public DbSet<CompletedOrder> CompletedOrders { get; set; }
        public DbSet<Receipt> Receipts { get; set; } = null!;
        public DbSet<WorkReport> WorkReports { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.OrderStatus)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.OrderStatusId);
        }
    }
}
