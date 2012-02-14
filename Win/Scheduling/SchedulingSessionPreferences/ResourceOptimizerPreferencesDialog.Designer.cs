using SchedulingSessionPreferencesPanel=
    Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences.SchedulingSessionPreferencesPanel;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    partial class ResourceOptimizerPreferencesDialog
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
        private void InitializeComponent()
        {
            this.tableLayoutPanel12 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDown7 = new System.Windows.Forms.NumericUpDown();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.tabControlResourceReOptimizerOptions = new System.Windows.Forms.TabControl();
            this.tabSchedulingOptions = new System.Windows.Forms.TabPage();
            this.schedulingSessionPreferencesPanel1 = new Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences.SchedulingSessionPreferencesPanel();
            this.tabPageUserDefinedOptions = new System.Windows.Forms.TabPage();
            this.resourceOptimizerUserPreferencesPanel1 = new Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences.ResourceOptimizerUserPreferencesPanel();
            this.tabPageDayOffPlannerOptions = new System.Windows.Forms.TabPage();
            this.dayOffPreferencesPanel1 = new Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences.ResourceOptimizerDayOffPreferencesPanel();
            this.tabPageAdvancedOptions = new System.Windows.Forms.TabPage();
            this.resourceOptimizerPerformancePreferencesPanel1 = new Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences.ResourceOptimizerAdvancedPreferencesPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.tableLayoutPanel12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.tabControlResourceReOptimizerOptions.SuspendLayout();
            this.tabSchedulingOptions.SuspendLayout();
            this.tabPageUserDefinedOptions.SuspendLayout();
            this.tabPageDayOffPlannerOptions.SuspendLayout();
            this.tabPageAdvancedOptions.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel12
            // 
            this.tableLayoutPanel12.ColumnCount = 2;
            this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel12.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel12.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel12.Name = "tableLayoutPanel12";
            this.tableLayoutPanel12.RowCount = 1;
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel12.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel12.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 43);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "label7";
            // 
            // numericUpDown7
            // 
            this.numericUpDown7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numericUpDown7.Location = new System.Drawing.Point(187, 6);
            this.numericUpDown7.Name = "numericUpDown7";
            this.numericUpDown7.Size = new System.Drawing.Size(64, 20);
            this.numericUpDown7.TabIndex = 1;
            this.numericUpDown7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
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
            this.ribbonControlAdv1.ShowLauncher = false;
            this.ribbonControlAdv1.ShowMinimizeButton = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(486, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "StartMenu";
            this.ribbonControlAdv1.TabIndex = 1;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // tabControlResourceReOptimizerOptions
            // 
            this.tabControlResourceReOptimizerOptions.Controls.Add(this.tabSchedulingOptions);
            this.tabControlResourceReOptimizerOptions.Controls.Add(this.tabPageUserDefinedOptions);
            this.tabControlResourceReOptimizerOptions.Controls.Add(this.tabPageDayOffPlannerOptions);
            this.tabControlResourceReOptimizerOptions.Controls.Add(this.tabPageAdvancedOptions);
            this.tabControlResourceReOptimizerOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControlResourceReOptimizerOptions.Location = new System.Drawing.Point(6, 34);
            this.tabControlResourceReOptimizerOptions.Name = "tabControlResourceReOptimizerOptions";
            this.tabControlResourceReOptimizerOptions.SelectedIndex = 0;
            this.tabControlResourceReOptimizerOptions.Size = new System.Drawing.Size(476, 592);
            this.tabControlResourceReOptimizerOptions.TabIndex = 4;
            this.tabControlResourceReOptimizerOptions.Click += new System.EventHandler(this.tabControlResourceReOptimizerPerformanceOptions_Click);
            // 
            // tabSchedulingOptions
            // 
            this.tabSchedulingOptions.Controls.Add(this.schedulingSessionPreferencesPanel1);
            this.tabSchedulingOptions.Location = new System.Drawing.Point(4, 22);
            this.tabSchedulingOptions.Name = "tabSchedulingOptions";
            this.tabSchedulingOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabSchedulingOptions.Size = new System.Drawing.Size(468, 566);
            this.tabSchedulingOptions.TabIndex = 3;
            this.tabSchedulingOptions.Text = "xxSchedulingOptions";
            this.tabSchedulingOptions.UseVisualStyleBackColor = true;
            // 
            // schedulingSessionPreferencesPanel1
            // 
            this.schedulingSessionPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.schedulingSessionPreferencesPanel1.BetweenDayOffVisible = false;
            this.schedulingSessionPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.schedulingSessionPreferencesPanel1.Location = new System.Drawing.Point(3, 3);
            this.schedulingSessionPreferencesPanel1.Name = "schedulingSessionPreferencesPanel1";
            this.schedulingSessionPreferencesPanel1.RefreshScreenVisible = false;
            this.schedulingSessionPreferencesPanel1.ScheduleOnlyAvailableDaysVisible = false;
            this.schedulingSessionPreferencesPanel1.ScheduleOnlyPreferenceDaysVisible = false;
            this.schedulingSessionPreferencesPanel1.ScheduleOnlyRotationDaysVisible = false;
            this.schedulingSessionPreferencesPanel1.SchedulePeriodVisible = false;
            this.schedulingSessionPreferencesPanel1.ShiftCategoryVisible = false;
            this.schedulingSessionPreferencesPanel1.Size = new System.Drawing.Size(462, 560);
            this.schedulingSessionPreferencesPanel1.TabIndex = 0;
            this.schedulingSessionPreferencesPanel1.UseBlockSchedulingVisible = false;
            this.schedulingSessionPreferencesPanel1.UseSameDayOffsVisible = false;
            // 
            // tabPageUserDefinedOptions
            // 
            this.tabPageUserDefinedOptions.Controls.Add(this.resourceOptimizerUserPreferencesPanel1);
            this.tabPageUserDefinedOptions.Location = new System.Drawing.Point(4, 22);
            this.tabPageUserDefinedOptions.Name = "tabPageUserDefinedOptions";
            this.tabPageUserDefinedOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUserDefinedOptions.Size = new System.Drawing.Size(468, 566);
            this.tabPageUserDefinedOptions.TabIndex = 0;
            this.tabPageUserDefinedOptions.Text = "xxUserDefinedOptions";
            this.tabPageUserDefinedOptions.UseVisualStyleBackColor = true;
            // 
            // resourceOptimizerUserPreferencesPanel1
            // 
            this.resourceOptimizerUserPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.resourceOptimizerUserPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourceOptimizerUserPreferencesPanel1.Location = new System.Drawing.Point(3, 3);
            this.resourceOptimizerUserPreferencesPanel1.Name = "resourceOptimizerUserPreferencesPanel1";
            this.resourceOptimizerUserPreferencesPanel1.OptimizerAdvancedPreferences = null;
            this.resourceOptimizerUserPreferencesPanel1.Size = new System.Drawing.Size(462, 560);
            this.resourceOptimizerUserPreferencesPanel1.TabIndex = 0;
            // 
            // tabPageDayOffPlannerOptions
            // 
            this.tabPageDayOffPlannerOptions.Controls.Add(this.dayOffPreferencesPanel1);
            this.tabPageDayOffPlannerOptions.Location = new System.Drawing.Point(4, 22);
            this.tabPageDayOffPlannerOptions.Name = "tabPageDayOffPlannerOptions";
            this.tabPageDayOffPlannerOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDayOffPlannerOptions.Size = new System.Drawing.Size(468, 566);
            this.tabPageDayOffPlannerOptions.TabIndex = 1;
            this.tabPageDayOffPlannerOptions.Text = "xxDayOffPlannerOptions";
            this.tabPageDayOffPlannerOptions.UseVisualStyleBackColor = true;
            // 
            // dayOffPreferencesPanel1
            // 
            this.dayOffPreferencesPanel1.KeepFreeWeekendDaysVisible = false;
            this.dayOffPreferencesPanel1.KeepFreeWeekendsVisible = false;
            this.dayOffPreferencesPanel1.Location = new System.Drawing.Point(6, 6);
            this.dayOffPreferencesPanel1.Name = "dayOffPreferencesPanel1";
            this.dayOffPreferencesPanel1.Size = new System.Drawing.Size(429, 415);
            this.dayOffPreferencesPanel1.TabIndex = 4;
            // 
            // tabPageAdvancedOptions
            // 
            this.tabPageAdvancedOptions.Controls.Add(this.resourceOptimizerPerformancePreferencesPanel1);
            this.tabPageAdvancedOptions.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdvancedOptions.Name = "tabPageAdvancedOptions";
            this.tabPageAdvancedOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAdvancedOptions.Size = new System.Drawing.Size(468, 566);
            this.tabPageAdvancedOptions.TabIndex = 2;
            this.tabPageAdvancedOptions.Text = "xxAdvancedOptions";
            this.tabPageAdvancedOptions.UseVisualStyleBackColor = true;
            // 
            // resourceOptimizerPerformancePreferencesPanel1
            // 
            this.resourceOptimizerPerformancePreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.resourceOptimizerPerformancePreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourceOptimizerPerformancePreferencesPanel1.Location = new System.Drawing.Point(3, 3);
            this.resourceOptimizerPerformancePreferencesPanel1.Name = "resourceOptimizerPerformancePreferencesPanel1";
            this.resourceOptimizerPerformancePreferencesPanel1.OptimizerAdvancedPreferences = null;
            this.resourceOptimizerPerformancePreferencesPanel1.Size = new System.Drawing.Size(462, 560);
            this.resourceOptimizerPerformancePreferencesPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.buttonOK, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(298, 632);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(175, 31);
            this.tableLayoutPanel2.TabIndex = 5;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonOK.Location = new System.Drawing.Point(9, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "xxOk";
            this.buttonOK.UseVisualStyle = true;
            this.buttonOK.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonCancel.Location = new System.Drawing.Point(97, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "xxCancel";
            this.buttonCancel.UseVisualStyle = true;
            this.buttonCancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ResourceOptimizerPreferencesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 681);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tabControlResourceReOptimizerOptions);
            this.Controls.Add(this.ribbonControlAdv1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResourceOptimizerPreferencesDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxResourceReReoptimizerOptions";
            this.Load += new System.EventHandler(this.Form_Load);
            this.tableLayoutPanel12.ResumeLayout(false);
            this.tableLayoutPanel12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.tabControlResourceReOptimizerOptions.ResumeLayout(false);
            this.tabSchedulingOptions.ResumeLayout(false);
            this.tabPageUserDefinedOptions.ResumeLayout(false);
            this.tabPageDayOffPlannerOptions.ResumeLayout(false);
            this.tabPageAdvancedOptions.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel12;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDown7;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private System.Windows.Forms.TabControl tabControlResourceReOptimizerOptions;
        private System.Windows.Forms.TabPage tabPageUserDefinedOptions;
        private System.Windows.Forms.TabPage tabPageDayOffPlannerOptions;
        private ResourceOptimizerDayOffPreferencesPanel dayOffPreferencesPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private ResourceOptimizerUserPreferencesPanel resourceOptimizerUserPreferencesPanel1;
        private System.Windows.Forms.TabPage tabPageAdvancedOptions;
        private ResourceOptimizerAdvancedPreferencesPanel resourceOptimizerPerformancePreferencesPanel1;
        private System.Windows.Forms.TabPage tabSchedulingOptions;
        private SchedulingSessionPreferencesPanel schedulingSessionPreferencesPanel1;
    }
}