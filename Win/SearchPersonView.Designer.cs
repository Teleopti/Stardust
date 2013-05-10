namespace Teleopti.Ccc.Win
{
    partial class SearchPersonView
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
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.gridListControl1 = new Syncfusion.Windows.Forms.Grid.GridListControl();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridListControl1)).BeginInit();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.textBox1.Location = new System.Drawing.Point(3, 5);
			this.textBox1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(388, 21);
			this.textBox1.TabIndex = 1;
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.gridListControl1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.textBox1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(394, 521);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// gridListControl1
			// 
			this.gridListControl1.BackColor = System.Drawing.SystemColors.Control;
			this.gridListControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.gridListControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridListControl1.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridListControl1.ItemHeight = 17;
			this.gridListControl1.Location = new System.Drawing.Point(3, 30);
			this.gridListControl1.MultiColumn = false;
			this.gridListControl1.Name = "gridListControl1";
			this.gridListControl1.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridListControl1.SelectedIndex = -1;
			this.gridListControl1.Size = new System.Drawing.Size(388, 488);
			this.gridListControl1.TabIndex = 3;
			this.gridListControl1.ThemesEnabled = true;
			this.gridListControl1.TopIndex = 0;
			// 
			// SearchPersonView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SearchPersonView";
			this.Size = new System.Drawing.Size(394, 521);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridListControl1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Grid.GridListControl gridListControl1;
    }
}
