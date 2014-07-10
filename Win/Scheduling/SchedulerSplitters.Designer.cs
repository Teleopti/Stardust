using Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction;

namespace Teleopti.Ccc.Win.Scheduling
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
				//if (restrictionSummaryGrid1 != null)
				//{
				//    restrictionSummaryGrid1.Dispose();
				//    restrictionSummaryGrid1 = null;
				//}

				if(agentRestrictionGrid1 != null)
				{
					agentRestrictionGrid1.Dispose();
					agentRestrictionGrid1 = null;
				}

                if (components != null)
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
			Syncfusion.Windows.Forms.Chart.ChartSeries chartSeries1 = new Syncfusion.Windows.Forms.Chart.ChartSeries();
			Syncfusion.Windows.Forms.Chart.ChartCustomShapeInfo chartCustomShapeInfo1 = new Syncfusion.Windows.Forms.Chart.ChartCustomShapeInfo();
			Syncfusion.Windows.Forms.Chart.ChartLineInfo chartLineInfo1 = new Syncfusion.Windows.Forms.Chart.ChartLineInfo();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle1 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			this.lessIntellegentSplitContainerAdvMain = new Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
			this.lessIntellegentSplitContainerAdvResultGraph = new Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
			this.chartControlSkillData = new Syncfusion.Windows.Forms.Chart.ChartControl();
			this.tabSkillData = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.PinnedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tabPageAdv1 = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1 = new Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
			this.teleoptiLessIntellegentSplitContainerView = new Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
			this.tableLayoutPanelRestrictionSummery = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelRestrictionButtons = new System.Windows.Forms.TableLayoutPanel();
			this.chbAvailability = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.chbRotations = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.chbPreferences = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.chbStudenAvailability = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.chbSchedules = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.agentRestrictionGrid1 = new Teleopti.Ccc.Win.Scheduling.AgentRestrictions.AgentRestrictionGrid(this.components);
			this.grid = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.elementHostRequests = new System.Windows.Forms.Integration.ElementHost();
			this.handlePersonRequestView1 = new Teleopti.Ccc.WpfControls.Controls.Requests.Views.HandlePersonRequestView();
			this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
			this.multipleHostControl1 = new Teleopti.Ccc.WpfControls.Common.Interop.MultipleHostControl();
			this.tabInfoPanels = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageAdvAgentInfo = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabPageAdvShiftCategoryDistribution = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.shiftCategoryDistributionControl1 = new Teleopti.Ccc.Win.Scheduling.PropertyPanel.ShiftCategoryDistributionControl();
			this.lessIntellegentSplitContainerAdvMainContainer = new Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer();
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
			((System.ComponentModel.ISupportInitialize)(this.teleoptiLessIntellegentSplitContainerView)).BeginInit();
			this.teleoptiLessIntellegentSplitContainerView.Panel1.SuspendLayout();
			this.teleoptiLessIntellegentSplitContainerView.Panel2.SuspendLayout();
			this.teleoptiLessIntellegentSplitContainerView.SuspendLayout();
			this.tableLayoutPanelRestrictionSummery.SuspendLayout();
			this.tableLayoutPanelRestrictionButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.chbAvailability)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.chbRotations)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.chbPreferences)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.chbStudenAvailability)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.chbSchedules)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.agentRestrictionGrid1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tabInfoPanels)).BeginInit();
			this.tabInfoPanels.SuspendLayout();
			this.tabPageAdvShiftCategoryDistribution.SuspendLayout();
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
			this.lessIntellegentSplitContainerAdvMain.BeforeTouchSize = 1;
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
			this.lessIntellegentSplitContainerAdvMain.Size = new System.Drawing.Size(335, 672);
			this.lessIntellegentSplitContainerAdvMain.SplitterDistance = 252;
			this.lessIntellegentSplitContainerAdvMain.SplitterWidth = 1;
			this.lessIntellegentSplitContainerAdvMain.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.lessIntellegentSplitContainerAdvMain.TabIndex = 1;
			this.lessIntellegentSplitContainerAdvMain.Text = "teleoptiLessIntellegentSplitContainer1";
			// 
			// lessIntellegentSplitContainerAdvResultGraph
			// 
			this.lessIntellegentSplitContainerAdvResultGraph.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.lessIntellegentSplitContainerAdvResultGraph.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.lessIntellegentSplitContainerAdvResultGraph.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Silver);
			this.lessIntellegentSplitContainerAdvResultGraph.BeforeTouchSize = 1;
			this.lessIntellegentSplitContainerAdvResultGraph.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lessIntellegentSplitContainerAdvResultGraph.Location = new System.Drawing.Point(0, 0);
			this.lessIntellegentSplitContainerAdvResultGraph.Name = "lessIntellegentSplitContainerAdvResultGraph";
			this.lessIntellegentSplitContainerAdvResultGraph.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// lessIntellegentSplitContainerAdvResultGraph.Panel1
			// 
			this.lessIntellegentSplitContainerAdvResultGraph.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.lessIntellegentSplitContainerAdvResultGraph.Panel1.Controls.Add(this.chartControlSkillData);
			this.lessIntellegentSplitContainerAdvResultGraph.Panel1MinSize = 0;
			// 
			// lessIntellegentSplitContainerAdvResultGraph.Panel2
			// 
			this.lessIntellegentSplitContainerAdvResultGraph.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.lessIntellegentSplitContainerAdvResultGraph.Panel2.Controls.Add(this.tabSkillData);
			this.lessIntellegentSplitContainerAdvResultGraph.Size = new System.Drawing.Size(335, 252);
			this.lessIntellegentSplitContainerAdvResultGraph.SplitterDistance = 127;
			this.lessIntellegentSplitContainerAdvResultGraph.SplitterWidth = 1;
			this.lessIntellegentSplitContainerAdvResultGraph.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.lessIntellegentSplitContainerAdvResultGraph.TabIndex = 0;
			this.lessIntellegentSplitContainerAdvResultGraph.Text = "teleoptiLessIntellegentSplitContainer1";
			// 
			// chartControlSkillData
			// 
			this.chartControlSkillData.ChartArea.BackInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
			this.chartControlSkillData.ChartArea.CursorLocation = new System.Drawing.Point(0, 0);
			this.chartControlSkillData.ChartArea.CursorReDraw = false;
			this.chartControlSkillData.DataSourceName = "";
			this.chartControlSkillData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartControlSkillData.ForeColor = System.Drawing.SystemColors.ControlText;
			this.chartControlSkillData.IsWindowLess = false;
			// 
			// 
			// 
			this.chartControlSkillData.Legend.Location = new System.Drawing.Point(231, 75);
			this.chartControlSkillData.Localize = null;
			this.chartControlSkillData.Location = new System.Drawing.Point(0, 0);
			this.chartControlSkillData.Name = "chartControlSkillData";
			this.chartControlSkillData.PrimaryXAxis.Crossing = double.NaN;
			this.chartControlSkillData.PrimaryXAxis.Margin = true;
			this.chartControlSkillData.PrimaryYAxis.Crossing = double.NaN;
			this.chartControlSkillData.PrimaryYAxis.ForceZero = true;
			this.chartControlSkillData.PrimaryYAxis.Margin = true;
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
			this.chartControlSkillData.Series.Add(chartSeries1);
			this.chartControlSkillData.Size = new System.Drawing.Size(335, 127);
			this.chartControlSkillData.TabIndex = 1;
			this.chartControlSkillData.Text = "Skill";
			// 
			// 
			// 
			this.chartControlSkillData.Title.ForeColor = System.Drawing.SystemColors.ControlText;
			this.chartControlSkillData.Title.Name = "Default";
			this.chartControlSkillData.Titles.Add(this.chartControlSkillData.Title);
			this.chartControlSkillData.Visible = false;
			// 
			// tabSkillData
			// 
			this.tabSkillData.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabSkillData.BeforeTouchSize = new System.Drawing.Size(335, 124);
			this.tabSkillData.ContextMenuStrip = this.contextMenuStrip1;
			this.tabSkillData.Controls.Add(this.tabPageAdv1);
			this.tabSkillData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabSkillData.KeepSelectedTabInFrontRow = false;
			this.tabSkillData.Location = new System.Drawing.Point(0, 0);
			this.tabSkillData.Name = "tabSkillData";
			this.tabSkillData.Size = new System.Drawing.Size(335, 124);
			this.tabSkillData.TabGap = 10;
			this.tabSkillData.TabIndex = 7;
			this.tabSkillData.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
			this.tabSkillData.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PinnedToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(171, 26);
			// 
			// PinnedToolStripMenuItem
			// 
			this.PinnedToolStripMenuItem.Image = global::Teleopti.Ccc.Win.Properties.Resources.pinned;
			this.PinnedToolStripMenuItem.Name = "PinnedToolStripMenuItem";
			this.PinnedToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
			this.PinnedToolStripMenuItem.Text = "xxTogglePinStatus";
			this.PinnedToolStripMenuItem.Click += new System.EventHandler(this.PinnedToolStripMenuItemClick);
			// 
			// tabPageAdv1
			// 
			this.tabPageAdv1.Image = null;
			this.tabPageAdv1.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdv1.Location = new System.Drawing.Point(1, 22);
			this.tabPageAdv1.Name = "tabPageAdv1";
			this.tabPageAdv1.ShowCloseButton = true;
			this.tabPageAdv1.Size = new System.Drawing.Size(332, 100);
			this.tabPageAdv1.TabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabPageAdv1.TabIndex = 1;
			this.tabPageAdv1.ThemesEnabled = false;
			// 
			// teleoptiLessIntelligentSplitContainerLessIntelligent1
			// 
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Silver);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.BeforeTouchSize = 1;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Location = new System.Drawing.Point(0, 0);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Name = "teleoptiLessIntelligentSplitContainerLessIntelligent1";
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1
			// 
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1.BackColor = System.Drawing.SystemColors.Control;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel1.Controls.Add(this.teleoptiLessIntellegentSplitContainerView);
			// 
			// teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2
			// 
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2.Controls.Add(this.elementHost1);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Panel2MinSize = 100;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Size = new System.Drawing.Size(335, 419);
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.SplitterDistance = 312;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.SplitterWidth = 1;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.TabIndex = 2;
			this.teleoptiLessIntelligentSplitContainerLessIntelligent1.Text = "teleoptiLessIntellegentSplitContainer1";
			// 
			// teleoptiLessIntellegentSplitContainerView
			// 
			this.teleoptiLessIntellegentSplitContainerView.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.teleoptiLessIntellegentSplitContainerView.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.teleoptiLessIntellegentSplitContainerView.BackColor = System.Drawing.SystemColors.Control;
			this.teleoptiLessIntellegentSplitContainerView.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Silver);
			this.teleoptiLessIntellegentSplitContainerView.BeforeTouchSize = 1;
			this.teleoptiLessIntellegentSplitContainerView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.teleoptiLessIntellegentSplitContainerView.Location = new System.Drawing.Point(0, 0);
			this.teleoptiLessIntellegentSplitContainerView.Name = "teleoptiLessIntellegentSplitContainerView";
			this.teleoptiLessIntellegentSplitContainerView.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// teleoptiLessIntellegentSplitContainerView.Panel1
			// 
			this.teleoptiLessIntellegentSplitContainerView.Panel1.BackColor = System.Drawing.SystemColors.Control;
			this.teleoptiLessIntellegentSplitContainerView.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.teleoptiLessIntellegentSplitContainerView.Panel1.Controls.Add(this.tableLayoutPanelRestrictionSummery);
			this.teleoptiLessIntellegentSplitContainerView.Panel1MinSize = 0;
			// 
			// teleoptiLessIntellegentSplitContainerView.Panel2
			// 
			this.teleoptiLessIntellegentSplitContainerView.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.teleoptiLessIntellegentSplitContainerView.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.teleoptiLessIntellegentSplitContainerView.Panel2.Controls.Add(this.grid);
			this.teleoptiLessIntellegentSplitContainerView.Panel2.Controls.Add(this.elementHostRequests);
			this.teleoptiLessIntellegentSplitContainerView.Panel2.MinimumSize = new System.Drawing.Size(30, 0);
			this.teleoptiLessIntellegentSplitContainerView.Panel2MinSize = 0;
			this.teleoptiLessIntellegentSplitContainerView.Size = new System.Drawing.Size(335, 312);
			this.teleoptiLessIntellegentSplitContainerView.SplitterDistance = 45;
			this.teleoptiLessIntellegentSplitContainerView.SplitterWidth = 1;
			this.teleoptiLessIntellegentSplitContainerView.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.teleoptiLessIntellegentSplitContainerView.TabIndex = 0;
			this.teleoptiLessIntellegentSplitContainerView.Text = "teleoptiLessIntellegentSplitContainer1";
			// 
			// tableLayoutPanelRestrictionSummery
			// 
			this.tableLayoutPanelRestrictionSummery.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(232)))), ((int)(((byte)(251)))));
			this.tableLayoutPanelRestrictionSummery.ColumnCount = 2;
			this.tableLayoutPanelRestrictionSummery.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelRestrictionSummery.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelRestrictionSummery.Controls.Add(this.tableLayoutPanelRestrictionButtons, 0, 0);
			this.tableLayoutPanelRestrictionSummery.Controls.Add(this.agentRestrictionGrid1, 1, 0);
			this.tableLayoutPanelRestrictionSummery.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelRestrictionSummery.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelRestrictionSummery.Name = "tableLayoutPanelRestrictionSummery";
			this.tableLayoutPanelRestrictionSummery.RowCount = 1;
			this.tableLayoutPanelRestrictionSummery.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelRestrictionSummery.Size = new System.Drawing.Size(335, 45);
			this.tableLayoutPanelRestrictionSummery.TabIndex = 1;
			// 
			// tableLayoutPanelRestrictionButtons
			// 
			this.tableLayoutPanelRestrictionButtons.AutoSize = true;
			this.tableLayoutPanelRestrictionButtons.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanelRestrictionButtons.ColumnCount = 1;
			this.tableLayoutPanelRestrictionButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelRestrictionButtons.Controls.Add(this.chbAvailability, 0, 0);
			this.tableLayoutPanelRestrictionButtons.Controls.Add(this.chbRotations, 0, 1);
			this.tableLayoutPanelRestrictionButtons.Controls.Add(this.chbPreferences, 0, 2);
			this.tableLayoutPanelRestrictionButtons.Controls.Add(this.chbStudenAvailability, 0, 3);
			this.tableLayoutPanelRestrictionButtons.Controls.Add(this.chbSchedules, 0, 4);
			this.tableLayoutPanelRestrictionButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelRestrictionButtons.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelRestrictionButtons.Name = "tableLayoutPanelRestrictionButtons";
			this.tableLayoutPanelRestrictionButtons.RowCount = 5;
			this.tableLayoutPanelRestrictionButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelRestrictionButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelRestrictionButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelRestrictionButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelRestrictionButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelRestrictionButtons.Size = new System.Drawing.Size(156, 135);
			this.tableLayoutPanelRestrictionButtons.TabIndex = 1;
			// 
			// chbAvailability
			// 
			this.chbAvailability.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.chbAvailability.Location = new System.Drawing.Point(3, 3);
			this.chbAvailability.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.chbAvailability.Name = "chbAvailability";
			this.chbAvailability.Size = new System.Drawing.Size(150, 21);
			this.chbAvailability.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
			this.chbAvailability.TabIndex = 0;
			this.chbAvailability.Text = "xxAvailability";
			this.chbAvailability.ThemesEnabled = false;
			this.chbAvailability.CheckedChanged += new System.EventHandler(this.chbAvailability_CheckedChanged);
			// 
			// chbRotations
			// 
			this.chbRotations.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.chbRotations.Location = new System.Drawing.Point(3, 30);
			this.chbRotations.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.chbRotations.Name = "chbRotations";
			this.chbRotations.Size = new System.Drawing.Size(150, 21);
			this.chbRotations.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
			this.chbRotations.TabIndex = 2;
			this.chbRotations.Text = "xxRotation";
			this.chbRotations.ThemesEnabled = false;
			this.chbRotations.CheckedChanged += new System.EventHandler(this.chbRotations_CheckedChanged);
			// 
			// chbPreferences
			// 
			this.chbPreferences.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.chbPreferences.Location = new System.Drawing.Point(3, 57);
			this.chbPreferences.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.chbPreferences.Name = "chbPreferences";
			this.chbPreferences.Size = new System.Drawing.Size(150, 21);
			this.chbPreferences.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
			this.chbPreferences.TabIndex = 2;
			this.chbPreferences.Text = "xxPreference";
			this.chbPreferences.ThemesEnabled = false;
			this.chbPreferences.CheckedChanged += new System.EventHandler(this.chbPreferences_CheckedChanged);
			// 
			// chbStudenAvailability
			// 
			this.chbStudenAvailability.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.chbStudenAvailability.Location = new System.Drawing.Point(3, 84);
			this.chbStudenAvailability.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.chbStudenAvailability.Name = "chbStudenAvailability";
			this.chbStudenAvailability.Size = new System.Drawing.Size(150, 21);
			this.chbStudenAvailability.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
			this.chbStudenAvailability.TabIndex = 2;
			this.chbStudenAvailability.Text = "xxStudentAvailability";
			this.chbStudenAvailability.ThemesEnabled = false;
			this.chbStudenAvailability.CheckedChanged += new System.EventHandler(this.chbStudenAvailability_CheckedChanged);
			// 
			// chbSchedules
			// 
			this.chbSchedules.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.chbSchedules.Location = new System.Drawing.Point(3, 111);
			this.chbSchedules.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.chbSchedules.Name = "chbSchedules";
			this.chbSchedules.Size = new System.Drawing.Size(150, 21);
			this.chbSchedules.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
			this.chbSchedules.TabIndex = 3;
			this.chbSchedules.Text = "xxSchedule";
			this.chbSchedules.ThemesEnabled = false;
			this.chbSchedules.CheckedChanged += new System.EventHandler(this.chbSchedules_CheckedChanged);
			// 
			// agentRestrictionGrid1
			// 
			this.agentRestrictionGrid1.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.DblClickOnCell;
			this.agentRestrictionGrid1.AllowSelection = Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Cell;
			this.agentRestrictionGrid1.ColCount = 12;
			this.agentRestrictionGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.agentRestrictionGrid1.ExcelLikeCurrentCell = true;
			this.agentRestrictionGrid1.ExcelLikeSelectionFrame = true;
			this.agentRestrictionGrid1.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.agentRestrictionGrid1.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.agentRestrictionGrid1.HorizontalThumbTrack = true;
			this.agentRestrictionGrid1.Location = new System.Drawing.Point(165, 3);
			this.agentRestrictionGrid1.Name = "agentRestrictionGrid1";
			this.agentRestrictionGrid1.NumberedColHeaders = false;
			this.agentRestrictionGrid1.Office2007ScrollBars = true;
			this.agentRestrictionGrid1.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
			this.agentRestrictionGrid1.Properties.ForceImmediateRepaint = false;
			this.agentRestrictionGrid1.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
			this.agentRestrictionGrid1.Properties.MarkColHeader = false;
			this.agentRestrictionGrid1.Properties.MarkRowHeader = false;
			this.agentRestrictionGrid1.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.agentRestrictionGrid1.RowCount = 1;
			this.agentRestrictionGrid1.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.agentRestrictionGrid1.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.agentRestrictionGrid1.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.agentRestrictionGrid1.Size = new System.Drawing.Size(167, 135);
			this.agentRestrictionGrid1.SmartSizeBox = false;
			this.agentRestrictionGrid1.TabIndex = 2;
			this.agentRestrictionGrid1.Text = "agentRestrictionGrid1";
			this.agentRestrictionGrid1.ThemesEnabled = true;
			this.agentRestrictionGrid1.UseRightToLeftCompatibleTextBox = true;
			this.agentRestrictionGrid1.VerticalThumbTrack = true;
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
			this.grid.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.grid.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.ExcelLikeCurrentCell = true;
			this.grid.ExcelLikeSelectionFrame = true;
			this.grid.ForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2003;
			this.grid.Location = new System.Drawing.Point(0, 0);
			this.grid.MinResizeColSize = 5;
			this.grid.Name = "grid";
			this.grid.NumberedColHeaders = false;
			this.grid.NumberedRowHeaders = false;
			gridRangeStyle1.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cells(1, 1, 10, 2);
			gridRangeStyle1.StyleInfo.BaseStyle = "Standard";
			this.grid.RangeStyles.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeStyle[] {
            gridRangeStyle1});
			this.grid.ReadOnly = true;
			this.grid.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.grid.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.grid.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.grid.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.grid.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.AlwaysVisible;
			this.grid.Size = new System.Drawing.Size(335, 266);
			this.grid.SmartSizeBox = false;
			this.grid.TabIndex = 3;
			this.grid.Text = "xxPeriodViewPROTOTYPE";
			this.grid.ThemesEnabled = true;
			this.grid.UseRightToLeftCompatibleTextBox = true;
			// 
			// elementHostRequests
			// 
			this.elementHostRequests.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementHostRequests.Location = new System.Drawing.Point(0, 0);
			this.elementHostRequests.Name = "elementHostRequests";
			this.elementHostRequests.Size = new System.Drawing.Size(335, 266);
			this.elementHostRequests.TabIndex = 5;
			this.elementHostRequests.Child = this.handlePersonRequestView1;
			// 
			// elementHost1
			// 
			this.elementHost1.AutoSize = true;
			this.elementHost1.BackColor = System.Drawing.Color.Transparent;
			this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementHost1.Location = new System.Drawing.Point(0, 0);
			this.elementHost1.Name = "elementHost1";
			this.elementHost1.Size = new System.Drawing.Size(335, 106);
			this.elementHost1.TabIndex = 1;
			this.elementHost1.Text = "elementHost1";
			this.elementHost1.Child = this.multipleHostControl1;
			// 
			// tabInfoPanels
			// 
			this.tabInfoPanels.ActiveTabColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.tabInfoPanels.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.tabInfoPanels.BeforeTouchSize = new System.Drawing.Size(436, 672);
			this.tabInfoPanels.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabInfoPanels.Controls.Add(this.tabPageAdvAgentInfo);
			this.tabInfoPanels.Controls.Add(this.tabPageAdvShiftCategoryDistribution);
			this.tabInfoPanels.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabInfoPanels.FixedSingleBorderColor = System.Drawing.Color.White;
			this.tabInfoPanels.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabInfoPanels.InactiveTabColor = System.Drawing.Color.White;
			this.tabInfoPanels.KeepSelectedTabInFrontRow = false;
			this.tabInfoPanels.Location = new System.Drawing.Point(0, 0);
			this.tabInfoPanels.Name = "tabInfoPanels";
			this.tabInfoPanels.Size = new System.Drawing.Size(436, 672);
			this.tabInfoPanels.TabIndex = 12;
			this.tabInfoPanels.TabPanelBackColor = System.Drawing.Color.White;
			this.tabInfoPanels.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			// 
			// tabPageAdvAgentInfo
			// 
			this.tabPageAdvAgentInfo.Image = null;
			this.tabPageAdvAgentInfo.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvAgentInfo.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdvAgentInfo.Name = "tabPageAdvAgentInfo";
			this.tabPageAdvAgentInfo.ShowCloseButton = true;
			this.tabPageAdvAgentInfo.Size = new System.Drawing.Size(436, 651);
			this.tabPageAdvAgentInfo.TabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
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
			this.tabPageAdvShiftCategoryDistribution.Size = new System.Drawing.Size(436, 651);
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
			this.shiftCategoryDistributionControl1.Size = new System.Drawing.Size(436, 651);
			this.shiftCategoryDistributionControl1.TabIndex = 0;
			// 
			// lessIntellegentSplitContainerAdvMainContainer
			// 
			this.lessIntellegentSplitContainerAdvMainContainer.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.lessIntellegentSplitContainerAdvMainContainer.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.lessIntellegentSplitContainerAdvMainContainer.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Silver);
			this.lessIntellegentSplitContainerAdvMainContainer.BeforeTouchSize = 1;
			this.lessIntellegentSplitContainerAdvMainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lessIntellegentSplitContainerAdvMainContainer.FixedPanel = Syncfusion.Windows.Forms.Tools.Enums.FixedPanel.Panel2;
			this.lessIntellegentSplitContainerAdvMainContainer.Location = new System.Drawing.Point(0, 0);
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
			this.lessIntellegentSplitContainerAdvMainContainer.Size = new System.Drawing.Size(772, 672);
			this.lessIntellegentSplitContainerAdvMainContainer.SplitterDistance = 335;
			this.lessIntellegentSplitContainerAdvMainContainer.SplitterWidth = 1;
			this.lessIntellegentSplitContainerAdvMainContainer.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.lessIntellegentSplitContainerAdvMainContainer.TabIndex = 10;
			this.lessIntellegentSplitContainerAdvMainContainer.Text = "lessIntellegentSplitContainerAdvMainContainer";
			this.lessIntellegentSplitContainerAdvMainContainer.Visible = false;
			// 
			// SchedulerSplitters
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lessIntellegentSplitContainerAdvMainContainer);
			this.Name = "SchedulerSplitters";
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
			this.teleoptiLessIntellegentSplitContainerView.Panel1.ResumeLayout(false);
			this.teleoptiLessIntellegentSplitContainerView.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.teleoptiLessIntellegentSplitContainerView)).EndInit();
			this.teleoptiLessIntellegentSplitContainerView.ResumeLayout(false);
			this.tableLayoutPanelRestrictionSummery.ResumeLayout(false);
			this.tableLayoutPanelRestrictionSummery.PerformLayout();
			this.tableLayoutPanelRestrictionButtons.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.chbAvailability)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chbRotations)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chbPreferences)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chbStudenAvailability)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chbSchedules)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.agentRestrictionGrid1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tabInfoPanels)).EndInit();
			this.tabInfoPanels.ResumeLayout(false);
			this.tabPageAdvShiftCategoryDistribution.ResumeLayout(false);
			this.lessIntellegentSplitContainerAdvMainContainer.Panel1.ResumeLayout(false);
			this.lessIntellegentSplitContainerAdvMainContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.lessIntellegentSplitContainerAdvMainContainer)).EndInit();
			this.lessIntellegentSplitContainerAdvMainContainer.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer lessIntellegentSplitContainerAdvMain;
        private Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer lessIntellegentSplitContainerAdvMainContainer;
        private Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer lessIntellegentSplitContainerAdvResultGraph;
        private Syncfusion.Windows.Forms.Chart.ChartControl chartControlSkillData;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabSkillData;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabInfoPanels;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdv1;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvAgentInfo;
        private Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer teleoptiLessIntelligentSplitContainerLessIntelligent1;
        private Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.TeleoptiLessIntelligentSplitContainer teleoptiLessIntellegentSplitContainerView;
        private Syncfusion.Windows.Forms.Grid.GridControl grid;
        private System.Windows.Forms.Integration.ElementHost elementHostRequests;
        private Teleopti.Ccc.WpfControls.Controls.Requests.Views.HandlePersonRequestView handlePersonRequestView1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private Teleopti.Ccc.WpfControls.Common.Interop.MultipleHostControl multipleHostControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelRestrictionSummery;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelRestrictionButtons;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv chbAvailability;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv chbRotations;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv chbPreferences;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv chbStudenAvailability;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv chbSchedules;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem PinnedToolStripMenuItem;
		private AgentRestrictions.AgentRestrictionGrid agentRestrictionGrid1;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvShiftCategoryDistribution;
		private PropertyPanel.ShiftCategoryDistributionControl shiftCategoryDistributionControl1;
    }
}
