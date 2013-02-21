namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
	partial class SelectHistoricalDateRangeForTemplates
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectHistoricalDateRangeForTemplates));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxSmoothing = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.TemplatesDatesFromTo = new Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector();
			this.label1 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.comboBoxSmoothing, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.TemplatesDatesFromTo, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(570, 390);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// comboBoxSmoothing
			// 
			this.comboBoxSmoothing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSmoothing.FormattingEnabled = true;
			this.comboBoxSmoothing.Location = new System.Drawing.Point(345, 9);
			this.comboBoxSmoothing.Margin = new System.Windows.Forms.Padding(5, 9, 6, 6);
			this.comboBoxSmoothing.Name = "comboBoxSmoothing";
			this.comboBoxSmoothing.Size = new System.Drawing.Size(166, 23);
			this.comboBoxSmoothing.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(204, 0);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
			this.label2.Size = new System.Drawing.Size(133, 27);
			this.label2.TabIndex = 5;
			this.label2.Text = "xxSmoothingStyleColon";
			// 
			// TemplatesDatesFromTo
			// 
			this.TemplatesDatesFromTo.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.TemplatesDatesFromTo.Location = new System.Drawing.Point(3, 6);
			this.TemplatesDatesFromTo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.TemplatesDatesFromTo.Name = "TemplatesDatesFromTo";
			this.TemplatesDatesFromTo.NullString = "xxNoDateIsSelected";
			this.TemplatesDatesFromTo.Size = new System.Drawing.Size(195, 61);
			this.TemplatesDatesFromTo.TabIndex = 0;
			this.TemplatesDatesFromTo.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("TemplatesDatesFromTo.WorkPeriodEnd")));
			this.TemplatesDatesFromTo.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("TemplatesDatesFromTo.WorkPeriodStart")));
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.SetColumnSpan(this.label1, 3);
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.label1.ImageAlign = System.Drawing.ContentAlignment.TopRight;
			this.label1.Location = new System.Drawing.Point(12, 82);
			this.label1.Margin = new System.Windows.Forms.Padding(12, 12, 12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(546, 296);
			this.label1.TabIndex = 1;
			this.label1.Text = "xxSelect historical data range for templates.";
			// 
			// SelectHistoricalDateRangeForTemplates
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "SelectHistoricalDateRangeForTemplates";
			this.Size = new System.Drawing.Size(570, 390);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Reporting.ReportDateFromToSelector TemplatesDatesFromTo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBoxSmoothing;

    }
}
