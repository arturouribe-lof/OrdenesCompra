using PurchaseOrders.Models;
using PurchaseOrders.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseOrders.Controllers
{
    public class PurchaseOrderController
    {
        private readonly PurchaseOrderService _service;
        public PurchaseOrderController(PurchaseOrderService service) { _service = service; }


        public IEnumerable<PurchaseOrder> GetOrders() => _service.GetAllOrders();
        public PurchaseOrder GetOrder(int id) => _service.GetOrder(id);


        public void SaveNewOrder(PurchaseOrder po) => _service.CreateOrder(po);
        public void SaveEditedOrder(PurchaseOrder po) => _service.UpdateOrder(po);
        public void RemoveOrder(int id) => _service.SoftDeleteOrder(id);
        public List<Branch> GetBranches()
        {
            return _service.GetBranches();
        }

        public List<Provider> GetProviders()
        {
            return _service.GetProviders();
        }
        public List<Product> GetProducts()
        {
            return _service.GetProducts();
        }
    }
}
