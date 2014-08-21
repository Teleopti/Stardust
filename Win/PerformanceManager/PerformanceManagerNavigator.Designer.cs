namespace Teleopti.Ccc.Win.PerformanceManager
{
    partial class PerformanceManagerNavigator
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformanceManagerNavigator));
			this.treeViewReports = new System.Windows.Forms.TreeView();
			this.imageListPlanningGroup = new System.Windows.Forms.ImageList(this.components);
			this.toolStripMenuItemNewReport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripLabelReportActions = new System.Windows.Forms.ToolStripLabel();
			this.toolStripLabelReports = new System.Windows.Forms.ToolStripLabel();
			this.toolStripRoot = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripDropDownViewReports = new System.Windows.Forms.ToolStripDropDownButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripDropDownNewReport = new System.Windows.Forms.ToolStripDropDownButton();
			this.toolStripReportGroups = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.toolStripRoot.SuspendLayout();
			this.toolStripReportGroups.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// treeViewReports
			// 
			this.treeViewReports.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeViewReports.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewReports.ImageIndex = 1;
			this.treeViewReports.ImageList = this.imageListPlanningGroup;
			this.treeViewReports.ItemHeight = 18;
			this.treeViewReports.Location = new System.Drawing.Point(0, 0);
			this.treeViewReports.Name = "treeViewReports";
			this.treeViewReports.SelectedImageIndex = 1;
			this.treeViewReports.Size = new System.Drawing.Size(232, 307);
			this.treeViewReports.TabIndex = 1;
			// 
			// imageListPlanningGroup
			// 
			this.imageListPlanningGroup.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListPlanningGroup.ImageStream")));
			this.imageListPlanningGroup.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListPlanningGroup.Images.SetKeyName(0, "ccc_SkillGeneral.png");
			this.imageListPlanningGroup.Images.SetKeyName(1, "ccc_result_view_16x16.png");
			this.imageListPlanningGroup.Images.SetKeyName(2, "ccc_Site_16x16.png");
			// 
			// toolStripMenuItemNewReport
			// 
			this.toolStripMenuItemNewReport.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripMenuItemNewReport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemNewReport.Name = "toolStripMenuItemNewReport";
			this.toolStripMenuItemNewReport.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemNewReport.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemNewReport.Size = new System.Drawing.Size(219, 28);
			this.toolStripMenuItemNewReport.Text = "xxNewPlanningGroupThreeDots";
			this.toolStripMenuItemNewReport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripLabelReportActions
			// 
			this.toolStripLabelReportActions.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.toolStripLabelReportActions.Name = "toolStripLabelReportActions";
			this.toolStripLabelReportActions.Size = new System.Drawing.Size(219, 19);
			this.toolStripLabelReportActions.Text = "xxActions";
			// 
			// toolStripLabelReports
			// 
			this.toolStripLabelReports.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.toolStripLabelReports.Name = "toolStripLabelReports";
			this.toolStripLabelReports.Size = new System.Drawing.Size(219, 19);
			this.toolStripLabelReports.Text = "xxActions";
			// 
			// toolStripRoot
			// 
			this.toolStripRoot.BackColor = System.Drawing.Color.Transparent;
			this.toolStripRoot.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripRoot.Image = null;
			this.toolStripRoot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelReports,
            this.toolStripDropDownViewReports,
            this.toolStripSeparator1,
            this.toolStripDropDownNewReport});
			this.toolStripRoot.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripRoot.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripRoot.Location = new System.Drawing.Point(10, 0);
			this.toolStripRoot.Name = "toolStripRoot";
			this.toolStripRoot.Office12Mode = false;
			this.toolStripRoot.Padding = new System.Windows.Forms.Padding(1);
			this.toolStripRoot.ShowCaption = false;
			this.toolStripRoot.ShowLauncher = false;
			this.toolStripRoot.Size = new System.Drawing.Size(205, 103);
			this.toolStripRoot.TabIndex = 5;
			this.toolStripRoot.Text = "xxActions";
			this.toolStripRoot.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripDropDownViewReports
			// 
			this.toolStripDropDownViewReports.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Reports_variant_32x32;
			this.toolStripDropDownViewReports.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripDropDownViewReports.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripDropDownViewReports.Name = "toolStripDropDownViewReports";
			this.toolStripDropDownViewReports.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripDropDownViewReports.ShowDropDownArrow = false;
			this.toolStripDropDownViewReports.Size = new System.Drawing.Size(219, 28);
			this.toolStripDropDownViewReports.Text = "xxViewReports";
			this.toolStripDropDownViewReports.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripDropDownViewReports.Click += new System.EventHandler(this.toolStripNewReport_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(219, 6);
			// 
			// toolStripDropDownNewReport
			// 
			this.toolStripDropDownNewReport.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripDropDownNewReport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripDropDownNewReport.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripDropDownNewReport.Name = "toolStripDropDownNewReport";
			this.toolStripDropDownNewReport.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripDropDownNewReport.ShowDropDownArrow = false;
			this.toolStripDropDownNewReport.Size = new System.Drawing.Size(219, 28);
			this.toolStripDropDownNewReport.Text = "xxNewReport";
			this.toolStripDropDownNewReport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripDropDownNewReport.ToolTipText = "xxNewReport";
			this.toolStripDropDownNewReport.Click += new System.EventHandler(this.toolStripNewReport_Click);
			// 
			// toolStripReportGroups
			// 
			this.toolStripReportGroups.BackColor = System.Drawing.Color.Transparent;
			this.toolStripReportGroups.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripReportGroups.Image = null;
			this.toolStripReportGroups.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelReportActions,
            this.toolStripMenuItemNewReport});
			this.toolStripReportGroups.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripReportGroups.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripReportGroups.Location = new System.Drawing.Point(10, 103);
			this.toolStripReportGroups.Name = "toolStripReportGroups";
			this.toolStripReportGroups.Office12Mode = false;
			this.toolStripReportGroups.Padding = new System.Windows.Forms.Padding(1);
			this.toolStripReportGroups.ShowCaption = false;
			this.toolStripReportGroups.ShowLauncher = false;
			this.toolStripReportGroups.Size = new System.Drawing.Size(205, 82);
			this.toolStripReportGroups.TabIndex = 5;
			this.toolStripReportGroups.Text = "xxActions";
			this.toolStripReportGroups.Visible = false;
			this.toolStripReportGroups.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// splitContainer1
			// 
			this.splitContainer1.BackColor = System.Drawing.Color.Silver;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeViewReports);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.AutoScroll = true;
			this.splitContainer1.Panel2.BackColor = System.Drawing.Color.White;
			this.splitContainer1.Panel2.Controls.Add(this.toolStripReportGroups);
			this.splitContainer1.Panel2.Controls.Add(this.toolStripRoot);
			this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.splitContainer1.Size = new System.Drawing.Size(232, 471);
			this.splitContainer1.SplitterDistance = 307;
			this.splitContainer1.SplitterWidth = 2;
			this.splitContainer1.TabIndex = 5;
			// 
			// PerformanceManagerNavigator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "PerformanceManagerNavigator";
			this.Padding = new System.Windows.Forms.Padding(0);
			this.Size = new System.Drawing.Size(232, 471);
			this.toolStripRoot.ResumeLayout(false);
			this.toolStripRoot.PerformLayout();
			this.toolStripReportGroups.ResumeLayout(false);
			this.toolStripReportGroups.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewReports;
        private System.Windows.Forms.ImageList imageListPlanningGroup;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewReport;
        private System.Windows.Forms.ToolStripLabel toolStripLabelReportActions;
        private System.Windows.Forms.ToolStripLabel toolStripLabelReports;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripRoot;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownNewReport;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripReportGroups;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownViewReports;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

    }
}
