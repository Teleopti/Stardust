namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    partial class SelectTargetDatesAndScenario
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectTargetDatesAndScenario));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.comboBoxScenario = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.TargetFromTo = new Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector();
			this.labelScenario = new System.Windows.Forms.Label();
			this.checkBoxUseDayOfMonth = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxScenario)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUseDayOfMonth)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.comboBoxScenario, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.TargetFromTo, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelScenario, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.checkBoxUseDayOfMonth, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 244F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(546, 358);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.SetColumnSpan(this.label1, 3);
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.label1.ImageAlign = System.Drawing.ContentAlignment.TopRight;
			this.label1.Location = new System.Drawing.Point(12, 126);
			this.label1.Margin = new System.Windows.Forms.Padding(12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(522, 220);
			this.label1.TabIndex = 4;
			this.label1.Text = "xxTheQuickForecatWillStartInBackgroundMessage";
			// 
			// comboBoxScenario
			// 
			this.comboBoxScenario.BackColor = System.Drawing.Color.White;
			this.comboBoxScenario.BeforeTouchSize = new System.Drawing.Size(166, 23);
			this.comboBoxScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxScenario.Location = new System.Drawing.Point(325, 9);
			this.comboBoxScenario.Margin = new System.Windows.Forms.Padding(5, 9, 6, 6);
			this.comboBoxScenario.Name = "comboBoxScenario";
			this.comboBoxScenario.Size = new System.Drawing.Size(166, 23);
			this.comboBoxScenario.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxScenario.TabIndex = 3;
			// 
			// TargetFromTo
			// 
			this.TargetFromTo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TargetFromTo.EnableNullDates = true;
			this.TargetFromTo.Font = new System.Drawing.Font("Segoe UI", 8F);
			this.TargetFromTo.Location = new System.Drawing.Point(3, 6);
			this.TargetFromTo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.TargetFromTo.Name = "TargetFromTo";
			this.TargetFromTo.NullString = "xxNoDateIsSelected";
			this.TargetFromTo.Size = new System.Drawing.Size(214, 61);
			this.TargetFromTo.TabIndex = 0;
			this.TargetFromTo.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("TargetFromTo.WorkPeriodEnd")));
			this.TargetFromTo.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("TargetFromTo.WorkPeriodStart")));
			// 
			// labelScenario
			// 
			this.labelScenario.AutoSize = true;
			this.labelScenario.Location = new System.Drawing.Point(223, 0);
			this.labelScenario.Name = "labelScenario";
			this.labelScenario.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
			this.labelScenario.Size = new System.Drawing.Size(94, 27);
			this.labelScenario.TabIndex = 3;
			this.labelScenario.Text = "xxScenarioColon";
			this.labelScenario.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkBoxUseDayOfMonth
			// 
			this.checkBoxUseDayOfMonth.BeforeTouchSize = new System.Drawing.Size(190, 24);
			this.tableLayoutPanel1.SetColumnSpan(this.checkBoxUseDayOfMonth, 3);
			this.checkBoxUseDayOfMonth.DrawFocusRectangle = false;
			this.checkBoxUseDayOfMonth.Location = new System.Drawing.Point(3, 73);
			this.checkBoxUseDayOfMonth.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxUseDayOfMonth.Name = "checkBoxUseDayOfMonth";
			this.checkBoxUseDayOfMonth.Padding = new System.Windows.Forms.Padding(7, 5, 0, 0);
			this.checkBoxUseDayOfMonth.Size = new System.Drawing.Size(190, 24);
			this.checkBoxUseDayOfMonth.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxUseDayOfMonth.TabIndex = 5;
			this.checkBoxUseDayOfMonth.Text = "xxUseDayOfMonthSeasonality";
			this.checkBoxUseDayOfMonth.ThemesEnabled = false;
			// 
			// SelectTargetDatesAndScenario
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "SelectTargetDatesAndScenario";
			this.Size = new System.Drawing.Size(546, 358);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxScenario)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUseDayOfMonth)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Reporting.ReportDateFromToSelector TargetFromTo;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxScenario;
        private System.Windows.Forms.Label labelScenario;
		private System.Windows.Forms.Label label1;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv  checkBoxUseDayOfMonth;

    }
}
