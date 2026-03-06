using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PurchaseOrders.Controllers;
using PurchaseOrders.Models;

namespace PurchaseOrders.Views
{
    public partial class OrderForm : Form
    {
        private readonly PurchaseOrderController _controller;
        private readonly PurchaseOrder _order;
        private readonly bool _esConsulta;
        private List<Product> _products;

        // Controles flotantes para el buscador
        private TextBox _txtSearch;
        private ListBox _lstSuggestions;
        private int _activeRow = -1;
        private bool _selecting = false;

        public OrderForm(PurchaseOrderController controller, PurchaseOrder order = null, bool esConsulta = false)
        {
            InitializeComponent();
            _controller = controller;
            _order = order ?? new PurchaseOrder { CreatedAt = DateTime.Now };
            _esConsulta = esConsulta;
            this.StartPosition = FormStartPosition.CenterParent;

            LoadBranches();
            LoadProviders();
            LoadProducts();
            InitFloatingControls();
            LoadOrderData();

            if (_esConsulta)
            {
                btnGuardar.Enabled = false;
                cboBranch.Enabled = false;
                cboProvider.Enabled = false;
                txtInvoice.ReadOnly = true;
                dgvItems.ReadOnly = true;
                dgvItems.AllowUserToAddRows = false;
                dgvItems.AllowUserToDeleteRows = false;
            }
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
                colProduct.DataSource = new List<Product>(_products);
                colProduct.DisplayMember = "Description";
                colProduct.ValueMember = "Id";
            }
        }

