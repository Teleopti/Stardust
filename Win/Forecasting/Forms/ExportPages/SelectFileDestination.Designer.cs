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
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.radioButtonImportWLAndStaffing = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.txtFileName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.btnChoose = new Syncfusion.Windows.Forms.ButtonAdv();
			this.radioButtonImportWorkload = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonImportStaffing = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportWLAndStaffing)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtFileName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportWorkload)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportStaffing)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 366F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.radioButtonImportWLAndStaffing, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.txtFileName, 0, 1);
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
			// radioButtonImportWLAndStaffing
			// 
			this.radioButtonImportWLAndStaffing.BeforeTouchSize = new System.Drawing.Size(166, 17);
			this.radioButtonImportWLAndStaffing.DrawFocusRectangle = false;
			this.radioButtonImportWLAndStaffing.Location = new System.Drawing.Point(3, 93);
			this.radioButtonImportWLAndStaffing.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonImportWLAndStaffing.Name = "radioButtonImportWLAndStaffing";
			this.radioButtonImportWLAndStaffing.Size = new System.Drawing.Size(166, 17);
			this.radioButtonImportWLAndStaffing.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonImportWLAndStaffing.TabIndex = 5;
			this.radioButtonImportWLAndStaffing.TabStop = false;
			this.radioButtonImportWLAndStaffing.Text = "xxExportWorkloadAndStaffing";
			this.radioButtonImportWLAndStaffing.ThemesEnabled = false;
			// 
			// txtFileName
			// 
			this.txtFileName.BackColor = System.Drawing.Color.White;
			this.txtFileName.BeforeTouchSize = new System.Drawing.Size(360, 22);
			this.txtFileName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.txtFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtFileName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.txtFileName.Location = new System.Drawing.Point(3, 43);
			this.txtFileName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.txtFileName.Name = "txtFileName";
			this.txtFileName.Size = new System.Drawing.Size(360, 22);
			this.txtFileName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.txtFileName.TabIndex = 0;
			// 
			// btnChoose
			// 
			this.btnChoose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnChoose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnChoose.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.btnChoose.ForeColor = System.Drawing.Color.White;
			this.btnChoose.IsBackStageButton = false;
			this.btnChoose.Location = new System.Drawing.Point(369, 43);
			this.btnChoose.Name = "btnChoose";
			this.btnChoose.Size = new System.Drawing.Size(75, 23);
			this.btnChoose.TabIndex = 1;
			this.btnChoose.Text = "xxSelectFileDestination";
			this.btnChoose.UseVisualStyle = true;
			this.btnChoose.UseVisualStyleBackColor = false;
			this.btnChoose.Click += new System.EventHandler(this.btnChoose_Click);
			// 
			// radioButtonImportWorkload
			// 
			this.radioButtonImportWorkload.BeforeTouchSize = new System.Drawing.Size(111, 17);
			this.radioButtonImportWorkload.Checked = true;
			this.radioButtonImportWorkload.DrawFocusRectangle = false;
			this.radioButtonImportWorkload.Location = new System.Drawing.Point(3, 139);
			this.radioButtonImportWorkload.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonImportWorkload.Name = "radioButtonImportWorkload";
			this.radioButtonImportWorkload.Size = new System.Drawing.Size(111, 17);
			this.radioButtonImportWorkload.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonImportWorkload.TabIndex = 3;
			this.radioButtonImportWorkload.Text = "xxExportWorkload";
			this.radioButtonImportWorkload.ThemesEnabled = false;
			// 
			// radioButtonImportStaffing
			// 
			this.radioButtonImportStaffing.BeforeTouchSize = new System.Drawing.Size(101, 17);
			this.radioButtonImportStaffing.DrawFocusRectangle = false;
			this.radioButtonImportStaffing.Location = new System.Drawing.Point(3, 116);
			this.radioButtonImportStaffing.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonImportStaffing.Name = "radioButtonImportStaffing";
			this.radioButtonImportStaffing.Size = new System.Drawing.Size(101, 17);
			this.radioButtonImportStaffing.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonImportStaffing.TabIndex = 4;
			this.radioButtonImportStaffing.Text = "xxExportStaffing";
			this.radioButtonImportStaffing.ThemesEnabled = false;
			// 
			// SelectFileDestination
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SelectFileDestination";
			this.Size = new System.Drawing.Size(489, 338);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportWLAndStaffing)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtFileName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportWorkload)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportStaffing)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		  private Syncfusion.Windows.Forms.Tools.TextBoxExt  txtFileName;
		  private Syncfusion.Windows.Forms.ButtonAdv  btnChoose;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonImportWLAndStaffing;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonImportWorkload;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonImportStaffing;

    }
}
