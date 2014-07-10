namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class ScenarioSelector
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
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvScenario, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.autoLabelScenario, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 44.23077F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55.76923F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(150, 52);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// comboBoxAdvScenario
			// 
			this.comboBoxAdvScenario.AllowNewText = false;
			this.comboBoxAdvScenario.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvScenario.BeforeTouchSize = new System.Drawing.Size(144, 21);
			this.comboBoxAdvScenario.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdvScenario.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvScenario.Location = new System.Drawing.Point(3, 26);
			this.comboBoxAdvScenario.MaxLength = 150;
			this.comboBoxAdvScenario.Name = "comboBoxAdvScenario";
			this.comboBoxAdvScenario.Size = new System.Drawing.Size(144, 21);
			this.comboBoxAdvScenario.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvScenario.TabIndex = 0;
			// 
			// autoLabelScenario
			// 
			this.autoLabelScenario.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabelScenario.Location = new System.Drawing.Point(3, 5);
			this.autoLabelScenario.Name = "autoLabelScenario";
			this.autoLabelScenario.Size = new System.Drawing.Size(86, 13);
			this.autoLabelScenario.TabIndex = 1;
			this.autoLabelScenario.Text = "xxScenarioColon";
			this.autoLabelScenario.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ScenarioSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "ScenarioSelector";
			this.Size = new System.Drawing.Size(150, 52);
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
