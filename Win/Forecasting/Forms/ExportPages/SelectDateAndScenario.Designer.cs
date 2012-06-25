namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    partial class SelectDateAndScenario
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectDateAndScenario));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.reportDateFromToSelector1 = new Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxScenario = new System.Windows.Forms.ComboBox();
            this.labelScenario = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxAdvLeaderMode = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.checkBoxAdvShrinkage = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.checkBoxAdvCalculation = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.checkBoxAdvValidation = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvLeaderMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShrinkage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvCalculation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvValidation)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.reportDateFromToSelector1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 113F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(468, 310);
            this.tableLayoutPanel1.TabIndex = 2;
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
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.03279F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.96721F));
            this.tableLayoutPanel2.Controls.Add(this.comboBoxScenario, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelScenario, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(187, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(244, 191);
            this.tableLayoutPanel2.TabIndex = 5;
            // 
            // comboBoxScenario
            // 
            this.comboBoxScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxScenario.FormattingEnabled = true;
            this.comboBoxScenario.Location = new System.Drawing.Point(96, 5);
            this.comboBoxScenario.Margin = new System.Windows.Forms.Padding(4, 5, 5, 5);
            this.comboBoxScenario.Name = "comboBoxScenario";
            this.comboBoxScenario.Size = new System.Drawing.Size(143, 21);
            this.comboBoxScenario.TabIndex = 3;
            // 
            // labelScenario
            // 
            this.labelScenario.Location = new System.Drawing.Point(3, 0);
            this.labelScenario.Name = "labelScenario";
            this.labelScenario.Size = new System.Drawing.Size(86, 26);
            this.labelScenario.TabIndex = 3;
            this.labelScenario.Text = "xxScenarioColon";
            this.labelScenario.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(3, 41);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(238, 147);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "xxToggle";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.checkBoxAdvLeaderMode, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxAdvShrinkage, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxAdvCalculation, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxAdvValidation, 0, 3);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(232, 128);
            this.tableLayoutPanel3.TabIndex = 4;
            // 
            // checkBoxAdvLeaderMode
            // 
            this.checkBoxAdvLeaderMode.Location = new System.Drawing.Point(3, 3);
            this.checkBoxAdvLeaderMode.Name = "checkBoxAdvLeaderMode";
            this.checkBoxAdvLeaderMode.Size = new System.Drawing.Size(226, 20);
            this.checkBoxAdvLeaderMode.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.checkBoxAdvLeaderMode.TabIndex = 3;
            this.checkBoxAdvLeaderMode.Text = "xxForecasts";
            this.checkBoxAdvLeaderMode.ThemesEnabled = false;
            // 
            // checkBoxAdvShrinkage
            // 
            this.checkBoxAdvShrinkage.Location = new System.Drawing.Point(15, 29);
            this.checkBoxAdvShrinkage.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.checkBoxAdvShrinkage.Name = "checkBoxAdvShrinkage";
            this.checkBoxAdvShrinkage.Size = new System.Drawing.Size(214, 20);
            this.checkBoxAdvShrinkage.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.checkBoxAdvShrinkage.TabIndex = 0;
            this.checkBoxAdvShrinkage.Text = "xxShrinkage";
            this.checkBoxAdvShrinkage.ThemesEnabled = false;
            // 
            // checkBoxAdvCalculation
            // 
            this.checkBoxAdvCalculation.Location = new System.Drawing.Point(15, 55);
            this.checkBoxAdvCalculation.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.checkBoxAdvCalculation.Name = "checkBoxAdvCalculation";
            this.checkBoxAdvCalculation.Size = new System.Drawing.Size(214, 20);
            this.checkBoxAdvCalculation.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.checkBoxAdvCalculation.TabIndex = 1;
            this.checkBoxAdvCalculation.Text = "xxCalculations";
            this.checkBoxAdvCalculation.ThemesEnabled = false;
            // 
            // checkBoxAdvValidation
            // 
            this.checkBoxAdvValidation.Location = new System.Drawing.Point(3, 81);
            this.checkBoxAdvValidation.Name = "checkBoxAdvValidation";
            this.checkBoxAdvValidation.Size = new System.Drawing.Size(226, 20);
            this.checkBoxAdvValidation.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.checkBoxAdvValidation.TabIndex = 2;
            this.checkBoxAdvValidation.Text = "xxValidations";
            this.checkBoxAdvValidation.ThemesEnabled = false;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.SetColumnSpan(this.label1, 2);
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.label1.Location = new System.Drawing.Point(10, 207);
            this.label1.Margin = new System.Windows.Forms.Padding(10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(414, 93);
            this.label1.TabIndex = 6;
            this.label1.Text = "xxTheExportWillStartInBackgroundMessage";
            // 
            // SelectDateAndScenario
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SelectDateAndScenario";
            this.Size = new System.Drawing.Size(468, 310);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvLeaderMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShrinkage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvCalculation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvValidation)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Reporting.ReportDateFromToSelector reportDateFromToSelector1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ComboBox comboBoxScenario;
        private System.Windows.Forms.Label labelScenario;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvLeaderMode;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvShrinkage;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvCalculation;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvValidation;

    }
}
