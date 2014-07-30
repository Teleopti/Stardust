namespace Teleopti.Ccc.Win.Reporting
{
    partial class ReportTimeZoneSelector
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
            this.comboBoxAdvTimeZone = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.autoLabelTimeZone = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTimeZone)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvTimeZone, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.autoLabelTimeZone, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(467, 31);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // comboBoxAdvTimeZone
            // 
            this.comboBoxAdvTimeZone.AllowNewText = false;
            this.comboBoxAdvTimeZone.BackColor = System.Drawing.Color.White;
            this.comboBoxAdvTimeZone.BeforeTouchSize = new System.Drawing.Size(226, 23);
            this.comboBoxAdvTimeZone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvTimeZone.Location = new System.Drawing.Point(236, 3);
            this.comboBoxAdvTimeZone.MaxLength = 150;
            this.comboBoxAdvTimeZone.Name = "comboBoxAdvTimeZone";
            this.comboBoxAdvTimeZone.Size = new System.Drawing.Size(226, 23);
            this.comboBoxAdvTimeZone.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
            this.comboBoxAdvTimeZone.TabIndex = 1;
            // 
            // autoLabelTimeZone
            // 
            this.autoLabelTimeZone.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelTimeZone.Location = new System.Drawing.Point(3, 8);
            this.autoLabelTimeZone.Name = "autoLabelTimeZone";
            this.autoLabelTimeZone.Size = new System.Drawing.Size(103, 15);
            this.autoLabelTimeZone.TabIndex = 2;
            this.autoLabelTimeZone.Text = "xxTimeZoneColon";
            this.autoLabelTimeZone.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ReportTimeZoneSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ReportTimeZoneSelector";
            this.Size = new System.Drawing.Size(467, 31);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTimeZone)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvTimeZone;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelTimeZone;
    }
}
