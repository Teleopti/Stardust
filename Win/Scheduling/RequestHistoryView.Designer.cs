namespace Teleopti.Ccc.Win.Scheduling
{
    partial class RequestHistoryView
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxSubject")]
		private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RequestHistoryView));
			this.comboBoxAdvPersons = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.listViewRequests = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdv1 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panel1 = new System.Windows.Forms.Panel();
			this.linkPrevious = new System.Windows.Forms.LinkLabel();
			this.linkNext = new System.Windows.Forms.LinkLabel();
			this.textBox1 = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvPersons)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// comboBoxAdvPersons
			// 
			this.comboBoxAdvPersons.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvPersons.BeforeTouchSize = new System.Drawing.Size(259, 24);
			this.comboBoxAdvPersons.DisplayMember = "Name";
			this.comboBoxAdvPersons.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvPersons.Font = new System.Drawing.Font("Tahoma", 9.5F);
			this.comboBoxAdvPersons.Items.AddRange(new object[] {
            "Banaggg"});
			this.comboBoxAdvPersons.ItemsImageIndexes.Add(new Syncfusion.Windows.Forms.Tools.ComboBoxAdv.ImageIndexItem(this.comboBoxAdvPersons, "Banaggg"));
			this.comboBoxAdvPersons.Location = new System.Drawing.Point(10, 10);
			this.comboBoxAdvPersons.Margin = new System.Windows.Forms.Padding(10, 10, 3, 3);
			this.comboBoxAdvPersons.MaxDropDownItems = 25;
			this.comboBoxAdvPersons.Name = "comboBoxAdvPersons";
			this.comboBoxAdvPersons.Size = new System.Drawing.Size(259, 24);
			this.comboBoxAdvPersons.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvPersons.TabIndex = 0;
			this.comboBoxAdvPersons.Text = "Banaggg";
			this.comboBoxAdvPersons.ValueMember = "Id";
			this.comboBoxAdvPersons.SelectedIndexChanged += new System.EventHandler(this.ComboBoxAdvPersonsSelectedIndexChanged);
			// 
			// listViewRequests
			// 
			this.listViewRequests.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listViewRequests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
			this.listViewRequests.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewRequests.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.listViewRequests.FullRowSelect = true;
			this.listViewRequests.HideSelection = false;
			this.listViewRequests.Location = new System.Drawing.Point(9, 43);
			this.listViewRequests.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
			this.listViewRequests.Name = "listViewRequests";
			this.listViewRequests.Size = new System.Drawing.Size(460, 468);
			this.listViewRequests.TabIndex = 1;
			this.listViewRequests.UseCompatibleStateImageBehavior = false;
			this.listViewRequests.View = System.Windows.Forms.View.Details;
			this.listViewRequests.SelectedIndexChanged += new System.EventHandler(this.listViewRequests_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "xxType";
			this.columnHeader1.Width = 84;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "xxStatus";
			this.columnHeader2.Width = 76;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "xxDate";
			this.columnHeader3.Width = 139;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "xxSubject";
			this.columnHeader4.Width = 157;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.tableLayoutPanel1.Controls.Add(this.buttonAdv1, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvPersons, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.listViewRequests, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.textBox1, 1, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(788, 564);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// buttonAdv1
			// 
			this.buttonAdv1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdv1.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdv1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdv1.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonAdv1.CustomManagedColor = System.Drawing.SystemColors.ActiveCaption;
			this.buttonAdv1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdv1.ForeColor = System.Drawing.Color.White;
			this.buttonAdv1.IsBackStageButton = false;
			this.buttonAdv1.Location = new System.Drawing.Point(653, 531);
			this.buttonAdv1.Margin = new System.Windows.Forms.Padding(3, 3, 60, 10);
			this.buttonAdv1.Name = "buttonAdv1";
			this.buttonAdv1.Size = new System.Drawing.Size(75, 23);
			this.buttonAdv1.TabIndex = 4;
			this.buttonAdv1.Text = "xxClose";
			this.buttonAdv1.UseVisualStyle = true;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.linkPrevious);
			this.panel1.Controls.Add(this.linkNext);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 524);
			this.panel1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(466, 40);
			this.panel1.TabIndex = 2;
			// 
			// linkPrevious
			// 
			this.linkPrevious.ActiveLinkColor = System.Drawing.Color.Blue;
			this.linkPrevious.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.linkPrevious.AutoSize = true;
			this.linkPrevious.Location = new System.Drawing.Point(130, 8);
			this.linkPrevious.Name = "linkPrevious";
			this.linkPrevious.Size = new System.Drawing.Size(60, 13);
			this.linkPrevious.TabIndex = 3;
			this.linkPrevious.TabStop = true;
			this.linkPrevious.Text = "xxPrevious";
			this.linkPrevious.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkPreviousLinkClicked);
			// 
			// linkNext
			// 
			this.linkNext.ActiveLinkColor = System.Drawing.Color.Blue;
			this.linkNext.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.linkNext.AutoSize = true;
			this.linkNext.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkNext.Location = new System.Drawing.Point(288, 8);
			this.linkNext.Name = "linkNext";
			this.linkNext.Size = new System.Drawing.Size(42, 13);
			this.linkNext.TabIndex = 2;
			this.linkNext.TabStop = true;
			this.linkNext.Text = "xxNext";
			this.linkNext.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkNextLinkClicked);
			// 
			// textBox1
			// 
			this.textBox1.BackColor = System.Drawing.Color.White;
			this.textBox1.BeforeTouchSize = new System.Drawing.Size(296, 461);
			this.textBox1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox1.Location = new System.Drawing.Point(482, 43);
			this.textBox1.Margin = new System.Windows.Forms.Padding(10, 3, 10, 10);
			this.textBox1.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(296, 461);
			this.textBox1.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBox1.TabIndex = 3;
			// 
			// RequestHistoryView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(788, 564);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(800, 600);
			this.Name = "RequestHistoryView";
			this.Text = "xxRequestHistory";
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvPersons)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBox1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvPersons;
        private System.Windows.Forms.ListView listViewRequests;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBox1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv1;
        private System.Windows.Forms.LinkLabel linkNext;
        private System.Windows.Forms.LinkLabel linkPrevious;
		private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}