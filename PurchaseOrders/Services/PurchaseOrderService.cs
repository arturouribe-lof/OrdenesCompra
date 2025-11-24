using PurchaseOrders.Data;
using PurchaseOrders.Models;
using PurchaseOrders.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseOrders.Services
{
    public class PurchaseOrderService
    {
        private readonly AppDbContext _ctx;
        private readonly PurchaseOrderRepository _repo;


        public PurchaseOrderService(AppDbContext ctx)
        {
            _ctx = ctx;
            _repo = new PurchaseOrderRepository(ctx);
        }


        public IEnumerable<PurchaseOrder> GetAllOrders() => _repo.GetAll();


        public PurchaseOrder GetOrder(int id) => _repo.GetById(id);


        public void CreateOrder(PurchaseOrder po)
        {
            po.CreatedAt = DateTime.Now;
            // validar que tenga al menos 1 línea
            if (!po.Lines.Any()) throw new InvalidOperationException("La orden debe contener al menos una línea.");


            using (var tran = _ctx.Database.BeginTransaction())
            {
                try
                {
                    _repo.Add(po);
                    _ctx.SaveChanges();
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }


        public void SoftDeleteOrder(int id)
        {
            _repo.SoftDelete(id);
            _ctx.SaveChanges();
        }

        public List<Branch> GetBranches()
        {
            return _ctx.Branches
                .Where(b => b.IsActive)
                .ToList();
        }

        public List<Provider> GetProviders()
        {
            return _ctx.Providers
                .Where(p => p.IsActive)
                .ToList();
        }
        public List<Product> GetProducts()
        {
            return _ctx.Products
                .Where(p => p.IsActive)
                .ToList();
        }

        public void CancelOrders(List<int> orderIds)
        {
            var orders = _ctx.PurchaseOrders
                .Where(o => orderIds.Contains(o.Id))
                .ToList();

            foreach (var order in orders)
            {
                order.IsDeleted = true;
            }

            _ctx.SaveChanges();
        }
    }
}
