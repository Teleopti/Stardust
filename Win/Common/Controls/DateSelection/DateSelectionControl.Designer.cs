﻿namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    partial class DateSelectionControl : BaseUserControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DateSelectionControl));
            this.tabControlAdvDateSelection = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
            this.tabPageAdvRolling = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.dateSelectionRolling1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionRolling();
            this.tabPageAdvFromTo = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.dateSelectionFromTo1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
            this.tabPageAdvCalendar = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.dateSelectionCalendar1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionCalendar();
            this.imageListTabs = new System.Windows.Forms.ImageList(this.components);
            this.designTimeTabTypeLoader = new Syncfusion.Reflection.TypeLoader(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdvDateSelection)).BeginInit();
            this.tabControlAdvDateSelection.SuspendLayout();
            this.tabPageAdvRolling.SuspendLayout();
            this.tabPageAdvFromTo.SuspendLayout();
            this.tabPageAdvCalendar.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlAdvDateSelection
            // 
            this.tabControlAdvDateSelection.ActiveTabFont = new System.Drawing.Font("Tahoma", 8F);
            this.tabControlAdvDateSelection.BackColor = System.Drawing.Color.White;
            this.tabControlAdvDateSelection.Controls.Add(this.tabPageAdvRolling);
            this.tabControlAdvDateSelection.Controls.Add(this.tabPageAdvFromTo);
            this.tabControlAdvDateSelection.Controls.Add(this.tabPageAdvCalendar);
            this.tabControlAdvDateSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlAdvDateSelection.ImageList = this.imageListTabs;
            this.tabControlAdvDateSelection.Location = new System.Drawing.Point(0, 0);
            this.tabControlAdvDateSelection.Name = "tabControlAdvDateSelection";
            this.tabControlAdvDateSelection.Size = new System.Drawing.Size(168, 335);
            this.tabControlAdvDateSelection.TabGap = 10;
            this.tabControlAdvDateSelection.TabIndex = 1;
            this.tabControlAdvDateSelection.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
            this.tabControlAdvDateSelection.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
            // 
            // tabPageAdvRolling
            // 
            this.tabPageAdvRolling.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageAdvRolling.Controls.Add(this.dateSelectionRolling1);
            this.tabPageAdvRolling.Image = null;
            this.tabPageAdvRolling.ImageIndex = 5;
            this.tabPageAdvRolling.ImageSize = new System.Drawing.Size(16, 16);
            this.tabPageAdvRolling.Location = new System.Drawing.Point(1, 29);
            this.tabPageAdvRolling.Name = "tabPageAdvRolling";
            this.tabPageAdvRolling.Size = new System.Drawing.Size(165, 304);
            this.tabPageAdvRolling.TabIndex = 1;
            this.tabPageAdvRolling.ThemesEnabled = false;
            // 
            // dateSelectionRolling1
            // 
            this.dateSelectionRolling1.BackColor = System.Drawing.Color.Transparent;
            this.dateSelectionRolling1.ButtonApplyText = "xxAdd";
            this.dateSelectionRolling1.Dock = System.Windows.Forms.DockStyle.Top;
            this.dateSelectionRolling1.Location = new System.Drawing.Point(0, 0);
            this.dateSelectionRolling1.Name = "dateSelectionRolling1";
            this.dateSelectionRolling1.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.dateSelectionRolling1.ShowApplyButton = false;
            this.dateSelectionRolling1.Size = new System.Drawing.Size(165, 62);
            this.dateSelectionRolling1.TabIndex = 0;
            this.dateSelectionRolling1.DateRangeChanged += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.DateSelection.DateRangeChangedEventArgs>(this.dateSelectionRolling1_DateRangeChanged);
            // 
            // tabPageAdvFromTo
            // 
            this.tabPageAdvFromTo.BackColor = System.Drawing.Color.Transparent;
            this.tabPageAdvFromTo.Controls.Add(this.dateSelectionFromTo1);
            this.tabPageAdvFromTo.Image = null;
            this.tabPageAdvFromTo.ImageIndex = 4;
            this.tabPageAdvFromTo.ImageSize = new System.Drawing.Size(16, 16);
            this.tabPageAdvFromTo.Location = new System.Drawing.Point(1, 29);
            this.tabPageAdvFromTo.Name = "tabPageAdvFromTo";
            this.tabPageAdvFromTo.Size = new System.Drawing.Size(165, 304);
            this.tabPageAdvFromTo.TabIndex = 1;
            this.tabPageAdvFromTo.ThemesEnabled = false;
            // 
            // dateSelectionFromTo1
            // 
            this.dateSelectionFromTo1.BackColor = System.Drawing.Color.Transparent;
            this.dateSelectionFromTo1.ButtonApplyText = "xxAdd";
            this.dateSelectionFromTo1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateSelectionFromTo1.HideNoneButtons = true;
            this.dateSelectionFromTo1.LabelDateSelectionText = "xxFrom";
            this.dateSelectionFromTo1.LabelDateSelectionToText = "xxTo";
            this.dateSelectionFromTo1.Location = new System.Drawing.Point(0, 0);
            this.dateSelectionFromTo1.Name = "dateSelectionFromTo1";
            this.dateSelectionFromTo1.NoneButtonText = "xxNone";
            this.dateSelectionFromTo1.NullString = "xxNoDateIsSelected";
            this.dateSelectionFromTo1.ShowApplyButton = false;
            this.dateSelectionFromTo1.Size = new System.Drawing.Size(165, 304);
            this.dateSelectionFromTo1.TabIndex = 0;
            this.dateSelectionFromTo1.TodayButtonText = "xxToday";
            this.dateSelectionFromTo1.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromTo1.WorkPeriodEnd")));
            this.dateSelectionFromTo1.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromTo1.WorkPeriodStart")));
            this.dateSelectionFromTo1.DateRangeChanged += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.DateSelection.DateRangeChangedEventArgs>(this.dateSelectionFromTo1_DateRangeChanged);
            // 
            // tabPageAdvCalendar
            // 
            this.tabPageAdvCalendar.BackColor = System.Drawing.Color.Transparent;
            this.tabPageAdvCalendar.Controls.Add(this.dateSelectionCalendar1);
            this.tabPageAdvCalendar.Image = null;
            this.tabPageAdvCalendar.ImageIndex = 3;
            this.tabPageAdvCalendar.ImageSize = new System.Drawing.Size(16, 16);
            this.tabPageAdvCalendar.Location = new System.Drawing.Point(1, 29);
            this.tabPageAdvCalendar.Name = "tabPageAdvCalendar";
            this.tabPageAdvCalendar.Size = new System.Drawing.Size(165, 304);
            this.tabPageAdvCalendar.TabIndex = 2;
            this.tabPageAdvCalendar.ThemesEnabled = false;
            // 
            // dateSelectionCalendar1
            // 
            this.dateSelectionCalendar1.ButtonApplyText = "xxAdd";
            this.dateSelectionCalendar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateSelectionCalendar1.Location = new System.Drawing.Point(0, 0);
            this.dateSelectionCalendar1.Name = "dateSelectionCalendar1";
            this.dateSelectionCalendar1.ShowApplyButton = false;
            this.dateSelectionCalendar1.Size = new System.Drawing.Size(165, 304);
            this.dateSelectionCalendar1.TabIndex = 0;
            this.dateSelectionCalendar1.DateRangeChanged += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.DateSelection.DateRangeChangedEventArgs>(this.dateSelectionCalendar1_DateRangeChanged);
            // 
            // imageListTabs
            // 
            this.imageListTabs.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTabs.ImageStream")));
            this.imageListTabs.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListTabs.Images.SetKeyName(0, "process.ico");
            this.imageListTabs.Images.SetKeyName(1, "Skill_Inactive_8bit_BG.png");
            this.imageListTabs.Images.SetKeyName(2, "tb_report-sched.png");
            this.imageListTabs.Images.SetKeyName(3, "day.png");
            this.imageListTabs.Images.SetKeyName(4, "fromto.png");
            this.imageListTabs.Images.SetKeyName(5, "rolling.png");
            // 
            // designTimeTabTypeLoader
            // 
            this.designTimeTabTypeLoader.InvokeMemberName = "TabStyleName";
            // 
            // DateSelectionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tabControlAdvDateSelection);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DateSelectionControl";
            this.Size = new System.Drawing.Size(168, 335);
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdvDateSelection)).EndInit();
            this.tabControlAdvDateSelection.ResumeLayout(false);
            this.tabPageAdvRolling.ResumeLayout(false);
            this.tabPageAdvFromTo.ResumeLayout(false);
            this.tabPageAdvCalendar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdvDateSelection;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvRolling;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvFromTo;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvCalendar;
        private DateSelectionRolling dateSelectionRolling1;
        private DateSelectionFromTo dateSelectionFromTo1;
        private DateSelectionCalendar dateSelectionCalendar1;
        private System.Windows.Forms.ImageList imageListTabs;
        private Syncfusion.Reflection.TypeLoader designTimeTabTypeLoader;
    }
}