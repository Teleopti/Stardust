namespace Teleopti.Ccc.Win.Scheduling
{
    partial class FormClipboardSpecial
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
			this.comboBoxAdvOvertime = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.checkBoxShiftAsOvertime = new System.Windows.Forms.CheckBox();
			this.checkBoxOvertimeAvailability = new System.Windows.Forms.CheckBox();
			this.checkBoxStudentAvailability = new System.Windows.Forms.CheckBox();
			this.checkBoxPreferences = new System.Windows.Forms.CheckBox();
			this.checkBoxOvertime = new System.Windows.Forms.CheckBox();
			this.buttonAdv2 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdv1 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.checkBoxPersonalAssignments = new System.Windows.Forms.CheckBox();
			this.checkBoxDayOffs = new System.Windows.Forms.CheckBox();
			this.checkBoxAbsences = new System.Windows.Forms.CheckBox();
			this.checkBoxAssignments = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvOvertime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.SuspendLayout();
			// 
			// comboBoxAdvOvertime
			// 
			this.comboBoxAdvOvertime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdvOvertime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvOvertime.Location = new System.Drawing.Point(12, 169);
			this.comboBoxAdvOvertime.Name = "comboBoxAdvOvertime";
			this.comboBoxAdvOvertime.Size = new System.Drawing.Size(148, 21);
			this.comboBoxAdvOvertime.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdvOvertime.TabIndex = 31;
			this.comboBoxAdvOvertime.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvOvertime_SelectedIndexChanged);
			// 
			// checkBoxShiftAsOvertime
			// 
			this.checkBoxShiftAsOvertime.AutoSize = true;
			this.checkBoxShiftAsOvertime.Location = new System.Drawing.Point(12, 146);
			this.checkBoxShiftAsOvertime.Name = "checkBoxShiftAsOvertime";
			this.checkBoxShiftAsOvertime.Size = new System.Drawing.Size(111, 17);
			this.checkBoxShiftAsOvertime.TabIndex = 30;
			this.checkBoxShiftAsOvertime.Text = "xxShiftAsOvertime";
			this.checkBoxShiftAsOvertime.UseVisualStyleBackColor = true;
			this.checkBoxShiftAsOvertime.CheckedChanged += new System.EventHandler(this.checkBoxShiftAsOvertime_CheckedChanged);
			// 
			// checkBoxOvertimeAvailability
			// 
			this.checkBoxOvertimeAvailability.AutoSize = true;
			this.checkBoxOvertimeAvailability.Location = new System.Drawing.Point(192, 90);
			this.checkBoxOvertimeAvailability.Name = "checkBoxOvertimeAvailability";
			this.checkBoxOvertimeAvailability.Size = new System.Drawing.Size(127, 17);
			this.checkBoxOvertimeAvailability.TabIndex = 29;
			this.checkBoxOvertimeAvailability.Text = "xxOvertimeAvailability";
			this.checkBoxOvertimeAvailability.UseVisualStyleBackColor = true;
			this.checkBoxOvertimeAvailability.CheckedChanged += new System.EventHandler(this.checkBoxOvertimeAvailability_CheckedChanged);
			// 
			// checkBoxStudentAvailability
			// 
			this.checkBoxStudentAvailability.AutoSize = true;
			this.checkBoxStudentAvailability.Location = new System.Drawing.Point(192, 113);
			this.checkBoxStudentAvailability.Name = "checkBoxStudentAvailability";
			this.checkBoxStudentAvailability.Size = new System.Drawing.Size(122, 17);
			this.checkBoxStudentAvailability.TabIndex = 7;
			this.checkBoxStudentAvailability.Text = "xxStudentAvailability";
			this.checkBoxStudentAvailability.UseVisualStyleBackColor = true;
			this.checkBoxStudentAvailability.CheckedChanged += new System.EventHandler(this.checkBoxStudentAvailability_CheckedChanged);
			// 
			// checkBoxPreferences
			// 
			this.checkBoxPreferences.AutoSize = true;
			this.checkBoxPreferences.Location = new System.Drawing.Point(12, 113);
			this.checkBoxPreferences.Name = "checkBoxPreferences";
			this.checkBoxPreferences.Size = new System.Drawing.Size(93, 17);
			this.checkBoxPreferences.TabIndex = 6;
			this.checkBoxPreferences.Text = "xxPreferences";
			this.checkBoxPreferences.UseVisualStyleBackColor = true;
			this.checkBoxPreferences.CheckedChanged += new System.EventHandler(this.checkBoxPreferences_CheckedChanged);
			// 
			// checkBoxOvertime
			// 
			this.checkBoxOvertime.AutoSize = true;
			this.checkBoxOvertime.Location = new System.Drawing.Point(192, 67);
			this.checkBoxOvertime.Name = "checkBoxOvertime";
			this.checkBoxOvertime.Size = new System.Drawing.Size(78, 17);
			this.checkBoxOvertime.TabIndex = 5;
			this.checkBoxOvertime.Text = "xxOvertime";
			this.checkBoxOvertime.UseVisualStyleBackColor = true;
			this.checkBoxOvertime.CheckedChanged += new System.EventHandler(this.checkBoxOvertime_CheckedChanged);
			// 
			// buttonAdv2
			// 
			this.buttonAdv2.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdv2.Location = new System.Drawing.Point(247, 235);
			this.buttonAdv2.Name = "buttonAdv2";
			this.buttonAdv2.Size = new System.Drawing.Size(75, 23);
			this.buttonAdv2.TabIndex = 9;
			this.buttonAdv2.Text = "xxCancel";
			this.buttonAdv2.UseVisualStyle = true;
			this.buttonAdv2.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonAdv1
			// 
			this.buttonAdv1.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdv1.Location = new System.Drawing.Point(166, 235);
			this.buttonAdv1.Name = "buttonAdv1";
			this.buttonAdv1.Size = new System.Drawing.Size(75, 23);
			this.buttonAdv1.TabIndex = 8;
			this.buttonAdv1.Text = "xxOk";
			this.buttonAdv1.UseVisualStyle = true;
			this.buttonAdv1.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(338, 33);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdv1.TabIndex = 28;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			// 
			// checkBoxPersonalAssignments
			// 
			this.checkBoxPersonalAssignments.AutoSize = true;
			this.checkBoxPersonalAssignments.Location = new System.Drawing.Point(192, 43);
			this.checkBoxPersonalAssignments.Name = "checkBoxPersonalAssignments";
			this.checkBoxPersonalAssignments.Size = new System.Drawing.Size(103, 17);
			this.checkBoxPersonalAssignments.TabIndex = 4;
			this.checkBoxPersonalAssignments.Text = "xxPersonalShifts";
			this.checkBoxPersonalAssignments.UseVisualStyleBackColor = true;
			this.checkBoxPersonalAssignments.CheckedChanged += new System.EventHandler(this.checkBoxPersonalAssignments_CheckedChanged);
			// 
			// checkBoxDayOffs
			// 
			this.checkBoxDayOffs.AutoSize = true;
			this.checkBoxDayOffs.Location = new System.Drawing.Point(12, 90);
			this.checkBoxDayOffs.Name = "checkBoxDayOffs";
			this.checkBoxDayOffs.Size = new System.Drawing.Size(74, 17);
			this.checkBoxDayOffs.TabIndex = 3;
			this.checkBoxDayOffs.Text = "xxDaysOff";
			this.checkBoxDayOffs.UseVisualStyleBackColor = true;
			this.checkBoxDayOffs.CheckedChanged += new System.EventHandler(this.checkBoxDayOffs_CheckedChanged);
			// 
			// checkBoxAbsences
			// 
			this.checkBoxAbsences.AutoSize = true;
			this.checkBoxAbsences.Location = new System.Drawing.Point(12, 66);
			this.checkBoxAbsences.Name = "checkBoxAbsences";
			this.checkBoxAbsences.Size = new System.Drawing.Size(83, 17);
			this.checkBoxAbsences.TabIndex = 2;
			this.checkBoxAbsences.Text = "xxAbsences";
			this.checkBoxAbsences.UseVisualStyleBackColor = true;
			this.checkBoxAbsences.CheckedChanged += new System.EventHandler(this.checkBoxAbsences_CheckedChanged);
			// 
			// checkBoxAssignments
			// 
			this.checkBoxAssignments.AutoSize = true;
			this.checkBoxAssignments.Location = new System.Drawing.Point(12, 43);
			this.checkBoxAssignments.Name = "checkBoxAssignments";
			this.checkBoxAssignments.Size = new System.Drawing.Size(62, 17);
			this.checkBoxAssignments.TabIndex = 1;
			this.checkBoxAssignments.Text = "xxShifts";
			this.checkBoxAssignments.UseVisualStyleBackColor = true;
			this.checkBoxAssignments.CheckedChanged += new System.EventHandler(this.checkBoxAssignments_CheckedChanged);
			// 
			// FormClipboardSpecial
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(340, 274);
			this.Controls.Add(this.comboBoxAdvOvertime);
			this.Controls.Add(this.checkBoxShiftAsOvertime);
			this.Controls.Add(this.checkBoxOvertimeAvailability);
			this.Controls.Add(this.checkBoxStudentAvailability);
			this.Controls.Add(this.checkBoxPreferences);
			this.Controls.Add(this.checkBoxOvertime);
			this.Controls.Add(this.buttonAdv2);
			this.Controls.Add(this.buttonAdv1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Controls.Add(this.checkBoxPersonalAssignments);
			this.Controls.Add(this.checkBoxDayOffs);
			this.Controls.Add(this.checkBoxAbsences);
			this.Controls.Add(this.checkBoxAssignments);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormClipboardSpecial";
			this.Text = "xxSpecial";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClipboardSpecial_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvOvertime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxPersonalAssignments;
        private System.Windows.Forms.CheckBox checkBoxDayOffs;
        private System.Windows.Forms.CheckBox checkBoxAbsences;
        private System.Windows.Forms.CheckBox checkBoxAssignments;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv2;
        private System.Windows.Forms.CheckBox checkBoxOvertime;
        private System.Windows.Forms.CheckBox checkBoxPreferences;
        private System.Windows.Forms.CheckBox checkBoxStudentAvailability;
        private System.Windows.Forms.CheckBox checkBoxOvertimeAvailability;
		private System.Windows.Forms.CheckBox checkBoxShiftAsOvertime;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvOvertime;
    }
}