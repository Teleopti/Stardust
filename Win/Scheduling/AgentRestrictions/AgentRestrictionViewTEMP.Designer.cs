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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StudentAvailability"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "agentRestrictionGrid"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AgentRestrictionViewTEMP"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.button1 = new System.Windows.Forms.Button();
			this.agentRestrictionGrid = new Teleopti.Ccc.Win.Scheduling.AgentRestrictions.AgentRestrictionGrid(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.checkBoxAvailability = new System.Windows.Forms.CheckBox();
			this.checkBoxRotation = new System.Windows.Forms.CheckBox();
			this.checkBoxPreference = new System.Windows.Forms.CheckBox();
			this.checkBoxStudentAvailability = new System.Windows.Forms.CheckBox();
			this.checkBoxSchedule = new System.Windows.Forms.CheckBox();
			this.buttonRecalculate = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.agentRestrictionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(1234, 628);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 5;
			this.button1.Text = "Close";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.Button1Click);
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
			this.agentRestrictionGrid.Size = new System.Drawing.Size(1110, 593);
			this.agentRestrictionGrid.SmartSizeBox = false;
			this.agentRestrictionGrid.TabIndex = 6;
			this.agentRestrictionGrid.Text = "agentRestrictionGrid";
			this.agentRestrictionGrid.ThemesEnabled = true;
			this.agentRestrictionGrid.UseRightToLeftCompatibleTextBox = true;
			this.agentRestrictionGrid.VerticalThumbTrack = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 628);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 7;
			this.label1.Text = "label1";
			// 
			// checkBoxAvailability
			// 
			this.checkBoxAvailability.AutoSize = true;
			this.checkBoxAvailability.Checked = true;
			this.checkBoxAvailability.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAvailability.Location = new System.Drawing.Point(1146, 12);
			this.checkBoxAvailability.Name = "checkBoxAvailability";
			this.checkBoxAvailability.Size = new System.Drawing.Size(75, 17);
			this.checkBoxAvailability.TabIndex = 8;
			this.checkBoxAvailability.Text = "Availability";
			this.checkBoxAvailability.UseVisualStyleBackColor = true;
			// 
			// checkBoxRotation
			// 
			this.checkBoxRotation.AutoSize = true;
			this.checkBoxRotation.Checked = true;
			this.checkBoxRotation.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxRotation.Location = new System.Drawing.Point(1146, 35);
			this.checkBoxRotation.Name = "checkBoxRotation";
			this.checkBoxRotation.Size = new System.Drawing.Size(66, 17);
			this.checkBoxRotation.TabIndex = 9;
			this.checkBoxRotation.Text = "Rotation";
			this.checkBoxRotation.UseVisualStyleBackColor = true;
			// 
			// checkBoxPreference
			// 
			this.checkBoxPreference.AutoSize = true;
			this.checkBoxPreference.Checked = true;
			this.checkBoxPreference.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxPreference.Location = new System.Drawing.Point(1146, 58);
			this.checkBoxPreference.Name = "checkBoxPreference";
			this.checkBoxPreference.Size = new System.Drawing.Size(78, 17);
			this.checkBoxPreference.TabIndex = 10;
			this.checkBoxPreference.Text = "Preference";
			this.checkBoxPreference.UseVisualStyleBackColor = true;
			// 
			// checkBoxStudentAvailability
			// 
			this.checkBoxStudentAvailability.AutoSize = true;
			this.checkBoxStudentAvailability.Checked = true;
			this.checkBoxStudentAvailability.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxStudentAvailability.Location = new System.Drawing.Point(1146, 81);
			this.checkBoxStudentAvailability.Name = "checkBoxStudentAvailability";
			this.checkBoxStudentAvailability.Size = new System.Drawing.Size(112, 17);
			this.checkBoxStudentAvailability.TabIndex = 11;
			this.checkBoxStudentAvailability.Text = "StudentAvailability";
			this.checkBoxStudentAvailability.UseVisualStyleBackColor = true;
			// 
			// checkBoxSchedule
			// 
			this.checkBoxSchedule.AutoSize = true;
			this.checkBoxSchedule.Checked = true;
			this.checkBoxSchedule.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxSchedule.Location = new System.Drawing.Point(1146, 104);
			this.checkBoxSchedule.Name = "checkBoxSchedule";
			this.checkBoxSchedule.Size = new System.Drawing.Size(71, 17);
			this.checkBoxSchedule.TabIndex = 12;
			this.checkBoxSchedule.Text = "Schedule";
			this.checkBoxSchedule.UseVisualStyleBackColor = true;
			// 
			// buttonRecalculate
			// 
			this.buttonRecalculate.Location = new System.Drawing.Point(1146, 136);
			this.buttonRecalculate.Name = "buttonRecalculate";
			this.buttonRecalculate.Size = new System.Drawing.Size(75, 23);
			this.buttonRecalculate.TabIndex = 13;
			this.buttonRecalculate.Text = "Recalculate";
			this.buttonRecalculate.UseVisualStyleBackColor = true;
			this.buttonRecalculate.Click += new System.EventHandler(this.buttonRecalculate_Click);
			// 
			// AgentRestrictionViewTemp
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1321, 658);
			this.Controls.Add(this.buttonRecalculate);
			this.Controls.Add(this.checkBoxSchedule);
			this.Controls.Add(this.checkBoxStudentAvailability);
			this.Controls.Add(this.checkBoxPreference);
			this.Controls.Add(this.checkBoxRotation);
			this.Controls.Add(this.checkBoxAvailability);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.agentRestrictionGrid);
			this.Controls.Add(this.button1);
			this.Name = "AgentRestrictionViewTemp";
			this.Text = "AgentRestrictionViewTEMP";
			this.Load += new System.EventHandler(this.AgentRestrictionViewTempLoad);
			((System.ComponentModel.ISupportInitialize)(this.agentRestrictionGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private AgentRestrictionGrid agentRestrictionGrid;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBoxAvailability;
		private System.Windows.Forms.CheckBox checkBoxRotation;
		private System.Windows.Forms.CheckBox checkBoxPreference;
		private System.Windows.Forms.CheckBox checkBoxStudentAvailability;
		private System.Windows.Forms.CheckBox checkBoxSchedule;
		private System.Windows.Forms.Button buttonRecalculate;
	}
}