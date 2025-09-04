using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PurchaseOrders.Controllers;
using PurchaseOrders.Models;

namespace PurchaseOrders.Views
{
    public partial class OrderForm : Form
    {
        private readonly PurchaseOrderController _controller;
        private readonly PurchaseOrder _order;
        private List<Product> _products;

        public OrderForm(PurchaseOrderController controller, PurchaseOrder order = null)
        {
            InitializeComponent();
            _controller = controller;
            _order = order ?? new PurchaseOrder { CreatedAt = DateTime.Now };

            LoadBranches();
            LoadProviders();
            LoadProducts();
            LoadOrderData();
        }

        private void LoadBranches()
        {
            var branches = _controller.GetBranches();
            cboBranch.DataSource = branches;
            cboBranch.DisplayMember = "Name";
            cboBranch.ValueMember = "Id";
        }

        private void LoadProviders()
        {
            var providers = _controller.GetProviders();
            cboProvider.DataSource = providers;
            cboProvider.DisplayMember = "Name";
            cboProvider.ValueMember = "Id";
        }

        private void LoadProducts()
        {
            _products = _controller.GetProducts();

            var colProduct = dgvItems.Columns["colProduct"] as DataGridViewComboBoxColumn;
            if (colProduct != null)
            {
                colProduct.DataSource = _products;
                colProduct.DisplayMember = "Description";
                colProduct.ValueMember = "Id";
            }
        }

        private void LoadOrderData()
        {
            if (_order.Id > 0)
            {
                cboBranch.SelectedValue = _order.BranchId;
                cboProvider.SelectedValue = _order.ProviderId;
                txtInvoice.Text = _order.InvoiceNumber;

                foreach (var line in _order.Lines)
                {
                    dgvItems.Rows.Add(line.ProductId, line.Quantity, line.Notes);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _order.BranchId = (int)cboBranch.SelectedValue;
            _order.ProviderId = (int)cboProvider.SelectedValue;
            _order.InvoiceNumber = txtInvoice.Text;
            _order.UpdatedAt = DateTime.Now;

            // limpiar líneas viejas
            _order.Lines.Clear();

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;

                var line = new PurchaseOrderLine
                {
                    ProductId = row.Cells["colProduct"].Value != null ? (int)row.Cells["colProduct"].Value : 0,
                    Quantity = row.Cells["colQuantity"].Value != null ? Convert.ToDecimal(row.Cells["colQuantity"].Value) : 0,
                    Notes = row.Cells["colNotes"].Value?.ToString()
                };

                if (line.ProductId > 0)
                    _order.Lines.Add(line);
            }

            if (_order.Id == 0)
                _controller.SaveNewOrder(_order);
            else
                _controller.SaveEditedOrder(_order);

            MessageBox.Show("Orden guardada correctamente.");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
