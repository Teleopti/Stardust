namespace Teleopti.Ccc.Win.Forecasting.Forms.WorkloadPages
{
    partial class WorkloadQueues
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "Description",
            "Log Object 1"}, -1);
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.textBoxExFilter = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.label1 = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.listViewQueues = new System.Windows.Forms.ListView();
			this.columnHeaderQueue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderLogObject = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.buttonImport = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExFilter)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel2.SetColumnSpan(this.tableLayoutPanel1, 2);
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.textBoxExFilter, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(273, 24);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// textBoxExFilter
			// 
			this.textBoxExFilter.BackColor = System.Drawing.Color.White;
			this.textBoxExFilter.BeforeTouchSize = new System.Drawing.Size(187, 22);
			this.textBoxExFilter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxExFilter.Location = new System.Drawing.Point(83, 3);
			this.textBoxExFilter.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExFilter.Name = "textBoxExFilter";
			this.textBoxExFilter.Size = new System.Drawing.Size(187, 22);
			this.textBoxExFilter.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExFilter.TabIndex = 0;
			this.textBoxExFilter.TextChanged += new System.EventHandler(this.textBoxExFilter_TextChanged);
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "xxFilterColon";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.listViewQueues, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonImport, 1, 2);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(10, 10);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(279, 170);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// listViewQueues
			// 
			this.listViewQueues.Activation = System.Windows.Forms.ItemActivation.TwoClick;
			this.listViewQueues.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listViewQueues.CheckBoxes = true;
			this.listViewQueues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderQueue,
            this.columnHeaderLogObject,
            this.columnHeaderDescription});
			this.tableLayoutPanel2.SetColumnSpan(this.listViewQueues, 2);
			this.listViewQueues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewQueues.FullRowSelect = true;
			listViewItem1.StateImageIndex = 0;
			this.listViewQueues.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
			this.listViewQueues.Location = new System.Drawing.Point(5, 30);
			this.listViewQueues.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.listViewQueues.Name = "listViewQueues";
			this.listViewQueues.ShowGroups = false;
			this.listViewQueues.Size = new System.Drawing.Size(269, 110);
			this.listViewQueues.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewQueues.TabIndex = 5;
			this.listViewQueues.UseCompatibleStateImageBehavior = false;
			this.listViewQueues.View = System.Windows.Forms.View.Details;
			this.listViewQueues.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewQueues_ColumnClick);
			this.listViewQueues.Resize += new System.EventHandler(this.listViewQueues_Resize);
			// 
			// columnHeaderQueue
			// 
			this.columnHeaderQueue.Text = "xxQueue";
			this.columnHeaderQueue.Width = 100;
			// 
			// columnHeaderLogObject
			// 
			this.columnHeaderLogObject.Text = "xxLogObject";
			this.columnHeaderLogObject.Width = 116;
			// 
			// columnHeaderDescription
			// 
			this.columnHeaderDescription.Text = "xxDescription";
			this.columnHeaderDescription.Width = 116;
			// 
			// buttonImport
			// 
			this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonImport.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonImport.BeforeTouchSize = new System.Drawing.Size(131, 23);
			this.buttonImport.ForeColor = System.Drawing.Color.White;
			this.buttonImport.IsBackStageButton = false;
			this.buttonImport.Location = new System.Drawing.Point(142, 144);
			this.buttonImport.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.buttonImport.Name = "buttonImport";
			this.buttonImport.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonImport.Size = new System.Drawing.Size(131, 23);
			this.buttonImport.TabIndex = 6;
			this.buttonImport.Text = "xxImportQueueFromFile";
			this.buttonImport.UseVisualStyle = true;
			this.buttonImport.Click += new System.EventHandler(this.import_Click);
			// 
			// WorkloadQueues
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanel2);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "WorkloadQueues";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(299, 190);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExFilter)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ListView listViewQueues;
        private System.Windows.Forms.ColumnHeader columnHeaderDescription;
        private System.Windows.Forms.ColumnHeader columnHeaderLogObject;
        private System.Windows.Forms.ColumnHeader columnHeaderQueue;
        private Syncfusion.Windows.Forms.ButtonAdv buttonImport;

    }
}