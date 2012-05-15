namespace Teleopti.Ccc.Win.Scheduling.AgentRestrictions
{
	partial class AgentRestrictionViewTemp
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "agentRestrictionGrid"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AgentRestrictionViewTEMP"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.agentRestrictionGrid = new Teleopti.Ccc.Win.Scheduling.AgentRestrictions.AgentRestrictionGrid(this.components);
			this.button3 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.agentRestrictionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(689, 386);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 5;
			this.button1.Text = "Close";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.Button1Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(12, 386);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 7;
			this.button2.Text = "Finished";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// agentRestrictionGrid
			// 
			this.agentRestrictionGrid.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.DblClickOnCell;
			
			this.agentRestrictionGrid.ColCount = 12;
			this.agentRestrictionGrid.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 45),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(1, 66),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(2, 65),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(3, 65),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(4, 65),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(5, 128),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(6, 65),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(7, 90),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(8, 65),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(9, 65),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(10, 65),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(11, 65),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(12, 65)});
			this.agentRestrictionGrid.CoveredRanges.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeInfo[] {
            Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cells(0, 2, 0, 6),
            Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cells(0, 7, 0, 8),
            Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cells(0, 9, 0, 12)});
			this.agentRestrictionGrid.ExcelLikeCurrentCell = true;
			this.agentRestrictionGrid.ExcelLikeSelectionFrame = true;
			this.agentRestrictionGrid.GridLineColor = System.Drawing.SystemColors.GrayText;
			this.agentRestrictionGrid.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.agentRestrictionGrid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.agentRestrictionGrid.HorizontalThumbTrack = true;
			this.agentRestrictionGrid.Location = new System.Drawing.Point(12, 12);
			this.agentRestrictionGrid.Name = "agentRestrictionGrid";
			this.agentRestrictionGrid.NumberedColHeaders = false;
			this.agentRestrictionGrid.NumberedRowHeaders = false;
			this.agentRestrictionGrid.Office2007ScrollBars = true;
			this.agentRestrictionGrid.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
			this.agentRestrictionGrid.ReadOnly = true;
			this.agentRestrictionGrid.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.agentRestrictionGrid.RowCount = 1;
			this.agentRestrictionGrid.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.agentRestrictionGrid.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.agentRestrictionGrid.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.agentRestrictionGrid.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.AlwaysVisible;
			this.agentRestrictionGrid.Size = new System.Drawing.Size(752, 212);
			this.agentRestrictionGrid.SmartSizeBox = false;
			this.agentRestrictionGrid.TabIndex = 6;
			this.agentRestrictionGrid.Text = "agentRestrictionGrid";
			this.agentRestrictionGrid.ThemesEnabled = true;
			this.agentRestrictionGrid.UseRightToLeftCompatibleTextBox = true;
			this.agentRestrictionGrid.VerticalThumbTrack = true;
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(93, 386);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 8;
			this.button3.Text = "Loading";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// AgentRestrictionViewTemp
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(776, 421);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.agentRestrictionGrid);
			this.Controls.Add(this.button1);
			this.Name = "AgentRestrictionViewTemp";
			this.Text = "AgentRestrictionViewTEMP";
			this.Load += new System.EventHandler(this.AgentRestrictionViewTempLoad);
			((System.ComponentModel.ISupportInitialize)(this.agentRestrictionGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private AgentRestrictionGrid agentRestrictionGrid;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
	}
}