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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Rectangle = iTextSharp.text.Rectangle;
using PurchaseOrders.Helpers;
using System.Diagnostics;

namespace PurchaseOrders.Views
{
    public partial class MainForm : Form
    {
        private readonly PurchaseOrderController _controller;
        private List<PurchaseOrder> _orders;

        public MainForm(PurchaseOrderController controller)
        {
            InitializeComponent();
            _controller = controller;

            ConfigureGrid();

            // Asociamos el evento Load al método MainForm_Load
            this.Load += MainForm_Load;

            this.Width = dgvOrders.PreferredSize.Width-36;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void btnNewOrder_Click(object sender, EventArgs e)
        {
            using (var form = new OrderForm(_controller))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
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

        private void ConfigureGrid()
        {
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.MultiSelect = true;
            dgvOrders.AutoGenerateColumns = false;
            dgvOrders.Columns.Clear();

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id"
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BranchName",
                HeaderText = "Sucursal",
                DataPropertyName = "BranchName"
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProviderName",
                HeaderText = "Proveedor",
                DataPropertyName = "ProviderName"
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InvoiceNumber",
                HeaderText = "Factura",
                DataPropertyName = "InvoiceNumber"
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreatedAt",
                HeaderText = "Creado",
                DataPropertyName = "CreatedAt"
            });

            dgvOrders.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsActive",
                HeaderText = "Activo",
                DataPropertyName = "IsActive"
            });
        }

        private void LoadOrders()
        {
            var orders = _controller.GetOrders();

            _orders = orders.ToList();


            dgvOrders.DataSource = _orders.Select(x => new
            {
                x.Id,
                BranchName = x.Branch.Name,
                ProviderName = x.Provider.Name,
                x.InvoiceNumber,
                x.CreatedAt,
                IsActive = !x.IsDeleted
            }).ToList();

            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvOrders.AutoResizeColumns();
        }

        private void dgvOrders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // evita encabezados

            // Obtener el ID de la orden seleccionada
            var row = dgvOrders.Rows[e.RowIndex];
            int orderId = (int)row.Cells["Id"].Value;

            // Llamar al controller
            var order = _controller.GetOrder(orderId);

            // Abrir el OrderForm centrado
            using (var frm = new OrderForm(_controller, order, true))
            {
                frm.ShowDialog(this);
            }

            // Recargar grid después de un guardado
            LoadOrders();
        }

        private void btnCancelOrders_Click(object sender, EventArgs e)
        {
            // 1. No hay filas seleccionadas
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione al menos una orden para cancelar.",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. Preguntar confirmación
            var confirm = MessageBox.Show(
                "¿Está seguro que desea cancelar las órdenes seleccionadas?",
                "Confirmar cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.No) return;

            // 3. Recolectar los IDs seleccionados
            List<int> selectedIds = new List<int>();

            foreach (DataGridViewRow row in dgvOrders.SelectedRows)
            {
                if (row.Cells["Id"].Value == null) continue;

                int id = Convert.ToInt32(row.Cells["Id"].Value);
                selectedIds.Add(id);
            }

            if (selectedIds.Count == 0)
            {
                MessageBox.Show("No se encontraron IDs válidos en la selección.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 4. Llamada al controller
            _controller.CancelOrders(selectedIds);

            // 5. Refrescar la tabla
            LoadOrders();

            // 6. Mensaje final
            MessageBox.Show("Órdenes canceladas correctamente.");
        }


        private void dgvOrders_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = dgvOrders.Rows[e.RowIndex];
            bool isActive = Convert.ToBoolean(row.Cells["IsActive"].Value);

            if (!isActive)
                row.DefaultCellStyle.BackColor = Color.Red;
        }

        private void dgvOrders_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;

            bool isActive = Convert.ToBoolean(dgvOrders.Rows[e.RowIndex].Cells["IsActive"].Value);

            if (!isActive)
            {
                // Limpiar selección para evitar que quede azul
                dgvOrders.ClearSelection();
            }
        }


        private void dgvOrders_SelectionChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvOrders.Rows)
            {
                bool isActive = Convert.ToBoolean(row.Cells["IsActive"].Value);

                if (!isActive && row.Selected)
                {
                    row.Selected = false;
                }
            }
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            // 1. Verificar que haya filas seleccionadas
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione al menos una orden para exportar.",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. Obtener los IDs seleccionados (igual que en CancelOrders)
            List<int> selectedIds = new List<int>();

            foreach (DataGridViewRow row in dgvOrders.SelectedRows)
            {
                if (row.Cells["Id"].Value == null) continue;

                int id = Convert.ToInt32(row.Cells["Id"].Value);
                selectedIds.Add(id);
            }

            if (selectedIds.Count == 0)
            {
                MessageBox.Show("No se encontraron IDs válidos en la selección.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. Obtener las órdenes seleccionadas desde memoria
            //    (asumo que LOADOrders() ya llenó una lista _orders en memoria)
            var selectedOrders = _orders
                .Where(x => selectedIds.Contains(x.Id))
                .ToList();

            // 4. Diálogo para guardar
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF|*.pdf",
                FileName = "Ordenes.pdf"
            };

            var pdf = new PurchaseOrderPdfGenerator();

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // 5. Generar el PDF
                pdf.Generate(selectedOrders, sfd.FileName = $"Ordenes_{DateTime.Now:yyyyMMdd}.pdf", @"C:\Users\SITE\source\repos\PurchaseOrders\PurchaseOrders\Resources\logo.png");

                MessageBox.Show("PDF exportado correctamente.",
                                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = sfd.FileName,
                    UseShellExecute = true
                });

            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {

            Application.Exit();
        }



    }
}

