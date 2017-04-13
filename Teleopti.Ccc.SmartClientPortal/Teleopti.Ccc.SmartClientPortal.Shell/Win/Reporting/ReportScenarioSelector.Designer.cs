namespace Teleopti.Ccc.Win.Reporting
{
    partial class ReportScenarioSelector
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
            this.comboBoxAdvScenario = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.autoLabelScenario = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvScenario)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvScenario, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.autoLabelScenario, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(467, 32);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // comboBoxAdvScenario
            // 
            this.comboBoxAdvScenario.AllowNewText = false;
            this.comboBoxAdvScenario.BackColor = System.Drawing.Color.White;
            this.comboBoxAdvScenario.BeforeTouchSize = new System.Drawing.Size(226, 23);
            this.comboBoxAdvScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvScenario.Location = new System.Drawing.Point(236, 3);
            this.comboBoxAdvScenario.MaxLength = 150;
            this.comboBoxAdvScenario.Name = "comboBoxAdvScenario";
            this.comboBoxAdvScenario.Size = new System.Drawing.Size(226, 23);
            this.comboBoxAdvScenario.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
            this.comboBoxAdvScenario.TabIndex = 1;
            // 
            // autoLabelScenario
            // 
            this.autoLabelScenario.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelScenario.Location = new System.Drawing.Point(3, 8);
            this.autoLabelScenario.Name = "autoLabelScenario";
            this.autoLabelScenario.Size = new System.Drawing.Size(94, 15);
            this.autoLabelScenario.TabIndex = 2;
            this.autoLabelScenario.Text = "xxScenarioColon";
            this.autoLabelScenario.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ReportScenarioSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ReportScenarioSelector";
            this.Size = new System.Drawing.Size(467, 32);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvScenario)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvScenario;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelScenario;
    }
}
