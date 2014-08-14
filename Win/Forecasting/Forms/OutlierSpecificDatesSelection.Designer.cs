using DateSelectionControl=Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionControl;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	partial class OutlierSpecificDatesSelection
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
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanelDateSelection = new System.Windows.Forms.TableLayoutPanel();
			this.listBoxSelectedDates = new System.Windows.Forms.ListBox();
			this.buttonAdvRemove = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvAdd = new Syncfusion.Windows.Forms.ButtonAdv();
			this.dateSelectionControl1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionControl();
			this.tableLayoutPanelDateSelection.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanelDateSelection
			// 
			this.tableLayoutPanelDateSelection.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelDateSelection.ColumnCount = 3;
			this.tableLayoutPanelDateSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
			this.tableLayoutPanelDateSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanelDateSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelDateSelection.Controls.Add(this.listBoxSelectedDates, 2, 0);
			this.tableLayoutPanelDateSelection.Controls.Add(this.buttonAdvRemove, 1, 2);
			this.tableLayoutPanelDateSelection.Controls.Add(this.buttonAdvAdd, 1, 1);
			this.tableLayoutPanelDateSelection.Controls.Add(this.dateSelectionControl1, 0, 0);
			this.tableLayoutPanelDateSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelDateSelection.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelDateSelection.Name = "tableLayoutPanelDateSelection";
			this.tableLayoutPanelDateSelection.RowCount = 4;
			this.tableLayoutPanelDateSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelDateSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelDateSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelDateSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelDateSelection.Size = new System.Drawing.Size(366, 231);
			this.tableLayoutPanelDateSelection.TabIndex = 0;
			// 
			// listBoxSelectedDates
			// 
			this.listBoxSelectedDates.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxSelectedDates.FormattingEnabled = true;
			this.listBoxSelectedDates.ItemHeight = 15;
			this.listBoxSelectedDates.Location = new System.Drawing.Point(263, 3);
			this.listBoxSelectedDates.Name = "listBoxSelectedDates";
			this.tableLayoutPanelDateSelection.SetRowSpan(this.listBoxSelectedDates, 4);
			this.listBoxSelectedDates.Size = new System.Drawing.Size(100, 225);
			this.listBoxSelectedDates.TabIndex = 1;
			// 
			// buttonAdvRemove
			// 
			this.buttonAdvRemove.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvRemove.BeforeTouchSize = new System.Drawing.Size(54, 25);
			this.buttonAdvRemove.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonAdvRemove.ForeColor = System.Drawing.Color.White;
			this.buttonAdvRemove.IsBackStageButton = false;
			this.buttonAdvRemove.Location = new System.Drawing.Point(203, 118);
			this.buttonAdvRemove.Name = "buttonAdvRemove";
			this.buttonAdvRemove.Size = new System.Drawing.Size(54, 25);
			this.buttonAdvRemove.TabIndex = 3;
			this.buttonAdvRemove.Text = "xxDoubleArrowRemove";
			this.buttonAdvRemove.UseVisualStyle = true;
			this.buttonAdvRemove.Click += new System.EventHandler(this.buttonAdvRemoveClick);
			// 
			// buttonAdvAdd
			// 
			this.buttonAdvAdd.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvAdd.BeforeTouchSize = new System.Drawing.Size(54, 25);
			this.buttonAdvAdd.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonAdvAdd.ForeColor = System.Drawing.Color.White;
			this.buttonAdvAdd.IsBackStageButton = false;
			this.buttonAdvAdd.Location = new System.Drawing.Point(203, 81);
			this.buttonAdvAdd.Name = "buttonAdvAdd";
			this.buttonAdvAdd.Size = new System.Drawing.Size(54, 25);
			this.buttonAdvAdd.TabIndex = 2;
			this.buttonAdvAdd.Text = "xxDoubleArrowAdd";
			this.buttonAdvAdd.UseVisualStyle = true;
			this.buttonAdvAdd.Click += new System.EventHandler(this.buttonAdvAddClick);
			// 
			// dateSelectionControl1
			// 
			this.dateSelectionControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(228)))), ((int)(((byte)(246)))));
			this.dateSelectionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateSelectionControl1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateSelectionControl1.Location = new System.Drawing.Point(0, 0);
			this.dateSelectionControl1.Margin = new System.Windows.Forms.Padding(0);
			this.dateSelectionControl1.Name = "dateSelectionControl1";
			this.tableLayoutPanelDateSelection.SetRowSpan(this.dateSelectionControl1, 4);
			this.dateSelectionControl1.ShowAddButtons = false;
			this.dateSelectionControl1.ShowDateSelectionFromTo = false;
			this.dateSelectionControl1.ShowDateSelectionRolling = false;
			this.dateSelectionControl1.ShowTabArea = true;
			this.dateSelectionControl1.Size = new System.Drawing.Size(200, 231);
			this.dateSelectionControl1.TabIndex = 4;
			// 
			// OutlierSpecificDatesSelection
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.tableLayoutPanelDateSelection);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "OutlierSpecificDatesSelection";
			this.Size = new System.Drawing.Size(366, 231);
			this.tableLayoutPanelDateSelection.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDateSelection;
		private System.Windows.Forms.ListBox listBoxSelectedDates;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRemove;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAdd;
		private DateSelectionControl dateSelectionControl1;
	}
}
