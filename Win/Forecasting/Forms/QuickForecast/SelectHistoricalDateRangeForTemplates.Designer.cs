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
			this.tableLayoutPanel1.Size = new System.Drawing.Size(489, 338);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// comboBoxSmoothing
			// 
			this.comboBoxSmoothing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSmoothing.FormattingEnabled = true;
			this.comboBoxSmoothing.Location = new System.Drawing.Point(284, 8);
			this.comboBoxSmoothing.Margin = new System.Windows.Forms.Padding(4, 8, 5, 5);
			this.comboBoxSmoothing.Name = "comboBoxSmoothing";
			this.comboBoxSmoothing.Size = new System.Drawing.Size(143, 21);
			this.comboBoxSmoothing.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(187, 0);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.label2.Size = new System.Drawing.Size(90, 23);
			this.label2.TabIndex = 5;
			this.label2.Text = "xxSmoothingStyle";
			// 
			// TemplatesDatesFromTo
			// 
			this.TemplatesDatesFromTo.Location = new System.Drawing.Point(3, 3);
			this.TemplatesDatesFromTo.Name = "TemplatesDatesFromTo";
			this.TemplatesDatesFromTo.NullString = "xxNoDateIsSelected";
			this.TemplatesDatesFromTo.Size = new System.Drawing.Size(178, 53);
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
			this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ImageAlign = System.Drawing.ContentAlignment.TopRight;
			this.label1.Location = new System.Drawing.Point(10, 69);
			this.label1.Margin = new System.Windows.Forms.Padding(10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(469, 259);
			this.label1.TabIndex = 1;
			this.label1.Text = "xxSelect historical data range for templates.";
			// 
			// SelectHistoricalDateRangeForTemplates
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SelectHistoricalDateRangeForTemplates";
			this.Size = new System.Drawing.Size(489, 338);
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
