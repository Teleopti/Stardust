namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
{
    partial class DailyRecurrenceView
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
            this.autoLabelEvery = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.integerTextBoxIncrementCount = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            this.autoLabelOfEvery = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxIncrementCount)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.24138F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.58886F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68.43501F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.autoLabelEvery, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.integerTextBoxIncrementCount, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.autoLabelOfEvery, 3, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(400, 90);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // autoLabelEvery
            // 
            this.autoLabelEvery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.autoLabelEvery.DX = 0;
            this.autoLabelEvery.DY = 0;
            this.autoLabelEvery.Location = new System.Drawing.Point(26, 16);
            this.autoLabelEvery.Name = "autoLabelEvery";
            this.autoLabelEvery.Size = new System.Drawing.Size(58, 13);
            this.autoLabelEvery.TabIndex = 14;
            this.autoLabelEvery.Text = "xxEvery";
            // 
            // integerTextBoxIncrementCount
            // 
            this.integerTextBoxIncrementCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.integerTextBoxIncrementCount.IntegerValue = ((long)(1));
            this.integerTextBoxIncrementCount.Location = new System.Drawing.Point(90, 12);
            this.integerTextBoxIncrementCount.MaxLength = 2;
            this.integerTextBoxIncrementCount.MaxValue = ((long)(31));
            this.integerTextBoxIncrementCount.MinValue = ((long)(1));
            this.integerTextBoxIncrementCount.Name = "integerTextBoxIncrementCount";
            this.integerTextBoxIncrementCount.NegativeInputPendingOnSelectAll = false;
            this.integerTextBoxIncrementCount.NullString = "0";
            this.integerTextBoxIncrementCount.OverflowIndicatorToolTipText = null;
            this.integerTextBoxIncrementCount.Size = new System.Drawing.Size(48, 20);
            this.integerTextBoxIncrementCount.TabIndex = 16;
            this.integerTextBoxIncrementCount.IntegerValueChanged += new System.EventHandler(integerTextBoxIncrementCount_IntegerValueChanged);
            // 
            // autoLabelOfEvery
            // 
            this.autoLabelOfEvery.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelOfEvery.DX = 0;
            this.autoLabelOfEvery.DY = 0;
            this.autoLabelOfEvery.Location = new System.Drawing.Point(144, 16);
            this.autoLabelOfEvery.Name = "autoLabelOfEvery";
            this.autoLabelOfEvery.Size = new System.Drawing.Size(98, 13);
            this.autoLabelOfEvery.TabIndex = 10;
            this.autoLabelOfEvery.Text = "xxDayParenthesisS";
            // 
            // DailyRecurrenceView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DailyRecurrenceView";
            this.Size = new System.Drawing.Size(400, 90);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxIncrementCount)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelEvery;
        private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBoxIncrementCount;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelOfEvery;

    }
}
