using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Requests.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    partial class SchedulerSplitters
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
				if(ChartControlSkillData != null)
				{
					ChartControlSkillData.ChartRegionClick -= chartControlSkillDataChartRegionClick;
					ChartControlSkillData.ChartRegionMouseHover -= chartControlSkillDataChartRegionMouseHover;
				}

				if (components != null)
					components.Dispose();

				if (_contextMenuSkillGrid != null)
					_contextMenuSkillGrid.Dispose();
				if (_splitterManager != null)
					_splitterManager.Dispose();
				if (GridChartManager != null)
					GridChartManager.Dispose();

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
			Syncfusion.Windows.Forms.Chart.ChartSeries chartSeries1 = new Syncfusion.Windows.Forms.Chart.ChartSeries();
			Syncfusion.Windows.Forms.Chart.ChartCustomShapeInfo chartCustomShapeInfo1 = new Syncfusion.Windows.Forms.Chart.ChartCustomShapeInfo();
			Syncfusion.Windows.Forms.Chart.ChartLineInfo chartLineInfo1 = new Syncfusion.Windows.Forms.Chart.ChartLineInfo();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle1 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchedulerSplitters));
			this.lessIntellegentSplitContainerAdvMain = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
			this.lessIntellegentSplitContainerAdvResultGraph = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
			this.ChartControlSkillData = new Syncfusion.Windows.Forms.Chart.ChartControl();
			this.tabSkillData = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.PinnedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tabPageAdv1 = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
			this.SplitContainerView = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
			this.agentsNotPossibleToSchedule1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions.AgentsNotPossibleToSchedule();
			this.Grid = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.ElementHostRequests = new System.Windows.Forms.Integration.ElementHost();
			this.HandlePersonRequestView1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Requests.Views.HandlePersonRequestView();
			this.ElementHost1 = new System.Windows.Forms.Integration.ElementHost();
			this.MultipleHostControl3 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop.MultipleHostControl();
			this.tabInfoPanels = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageAdvAgentInfo = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabPageAdvShiftCategoryDistribution = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.shiftCategoryDistributionControl1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel.ShiftCategoryDistributionControl();
			this.tabPageAdvValidationAlerts = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.validationAlertsView1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ValidationAlertsView();
			this.lessIntellegentSplitContainerAdvMainContainer = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
			this.imageListSkillTypeIcons = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.lessIntellegentSplitContainerAdvMain)).BeginInit();
			this.lessIntellegentSplitContainerAdvMain.Panel1.SuspendLayout();
			this.lessIntellegentSplitContainerAdvMain.Panel2.SuspendLayout();
			this.lessIntellegentSplitContainerAdvMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lessIntellegentSplitContainerAdvResultGraph)).BeginInit();
			this.lessIntellegentSplitContainerAdvResultGraph.Panel1.SuspendLayout();
			this.lessIntellegentSplitContainerAdvResultGraph.Panel2.SuspendLayout();
			this.lessIntellegentSplitContainerAdvResultGraph.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tabSkillData)).BeginInit();
			this.tabSkillData.SuspendLayout();
			this.contextMenuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.teleoptiLessIntelligentSplitContainerLessIntelligent1)).BeginInit();
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1.SuspendLayout();
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2.SuspendLayout();
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerView)).BeginInit();
			this.SplitContainerView.Panel1.SuspendLayout();
			this.SplitContainerView.Panel2.SuspendLayout();
			this.SplitContainerView.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tabInfoPanels)).BeginInit();
			this.tabInfoPanels.SuspendLayout();
			this.tabPageAdvShiftCategoryDistribution.SuspendLayout();
			this.tabPageAdvValidationAlerts.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lessIntellegentSplitContainerAdvMainContainer)).BeginInit();
			this.lessIntellegentSplitContainerAdvMainContainer.Panel1.SuspendLayout();
			this.lessIntellegentSplitContainerAdvMainContainer.Panel2.SuspendLayout();
			this.lessIntellegentSplitContainerAdvMainContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// lessIntellegentSplitContainerAdvMain
			// 
			this.lessIntellegentSplitContainerAdvMain.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.lessIntellegentSplitContainerAdvMain.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.lessIntellegentSplitContainerAdvMain.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Silver);
			this.lessIntellegentSplitContainerAdvMain.BeforeTouchSize = 3;
			this.lessIntellegentSplitContainerAdvMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lessIntellegentSplitContainerAdvMain.Location = new System.Drawing.Point(0, 0);
			this.lessIntellegentSplitContainerAdvMain.Name = "lessIntellegentSplitContainerAdvMain";
			this.lessIntellegentSplitContainerAdvMain.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// lessIntellegentSplitContainerAdvMain.Panel1
			// 
			this.lessIntellegentSplitContainerAdvMain.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.lessIntellegentSplitContainerAdvMain.Panel1.Controls.Add(this.lessIntellegentSplitContainerAdvResultGraph);
			this.lessIntellegentSplitContainerAdvMain.Panel1MinSize = 32;
			// 
			// lessIntellegentSplitContainerAdvMain.Panel2
			// 
			this.lessIntellegentSplitContainerAdvMain.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.lessIntellegentSplitContainerAdvMain.Panel2.Controls.Add(this.teleoptiLessIntelligentSplitContainerLessIntelligent1);
			this.lessIntellegentSplitContainerAdvMain.Panel2MinSize = 32;
			this.lessIntellegentSplitContainerAdvMain.Size = new System.Drawing.Size(330, 672);
			this.lessIntellegentSplitContainerAdvMain.SplitterDistance = 252;
			this.lessIntellegentSplitContainerAdvMain.SplitterWidth = 3;
			this.lessIntellegentSplitContainerAdvMain.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.lessIntellegentSplitContainerAdvMain.TabIndex = 1;
			this.lessIntellegentSplitContainerAdvMain.Text = "teleoptiLessIntellegentSplitContainer1";
			// 
			// lessIntellegentSplitContainerAdvResultGraph
			// 
			this.lessIntellegentSplitContainerAdvResultGraph.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.lessIntellegentSplitContainerAdvResultGraph.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.lessIntellegentSplitContainerAdvResultGraph.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Silver);
			this.lessIntellegentSplitContainerAdvResultGraph.BeforeTouchSize = 3;
			this.lessIntellegentSplitContainerAdvResultGraph.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lessIntellegentSplitContainerAdvResultGraph.Location = new System.Drawing.Point(0, 0);
			this.lessIntellegentSplitContainerAdvResultGraph.Name = "lessIntellegentSplitContainerAdvResultGraph";
			this.lessIntellegentSplitContainerAdvResultGraph.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// lessIntellegentSplitContainerAdvResultGraph.Panel1
			// 
			this.lessIntellegentSplitContainerAdvResultGraph.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.lessIntellegentSplitContainerAdvResultGraph.Panel1.Controls.Add(this.ChartControlSkillData);
			this.lessIntellegentSplitContainerAdvResultGraph.Panel1MinSize = 0;
			// 
			// lessIntellegentSplitContainerAdvResultGraph.Panel2
			// 
			this.lessIntellegentSplitContainerAdvResultGraph.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.lessIntellegentSplitContainerAdvResultGraph.Panel2.Controls.Add(this.tabSkillData);
			this.lessIntellegentSplitContainerAdvResultGraph.Size = new System.Drawing.Size(330, 252);
			this.lessIntellegentSplitContainerAdvResultGraph.SplitterDistance = 127;
			this.lessIntellegentSplitContainerAdvResultGraph.SplitterWidth = 3;
			this.lessIntellegentSplitContainerAdvResultGraph.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.lessIntellegentSplitContainerAdvResultGraph.TabIndex = 0;
			this.lessIntellegentSplitContainerAdvResultGraph.Text = "teleoptiLessIntellegentSplitContainer1";
			// 
			// chartControlSkillData
			// 
			this.ChartControlSkillData.ChartArea.BackInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
			this.ChartControlSkillData.ChartArea.CursorLocation = new System.Drawing.Point(0, 0);
			this.ChartControlSkillData.ChartArea.CursorReDraw = false;
			this.ChartControlSkillData.DataSourceName = "";
			this.ChartControlSkillData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChartControlSkillData.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ChartControlSkillData.IsWindowLess = false;
			// 
			// 
			// 
			this.ChartControlSkillData.Legend.Location = new System.Drawing.Point(226, 75);
			this.ChartControlSkillData.Localize = null;
			this.ChartControlSkillData.Location = new System.Drawing.Point(0, 0);
			this.ChartControlSkillData.Name = "ChartControlSkillData";
			this.ChartControlSkillData.PrimaryXAxis.Crossing = double.NaN;
			this.ChartControlSkillData.PrimaryXAxis.Margin = true;
			this.ChartControlSkillData.PrimaryYAxis.Crossing = double.NaN;
			this.ChartControlSkillData.PrimaryYAxis.Margin = true;
			chartSeries1.FancyToolTip.ResizeInsideSymbol = true;
			chartSeries1.Name = "Default";
			chartSeries1.Points.Add(0D, ((double)(54D)), ((double)(319D)), ((double)(249D)), ((double)(127D)));
			chartSeries1.Points.Add(1D, ((double)(68D)), ((double)(305D)), ((double)(159D)), ((double)(222D)));
			chartSeries1.Points.Add(2D, ((double)(94D)), ((double)(194D)), ((double)(138D)), ((double)(128D)));
			chartSeries1.Points.Add(3D, ((double)(60D)), ((double)(113D)), ((double)(82D)), ((double)(110D)));
			chartSeries1.Points.Add(4D, ((double)(85D)), ((double)(94D)), ((double)(86D)), ((double)(88D)));
			chartSeries1.Resolution = 0D;
			chartSeries1.StackingGroup = "Default Group";
			chartSeries1.Style.AltTagFormat = "";
			chartSeries1.Style.Border.Width = 2F;
			chartSeries1.Style.DisplayShadow = true;
			chartSeries1.Style.DrawTextShape = false;
			chartSeries1.Style.Font.Facename = "Microsoft Sans Serif";
			chartLineInfo1.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
			chartLineInfo1.Color = System.Drawing.SystemColors.ControlText;
			chartLineInfo1.DashPattern = null;
			chartLineInfo1.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
			chartLineInfo1.Width = 1F;
			chartCustomShapeInfo1.Border = chartLineInfo1;
			chartCustomShapeInfo1.Color = System.Drawing.SystemColors.HighlightText;
			chartCustomShapeInfo1.Type = Syncfusion.Windows.Forms.Chart.ChartCustomShape.Square;
			chartSeries1.Style.TextShape = chartCustomShapeInfo1;
			chartSeries1.Text = "Default";
			chartSeries1.Type = Syncfusion.Windows.Forms.Chart.ChartSeriesType.Line;
			this.ChartControlSkillData.Series.Add(chartSeries1);
			this.ChartControlSkillData.Size = new System.Drawing.Size(330, 127);
			this.ChartControlSkillData.Skins = Syncfusion.Windows.Forms.Chart.Skins.Metro;
			this.ChartControlSkillData.TabIndex = 1;
			this.ChartControlSkillData.Text = "Skill";
			// 
			// 
			// 
			this.ChartControlSkillData.Title.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ChartControlSkillData.Title.Name = "Default";
			this.ChartControlSkillData.Titles.Add(this.ChartControlSkillData.Title);
			this.ChartControlSkillData.Visible = false;
			// 
			// tabSkillData
			// 
			this.tabSkillData.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabSkillData.BeforeTouchSize = new System.Drawing.Size(330, 122);
			this.tabSkillData.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabSkillData.ContextMenuStrip = this.contextMenuStrip1;
			this.tabSkillData.Controls.Add(this.tabPageAdv1);
			this.tabSkillData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabSkillData.KeepSelectedTabInFrontRow = false;
			this.tabSkillData.Location = new System.Drawing.Point(0, 0);
			this.tabSkillData.Name = "tabSkillData";
			this.tabSkillData.ShowScroll = false;
			this.tabSkillData.Size = new System.Drawing.Size(330, 122);
			this.tabSkillData.TabGap = 10;
			this.tabSkillData.TabIndex = 7;
			this.tabSkillData.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
			this.tabSkillData.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.FirstTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive0", ""));
			this.tabSkillData.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.PreviousTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive1", ""));
			this.tabSkillData.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.NextTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive2", ""));
			this.tabSkillData.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.LastTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive3", ""));
			this.tabSkillData.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.DropDown, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive4", ""));
			this.tabSkillData.TabPrimitivesHost.Visible = true;
			this.tabSkillData.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PinnedToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(170, 26);
			// 
			// PinnedToolStripMenuItem
			// 
			this.PinnedToolStripMenuItem.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.pinned;
			this.PinnedToolStripMenuItem.Name = "PinnedToolStripMenuItem";
			this.PinnedToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
			this.PinnedToolStripMenuItem.Text = "xxTogglePinStatus";
			this.PinnedToolStripMenuItem.Click += new System.EventHandler(this.pinnedToolStripMenuItemClick);
			// 
			// tabPageAdv1
			// 
			this.tabPageAdv1.Image = null;
			this.tabPageAdv1.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdv1.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdv1.Name = "tabPageAdv1";
			this.tabPageAdv1.ShowCloseButton = true;
			this.tabPageAdv1.Size = new System.Drawing.Size(330, 101);
			this.tabPageAdv1.TabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabPageAdv1.TabIndex = 1;
			this.tabPageAdv1.ThemesEnabled = false;
			// 
			// teleoptiLessIntelligentSplitContainerLessIntelligent1
			// 
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Silver);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.BeforeTouchSize = 3;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Location = new System.Drawing.Point(0, 0);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Name = "teleoptiLessIntelligentSplitContainerLessIntelligent1";
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1
			// 
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1.BackColor = System.Drawing.SystemColors.Control;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1.Controls.Add(this.SplitContainerView);
			// 
			// teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2
			// 
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2.Controls.Add(this.ElementHost1);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2MinSize = 100;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Size = new System.Drawing.Size(330, 417);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.SplitterDistance = 310;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.SplitterWidth = 3;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.TabIndex = 2;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Text = "teleoptiLessIntellegentSplitContainer1";
			// 
			// teleoptiLessIntellegentSplitContainerView
			// 
			this.SplitContainerView.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.SplitContainerView.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.SplitContainerView.BackColor = System.Drawing.SystemColors.Control;
			this.SplitContainerView.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Silver);
			this.SplitContainerView.BeforeTouchSize = 3;
			this.SplitContainerView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainerView.Location = new System.Drawing.Point(0, 0);
			this.SplitContainerView.Name = "SplitContainerView";
			this.SplitContainerView.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// teleoptiLessIntellegentSplitContainerView.Panel1
			// 
			this.SplitContainerView.Panel1.BackColor = System.Drawing.SystemColors.Control;
			this.SplitContainerView.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.SplitContainerView.Panel1.Controls.Add(this.agentsNotPossibleToSchedule1);
			this.SplitContainerView.Panel1MinSize = 140;
			// 
			// teleoptiLessIntellegentSplitContainerView.Panel2
			// 
			this.SplitContainerView.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.SplitContainerView.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.SplitContainerView.Panel2.Controls.Add(this.Grid);
			this.SplitContainerView.Panel2.Controls.Add(this.ElementHostRequests);
			this.SplitContainerView.Panel2.MinimumSize = new System.Drawing.Size(30, 0);
			this.SplitContainerView.Panel2MinSize = 0;
			this.SplitContainerView.Size = new System.Drawing.Size(330, 310);
			this.SplitContainerView.SplitterDistance = 140;
			this.SplitContainerView.SplitterWidth = 3;
			this.SplitContainerView.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.SplitContainerView.TabIndex = 0;
			this.SplitContainerView.Text = "teleoptiLessIntellegentSplitContainer1";
			// 
			// agentsNotPossibleToSchedule1
			// 
			this.agentsNotPossibleToSchedule1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.agentsNotPossibleToSchedule1.Location = new System.Drawing.Point(0, 0);
			this.agentsNotPossibleToSchedule1.Name = "agentsNotPossibleToSchedule1";
			this.agentsNotPossibleToSchedule1.Size = new System.Drawing.Size(330, 140);
			this.agentsNotPossibleToSchedule1.TabIndex = 2;
			// 
			// grid
			// 
			gridBaseStyle1.Name = "Header";
			gridBaseStyle1.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.CellType = "Header";
			gridBaseStyle1.StyleInfo.Font.Bold = true;
			gridBaseStyle1.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle1.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle2.Name = "Standard";
			gridBaseStyle2.StyleInfo.Font.Facename = "Tahoma";
			gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle2.StyleInfo.TextAlign = Syncfusion.Windows.Forms.Grid.GridTextAlign.Default;
			gridBaseStyle3.Name = "Column Header";
			gridBaseStyle3.StyleInfo.BaseStyle = "Header";
			gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle3.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle4.Name = "Row Header";
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle4.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.Grid.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.Grid.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.ExcelLikeCurrentCell = true;
			this.Grid.ExcelLikeSelectionFrame = true;
			this.Grid.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2003;
			this.Grid.Location = new System.Drawing.Point(0, 0);
			this.Grid.MinResizeColSize = 5;
			this.Grid.Name = "Grid";
			this.Grid.NumberedColHeaders = false;
			this.Grid.NumberedRowHeaders = false;
			gridRangeStyle1.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cells(1, 1, 10, 2);
			gridRangeStyle1.StyleInfo.BaseStyle = "Standard";
			this.Grid.RangeStyles.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeStyle[] {
            gridRangeStyle1});
			this.Grid.ReadOnly = true;
			this.Grid.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Grid.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.Grid.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.Grid.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.Grid.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.AlwaysVisible;
			this.Grid.Size = new System.Drawing.Size(330, 167);
			this.Grid.SmartSizeBox = false;
			this.Grid.TabIndex = 3;
			this.Grid.Text = "xxPeriodViewPROTOTYPE";
			this.Grid.ThemesEnabled = true;
			this.Grid.UseRightToLeftCompatibleTextBox = true;
			this.Grid.Resize += new System.EventHandler(this.gridResize);
			// 
			// elementHostRequests
			// 
			this.ElementHostRequests.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ElementHostRequests.Location = new System.Drawing.Point(0, 0);
			this.ElementHostRequests.Name = "ElementHostRequests";
			this.ElementHostRequests.Size = new System.Drawing.Size(330, 167);
			this.ElementHostRequests.TabIndex = 5;
			this.ElementHostRequests.Child = this.HandlePersonRequestView1;
			// 
			// elementHost1
			// 
			this.ElementHost1.AutoSize = true;
			this.ElementHost1.BackColor = System.Drawing.Color.Transparent;
			this.ElementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ElementHost1.Location = new System.Drawing.Point(0, 0);
			this.ElementHost1.Name = "ElementHost1";
			this.ElementHost1.Size = new System.Drawing.Size(330, 104);
			this.ElementHost1.TabIndex = 1;
			this.ElementHost1.Text = "elementHost1";
			this.ElementHost1.Child = this.MultipleHostControl3;
			// 
			// tabInfoPanels
			// 
			this.tabInfoPanels.ActiveTabColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.tabInfoPanels.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabInfoPanels.BeforeTouchSize = new System.Drawing.Size(434, 672);
			this.tabInfoPanels.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabInfoPanels.Controls.Add(this.tabPageAdvAgentInfo);
			this.tabInfoPanels.Controls.Add(this.tabPageAdvShiftCategoryDistribution);
			this.tabInfoPanels.Controls.Add(this.tabPageAdvValidationAlerts);
			this.tabInfoPanels.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabInfoPanels.FixedSingleBorderColor = System.Drawing.Color.White;
			this.tabInfoPanels.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabInfoPanels.InactiveTabColor = System.Drawing.Color.White;
			this.tabInfoPanels.KeepSelectedTabInFrontRow = false;
			this.tabInfoPanels.Location = new System.Drawing.Point(0, 0);
			this.tabInfoPanels.Name = "tabInfoPanels";
			this.tabInfoPanels.Size = new System.Drawing.Size(434, 672);
			this.tabInfoPanels.TabIndex = 12;
			this.tabInfoPanels.TabPanelBackColor = System.Drawing.Color.White;
			this.tabInfoPanels.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			this.tabInfoPanels.SelectedIndexChanged += new System.EventHandler(this.tabInfoPanelsSelectedIndexChanged);
			// 
			// tabPageAdvAgentInfo
			// 
			this.tabPageAdvAgentInfo.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tabPageAdvAgentInfo.Image = null;
			this.tabPageAdvAgentInfo.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvAgentInfo.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdvAgentInfo.Name = "tabPageAdvAgentInfo";
			this.tabPageAdvAgentInfo.ShowCloseButton = true;
			this.tabPageAdvAgentInfo.Size = new System.Drawing.Size(434, 651);
			this.tabPageAdvAgentInfo.TabFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabPageAdvAgentInfo.TabIndex = 11;
			this.tabPageAdvAgentInfo.Text = "xxAgentInfo";
			this.tabPageAdvAgentInfo.ThemesEnabled = false;
			// 
			// tabPageAdvShiftCategoryDistribution
			// 
			this.tabPageAdvShiftCategoryDistribution.Controls.Add(this.shiftCategoryDistributionControl1);
			this.tabPageAdvShiftCategoryDistribution.Image = null;
			this.tabPageAdvShiftCategoryDistribution.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvShiftCategoryDistribution.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdvShiftCategoryDistribution.Name = "tabPageAdvShiftCategoryDistribution";
			this.tabPageAdvShiftCategoryDistribution.ShowCloseButton = true;
			this.tabPageAdvShiftCategoryDistribution.Size = new System.Drawing.Size(434, 651);
			this.tabPageAdvShiftCategoryDistribution.TabFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
			this.tabPageAdvShiftCategoryDistribution.TabIndex = 12;
			this.tabPageAdvShiftCategoryDistribution.Text = "xxShiftCategoryDistribution";
			this.tabPageAdvShiftCategoryDistribution.ThemesEnabled = false;
			// 
			// shiftCategoryDistributionControl1
			// 
			this.shiftCategoryDistributionControl1.BackColor = System.Drawing.Color.White;
			this.shiftCategoryDistributionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shiftCategoryDistributionControl1.Location = new System.Drawing.Point(0, 0);
			this.shiftCategoryDistributionControl1.Name = "shiftCategoryDistributionControl1";
			this.shiftCategoryDistributionControl1.Size = new System.Drawing.Size(434, 651);
			this.shiftCategoryDistributionControl1.TabIndex = 0;
			// 
			// tabPageAdvValidationAlerts
			// 
			this.tabPageAdvValidationAlerts.Controls.Add(this.validationAlertsView1);
			this.tabPageAdvValidationAlerts.Image = null;
			this.tabPageAdvValidationAlerts.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvValidationAlerts.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdvValidationAlerts.Name = "tabPageAdvValidationAlerts";
			this.tabPageAdvValidationAlerts.ShowCloseButton = true;
			this.tabPageAdvValidationAlerts.Size = new System.Drawing.Size(434, 651);
			this.tabPageAdvValidationAlerts.TabFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
			this.tabPageAdvValidationAlerts.TabIndex = 13;
			this.tabPageAdvValidationAlerts.Text = "xxValidationAlerts";
			this.tabPageAdvValidationAlerts.ThemesEnabled = false;
			// 
			// validationAlertsView1
			// 
			this.validationAlertsView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.validationAlertsView1.Location = new System.Drawing.Point(0, 0);
			this.validationAlertsView1.Name = "validationAlertsView1";
			this.validationAlertsView1.Size = new System.Drawing.Size(434, 651);
			this.validationAlertsView1.TabIndex = 0;
			// 
			// lessIntellegentSplitContainerAdvMainContainer
			// 
			this.lessIntellegentSplitContainerAdvMainContainer.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.lessIntellegentSplitContainerAdvMainContainer.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.lessIntellegentSplitContainerAdvMainContainer.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Silver);
			this.lessIntellegentSplitContainerAdvMainContainer.BeforeTouchSize = 3;
			this.lessIntellegentSplitContainerAdvMainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lessIntellegentSplitContainerAdvMainContainer.FixedPanel = Syncfusion.Windows.Forms.Tools.Enums.FixedPanel.Panel2;
			this.lessIntellegentSplitContainerAdvMainContainer.Location = new System.Drawing.Point(5, 0);
			this.lessIntellegentSplitContainerAdvMainContainer.Name = "lessIntellegentSplitContainerAdvMainContainer";
			// 
			// lessIntellegentSplitContainerAdvMainContainer.Panel1
			// 
			this.lessIntellegentSplitContainerAdvMainContainer.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.lessIntellegentSplitContainerAdvMainContainer.Panel1.Controls.Add(this.lessIntellegentSplitContainerAdvMain);
			this.lessIntellegentSplitContainerAdvMainContainer.Panel1MinSize = 32;
			// 
			// lessIntellegentSplitContainerAdvMainContainer.Panel2
			// 
			this.lessIntellegentSplitContainerAdvMainContainer.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.lessIntellegentSplitContainerAdvMainContainer.Panel2.Controls.Add(this.tabInfoPanels);
			this.lessIntellegentSplitContainerAdvMainContainer.Panel2MinSize = 32;
			this.lessIntellegentSplitContainerAdvMainContainer.Size = new System.Drawing.Size(767, 672);
			this.lessIntellegentSplitContainerAdvMainContainer.SplitterDistance = 330;
			this.lessIntellegentSplitContainerAdvMainContainer.SplitterWidth = 3;
			this.lessIntellegentSplitContainerAdvMainContainer.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.lessIntellegentSplitContainerAdvMainContainer.TabIndex = 10;
			this.lessIntellegentSplitContainerAdvMainContainer.Text = "lessIntellegentSplitContainerAdvMainContainer";
			this.lessIntellegentSplitContainerAdvMainContainer.Visible = false;
			// 
			// imageListSkillTypeIcons
			// 
			this.imageListSkillTypeIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSkillTypeIcons.ImageStream")));
			this.imageListSkillTypeIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListSkillTypeIcons.Images.SetKeyName(0, "ccc_SkillEmail_16x16.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(1, "ccc_Skill_Fax_16x16.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(2, "ccc_SkillTelephone_16x16.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(3, "ccc_Skill_Backoffice_16x16.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(4, "ccc_PeopleScehdulePeriodView.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(5, "desktop.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(6, "skill_retail.png");
			// 
			// SchedulerSplitters
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.lessIntellegentSplitContainerAdvMainContainer);
			this.Name = "SchedulerSplitters";
			this.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.Size = new System.Drawing.Size(772, 672);
			this.lessIntellegentSplitContainerAdvMain.Panel1.ResumeLayout(false);
			this.lessIntellegentSplitContainerAdvMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.lessIntellegentSplitContainerAdvMain)).EndInit();
			this.lessIntellegentSplitContainerAdvMain.ResumeLayout(false);
			this.lessIntellegentSplitContainerAdvResultGraph.Panel1.ResumeLayout(false);
			this.lessIntellegentSplitContainerAdvResultGraph.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.lessIntellegentSplitContainerAdvResultGraph)).EndInit();
			this.lessIntellegentSplitContainerAdvResultGraph.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tabSkillData)).EndInit();
			this.tabSkillData.ResumeLayout(false);
			this.contextMenuStrip1.ResumeLayout(false);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1.ResumeLayout(false);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2.ResumeLayout(false);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.teleoptiLessIntelligentSplitContainerLessIntelligent1)).EndInit();
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.ResumeLayout(false);
			this.SplitContainerView.Panel1.ResumeLayout(false);
			this.SplitContainerView.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerView)).EndInit();
			this.SplitContainerView.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tabInfoPanels)).EndInit();
			this.tabInfoPanels.ResumeLayout(false);
			this.tabPageAdvShiftCategoryDistribution.ResumeLayout(false);
			this.tabPageAdvValidationAlerts.ResumeLayout(false);
			this.lessIntellegentSplitContainerAdvMainContainer.Panel1.ResumeLayout(false);
			this.lessIntellegentSplitContainerAdvMainContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.lessIntellegentSplitContainerAdvMainContainer)).EndInit();
			this.lessIntellegentSplitContainerAdvMainContainer.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private TeleoptiLessIntelligentSplitContainer lessIntellegentSplitContainerAdvMain;
        private TeleoptiLessIntelligentSplitContainer lessIntellegentSplitContainerAdvMainContainer;
        private TeleoptiLessIntelligentSplitContainer lessIntellegentSplitContainerAdvResultGraph;
		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabSkillData;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabInfoPanels;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdv1;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvAgentInfo;
        private TeleoptiLessIntelligentSplitContainer teleoptiLessIntelligentSplitContainerLessIntelligent1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem PinnedToolStripMenuItem;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvShiftCategoryDistribution;
		private PropertyPanel.ShiftCategoryDistributionControl shiftCategoryDistributionControl1;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvValidationAlerts;
		private ValidationAlertsView validationAlertsView1;
		private AgentsNotPossibleToSchedule agentsNotPossibleToSchedule1;
		private System.Windows.Forms.ImageList imageListSkillTypeIcons;
	}
}
