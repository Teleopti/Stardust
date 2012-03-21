﻿using SchedulingSessionPreferencesPanel=
    Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences.SchedulingSessionPreferencesPanel;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    partial class SchedulingSessionPreferencesDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchedulingSessionPreferencesDialog));
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.tabControlTopLevel = new System.Windows.Forms.TabControl();
            this.tabPageSchedulingOptions = new System.Windows.Forms.TabPage();
            this.schedulingSessionPreferencesPanel1 = new Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences.SchedulingSessionPreferencesPanel();
            this.tabPageDayOffPlanningOptions = new System.Windows.Forms.TabPage();
            this.dayOffPreferencesPanel1 = new Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences.ResourceOptimizerDayOffPreferencesPanel();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabControlTopLevel.SuspendLayout();
            this.tabPageSchedulingOptions.SuspendLayout();
            this.tabPageDayOffPlanningOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.ribbonControlAdv1.Size = new System.Drawing.Size(467, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "StartMenu";
            this.ribbonControlAdv1.TabIndex = 2;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tabControlTopLevel, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(457, 722);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.buttonOK, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(279, 688);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(175, 31);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // buttonOK
            // 
            this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(3, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "xxOk";
            this.buttonOK.UseVisualStyle = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(90, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "xxCancel";
            this.buttonCancel.UseVisualStyle = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // tabControlTopLevel
            // 
            this.tabControlTopLevel.Controls.Add(this.tabPageSchedulingOptions);
            this.tabControlTopLevel.Controls.Add(this.tabPageDayOffPlanningOptions);
            this.tabControlTopLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlTopLevel.ImageList = this.imageList1;
            this.tabControlTopLevel.ItemSize = new System.Drawing.Size(111, 22);
            this.tabControlTopLevel.Location = new System.Drawing.Point(3, 3);
            this.tabControlTopLevel.Name = "tabControlTopLevel";
            this.tabControlTopLevel.SelectedIndex = 0;
            this.tabControlTopLevel.Size = new System.Drawing.Size(451, 676);
            this.tabControlTopLevel.TabIndex = 6;
            this.tabControlTopLevel.Click += new System.EventHandler(this.tabControlTopLevel_Click);
            // 
            // tabPageSchedulingOptions
            // 
            this.tabPageSchedulingOptions.Controls.Add(this.schedulingSessionPreferencesPanel1);
            this.tabPageSchedulingOptions.Location = new System.Drawing.Point(4, 26);
            this.tabPageSchedulingOptions.Name = "tabPageSchedulingOptions";
            this.tabPageSchedulingOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSchedulingOptions.Size = new System.Drawing.Size(443, 646);
            this.tabPageSchedulingOptions.TabIndex = 0;
            this.tabPageSchedulingOptions.Text = "xxSchedulingOptions";
            this.tabPageSchedulingOptions.UseVisualStyleBackColor = true;
            // 
            // schedulingSessionPreferencesPanel1
            // 
            this.schedulingSessionPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.schedulingSessionPreferencesPanel1.BetweenDayOffVisible = true;
            this.schedulingSessionPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.schedulingSessionPreferencesPanel1.Location = new System.Drawing.Point(3, 3);
            this.schedulingSessionPreferencesPanel1.Name = "schedulingSessionPreferencesPanel1";
            this.schedulingSessionPreferencesPanel1.RefreshScreenVisible = false;
            this.schedulingSessionPreferencesPanel1.ScheduleOnlyAvailableDaysVisible = false;
            this.schedulingSessionPreferencesPanel1.ScheduleOnlyPreferenceDaysVisible = false;
            this.schedulingSessionPreferencesPanel1.ScheduleOnlyRotationDaysVisible = false;
            this.schedulingSessionPreferencesPanel1.SchedulePeriodVisible = true;
            this.schedulingSessionPreferencesPanel1.ShiftCategoryVisible = false;
            this.schedulingSessionPreferencesPanel1.Size = new System.Drawing.Size(437, 640);
            this.schedulingSessionPreferencesPanel1.TabIndex = 5;
            this.schedulingSessionPreferencesPanel1.UseBlockSchedulingVisible = true;
            this.schedulingSessionPreferencesPanel1.UseSameDayOffsVisible = true;
            // 
            // tabPageDayOffPlanningOptions
            // 
            this.tabPageDayOffPlanningOptions.Controls.Add(this.dayOffPreferencesPanel1);
            this.tabPageDayOffPlanningOptions.ImageKey = "on";
            this.tabPageDayOffPlanningOptions.Location = new System.Drawing.Point(4, 26);
            this.tabPageDayOffPlanningOptions.Name = "tabPageDayOffPlanningOptions";
            this.tabPageDayOffPlanningOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDayOffPlanningOptions.Size = new System.Drawing.Size(443, 646);
            this.tabPageDayOffPlanningOptions.TabIndex = 1;
            this.tabPageDayOffPlanningOptions.Text = "xxDayOffPlannerOptions";
            this.tabPageDayOffPlanningOptions.UseVisualStyleBackColor = true;
            // 
            // dayOffPreferencesPanel1
            // 
            this.dayOffPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dayOffPreferencesPanel1.KeepFreeWeekendDaysVisible = false;
            this.dayOffPreferencesPanel1.KeepFreeWeekendsVisible = false;
            this.dayOffPreferencesPanel1.Location = new System.Drawing.Point(3, 3);
            this.dayOffPreferencesPanel1.Name = "dayOffPreferencesPanel1";
            this.dayOffPreferencesPanel1.Size = new System.Drawing.Size(186, 68);
            this.dayOffPreferencesPanel1.TabIndex = 5;
            this.dayOffPreferencesPanel1.StatusChanged += new System.EventHandler<System.EventArgs>(this.dayOffPreferencesPanel1_StatusChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.White;
            this.imageList1.Images.SetKeyName(0, "off");
            this.imageList1.Images.SetKeyName(1, "on");
            // 
            // SchedulingSessionPreferencesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 762);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.ribbonControlAdv1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SchedulingSessionPreferencesDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxSchedulingSessionOptions";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabControlTopLevel.ResumeLayout(false);
            this.tabPageSchedulingOptions.ResumeLayout(false);
            this.tabPageDayOffPlanningOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private SchedulingSessionPreferencesPanel schedulingSessionPreferencesPanel1;
        private System.Windows.Forms.TabControl tabControlTopLevel;
        private System.Windows.Forms.TabPage tabPageSchedulingOptions;
        private System.Windows.Forms.TabPage tabPageDayOffPlanningOptions;
        private ResourceOptimizerDayOffPreferencesPanel dayOffPreferencesPanel1;
        private System.Windows.Forms.ImageList imageList1;
    }
}