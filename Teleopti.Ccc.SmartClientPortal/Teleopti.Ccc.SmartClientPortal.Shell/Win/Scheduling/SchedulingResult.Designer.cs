namespace Teleopti.Ccc.Win.Scheduling
{
	partial class SchedulingResult
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
			this.components = new System.ComponentModel.Container();
			this.masterGrid = new Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl();
			this.detailGrid = new Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl();
			this.splitContainerAdv1 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.buttonAdvClose = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.masterGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.detailGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).BeginInit();
			this.splitContainerAdv1.Panel1.SuspendLayout();
			this.splitContainerAdv1.Panel2.SuspendLayout();
			this.splitContainerAdv1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// masterGrid
			// 
			this.masterGrid.BackColor = System.Drawing.SystemColors.Window;
			this.masterGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.masterGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.masterGrid.FreezeCaption = false;
			this.masterGrid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.masterGrid.Location = new System.Drawing.Point(0, 0);
			this.masterGrid.Name = "masterGrid";
			this.masterGrid.Size = new System.Drawing.Size(788, 280);
			this.masterGrid.TabIndex = 4;
			this.masterGrid.TableOptions.AllowSelection = ((Syncfusion.Windows.Forms.Grid.GridSelectionFlags)((((Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Row | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Column) 
			| Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Table) 
			| Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Multiple)));
			this.masterGrid.TableOptions.ListBoxSelectionColorOptions = Syncfusion.Windows.Forms.Grid.Grouping.GridListBoxSelectionColorOptions.None;
			this.masterGrid.TableOptions.ListBoxSelectionMode = System.Windows.Forms.SelectionMode.None;
			this.masterGrid.Text = "gridGroupingControl2";
			this.masterGrid.TopLevelGroupOptions.ShowAddNewRecordBeforeDetails = false;
			this.masterGrid.TopLevelGroupOptions.ShowCaption = false;
			this.masterGrid.VersionInfo = "6.403.0.15";
			// 
			// detailGrid
			// 
			this.detailGrid.BackColor = System.Drawing.SystemColors.Window;
			this.detailGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.detailGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailGrid.FreezeCaption = false;
			this.detailGrid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.detailGrid.Location = new System.Drawing.Point(0, 0);
			this.detailGrid.Name = "detailGrid";
			this.detailGrid.ShowRelationFields = Syncfusion.Grouping.ShowRelationFields.ShowAllRelatedFields;
			this.detailGrid.Size = new System.Drawing.Size(788, 177);
			this.detailGrid.SortMappingNames = true;
			this.detailGrid.TabIndex = 5;
			this.detailGrid.TableOptions.AllowSelection = ((Syncfusion.Windows.Forms.Grid.GridSelectionFlags)(((Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Row | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Column) 
			| Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Table)));
			this.detailGrid.Text = "gridGroupingControl3";
			this.detailGrid.TopLevelGroupOptions.ShowAddNewRecordBeforeDetails = false;
			this.detailGrid.TopLevelGroupOptions.ShowCaption = false;
			this.detailGrid.VersionInfo = "6.403.0.15";
			// 
			// splitContainerAdv1
			// 
			this.splitContainerAdv1.BeforeTouchSize = 3;
			this.splitContainerAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdv1.Location = new System.Drawing.Point(0, 0);
			this.splitContainerAdv1.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainerAdv1.Name = "splitContainerAdv1";
			this.splitContainerAdv1.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// splitContainerAdv1.Panel1
			// 
			this.splitContainerAdv1.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv1.Panel1.Controls.Add(this.masterGrid);
			// 
			// splitContainerAdv1.Panel2
			// 
			this.splitContainerAdv1.Panel2.AutoSize = true;
			this.splitContainerAdv1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv1.Panel2.Controls.Add(this.detailGrid);
			this.splitContainerAdv1.Panel2.MinimumSize = new System.Drawing.Size(0, 115);
			this.splitContainerAdv1.Size = new System.Drawing.Size(788, 460);
			this.splitContainerAdv1.SplitterDistance = 280;
			this.splitContainerAdv1.SplitterWidth = 3;
			this.splitContainerAdv1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdv1.TabIndex = 6;
			this.splitContainerAdv1.Text = "splitContainerAdv1";
			this.splitContainerAdv1.ThemesEnabled = true;
			// 
			// buttonAdvClose
			// 
			this.buttonAdvClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvClose.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvClose.ForeColor = System.Drawing.Color.White;
			this.buttonAdvClose.IsBackStageButton = false;
			this.buttonAdvClose.Location = new System.Drawing.Point(691, 13);
			this.buttonAdvClose.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvClose.Name = "buttonAdvClose";
			this.buttonAdvClose.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvClose.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvClose.TabIndex = 1;
			this.buttonAdvClose.Text = "xxClose";
			this.buttonAdvClose.UseVisualStyle = true;
			this.buttonAdvClose.Click += new System.EventHandler(this.buttonAdvCloseClick);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvClose, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 410);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(788, 50);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// SchedulingResult
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(788, 460);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.splitContainerAdv1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinimizeBox = false;
			this.Name = "SchedulingResult";
			this.ShowIcon = false;
			this.Text = "xxSchedulingResult";
			this.Load += new System.EventHandler(this.schedulingResultLoad);
			((System.ComponentModel.ISupportInitialize)(this.masterGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.detailGrid)).EndInit();
			this.splitContainerAdv1.Panel1.ResumeLayout(false);
			this.splitContainerAdv1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).EndInit();
			this.splitContainerAdv1.ResumeLayout(false);
			this.splitContainerAdv1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl masterGrid;
		private Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl detailGrid;
		private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvClose;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;


	}
}