using System;
using System.Drawing;
using System.Windows.Forms;

namespace PurchaseOrders.Views
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelGrid = new System.Windows.Forms.Panel();
            this.dgvOrders = new System.Windows.Forms.DataGridView();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnCancelOrders = new System.Windows.Forms.Button();
            this.btnExportar = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();

            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).BeginInit();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();

            // 
            // panelGrid
            // 
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Controls.Add(this.dgvOrders);
            this.panelGrid.Location = new System.Drawing.Point(0, 0);
            this.panelGrid.Name = "panelGrid";

            // 
            // dgvOrders
            // 
            this.dgvOrders.AllowUserToAddRows = false;
            this.dgvOrders.AllowUserToDeleteRows = false;
            this.dgvOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvOrders.RowHeadersVisible = false;
            this.dgvOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOrders.MultiSelect = false;
            this.dgvOrders.ReadOnly = true;
            this.dgvOrders.AllowUserToOrderColumns = true;
            this.dgvOrders.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOrders.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvOrders_CellDoubleClick);
            this.dgvOrders.RowPrePaint += dgvOrders_RowPrePaint;
            this.dgvOrders.CellMouseDown += dgvOrders_CellMouseDown;
            this.dgvOrders.SelectionChanged += dgvOrders_SelectionChanged;
            this.dgvOrders.ColumnHeaderMouseClick += dgvOrders_ColumnHeaderMouseClick;

            // 
            // panelButtons
            // 
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.BorderStyle = BorderStyle.FixedSingle;
            this.panelButtons.Height = 50;
            this.panelButtons.Controls.Add(this.btnCancelOrders);
            this.panelButtons.Controls.Add(this.btnExportar);
            this.panelButtons.Controls.Add(this.btnRefresh);
            this.panelButtons.Controls.Add(this.btnNew);
            this.panelButtons.Controls.Add(this.btnExit);

            // 
            // btnCancelOrders
            // 
            this.btnCancelOrders.Location = new System.Drawing.Point(10, 13);
            this.btnCancelOrders.Size = new System.Drawing.Size(110, 24);
            this.btnCancelOrders.Text = "Cancelar orden";
            this.btnCancelOrders.Click += new System.EventHandler(this.btnCancelOrders_Click);

            // 
            // btnExportar
            // 
            this.btnExportar.Location = new System.Drawing.Point(130, 13);
            this.btnExportar.Size = new System.Drawing.Size(110, 24);
            this.btnExportar.Text = "Exportar PDF";
            this.btnExportar.Click += new System.EventHandler(this.btnExportPdf_Click);

            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnRefresh.Location = new Point(panelButtons.Width - 240, 13);
            this.btnRefresh.Size = new System.Drawing.Size(75, 24);
            this.btnRefresh.Text = "Refrescar";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // 
            // btnNew
            // 
            this.btnNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnNew.Location = new Point(panelButtons.Width - 160, 13);
            this.btnNew.Size = new System.Drawing.Size(65, 24);
            this.btnNew.Text = "Nuevo";
            this.btnNew.Click += new System.EventHandler(this.btnNewOrder_Click);

            // 
            // btnExit
            // 
            this.btnExit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnExit.Location = new Point(panelButtons.Width - 80, 13);
            this.btnExit.Size = new System.Drawing.Size(65, 24);
            this.btnExit.Text = "Salir";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(618, 375);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Padding = new Padding(8);
            this.Controls.Add(this.panelGrid);
            this.Controls.Add(this.panelButtons);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gestión de Órdenes de Compra";
            this.Load += new System.EventHandler(this.MainForm_Load);

            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrders)).EndInit();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }




        #endregion

        private System.Windows.Forms.Panel panelGrid;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.DataGridView dgvOrders;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnCancelOrders;
        private System.Windows.Forms.Button btnExportar;
    }
}