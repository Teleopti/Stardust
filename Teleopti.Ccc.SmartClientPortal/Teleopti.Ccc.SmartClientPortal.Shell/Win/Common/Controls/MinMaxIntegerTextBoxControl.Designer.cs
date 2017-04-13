namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    partial class MinMaxIntegerTextBoxControl
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
            this.tableLayoutPanelOpenForShiftTrade = new System.Windows.Forms.TableLayoutPanel();
            this.labelFrom = new System.Windows.Forms.Label();
            this.integerTextBoxMinDays = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            this.integerTextBoxMaxDays = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            this.labelMinDays = new System.Windows.Forms.Label();
            this.labelMaxDays = new System.Windows.Forms.Label();
            this.labelTo = new System.Windows.Forms.Label();
            this.tableLayoutPanelOpenForShiftTrade.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMinDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMaxDays)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanelOpenForShiftTrade
            // 
            this.tableLayoutPanelOpenForShiftTrade.ColumnCount = 6;
            this.tableLayoutPanelOpenForShiftTrade.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanelOpenForShiftTrade.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanelOpenForShiftTrade.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanelOpenForShiftTrade.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanelOpenForShiftTrade.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanelOpenForShiftTrade.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 541F));
            this.tableLayoutPanelOpenForShiftTrade.Controls.Add(this.labelFrom, 0, 0);
            this.tableLayoutPanelOpenForShiftTrade.Controls.Add(this.integerTextBoxMinDays, 1, 0);
            this.tableLayoutPanelOpenForShiftTrade.Controls.Add(this.integerTextBoxMaxDays, 4, 0);
            this.tableLayoutPanelOpenForShiftTrade.Controls.Add(this.labelMinDays, 2, 0);
            this.tableLayoutPanelOpenForShiftTrade.Controls.Add(this.labelMaxDays, 5, 0);
            this.tableLayoutPanelOpenForShiftTrade.Controls.Add(this.labelTo, 3, 0);
            this.tableLayoutPanelOpenForShiftTrade.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelOpenForShiftTrade.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelOpenForShiftTrade.Name = "tableLayoutPanelOpenForShiftTrade";
            this.tableLayoutPanelOpenForShiftTrade.RowCount = 1;
            this.tableLayoutPanelOpenForShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelOpenForShiftTrade.Size = new System.Drawing.Size(481, 29);
            this.tableLayoutPanelOpenForShiftTrade.TabIndex = 2;
            // 
            // labelFrom
            // 
            this.labelFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelFrom.AutoSize = true;
            this.labelFrom.Location = new System.Drawing.Point(3, 8);
            this.labelFrom.Name = "labelFrom";
            this.labelFrom.Size = new System.Drawing.Size(40, 13);
            this.labelFrom.TabIndex = 0;
            this.labelFrom.Text = "xxFrom";
            // 
            // integerTextBoxMinDays
            // 
            this.integerTextBoxMinDays.IntegerValue = ((long)(1));
            this.integerTextBoxMinDays.Location = new System.Drawing.Point(73, 3);
            this.integerTextBoxMinDays.MaxLength = 4;
            this.integerTextBoxMinDays.MaxValue = ((long)(9999));
            this.integerTextBoxMinDays.MinValue = ((long)(0));
            this.integerTextBoxMinDays.Name = "integerTextBoxMinDays";
            this.integerTextBoxMinDays.NullString = "0";
            this.integerTextBoxMinDays.Size = new System.Drawing.Size(64, 20);
            this.integerTextBoxMinDays.TabIndex = 3;
            this.integerTextBoxMinDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // integerTextBoxMaxDays
            // 
            this.integerTextBoxMaxDays.IntegerValue = ((long)(1));
            this.integerTextBoxMaxDays.Location = new System.Drawing.Point(313, 3);
            this.integerTextBoxMaxDays.MaxLength = 4;
            this.integerTextBoxMaxDays.MaxValue = ((long)(9999));
            this.integerTextBoxMaxDays.MinValue = ((long)(0));
            this.integerTextBoxMaxDays.Name = "integerTextBoxMaxDays";
            this.integerTextBoxMaxDays.NullString = "0";
            this.integerTextBoxMaxDays.Size = new System.Drawing.Size(64, 20);
            this.integerTextBoxMaxDays.TabIndex = 4;
            this.integerTextBoxMaxDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelMinDays
            // 
            this.labelMinDays.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMinDays.AutoSize = true;
            this.labelMinDays.Location = new System.Drawing.Point(143, 8);
            this.labelMinDays.Name = "labelMinDays";
            this.labelMinDays.Size = new System.Drawing.Size(41, 13);
            this.labelMinDays.TabIndex = 5;
            this.labelMinDays.Text = "xxDays";
            // 
            // labelMaxDays
            // 
            this.labelMaxDays.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMaxDays.AutoSize = true;
            this.labelMaxDays.Location = new System.Drawing.Point(383, 8);
            this.labelMaxDays.Name = "labelMaxDays";
            this.labelMaxDays.Size = new System.Drawing.Size(41, 13);
            this.labelMaxDays.TabIndex = 6;
            this.labelMaxDays.Text = "xxDays";
            // 
            // labelTo
            // 
            this.labelTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(243, 8);
            this.labelTo.Name = "labelTo";
            this.labelTo.Size = new System.Drawing.Size(30, 13);
            this.labelTo.TabIndex = 7;
            this.labelTo.Text = "xxTo";
            // 
            // MinMaxIntegerTextBoxControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelOpenForShiftTrade);
            this.Name = "MinMaxIntegerTextBoxControl";
            this.Size = new System.Drawing.Size(481, 29);
            this.tableLayoutPanelOpenForShiftTrade.ResumeLayout(false);
            this.tableLayoutPanelOpenForShiftTrade.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMinDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMaxDays)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelOpenForShiftTrade;
        private System.Windows.Forms.Label labelFrom;
        private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBoxMinDays;
        private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBoxMaxDays;
        private System.Windows.Forms.Label labelMinDays;
        private System.Windows.Forms.Label labelMaxDays;
        private System.Windows.Forms.Label labelTo;
    }
}
