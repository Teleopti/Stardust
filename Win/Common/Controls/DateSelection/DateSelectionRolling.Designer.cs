namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    partial class DateSelectionRolling : BaseUserControl
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
            this.buttonApply = new Syncfusion.Windows.Forms.ButtonAdv();
            this.comboBoxPeriodScaleUnit = new System.Windows.Forms.ComboBox();
            this.maskedTextBoxNumberOf = new System.Windows.Forms.MaskedTextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonApply.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.tableLayoutPanel1.SetColumnSpan(this.buttonApply, 2);
            this.buttonApply.Location = new System.Drawing.Point(10, 40);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(139, 19);
            this.buttonApply.TabIndex = 6;
            this.buttonApply.Text = "xxApply";
            this.buttonApply.UseVisualStyle = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // comboBoxPeriodScaleUnit
            // 
            this.comboBoxPeriodScaleUnit.FormattingEnabled = true;
            this.comboBoxPeriodScaleUnit.Location = new System.Drawing.Point(83, 3);
            this.comboBoxPeriodScaleUnit.Name = "comboBoxPeriodScaleUnit";
            this.comboBoxPeriodScaleUnit.Size = new System.Drawing.Size(74, 21);
            this.comboBoxPeriodScaleUnit.TabIndex = 5;
            // 
            // maskedTextBoxNumberOf
            // 
            this.maskedTextBoxNumberOf.Location = new System.Drawing.Point(3, 3);
            this.maskedTextBoxNumberOf.Mask = "0000";
            this.maskedTextBoxNumberOf.Name = "maskedTextBoxNumberOf";
            this.maskedTextBoxNumberOf.PromptChar = ' ';
            this.maskedTextBoxNumberOf.Size = new System.Drawing.Size(50, 20);
            this.maskedTextBoxNumberOf.TabIndex = 4;
            this.maskedTextBoxNumberOf.Text = "8";
            this.maskedTextBoxNumberOf.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.maskedTextBoxNumberOf.ValidatingType = typeof(int);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.comboBoxPeriodScaleUnit, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.maskedTextBoxNumberOf, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonApply, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(160, 62);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // DateSelectionRolling
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DateSelectionRolling";
            this.Size = new System.Drawing.Size(160, 62);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.ButtonAdv buttonApply;
        private System.Windows.Forms.ComboBox comboBoxPeriodScaleUnit;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxNumberOf;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}