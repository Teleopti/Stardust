namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    partial class SelectExportType
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.rbtExportToBU = new System.Windows.Forms.RadioButton();
            this.rbtExportToFile = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 161F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.rbtExportToFile, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.rbtExportToBU, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(489, 338);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // rbtExportToBU
            // 
            this.rbtExportToBU.AutoSize = true;
            this.rbtExportToBU.Location = new System.Drawing.Point(23, 43);
            this.rbtExportToBU.Name = "rbtExportToBU";
            this.rbtExportToBU.Size = new System.Drawing.Size(93, 17);
            this.rbtExportToBU.TabIndex = 5;
            this.rbtExportToBU.TabStop = true;
            this.rbtExportToBU.Text = "xxExportToBU";
            this.rbtExportToBU.UseVisualStyleBackColor = true;
            // 
            // rbtExportToFile
            // 
            this.rbtExportToFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbtExportToFile.AutoSize = true;
            this.rbtExportToFile.Location = new System.Drawing.Point(23, 20);
            this.rbtExportToFile.Name = "rbtExportToFile";
            this.rbtExportToFile.Size = new System.Drawing.Size(94, 17);
            this.rbtExportToFile.TabIndex = 4;
            this.rbtExportToFile.TabStop = true;
            this.rbtExportToFile.Text = "xxExportToFile";
            this.rbtExportToFile.UseVisualStyleBackColor = true;
            // 
            // SelectExportType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SelectExportType";
            this.Size = new System.Drawing.Size(489, 338);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RadioButton rbtExportToFile;
        private System.Windows.Forms.RadioButton rbtExportToBU;

    }
}
