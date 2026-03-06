using iTextSharp.text;
using iTextSharp.text.pdf;
using PurchaseOrders.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseOrders.Helpers
{
    class PurchaseOrderPdfGenerator
    {

        private Font _bold;
        private Font _normal;
        private Font _italic;

        public void Generate(List<PurchaseOrder> orders, string path, string logoPath)
        {
            InitFonts();

            Document doc = new Document(PageSize.LETTER); // ← TAMAÑO CARTA
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
            doc.Open();

            PdfContentByte cb = writer.DirectContent;

            RenderOrders(doc, writer, cb, orders, logoPath);

            doc.Close();
        }


        private void InitFonts()
        {
            _bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            _normal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            _italic = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 7);
        }


        private void RenderOrders(Document doc, PdfWriter writer, PdfContentByte cb, List<PurchaseOrder> orders, string logoPath)
        {
            float pageHeight = PageSize.LETTER.Height;
            float blockHeight = (pageHeight - 60) / 2;
            float gap = 10f;

            int index = 0;

            foreach (var order in orders)
            {
                // 🔥 Tomar solo líneas activas
                var activeLines = order.Lines.Where(l => l.IsActive == 1).ToList();

                // Cada orden tendrá SOLO 6 artículos
                var chunks = activeLines
                    .Select((x, i) => new { x, idx = i })
                    .GroupBy(g => g.idx / 6)   // ← CAMBIADO DE 7 → 6
                    .Select(grp => grp.Select(a => a.x).ToList())
                    .ToList();


                foreach (var group in chunks)
                {
                    // Crear una copia del objeto para dibujar solamente este grupo
                    var partialOrder = new PurchaseOrder()
                    {
                        Branch = order.Branch,
                        Provider = order.Provider,
                        CreatedAt = order.CreatedAt,
                        InvoiceNumber = order.InvoiceNumber,
                        Lines = group
                    };

                    if (index % 2 == 0)
                        doc.NewPage();

                    float yStart = (index % 2 == 0)
                        ? pageHeight - 20
                        : blockHeight + gap;

                    DrawOrderBlock(doc, writer, cb, partialOrder, yStart, blockHeight, logoPath);
                    index++;
                }
            }
        }





        private void DrawOrderBlock(Document doc,PdfWriter writer,PdfContentByte cb,PurchaseOrder order,float yStart, float blockHeight,string logoPath)
        {
            DrawContainer(cb, yStart, blockHeight);
            DrawHeader(doc, cb, order, yStart, logoPath);

            // ⬛ CUADROS
            DrawInfoBoxes(cb, yStart);

            // ⬛ CONTENIDO
            AddProveedor(cb, order, yStart);
            AddFechaFactura(cb, order, yStart);
            AddItemsTable(cb, order, yStart);

            DrawSignatures(cb, yStart, blockHeight);
        }

        private void AddProveedor(PdfContentByte cb, PurchaseOrder order, float yStart)
        {
            ColumnText ct = new ColumnText(cb);
            ct.SetSimpleColumn(55, yStart - 135, 360, yStart - 75);

            Paragraph p = new Paragraph();
            p.Add(new Phrase("Proveedor\n", _bold));
            p.Add(new Phrase(order.Provider.Name, _normal));

            ct.AddElement(p);
            ct.Go();
        }


        private void AddFechaFactura(PdfContentByte cb, PurchaseOrder order, float yStart)
        {
            ColumnText ct = new ColumnText(cb);
            ct.SetSimpleColumn(395, yStart - 135, 530, yStart - 75);

            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1, 1 });

            // Celda Fecha
            PdfPCell fechaCell = new PdfPCell();
            fechaCell.Border = Rectangle.NO_BORDER;
            fechaCell.AddElement(new Phrase("Fecha", _bold));
            fechaCell.AddElement(new Phrase(order.CreatedAt.ToString("dd/MM/yyyy"), _normal));
            fechaCell.HorizontalAlignment = Element.ALIGN_LEFT;

            // Celda No. Factura
            PdfPCell facturaCell = new PdfPCell();
            facturaCell.Border = Rectangle.NO_BORDER;
            facturaCell.AddElement(new Phrase("No. Factura", _bold));
            facturaCell.AddElement(new Phrase(order.InvoiceNumber, _normal));
            facturaCell.HorizontalAlignment = Element.ALIGN_RIGHT;

            // Agregar celdas
            table.AddCell(fechaCell);
            table.AddCell(facturaCell);

            // Insertarlo en el ColumnText
            ct.AddElement(table);
            ct.Go();
        }



        private void AddItemsTable(PdfContentByte cb, PurchaseOrder order, float yStart)
        {
            // Coordenadas de encabezado (alineadas con los cuadros)
            float headerY = yStart - 150f; // encabezado del área de artículos
            float boxTop = headerY + 12f;
            float boxBottom = headerY - 180f + 12f; // coincide con altura 180 en DrawInfoBoxes

            // Dibujar encabezados
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT,
                new Phrase("Artículo", _bold), 60, headerY, 0);

            ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT,
                new Phrase("Cantidad", _bold), 520, headerY, 0);

            // Área disponible (restamos algunos pixeles para padding)
            float availableHeight = (headerY - boxBottom) - 10f; // espacio para filas
            int maxRows = 6; // siempre máximo 6 por bloque como pediste

            // Line height calculado para que 6 filas entren en availableHeight
            float lineHeight = Math.Max(16f, availableHeight / maxRows); // no menos de 16px

            // Posición de la primera fila (un poco debajo del encabezado)
            float currentY = headerY - 18f;

            // Dibujar cada línea (order.Lines contiene solo el chunk de 6 que pasan desde RenderOrders)
            foreach (var line in order.Lines)
            {
                // Título del articulo
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT,
                    new Phrase(line.Product.Description, _normal),
                    60, currentY, 0);

                // Nota del artículo (si existe) debajo de la descripción, con pequeño offset
                if (!string.IsNullOrWhiteSpace(line.Notes))
                {
                    ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT,
                        new Phrase(line.Notes, _italic),
                        60, currentY - (lineHeight * 0.35f), 0);
                }

                // Cantidad alineada a la derecha
                ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT,
                    new Phrase(line.Quantity.ToString("0.00"), _normal),
                    520, currentY, 0);

                // Bajar a la siguiente fila
                currentY -= lineHeight;
            }

        }








        private void DrawInfoBoxes(PdfContentByte cb, float yStart)
        {
            cb.SetLineWidth(1f);

            // Punto de referencia superior para los cuadros
            float top = yStart - 120f;

            // Cuadro Proveedor (más ancho)
            cb.RoundRectangle(45, top, 330, 55, 8);

            // Cuadro Fecha / Factura (a la derecha)
            cb.RoundRectangle(385, top, 155, 55, 8);

            // Cuadro Artículos (debajo, altura suficiente para 6 líneas)
            // 180 de alto aprox garantiza espacio para 6 filas + encabezado
            cb.RoundRectangle(45, top - 180f - 10f, 495, 180f, 8);

            cb.Stroke();
        }







        private void DrawContainer(PdfContentByte cb, float yStart, float blockHeight)
        {
            // Mantener la caja principal en la misma posición calculada por blockHeight
            cb.SetLineWidth(1f);
            cb.RoundRectangle(20, yStart - blockHeight, 555, blockHeight - 20, 12);
            cb.Stroke();
        }




        private void DrawHeader(Document doc, PdfContentByte cb, PurchaseOrder order, float yStart, string logoPath)
        {
            if (File.Exists(logoPath))
            {
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(90, 40);
                logo.SetAbsolutePosition(35, yStart - 50);
                doc.Add(logo);
            }

            Font branchFont = new Font(_bold);
            branchFont.Size = 16; // tamaño que tú quieras

            ColumnText.ShowTextAligned(
                cb,
                Element.ALIGN_RIGHT,
                new Phrase(order.Branch.Name, _bold),
                560,
                yStart - 35,
                0
            );
        }

        private void DrawSignatures(PdfContentByte cb, float yStart, float blockHeight)
        {
            float y = yStart - blockHeight + 20;

            DrawLine(cb, 120, y, 260, "Solicita");
            DrawLine(cb, 340, y, 480, "Autoriza");
        }



        private PdfPCell HeaderCell(string text, int align = Element.ALIGN_LEFT)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, _bold));
            cell.HorizontalAlignment = align;
            cell.Border = PdfPCell.NO_BORDER;
            cell.PaddingBottom = 4;
            return cell;
        }


        private PdfPCell BodyCell(object content, int align = Element.ALIGN_LEFT)
        {
            PdfPCell cell;

            if (content is Paragraph p)
                cell = new PdfPCell(p);
            else
                cell = new PdfPCell(new Phrase(content.ToString(), _normal));

            cell.HorizontalAlignment = align;
            cell.Border = Rectangle.NO_BORDER;

            // 🟢 Ajuste dinámico del espacio
            cell.PaddingTop = 6;
            cell.PaddingBottom = 6;

            return cell;
        }

        private void DrawLine(PdfContentByte cb, float x1, float y, float x2, string caption)
        {
            // Línea
            cb.MoveTo(x1, y);
            cb.LineTo(x2, y);
            cb.Stroke();

            // Texto centrado
            ColumnText.ShowTextAligned(
                cb,
                Element.ALIGN_CENTER,
                new Phrase(caption, _bold),
                (x1 + x2) / 2,
                y - 12,
                0
            );
        }







    }
}
