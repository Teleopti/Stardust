namespace Teleopti.Ccc.Win.Scheduling
{
    partial class OptimizeActivities
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu")]
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptimizeActivities));
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
			this.checkBoxShiftCategory = new System.Windows.Forms.CheckBox();
			this.checkBoxStartTime = new System.Windows.Forms.CheckBox();
			this.checkBoxEndTime = new System.Windows.Forms.CheckBox();
			this.checkBoxBetween = new System.Windows.Forms.CheckBox();
			this.twoListSelectorActivities = new Teleopti.Ccc.Win.Common.Controls.TwoListSelector();
			this.fromToTimePicker1 = new Teleopti.Ccc.Win.Common.Controls.FromToTimePicker();
			this.buttonOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.ShowContextMenu = false;
			this.ribbonControlAdv1.ShowMinimizeButton = false;
			this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(470, 33);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdv1.TabIndex = 0;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			// 
			// checkBoxShiftCategory
			// 
			this.checkBoxShiftCategory.AutoSize = true;
			this.checkBoxShiftCategory.Location = new System.Drawing.Point(6, 23);
			this.checkBoxShiftCategory.Name = "checkBoxShiftCategory";
			this.checkBoxShiftCategory.Size = new System.Drawing.Size(99, 17);
			this.checkBoxShiftCategory.TabIndex = 2;
			this.checkBoxShiftCategory.Text = "xxShiftCategory";
			this.checkBoxShiftCategory.UseVisualStyleBackColor = true;
			this.checkBoxShiftCategory.CheckedChanged += new System.EventHandler(this.checkBoxShiftCategory_CheckedChanged);
			// 
			// checkBoxStartTime
			// 
			this.checkBoxStartTime.AutoSize = true;
			this.checkBoxStartTime.Location = new System.Drawing.Point(6, 46);
			this.checkBoxStartTime.Name = "checkBoxStartTime";
			this.checkBoxStartTime.Size = new System.Drawing.Size(81, 17);
			this.checkBoxStartTime.TabIndex = 3;
			this.checkBoxStartTime.Text = "xxStartTime";
			this.checkBoxStartTime.UseVisualStyleBackColor = true;
			this.checkBoxStartTime.CheckedChanged += new System.EventHandler(this.checkBoxStartTime_CheckedChanged);
			// 
			// checkBoxEndTime
			// 
			this.checkBoxEndTime.AutoSize = true;
			this.checkBoxEndTime.Location = new System.Drawing.Point(6, 69);
			this.checkBoxEndTime.Name = "checkBoxEndTime";
			this.checkBoxEndTime.Size = new System.Drawing.Size(78, 17);
			this.checkBoxEndTime.TabIndex = 4;
			this.checkBoxEndTime.Text = "xxEndTime";
			this.checkBoxEndTime.UseVisualStyleBackColor = true;
			this.checkBoxEndTime.CheckedChanged += new System.EventHandler(this.checkBoxEndTime_CheckedChanged);
			// 
			// checkBoxBetween
			// 
			this.checkBoxBetween.AutoSize = true;
			this.checkBoxBetween.Location = new System.Drawing.Point(6, 19);
			this.checkBoxBetween.Name = "checkBoxBetween";
			this.checkBoxBetween.Size = new System.Drawing.Size(99, 17);
			this.checkBoxBetween.TabIndex = 5;
			this.checkBoxBetween.Text = "xxAlterBetween";
			this.checkBoxBetween.UseVisualStyleBackColor = true;
			// 
			// twoListSelectorActivities
			// 
			this.twoListSelectorActivities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.twoListSelectorActivities.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.twoListSelectorActivities.Location = new System.Drawing.Point(6, 19);
			this.twoListSelectorActivities.Name = "twoListSelectorActivities";
			this.twoListSelectorActivities.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorActivities.Size = new System.Drawing.Size(421, 305);
			this.twoListSelectorActivities.TabIndex = 7;
			// 
			// fromToTimePicker1
			// 
			this.fromToTimePicker1.Location = new System.Drawing.Point(130, 12);
			this.fromToTimePicker1.MinMaxEndTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("fromToTimePicker1.MinMaxEndTime")));
			this.fromToTimePicker1.MinMaxStartTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("fromToTimePicker1.MinMaxStartTime")));
			this.fromToTimePicker1.Name = "fromToTimePicker1";
			this.fromToTimePicker1.Size = new System.Drawing.Size(277, 27);
			this.fromToTimePicker1.TabIndex = 10;
			this.fromToTimePicker1.WholeDayText = "xxNextDay";
			// 
			// buttonOk
			// 
			this.buttonOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonOk.Location = new System.Drawing.Point(3, 3);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(75, 23);
			this.buttonOk.TabIndex = 11;
			this.buttonOk.Text = "xxOk";
			this.buttonOk.UseVisualStyle = true;
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonCancel.Location = new System.Drawing.Point(90, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 12;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(460, 563);
			this.tableLayoutPanel1.TabIndex = 13;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.buttonOk, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 1, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(282, 529);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(175, 31);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(3, 3);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(454, 517);
			this.tabControl1.TabIndex = 1;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.groupBox3);
			this.tabPage1.Controls.Add(this.groupBox2);
			this.tabPage1.Controls.Add(this.groupBox1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(446, 491);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "xxGeneral";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.twoListSelectorActivities);
			this.groupBox3.Location = new System.Drawing.Point(7, 164);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(433, 327);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.fromToTimePicker1);
			this.groupBox2.Controls.Add(this.checkBoxBetween);
			this.groupBox2.Location = new System.Drawing.Point(7, 109);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(433, 49);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.checkBoxEndTime);
			this.groupBox1.Controls.Add(this.checkBoxShiftCategory);
			this.groupBox1.Controls.Add(this.checkBoxStartTime);
			this.groupBox1.Location = new System.Drawing.Point(7, 7);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(433, 95);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "xxKeep";
			// 
			// OptimizeActivities
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(472, 603);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OptimizeActivities";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxOptimizeActivities";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptimizeActivities_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
        private System.Windows.Forms.CheckBox checkBoxShiftCategory;
        private System.Windows.Forms.CheckBox checkBoxStartTime;
        private System.Windows.Forms.CheckBox checkBoxEndTime;
        private System.Windows.Forms.CheckBox checkBoxBetween;
        private Teleopti.Ccc.Win.Common.Controls.TwoListSelector twoListSelectorActivities;
        private Teleopti.Ccc.Win.Common.Controls.FromToTimePicker fromToTimePicker1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOk;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;

    }
}