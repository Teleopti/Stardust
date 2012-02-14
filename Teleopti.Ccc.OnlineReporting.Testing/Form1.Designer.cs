namespace Teleopti.Ccc.OnlineReporting.Testing
{
    partial class Form1
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonShow = new System.Windows.Forms.Button();
            this.comboBoxReports = new System.Windows.Forms.ComboBox();
            this.reportViewerControl1 = new Teleopti.Ccc.OnlineReporting.ReportViewerControl();
            this.comboBoxCulture = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 330F));
            this.tableLayoutPanel1.Controls.Add(this.comboBoxReports, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.reportViewerControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonShow, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxCulture, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.032128F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.96787F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(951, 525);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // buttonShow
            // 
            this.buttonShow.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonShow.Location = new System.Drawing.Point(694, 7);
            this.buttonShow.Margin = new System.Windows.Forms.Padding(3, 3, 30, 3);
            this.buttonShow.Name = "buttonShow";
            this.buttonShow.Size = new System.Drawing.Size(227, 28);
            this.buttonShow.TabIndex = 0;
            this.buttonShow.Text = "Visa rapporten";
            this.buttonShow.UseVisualStyleBackColor = true;
            this.buttonShow.Click += new System.EventHandler(this.buttonShow_Click);
            // 
            // comboBoxReports
            // 
            this.comboBoxReports.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBoxReports.DisplayMember = "Name";
            this.comboBoxReports.FormattingEnabled = true;
            this.comboBoxReports.Location = new System.Drawing.Point(30, 10);
            this.comboBoxReports.Margin = new System.Windows.Forms.Padding(30, 3, 3, 3);
            this.comboBoxReports.Name = "comboBoxReports";
            this.comboBoxReports.Size = new System.Drawing.Size(237, 21);
            this.comboBoxReports.TabIndex = 1;
            // 
            // reportViewerControl1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.reportViewerControl1, 3);
            this.reportViewerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportViewerControl1.Location = new System.Drawing.Point(3, 45);
            this.reportViewerControl1.Name = "reportViewerControl1";
            this.reportViewerControl1.Size = new System.Drawing.Size(945, 477);
            this.reportViewerControl1.TabIndex = 2;
            // 
            // comboBoxCulture
            // 
            this.comboBoxCulture.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBoxCulture.DisplayMember = "DisplayName";
            this.comboBoxCulture.FormattingEnabled = true;
            this.comboBoxCulture.Location = new System.Drawing.Point(340, 10);
            this.comboBoxCulture.Margin = new System.Windows.Forms.Padding(30, 3, 3, 3);
            this.comboBoxCulture.Name = "comboBoxCulture";
            this.comboBoxCulture.Size = new System.Drawing.Size(237, 21);
            this.comboBoxCulture.Sorted = true;
            this.comboBoxCulture.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 525);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonShow;
        private System.Windows.Forms.ComboBox comboBoxReports;
        private ReportViewerControl reportViewerControl1;
        private System.Windows.Forms.ComboBox comboBoxCulture;
    }
}

