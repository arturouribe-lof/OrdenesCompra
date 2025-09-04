using PurchaseOrders.Controllers;
using PurchaseOrders.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PurchaseOrders.Views
{
    public partial class MainForm : Form
    {
        private readonly PurchaseOrderController _controller;

        public MainForm(PurchaseOrderController controller)
        {
            InitializeComponent();
            _controller = controller;

            // Asociamos el evento Load al método MainForm_Load
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void btnNewOrder_Click(object sender, EventArgs e)
        {
            using (var form = new OrderForm(_controller))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadOrders();
                }
            }
        }

        private void btnEditOrder_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una orden para editar.");
                return;
            }

            var selectedOrder = (PurchaseOrder)dgvOrders.CurrentRow.DataBoundItem;

            using (var form = new OrderForm(_controller, selectedOrder))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadOrders();
                }
            }
        }

        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una orden para eliminar.");
                return;
            }

            var selectedOrder = (PurchaseOrder)dgvOrders.CurrentRow.DataBoundItem;

            var confirm = MessageBox.Show(
                "¿Está seguro de eliminar esta orden?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                _controller.RemoveOrder(selectedOrder.Id);
                LoadOrders();
            }
        }

        private void btnPrintOrder_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una orden para imprimir.");
                return;
            }

            var selectedOrder = (PurchaseOrder)dgvOrders.CurrentRow.DataBoundItem;

            // Aquí podrías abrir una vista de impresión o mandar directo a la impresora
            MessageBox.Show($"Imprimiendo orden #{selectedOrder.Id}...");
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            var orders = _controller.GetOrders();
            dgvOrders.DataSource = null;
            dgvOrders.DataSource = orders;
        }
    }
}

