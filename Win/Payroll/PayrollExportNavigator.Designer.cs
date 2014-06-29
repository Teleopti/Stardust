using System;
using Teleopti.Common.UI.SmartPartControls.SmartParts;

namespace Teleopti.Ccc.Win.Payroll
{
    partial class PayrollExportNavigator
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
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
                SmartPartInvoker.ClearAllSmartParts();
                Main.EntityEventAggregator.EntitiesNeedsRefresh -= entitiesNeedsRefresh;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PayrollExportNavigator));
			this.treeViewMain = new System.Windows.Forms.TreeView();
			this.contextMenuStripPayrollExport = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemNew2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemContextRunExport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemDelete2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemProperties2 = new System.Windows.Forms.ToolStripMenuItem();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.toolStripMenuItemWorkloadSkillNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemNewWorkload = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.xxEditForecastThreeDotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemDeleteWorkload = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemWorkloadProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.toolStripPayrollExport = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripLabelSkillActions = new System.Windows.Forms.ToolStripLabel();
			this.toolStripMenuItemNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemRunExport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripPayrollExport.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.toolStripPayrollExport.SuspendLayout();
			this.SuspendLayout();
			// 
			// treeViewMain
			// 
			this.treeViewMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeViewMain.ContextMenuStrip = this.contextMenuStripPayrollExport;
			this.treeViewMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewMain.ImageIndex = 0;
			this.treeViewMain.ImageList = this.imageList1;
			this.treeViewMain.ItemHeight = 18;
			this.treeViewMain.Location = new System.Drawing.Point(0, 0);
			this.treeViewMain.Margin = new System.Windows.Forms.Padding(0);
			this.treeViewMain.Name = "treeViewMain";
			this.treeViewMain.RightToLeftLayout = true;
			this.treeViewMain.SelectedImageIndex = 0;
			this.treeViewMain.ShowLines = false;
			this.treeViewMain.Size = new System.Drawing.Size(218, 354);
			this.treeViewMain.TabIndex = 1;
			this.treeViewMain.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewMain_BeforeSelect);
			this.treeViewMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeViewMain_MouseDown);
			// 
			// contextMenuStripPayrollExport
			// 
			this.contextMenuStripPayrollExport.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemNew2,
            this.toolStripSeparator6,
            this.toolStripMenuItemContextRunExport,
            this.toolStripSeparator2,
            this.toolStripMenuItemDelete2,
            this.toolStripMenuItemProperties2});
			this.contextMenuStripPayrollExport.Name = "contextMenuStripPayrollExport";
			this.contextMenuStripPayrollExport.Size = new System.Drawing.Size(192, 104);
			// 
			// toolStripMenuItemNew2
			// 
			this.toolStripMenuItemNew2.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripMenuItemNew2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemNew2.Name = "toolStripMenuItemNew2";
			this.toolStripMenuItemNew2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemNew2.Size = new System.Drawing.Size(191, 22);
			this.toolStripMenuItemNew2.Text = "xxNewThreeDots";
			this.toolStripMenuItemNew2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemNew2.Click += new System.EventHandler(this.toolStripMenuItemNew2_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(188, 6);
			// 
			// toolStripMenuItemContextRunExport
			// 
			this.toolStripMenuItemContextRunExport.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Export2;
			this.toolStripMenuItemContextRunExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemContextRunExport.Name = "toolStripMenuItemContextRunExport";
			this.toolStripMenuItemContextRunExport.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemContextRunExport.Size = new System.Drawing.Size(191, 22);
			this.toolStripMenuItemContextRunExport.Text = "xxRunExport";
			this.toolStripMenuItemContextRunExport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemContextRunExport.Click += new System.EventHandler(this.toolStripMenuItemContextRunExport_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(188, 6);
			// 
			// toolStripMenuItemDelete2
			// 
			this.toolStripMenuItemDelete2.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItemDelete2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemDelete2.Name = "toolStripMenuItemDelete2";
			this.toolStripMenuItemDelete2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemDelete2.Size = new System.Drawing.Size(191, 22);
			this.toolStripMenuItemDelete2.Text = "xxDelete";
			this.toolStripMenuItemDelete2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemDelete2.Click += new System.EventHandler(this.toolStripMenuItemDelete2_Click);
			// 
			// toolStripMenuItemProperties2
			// 
			this.toolStripMenuItemProperties2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemProperties2.Image")));
			this.toolStripMenuItemProperties2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemProperties2.Name = "toolStripMenuItemProperties2";
			this.toolStripMenuItemProperties2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemProperties2.Size = new System.Drawing.Size(191, 22);
			this.toolStripMenuItemProperties2.Text = "xxPropertiesThreeDots";
			this.toolStripMenuItemProperties2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemProperties2.Click += new System.EventHandler(this.toolStripMenuItemProperties2_Click);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ccc_saved_payroll_export.png");
			// 
			// toolStripMenuItemWorkloadSkillNew
			// 
			this.toolStripMenuItemWorkloadSkillNew.Name = "toolStripMenuItemWorkloadSkillNew";
			this.toolStripMenuItemWorkloadSkillNew.Size = new System.Drawing.Size(32, 19);
			// 
			// toolStripMenuItemNewWorkload
			// 
			this.toolStripMenuItemNewWorkload.Name = "toolStripMenuItemNewWorkload";
			this.toolStripMenuItemNewWorkload.Size = new System.Drawing.Size(32, 19);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 6);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			// 
			// xxEditForecastThreeDotsToolStripMenuItem
			// 
			this.xxEditForecastThreeDotsToolStripMenuItem.Name = "xxEditForecastThreeDotsToolStripMenuItem";
			this.xxEditForecastThreeDotsToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 6);
			// 
			// toolStripMenuItemDeleteWorkload
			// 
			this.toolStripMenuItemDeleteWorkload.Name = "toolStripMenuItemDeleteWorkload";
			this.toolStripMenuItemDeleteWorkload.Size = new System.Drawing.Size(32, 19);
			// 
			// toolStripMenuItemWorkloadProperties
			// 
			this.toolStripMenuItemWorkloadProperties.Name = "toolStripMenuItemWorkloadProperties";
			this.toolStripMenuItemWorkloadProperties.Size = new System.Drawing.Size(32, 19);
			// 
			// splitContainer1
			// 
			this.splitContainer1.BackColor = System.Drawing.Color.Gainsboro;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeViewMain);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.AutoScroll = true;
			this.splitContainer1.Panel2.BackColor = System.Drawing.Color.White;
			this.splitContainer1.Panel2.Controls.Add(this.toolStripPayrollExport);
			this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.splitContainer1.Size = new System.Drawing.Size(218, 556);
			this.splitContainer1.SplitterDistance = 354;
			this.splitContainer1.SplitterWidth = 2;
			this.splitContainer1.TabIndex = 4;
			// 
			// toolStripPayrollExport
			// 
			this.toolStripPayrollExport.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripPayrollExport.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPayrollExport.Image = null;
			this.toolStripPayrollExport.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelSkillActions,
            this.toolStripMenuItemNew,
            this.toolStripSeparator3,
            this.toolStripMenuItemRunExport,
            this.toolStripSeparator1,
            this.toolStripMenuItemDelete,
            this.toolStripMenuItemProperties});
			this.toolStripPayrollExport.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripPayrollExport.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripPayrollExport.Location = new System.Drawing.Point(10, 0);
			this.toolStripPayrollExport.Name = "toolStripPayrollExport";
			this.toolStripPayrollExport.Office12Mode = false;
			this.toolStripPayrollExport.ShowCaption = false;
			this.toolStripPayrollExport.ShowLauncher = false;
			this.toolStripPayrollExport.Size = new System.Drawing.Size(208, 200);
			this.toolStripPayrollExport.TabIndex = 0;
			this.toolStripPayrollExport.Text = "toolStrip1";
			this.toolStripPayrollExport.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripLabelSkillActions
			// 
			this.toolStripLabelSkillActions.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.toolStripLabelSkillActions.Name = "toolStripLabelSkillActions";
			this.toolStripLabelSkillActions.Size = new System.Drawing.Size(206, 19);
			this.toolStripLabelSkillActions.Text = "xxActions";
			// 
			// toolStripMenuItemNew
			// 
			this.toolStripMenuItemNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripMenuItemNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemNew.Name = "toolStripMenuItemNew";
			this.toolStripMenuItemNew.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemNew.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemNew.Size = new System.Drawing.Size(206, 28);
			this.toolStripMenuItemNew.Text = "xxNewThreeDots";
			this.toolStripMenuItemNew.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemNew.Click += new System.EventHandler(this.toolStripMenuItemNew_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(206, 6);
			// 
			// toolStripMenuItemRunExport
			// 
			this.toolStripMenuItemRunExport.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Export2;
			this.toolStripMenuItemRunExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemRunExport.Name = "toolStripMenuItemRunExport";
			this.toolStripMenuItemRunExport.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemRunExport.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemRunExport.Size = new System.Drawing.Size(206, 28);
			this.toolStripMenuItemRunExport.Text = "xxRunExport";
			this.toolStripMenuItemRunExport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemRunExport.Click += new System.EventHandler(this.toolStripMenuItemRunExport_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(206, 6);
			// 
			// toolStripMenuItemDelete
			// 
			this.toolStripMenuItemDelete.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripMenuItemDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItemDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
			this.toolStripMenuItemDelete.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemDelete.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemDelete.Size = new System.Drawing.Size(206, 28);
			this.toolStripMenuItemDelete.Text = "xxDelete";
			this.toolStripMenuItemDelete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
			// 
			// toolStripMenuItemProperties
			// 
			this.toolStripMenuItemProperties.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemProperties.Image")));
			this.toolStripMenuItemProperties.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemProperties.Name = "toolStripMenuItemProperties";
			this.toolStripMenuItemProperties.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemProperties.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemProperties.Size = new System.Drawing.Size(206, 28);
			this.toolStripMenuItemProperties.Text = "xxPropertiesThreeDots";
			this.toolStripMenuItemProperties.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemProperties.Click += new System.EventHandler(this.toolStripMenuItemProperties_Click);
			// 
			// PayrollExportNavigator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "PayrollExportNavigator";
			this.Padding = new System.Windows.Forms.Padding(0);
			this.Size = new System.Drawing.Size(218, 556);
			this.contextMenuStripPayrollExport.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.toolStripPayrollExport.ResumeLayout(false);
			this.toolStripPayrollExport.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewMain;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewWorkload;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteWorkload;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWorkloadProperties;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWorkloadSkillNew;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem xxEditForecastThreeDotsToolStripMenuItem;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripPayrollExport;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSkillActions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNew;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPayrollExport;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNew2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemProperties;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemProperties2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRunExport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemContextRunExport;
    }
}
