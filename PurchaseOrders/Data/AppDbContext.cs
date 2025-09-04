using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using PurchaseOrders.Models;

namespace PurchaseOrders.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=PurchaseOrders")
        {
            // Desactivar lazy loading si prefieres control explicito
            this.Configuration.LazyLoadingEnabled = true;
            this.Configuration.ProxyCreationEnabled = true;
        }


        public DbSet<Branch> Branches { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PurchaseOrder>()
            .HasMany(p => p.Lines)
            .WithRequired(l => l.PurchaseOrder)
            .HasForeignKey(l => l.PurchaseOrderId)
            .WillCascadeOnDelete(true);
        }
    }
}
