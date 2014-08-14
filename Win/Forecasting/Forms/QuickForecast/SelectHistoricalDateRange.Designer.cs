namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    partial class SelectHistoricalDateRange
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectHistoricalDateRange));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.HistoricalFromTo = new Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 257F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 167F));
			this.tableLayoutPanel1.Controls.Add(this.HistoricalFromTo, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 81F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(468, 323);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// HistoricalFromTo
			// 
			this.HistoricalFromTo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HistoricalFromTo.EnableNullDates = true;
			this.HistoricalFromTo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HistoricalFromTo.Location = new System.Drawing.Point(3, 7);
			this.HistoricalFromTo.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
			this.HistoricalFromTo.Name = "HistoricalFromTo";
			this.HistoricalFromTo.NullString = "xxNoDateIsSelected";
			this.HistoricalFromTo.Size = new System.Drawing.Size(251, 71);
			this.HistoricalFromTo.TabIndex = 0;
			this.HistoricalFromTo.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("HistoricalFromTo.WorkPeriodEnd")));
			this.HistoricalFromTo.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("HistoricalFromTo.WorkPeriodStart")));
			// 
			// SelectHistoricalDateRange
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SelectHistoricalDateRange";
			this.Size = new System.Drawing.Size(468, 323);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Reporting.ReportDateFromToSelector HistoricalFromTo;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

    }
}
