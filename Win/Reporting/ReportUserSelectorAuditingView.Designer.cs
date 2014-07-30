namespace Teleopti.Ccc.Win.Reporting
{
    partial class ReportUserSelectorAuditingView
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxUserColon"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxAdvUser = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.autoLabelUser = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvUser)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvUser, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.autoLabelUser, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(467, 32);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // comboBoxAdvUser
            // 
            this.comboBoxAdvUser.AllowNewText = false;
            this.comboBoxAdvUser.BackColor = System.Drawing.Color.White;
            this.comboBoxAdvUser.BeforeTouchSize = new System.Drawing.Size(226, 23);
            this.comboBoxAdvUser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvUser.Location = new System.Drawing.Point(236, 3);
            this.comboBoxAdvUser.MaxLength = 150;
            this.comboBoxAdvUser.Name = "comboBoxAdvUser";
            this.comboBoxAdvUser.Size = new System.Drawing.Size(226, 23);
            this.comboBoxAdvUser.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
            this.comboBoxAdvUser.TabIndex = 1;
            // 
            // autoLabelUser
            // 
            this.autoLabelUser.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelUser.Location = new System.Drawing.Point(3, 8);
            this.autoLabelUser.Name = "autoLabelUser";
            this.autoLabelUser.Size = new System.Drawing.Size(72, 15);
            this.autoLabelUser.TabIndex = 2;
            this.autoLabelUser.Text = "xxUserColon";
            this.autoLabelUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ReportUserSelectorAuditingView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ReportUserSelectorAuditingView";
            this.Size = new System.Drawing.Size(467, 32);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvUser)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvUser;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelUser;
    }
}
