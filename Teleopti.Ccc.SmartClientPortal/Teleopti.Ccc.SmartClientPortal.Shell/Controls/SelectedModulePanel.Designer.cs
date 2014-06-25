namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	partial class SelectedModulePanel
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
			this.paneImagelHeader = new System.Windows.Forms.Panel();
			this.autoLabelHeader = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.paneImagelHeader, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.autoLabelHeader, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(271, 32);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// paneImagelHeader
			// 
			this.paneImagelHeader.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.paneImagelHeader.Location = new System.Drawing.Point(0, 0);
			this.paneImagelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.paneImagelHeader.Name = "paneImagelHeader";
			this.paneImagelHeader.Size = new System.Drawing.Size(32, 32);
			this.paneImagelHeader.TabIndex = 0;
			// 
			// autoLabelHeader
			// 
			this.autoLabelHeader.AutoSize = false;
			this.autoLabelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabelHeader.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.autoLabelHeader.Location = new System.Drawing.Point(35, 0);
			this.autoLabelHeader.Name = "autoLabelHeader";
			this.autoLabelHeader.Size = new System.Drawing.Size(233, 32);
			this.autoLabelHeader.TabIndex = 1;
			this.autoLabelHeader.Text = "Header";
			this.autoLabelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SelectedModulePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.LightGray;
			this.Controls.Add(this.tableLayoutPanel1);
			this.MaximumSize = new System.Drawing.Size(400, 32);
			this.MinimumSize = new System.Drawing.Size(32, 32);
			this.Name = "SelectedModulePanel";
			this.Size = new System.Drawing.Size(271, 32);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel paneImagelHeader;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelHeader;
	}
}
