namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    partial class SelectFileDestination
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnChoose = new System.Windows.Forms.Button();
            this.radioButtonImportStaffing = new System.Windows.Forms.RadioButton();
            this.radioButtonImportWLAndStaffing = new System.Windows.Forms.RadioButton();
            this.radioButtonImportWorkload = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 366F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.radioButtonImportWLAndStaffing, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnChoose, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.radioButtonImportWorkload, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.radioButtonImportStaffing, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 147F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(489, 338);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 43);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(360, 20);
            this.textBox1.TabIndex = 0;
            // 
            // btnChoose
            // 
            this.btnChoose.Location = new System.Drawing.Point(369, 43);
            this.btnChoose.Name = "btnChoose";
            this.btnChoose.Size = new System.Drawing.Size(75, 23);
            this.btnChoose.TabIndex = 1;
            this.btnChoose.Text = "xxSelectFileDestination";
            this.btnChoose.UseVisualStyleBackColor = true;
            this.btnChoose.Click += new System.EventHandler(this.btnChoose_Click);
            // 
            // radioButtonImportStaffing
            // 
            this.radioButtonImportStaffing.AutoSize = true;
            this.radioButtonImportStaffing.Location = new System.Drawing.Point(3, 116);
            this.radioButtonImportStaffing.Name = "radioButtonImportStaffing";
            this.radioButtonImportStaffing.Size = new System.Drawing.Size(100, 17);
            this.radioButtonImportStaffing.TabIndex = 4;
            this.radioButtonImportStaffing.Text = "xxExportStaffing";
            this.radioButtonImportStaffing.UseVisualStyleBackColor = true;
            // 
            // radioButtonImportWLAndStaffing
            // 
            this.radioButtonImportWLAndStaffing.AutoSize = true;
            this.radioButtonImportWLAndStaffing.Location = new System.Drawing.Point(3, 93);
            this.radioButtonImportWLAndStaffing.Name = "radioButtonImportWLAndStaffing";
            this.radioButtonImportWLAndStaffing.Size = new System.Drawing.Size(165, 17);
            this.radioButtonImportWLAndStaffing.TabIndex = 5;
            this.radioButtonImportWLAndStaffing.Text = "xxExportWorkloadAndStaffing";
            this.radioButtonImportWLAndStaffing.UseVisualStyleBackColor = true;
            // 
            // radioButtonImportWorkload
            // 
            this.radioButtonImportWorkload.AutoSize = true;
            this.radioButtonImportWorkload.Checked = true;
            this.radioButtonImportWorkload.Location = new System.Drawing.Point(3, 139);
            this.radioButtonImportWorkload.Name = "radioButtonImportWorkload";
            this.radioButtonImportWorkload.Size = new System.Drawing.Size(110, 17);
            this.radioButtonImportWorkload.TabIndex = 3;
            this.radioButtonImportWorkload.TabStop = true;
            this.radioButtonImportWorkload.Text = "xxExportWorkload";
            this.radioButtonImportWorkload.UseVisualStyleBackColor = true;
            // 
            // SelectFileDestination
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SelectFileDestination";
            this.Size = new System.Drawing.Size(489, 338);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnChoose;
        private System.Windows.Forms.RadioButton radioButtonImportWLAndStaffing;
        private System.Windows.Forms.RadioButton radioButtonImportWorkload;
        private System.Windows.Forms.RadioButton radioButtonImportStaffing;

    }
}
