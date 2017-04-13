namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
	partial class SelectExportType
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.rbtExportToFile = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.rbtExportToBU = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rbtExportToFile)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.rbtExportToBU)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.rbtExportToFile, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.rbtExportToBU, 1, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(400, 340);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// rbtExportToFile
			// 
			this.rbtExportToFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.rbtExportToFile.BeforeTouchSize = new System.Drawing.Size(386, 23);
			this.rbtExportToFile.DrawFocusRectangle = false;
			this.rbtExportToFile.Location = new System.Drawing.Point(11, 26);
			this.rbtExportToFile.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.rbtExportToFile.Name = "rbtExportToFile";
			this.rbtExportToFile.Size = new System.Drawing.Size(386, 23);
			this.rbtExportToFile.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.rbtExportToFile.TabIndex = 4;
			this.rbtExportToFile.Text = "xxExportToFile";
			this.rbtExportToFile.ThemesEnabled = false;
			// 
			// rbtExportToBU
			// 
			this.rbtExportToBU.BeforeTouchSize = new System.Drawing.Size(386, 23);
			this.rbtExportToBU.DrawFocusRectangle = false;
			this.rbtExportToBU.Location = new System.Drawing.Point(11, 55);
			this.rbtExportToBU.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.rbtExportToBU.Name = "rbtExportToBU";
			this.rbtExportToBU.Size = new System.Drawing.Size(386, 23);
			this.rbtExportToBU.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.rbtExportToBU.TabIndex = 5;
			this.rbtExportToBU.Text = "xxExportToBU";
			this.rbtExportToBU.ThemesEnabled = false;
			// 
			// SelectExportType
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.Name = "SelectExportType";
			this.Size = new System.Drawing.Size(400, 340);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.rbtExportToFile)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.rbtExportToBU)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv rbtExportToFile;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv rbtExportToBU;

	}
}
