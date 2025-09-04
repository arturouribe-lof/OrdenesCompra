using PurchaseOrders.Controllers;
using PurchaseOrders.Data;
using PurchaseOrders.Services;
using PurchaseOrders.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PurchaseOrders
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Crear DbContext
            var context = new AppDbContext();

            // Crear Service
            var service = new PurchaseOrderService(context);

            // Crear Controller
            var controller = new PurchaseOrderController(service);

            // Pasar Controller al MainForm
            Application.Run(new MainForm(controller));
        }
    }
}
