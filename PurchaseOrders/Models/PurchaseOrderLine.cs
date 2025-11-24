using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseOrders.Models
{
    // Models/PurchaseOrderLine.cs
    public class PurchaseOrderLine
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }


        public int LineNumber { get; set; }
        public int ProductId { get; set; }
        public virtual  Product Product { get; set; }
        public decimal Quantity { get; set; }
        public string Notes { get; set; }
        public int IsActive { get; set; } = 1;
    }
}
