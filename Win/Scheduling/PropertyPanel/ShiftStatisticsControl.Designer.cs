namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	partial class ShiftStatisticsControl
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
			this.gridControl1 = new Syncfusion.Windows.Forms.Grid.GridControl();
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
			this.SuspendLayout();
			// 
			// gridControl1
			// 
			this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControl1.Location = new System.Drawing.Point(0, 0);
			this.gridControl1.Name = "gridControl1";
			this.gridControl1.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControl1.Size = new System.Drawing.Size(289, 281);
			this.gridControl1.SmartSizeBox = false;
			this.gridControl1.TabIndex = 0;
			this.gridControl1.Text = "gridControl1";
			this.gridControl1.UseRightToLeftCompatibleTextBox = true;
			this.gridControl1.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.gridControl1_QueryCellInfo);
			this.gridControl1.QueryColCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.gridControl1_QueryColCount);
			this.gridControl1.QueryRowCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.gridControl1_QueryRowCount);
			this.gridControl1.CellDoubleClick += new Syncfusion.Windows.Forms.Grid.GridCellClickEventHandler(this.gridControl1_CellDoubleClick);
			// 
			// ShiftStatisticsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.gridControl1);
			this.Name = "ShiftStatisticsControl";
			this.Size = new System.Drawing.Size(289, 281);
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Grid.GridControl gridControl1;
	}
}
