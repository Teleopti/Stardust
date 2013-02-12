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
			this.comboBoxScenario = new System.Windows.Forms.ComboBox();
			this.reportDateFromToSelector1 = new Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector();
			this.labelScenario = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.comboBoxScenario, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.reportDateFromToSelector1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelScenario, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(468, 310);
			this.tableLayoutPanel1.TabIndex = 2;
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
			this.label1.Size = new System.Drawing.Size(448, 231);
			this.label1.TabIndex = 4;
			this.label1.Text = "xxSelect the dates and scenario where the forecast should be saved";
			// 
			// comboBoxScenario
			// 
			this.comboBoxScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxScenario.FormattingEnabled = true;
			this.comboBoxScenario.Location = new System.Drawing.Point(280, 8);
			this.comboBoxScenario.Margin = new System.Windows.Forms.Padding(4, 8, 5, 5);
			this.comboBoxScenario.Name = "comboBoxScenario";
			this.comboBoxScenario.Size = new System.Drawing.Size(143, 21);
			this.comboBoxScenario.TabIndex = 3;
			// 
			// reportDateFromToSelector1
			// 
			this.reportDateFromToSelector1.Location = new System.Drawing.Point(3, 3);
			this.reportDateFromToSelector1.Name = "reportDateFromToSelector1";
			this.reportDateFromToSelector1.NullString = "xxNoDateIsSelected";
			this.reportDateFromToSelector1.Size = new System.Drawing.Size(178, 53);
			this.reportDateFromToSelector1.TabIndex = 0;
			this.reportDateFromToSelector1.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("reportDateFromToSelector1.WorkPeriodEnd")));
			this.reportDateFromToSelector1.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("reportDateFromToSelector1.WorkPeriodStart")));
			// 
			// labelScenario
			// 
			this.labelScenario.AutoSize = true;
			this.labelScenario.Location = new System.Drawing.Point(187, 0);
			this.labelScenario.Name = "labelScenario";
			this.labelScenario.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.labelScenario.Size = new System.Drawing.Size(86, 23);
			this.labelScenario.TabIndex = 3;
			this.labelScenario.Text = "xxScenarioColon";
			this.labelScenario.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SelectTargetDatesAndScenario
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SelectTargetDatesAndScenario";
			this.Size = new System.Drawing.Size(468, 310);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Reporting.ReportDateFromToSelector reportDateFromToSelector1;
        private System.Windows.Forms.ComboBox comboBoxScenario;
        private System.Windows.Forms.Label labelScenario;
		private System.Windows.Forms.Label label1;

    }
}
