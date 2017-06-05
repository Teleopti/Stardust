namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	partial class AgentValidationResult
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvClose = new Syncfusion.Windows.Forms.ButtonAdv();
			this.masterGrid = new Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.masterGrid)).BeginInit();
			this.SuspendLayout();
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
			this.tableLayoutPanel1.TabIndex = 3;
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
			// masterGrid
			// 
			this.masterGrid.BackColor = System.Drawing.SystemColors.Window;
			this.masterGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.masterGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.masterGrid.FreezeCaption = false;
			this.masterGrid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.masterGrid.Location = new System.Drawing.Point(0, 0);
			this.masterGrid.Name = "masterGrid";
			this.masterGrid.Size = new System.Drawing.Size(788, 410);
			this.masterGrid.TabIndex = 5;
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
			// AgentValidationResult
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(788, 460);
			this.Controls.Add(this.masterGrid);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "AgentValidationResult";
			this.ShowIcon = false;
			this.Text = "xxSchedulingResult";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.agentValidationResultFormClosing);
			this.Load += new System.EventHandler(this.agentValidationResultLoad);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.masterGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvClose;
		private Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl masterGrid;
	}
}