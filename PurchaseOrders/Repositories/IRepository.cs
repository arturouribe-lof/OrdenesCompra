using PurchaseOrders.Data;
using PurchaseOrders.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseOrders.Repositories
{
    // Repositories/IRepository.cs
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity); // para soft delete, implementarlo en cada repo si aplica
    }


    // Repositories/PurchaseOrderRepository.cs (ejemplo)
    public class PurchaseOrderRepository
    {
        private readonly AppDbContext _ctx;
        public PurchaseOrderRepository(AppDbContext ctx) { _ctx = ctx; }


        public IEnumerable<PurchaseOrder> GetAll(bool includeDeleted = false)
        {
            var q = _ctx.PurchaseOrders
            .Include("Branch")
            .Include("Provider")
            .Include("Lines.Product")
            .AsQueryable();
            //if (!includeDeleted) q = q.Where(x => !x.IsDeleted);
            return q.ToList();
        }


        public PurchaseOrder GetById(int id)
        {
            return _ctx.PurchaseOrders
            .Include("Lines.Product")
            .Include("Branch")
            .Include("Provider")
            .FirstOrDefault(x => x.Id == id);
        }

        public Product GetProductById(int id)
        {
            return _ctx.Products.FirstOrDefault(p => p.Id == id);
        }

        public void Add(PurchaseOrder po)
        {
            _ctx.PurchaseOrders.Add(po);
        }


        public void Update(PurchaseOrder po)
        {
            // vacío, ahora toda la lógica está en el Service
        }


        public void SoftDelete(int id)
        {
            var po = _ctx.PurchaseOrders.Find(id);
            if (po == null) return;
            po.IsDeleted = true;
            _ctx.Entry(po).State = EntityState.Modified;
        }
    }
}
