using PurchaseOrders.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseOrders.Models
{
    // Models/PurchaseOrder.cs
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }
        public int ProviderId { get; set; }
        public virtual Provider Provider { get; set; }


        public string InvoiceNumber { get; set; } // N. de factura (opcional)
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false; // soft-delete


        public virtual ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();

    }
}
