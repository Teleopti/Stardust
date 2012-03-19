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
        private void InitializeComponent()
        {
            this.comboBoxAdvPersons = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.listViewRequests = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdv1 = new Syncfusion.Windows.Forms.ButtonAdv();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonAdvNext = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvPrevious = new Syncfusion.Windows.Forms.ButtonAdv();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvPersons)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxAdvPersons
            // 
            this.comboBoxAdvPersons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
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
            this.comboBoxAdvPersons.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvPersons.TabIndex = 0;
            this.comboBoxAdvPersons.Text = "Banaggg";
            this.comboBoxAdvPersons.ValueMember = "Id";
            this.comboBoxAdvPersons.SelectedIndexChanged += new System.EventHandler(this.ComboBoxAdvPersonsSelectedIndexChanged);
            // 
            // listViewRequests
            // 
            this.listViewRequests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewRequests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewRequests.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.listViewRequests.FullRowSelect = true;
            this.listViewRequests.HideSelection = false;
            this.listViewRequests.Location = new System.Drawing.Point(10, 47);
            this.listViewRequests.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.listViewRequests.Name = "listViewRequests";
            this.listViewRequests.Size = new System.Drawing.Size(381, 463);
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
            this.columnHeader2.Width = 115;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "xxDate";
            this.columnHeader3.Width = 166;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.buttonAdv1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvPersons, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.listViewRequests, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.610567F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.38943F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(788, 564);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // buttonAdv1
            // 
            this.buttonAdv1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdv1.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdv1.CustomManagedColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonAdv1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
            this.panel1.Controls.Add(this.buttonAdvNext);
            this.panel1.Controls.Add(this.buttonAdvPrevious);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 523);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(388, 41);
            this.panel1.TabIndex = 2;
            // 
            // buttonAdvNext
            // 
            this.buttonAdvNext.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvNext.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvNext.CustomManagedColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonAdvNext.Location = new System.Drawing.Point(198, 6);
            this.buttonAdvNext.Name = "buttonAdvNext";
            this.buttonAdvNext.Size = new System.Drawing.Size(50, 23);
            this.buttonAdvNext.TabIndex = 1;
            this.buttonAdvNext.Text = ">";
            this.buttonAdvNext.UseVisualStyle = true;
            this.buttonAdvNext.Click += new System.EventHandler(this.ButtonAdvNextClick);
            // 
            // buttonAdvPrevious
            // 
            this.buttonAdvPrevious.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdvPrevious.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvPrevious.CustomManagedColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonAdvPrevious.Location = new System.Drawing.Point(140, 6);
            this.buttonAdvPrevious.Name = "buttonAdvPrevious";
            this.buttonAdvPrevious.Size = new System.Drawing.Size(50, 23);
            this.buttonAdvPrevious.TabIndex = 0;
            this.buttonAdvPrevious.Text = "<";
            this.buttonAdvPrevious.UseVisualStyle = true;
            this.buttonAdvPrevious.Click += new System.EventHandler(this.ButtonAdvPreviousClick);
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(404, 10);
            this.textBox1.Margin = new System.Windows.Forms.Padding(10);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.tableLayoutPanel1.SetRowSpan(this.textBox1, 2);
            this.textBox1.Size = new System.Drawing.Size(374, 493);
            this.textBox1.TabIndex = 3;
            // 
            // RequestHistoryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 564);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "RequestHistoryView";
            this.Text = "xxRequestHistoryView";
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvPersons)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
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
        private System.Windows.Forms.TextBox textBox1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvNext;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvPrevious;
    }
}