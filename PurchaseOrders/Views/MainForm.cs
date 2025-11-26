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

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // 5. Generar el PDF
                GeneratePdf(selectedOrders, sfd.FileName);

                MessageBox.Show("PDF exportado correctamente.",
                                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void GeneratePdf(List<PurchaseOrder> orders, string path)
        {
            Document doc = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));

            doc.Open();

            PdfContentByte cb = writer.DirectContent;

            float pageWidth = PageSize.A4.Width;
            float pageHeight = PageSize.A4.Height;

            float boxWidth = pageWidth / 2;
            float boxHeight = pageHeight / 2;

            int cardIndex = 0;

            foreach (var order in orders)
            {
                if (cardIndex % 4 == 0)
                    doc.NewPage();

                int pos = cardIndex % 4;

                float x = (pos % 2 == 0) ? 0 : boxWidth;
                float y = (pos < 2) ? boxHeight : 0;

                // Marco del recuadro
                iTextSharp.text.Rectangle box = new iTextSharp.text.Rectangle(x, y, x + boxWidth, y + boxHeight);
                box.Border = iTextSharp.text.Rectangle.BOX;
                box.BorderWidth = 0.8f;
                cb.Rectangle(box);

                // ColumnText para contenido
                ColumnText ct = new ColumnText(cb);
                ct.SetSimpleColumn(
                    x + 25,
                    y + 25,
                    x + boxWidth - 25,
                    y + boxHeight - 25
                );

                Paragraph p = new Paragraph();
                p.SpacingAfter = 6;

                // dibujas el rectángulo
                cb.Rectangle(box);

                //  AQUI AGREGA EL FOLIO ⬇️ 
                Phrase folioPhrase = new Phrase();
                folioPhrase.Add(new Chunk("Folio: ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
                folioPhrase.Add(new Chunk(order.InvoiceNumber.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10)));

                ColumnText.ShowTextAligned(
                    cb,
                    Element.ALIGN_RIGHT,
                    folioPhrase,
                    x + boxWidth - 20,   // margen derecho
                    y + boxHeight - 30,  // un poco abajo del borde superior
                    0
                );

                // Encabezados
                p.Add(new Phrase(order.Branch.Name + "\n\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
                p.Add(new Phrase("Proveedor\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
                p.Add(new Phrase(order.Provider.Name + "\n\n", FontFactory.GetFont(FontFactory.HELVETICA, 11)));

                // Encabezado artículos
                Paragraph header = new Paragraph();
                header.Add(new Phrase("Artículos:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
                header.Add(new Chunk(new iTextSharp.text.pdf.draw.VerticalPositionMark()));
                header.Add(new Phrase("Cantidad", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
                p.Add(header);

                p.Add(new Phrase("\n")); // espacio extra antes de los artículos

                // Tabla para artículos (2 columnas)
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 80, 20 }); // descripción 80%, cantidad 20%
                table.SpacingBefore = 10f;
                table.SpacingAfter = 10f;

                foreach (var line in order.Lines.Where(l => l.IsActive == 1))
                {
                    // Celda de descripción
                    PdfPCell descCell = new PdfPCell(
                        new Phrase(line.Product.Description, FontFactory.GetFont(FontFactory.HELVETICA, 11))
                    );
                    descCell.Border = 0;
                    descCell.PaddingBottom = 4;

                    // Línea punteada inferior
                    descCell.CellEvent = new DottedBorderVerticalAndBottom();

                    // Celda de cantidad
                    PdfPCell qtyCell = new PdfPCell(
                        new Phrase(line.Quantity.ToString("0.##"), FontFactory.GetFont(FontFactory.HELVETICA, 11))
                    );
                    qtyCell.Border = 0;
                    qtyCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    qtyCell.PaddingBottom = 4;

                    // Línea vertical punteada + línea horizontal punteada
                    qtyCell.CellEvent = new DottedBorderVerticalAndBottom();

                    table.AddCell(descCell);
                    table.AddCell(qtyCell);
                }

                p.Add(table);

                // Espacio antes de la firma (aprox. 5 cm)
                p.Add(new Phrase("\n\n\n\n\n\n\n\n\n"));

                p.Add(new Phrase("Nombre y firma", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));

                ct.AddElement(p);
                ct.Go();

                cardIndex++;
            }

            doc.Close();
        }

        public class DottedBorderVerticalAndBottom : IPdfPCellEvent
        {
            public void CellLayout(PdfPCell cell, iTextSharp.text.Rectangle position, PdfContentByte[] canvases)
            {
                PdfContentByte cb = canvases[PdfPTable.LINECANVAS];
                cb.SetLineDash(2f, 2f);

                // Línea vertical punteada
                cb.MoveTo(position.Left, position.Bottom);
                cb.LineTo(position.Left, position.Top);
                cb.Stroke();

                // Línea horizontal punteada
                cb.MoveTo(position.Left, position.Bottom);
                cb.LineTo(position.Right, position.Bottom);
                cb.Stroke();
            }

        }





        private void btnExit_Click(object sender, EventArgs e)
        {

            Application.Exit();
        }



    }
}

