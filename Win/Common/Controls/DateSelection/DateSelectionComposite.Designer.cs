using DateRangeChangedEventArgs=Teleopti.Ccc.Win.Common.Controls.DateSelection.DateRangeChangedEventArgs;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    partial class DateSelectionComposite : BaseUserControl
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
			this.btnApply = new Syncfusion.Windows.Forms.ButtonAdv();
			this.periodListSelectionBox1 = new Teleopti.Ccc.Win.Common.Controls.PeriodListSelectionBox();
			this.dateSelectionControl1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionControl();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.btnApply, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.periodListSelectionBox1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.dateSelectionControl1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62.4183F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 37.5817F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(168, 359);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// btnApply
			// 
			this.btnApply.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnApply.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnApply.BeforeTouchSize = new System.Drawing.Size(141, 22);
			this.btnApply.ForeColor = System.Drawing.Color.White;
			this.btnApply.IsBackStageButton = false;
			this.btnApply.Location = new System.Drawing.Point(13, 333);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(141, 22);
			this.btnApply.TabIndex = 8;
			this.btnApply.Text = "xxApply";
			this.btnApply.UseVisualStyle = true;
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// periodListSelectionBox1
			// 
			this.periodListSelectionBox1.BackColor = System.Drawing.Color.Transparent;
			this.periodListSelectionBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.periodListSelectionBox1.Location = new System.Drawing.Point(3, 209);
			this.periodListSelectionBox1.Name = "periodListSelectionBox1";
			this.periodListSelectionBox1.Padding = new System.Windows.Forms.Padding(2);
			this.periodListSelectionBox1.Size = new System.Drawing.Size(162, 118);
			this.periodListSelectionBox1.TabIndex = 0;
			// 
			// dateSelectionControl1
			// 
			this.dateSelectionControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(195)))), ((int)(((byte)(211)))), ((int)(((byte)(232)))));
			this.dateSelectionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateSelectionControl1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateSelectionControl1.Location = new System.Drawing.Point(0, 0);
			this.dateSelectionControl1.Margin = new System.Windows.Forms.Padding(0);
			this.dateSelectionControl1.Name = "dateSelectionControl1";
			this.dateSelectionControl1.ShowAddButtons = true;
			this.dateSelectionControl1.ShowTabArea = true;
			this.dateSelectionControl1.Size = new System.Drawing.Size(168, 206);
			this.dateSelectionControl1.TabIndex = 9;
			//this.dateSelectionControl1.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
			//this.dateSelectionControl1.TabPanelBorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.dateSelectionControl1.DateRangeChanged += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.DateSelection.DateRangeChangedEventArgs>(this.dateSelectionControl1_DateRangeChanged);
			// 
			// DateSelectionComposite
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "DateSelectionComposite";
			this.Size = new System.Drawing.Size(168, 359);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private PeriodListSelectionBox periodListSelectionBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.ButtonAdv btnApply;
		private DateSelectionControl dateSelectionControl1;
    }
}