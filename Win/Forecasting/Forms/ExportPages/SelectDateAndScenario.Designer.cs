﻿namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
	partial class SelectDateAndScenario
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectDateAndScenario));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.reportDateFromToSelector1 = new Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxScenario = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.labelScenario = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxScenario)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.reportDateFromToSelector1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(400, 340);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// reportDateFromToSelector1
			// 
			this.reportDateFromToSelector1.EnableNullDates = true;
			this.reportDateFromToSelector1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.reportDateFromToSelector1.Location = new System.Drawing.Point(0, 23);
			this.reportDateFromToSelector1.Margin = new System.Windows.Forms.Padding(0, 23, 0, 0);
			this.reportDateFromToSelector1.Name = "reportDateFromToSelector1";
			this.reportDateFromToSelector1.NullString = "xxNoDateIsSelected";
			this.reportDateFromToSelector1.Size = new System.Drawing.Size(305, 69);
			this.reportDateFromToSelector1.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 2);
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.03279F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.96721F));
			this.tableLayoutPanel2.Controls.Add(this.comboBoxScenario, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.labelScenario, 0, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 152);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(374, 102);
			this.tableLayoutPanel2.TabIndex = 5;
			// 
			// comboBoxScenario
			// 
			this.comboBoxScenario.BackColor = System.Drawing.Color.White;
			this.comboBoxScenario.BeforeTouchSize = new System.Drawing.Size(166, 25);
			this.comboBoxScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxScenario.Location = new System.Drawing.Point(147, 7);
			this.comboBoxScenario.Margin = new System.Windows.Forms.Padding(5, 7, 6, 7);
			this.comboBoxScenario.Name = "comboBoxScenario";
			this.comboBoxScenario.Size = new System.Drawing.Size(166, 25);
			this.comboBoxScenario.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxScenario.TabIndex = 3;
			// 
			// labelScenario
			// 
			this.labelScenario.Location = new System.Drawing.Point(3, 0);
			this.labelScenario.Name = "labelScenario";
			this.labelScenario.Size = new System.Drawing.Size(100, 34);
			this.labelScenario.TabIndex = 3;
			this.labelScenario.Text = "xxScenarioColon";
			this.labelScenario.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SelectDateAndScenario
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.Name = "SelectDateAndScenario";
			this.Size = new System.Drawing.Size(400, 340);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.comboBoxScenario)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Reporting.ReportDateFromToSelector reportDateFromToSelector1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxScenario;
		private System.Windows.Forms.Label labelScenario;

	}
}