        private void InitFloatingControls()
        {
            _txtSearch = new TextBox
            {
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            _lstSuggestions = new ListBox
            {
                Visible = false,
                IntegralHeight = false
            };

            _txtSearch.TextChanged += TxtSearch_TextChanged;
            _txtSearch.KeyDown += TxtSearch_KeyDown;
            _txtSearch.LostFocus += TxtSearch_LostFocus;

            _lstSuggestions.MouseClick += LstSuggestions_MouseClick;
            _lstSuggestions.KeyDown += LstSuggestions_KeyDown;
            _lstSuggestions.LostFocus += LstSuggestions_LostFocus;

            this.Controls.Add(_lstSuggestions);
            this.Controls.Add(_txtSearch);
            _lstSuggestions.BringToFront();
            _txtSearch.BringToFront();

            dgvItems.CellClick += DgvItems_CellClick;
            dgvItems.CellEnter += DgvItems_CellEnter;
            dgvItems.Scroll += (s, e) => HideFloating();

            dgvItems.EditingControlShowing += (s, e) =>
            {
                if (dgvItems.CurrentCell?.ColumnIndex == dgvItems.Columns["colProduct"].Index)
                    dgvItems.CurrentCell.ReadOnly = false;
            };
        }

        private void DgvItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_esConsulta || e.RowIndex < 0 || dgvItems.Columns[e.ColumnIndex].Name != "colProduct")
            {
                HideFloating();
                return;
            }
            ShowFloating(e.RowIndex, e.ColumnIndex);
        }

        private void DgvItems_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_esConsulta || e.RowIndex < 0 || dgvItems.Columns[e.ColumnIndex].Name != "colProduct")
                return;
            ShowFloating(e.RowIndex, e.ColumnIndex);
        }

        private void ShowFloating(int rowIndex, int colIndex)
        {
            _activeRow = rowIndex;

            Rectangle cellRect = dgvItems.GetCellDisplayRectangle(colIndex, rowIndex, true);
            Point pos = dgvItems.PointToScreen(cellRect.Location);
            pos = this.PointToClient(pos);

            _txtSearch.SetBounds(pos.X, pos.Y, cellRect.Width, cellRect.Height);

            var currentValue = dgvItems.Rows[rowIndex].Cells["colProduct"].Value;
            if (currentValue != null && (int)currentValue > 0)
            {
                var current = _products.FirstOrDefault(p => p.Id == (int)currentValue);
                _txtSearch.Text = current?.Description ?? "";
            }
            else
            {
                _txtSearch.Text = "";
            }

            _txtSearch.Visible = true;
            _txtSearch.BringToFront();
            _txtSearch.Focus();
            _txtSearch.SelectAll();

            ShowSuggestions(_txtSearch.Text);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_selecting) return;
            ShowSuggestions(_txtSearch.Text);
        }

        private void ShowSuggestions(string filter)
        {
            var results = string.IsNullOrWhiteSpace(filter)
                ? _products
                : _products
                    .Where(p => p.Description.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            _lstSuggestions.Items.Clear();

            if (!results.Any())
            {
                _lstSuggestions.Visible = false;
                return;
            }

            foreach (var p in results)
                _lstSuggestions.Items.Add(new ProductItem { Id = p.Id, Description = p.Description });

            int height = Math.Min(results.Count * 16 + 6, 150);
            _lstSuggestions.SetBounds(
                _txtSearch.Left,
                _txtSearch.Bottom,
                _txtSearch.Width,
                height
            );
            _lstSuggestions.Visible = true;
            _lstSuggestions.BringToFront();
        }

        private void LstSuggestions_MouseClick(object sender, MouseEventArgs e)
        {
            int index = _lstSuggestions.IndexFromPoint(e.Location);
            if (index >= 0)
                SelectProduct(index);
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && _lstSuggestions.Visible && _lstSuggestions.Items.Count > 0)
            {
                _lstSuggestions.Focus();
                _lstSuggestions.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && _lstSuggestions.Visible && _lstSuggestions.Items.Count > 0)
            {
                SelectProduct(0);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideFloating();
            }
        }

        private void LstSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _lstSuggestions.SelectedIndex >= 0)
            {
                SelectProduct(_lstSuggestions.SelectedIndex);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideFloating();
                _txtSearch.Focus();
            }
        }

        private void SelectProduct(int listIndex)
        {
            if (_activeRow < 0 || listIndex < 0 || listIndex >= _lstSuggestions.Items.Count)
                return;

            _selecting = true;

            var item = (ProductItem)_lstSuggestions.Items[listIndex];

            dgvItems.Rows[_activeRow].Cells["colProduct"].Value = item.Id;
            _txtSearch.Text = item.Description;

            _selecting = false;
            HideFloating();

            // Mover foco a cantidad
            int qtyIndex = dgvItems.Columns["colQuantity"].Index;
            dgvItems.CurrentCell = dgvItems.Rows[_activeRow].Cells[qtyIndex];
        }

        private void TxtSearch_LostFocus(object sender, EventArgs e)
        {
            Task.Delay(150).ContinueWith(_ =>
            {
                this.Invoke((Action)(() =>
                {
                    if (!_lstSuggestions.Focused)
                        HideFloating();
                }));
            });
        }

        private void LstSuggestions_LostFocus(object sender, EventArgs e)
        {
            Task.Delay(150).ContinueWith(_ =>
            {
                this.Invoke((Action)(() =>
                {
                    if (!_txtSearch.Focused)
                        HideFloating();
                }));
            });
        }

        private void HideFloating()
        {
            _txtSearch.Visible = false;
            _lstSuggestions.Visible = false;
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
                    dgvItems.Rows.Add(
                        line.Id,
                        line.ProductId,
                        line.Quantity,
                        line.Notes
                    );
                }
            }

            HideFloating();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cboBranch.SelectedValue == null || cboProvider.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar una sucursal y un proveedor.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _order.BranchId = (int)cboBranch.SelectedValue;
            _order.ProviderId = (int)cboProvider.SelectedValue;
            _order.InvoiceNumber = txtInvoice.Text.Trim();
            _order.Lines.Clear();

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;

                var productIdValue = row.Cells["colProduct"].Value;
                var quantityValue = row.Cells["colQuantity"].Value;

                if (productIdValue == null) continue;

                var line = new PurchaseOrderLine
                {
                    ProductId = (int)productIdValue,
                    Quantity = quantityValue != null ? Convert.ToDecimal(quantityValue) : 0,
                    Notes = row.Cells["colNotes"].Value?.ToString()
                };

                if (line.ProductId > 0)
                    _order.Lines.Add(line);
            }

            if (!_order.Lines.Any())
            {
                MessageBox.Show("Debe agregar al menos un producto.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _controller.SaveNewOrder(_order);
                MessageBox.Show("Orden guardada correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private class ProductItem
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public override string ToString() => Description;
        }
    }
}