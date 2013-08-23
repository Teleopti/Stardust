using System;
using Teleopti.Ccc.WpfControls.Controls.Intraday;

namespace Teleopti.Ccc.Win.Intraday
{
    partial class IntradayViewContent
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
                cleanUp();
                if (components != null)
                    components.Dispose();

                if (_scheduleView != null)
                    _scheduleView.Dispose();

                if (_gridChartManager != null)
                    _gridChartManager.Dispose();

                if (_skillIntradayGridControl != null)
                    _skillIntradayGridControl.Dispose();

                if (_skillGridBackgroundLoader != null)
                    _skillGridBackgroundLoader.Dispose();

                if (_backgroundWorkerResources != null)
                    _backgroundWorkerResources.Dispose();

				if (timerRefreshSchedule != null)
				{
					timerRefreshSchedule.Tick -= new EventHandler(this.timerRefreshSchedule_Tick);
					timerRefreshSchedule.Stop();
				}

                _owner = null;
                _presenter = null;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntradayViewContent));
            Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection ccbpanelSkillChart = new Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection();
            Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection ccbpanelSkillGrid = new Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection();
            Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection ccbelementHostShiftEditor = new Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection();
            Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection ccbelementHostDayLayerView = new Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection();
            Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection ccbelementHostPinnedLayerView = new Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection();
            Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection ccbelementHostAgentState = new Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection();
            Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection ccbelementHostStaffingEffect = new Syncfusion.Windows.Forms.Tools.CaptionButtonsCollection();
            Syncfusion.Windows.Forms.Chart.ChartSeries chartSeries1 = new Syncfusion.Windows.Forms.Chart.ChartSeries();
            this.dockingManager1 = new Syncfusion.Windows.Forms.Tools.DockingManager(this.components);
            this.panelSkillChart = new System.Windows.Forms.Panel();
            this.chartControlSkillData = new Syncfusion.Windows.Forms.Chart.ChartControl();
            this.panelSkillGrid = new System.Windows.Forms.Panel();
            this.tabSkillData = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
            this.tabPageAdv1 = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.elementHostShiftEditor = new System.Windows.Forms.Integration.ElementHost();
            this.elementHostDayLayerView = new System.Windows.Forms.Integration.ElementHost();
            this.dayLayerView1 = new Teleopti.Ccc.WpfControls.Controls.Intraday.RealTimeScheduleControl();
            this.elementHostPinnedLayerView = new System.Windows.Forms.Integration.ElementHost();
            this.elementHostAgentState = new System.Windows.Forms.Integration.ElementHost();
            this.agentStateLayerView1 = new Teleopti.Ccc.WpfControls.Controls.Intraday.AgentStateLayerView();
            this.elementHostStaffingEffect = new System.Windows.Forms.Integration.ElementHost();
            this.staffingEffectView1 = new Teleopti.Ccc.WpfControls.Controls.Intraday.StaffingEffectView();
            this.timerRefreshSchedule = new System.Windows.Forms.Timer(this.components);
            this.backgroundWorkerFetchData = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.dockingManager1)).BeginInit();
            this.panelSkillChart.SuspendLayout();
            this.panelSkillGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabSkillData)).BeginInit();
            this.tabSkillData.SuspendLayout();
            this.SuspendLayout();
            // 
            // dockingManager1
            // 
            this.dockingManager1.ActiveCaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.dockingManager1.AutoHideInterval = 5;
            this.dockingManager1.AutoHideTabFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.dockingManager1.CloseEnabled = false;
            this.dockingManager1.DockLayoutStream = ((System.IO.MemoryStream)(resources.GetObject("dockingManager1.DockLayoutStream")));
            this.dockingManager1.DockTabFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.dockingManager1.DockToFill = true;
            this.dockingManager1.DragProviderStyle = Syncfusion.Windows.Forms.Tools.DragProviderStyle.VS2008;
            this.dockingManager1.HostControl = this;
            this.dockingManager1.InActiveCaptionBackground = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Transparent);
            this.dockingManager1.InActiveCaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.dockingManager1.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dockingManager1.ReduceFlickeringInRtl = false;
            this.dockingManager1.SplitterWidth = 3;
            this.dockingManager1.ThemesEnabled = true;
            this.dockingManager1.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.Office2007Outlook;
            this.dockingManager1.DockStateChanged += new Syncfusion.Windows.Forms.Tools.DockStateChangeEventHandler(this.dockingManager1_DockStateChanged);
            this.dockingManager1.AutoHideAnimationStart += new Syncfusion.Windows.Forms.Tools.AutoHideAnimationEventHandler(this.dockingManager1_AutoHideAnimationStart);
            this.dockingManager1.NewDockStateEndLoad += new System.EventHandler(this.dockingManager1_NewDockStateEndLoad);
            this.dockingManager1.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Close, "CloseButton"));
            this.dockingManager1.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Pin, "PinButton"));
            this.dockingManager1.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Menu, "MenuButton"));
            this.dockingManager1.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Maximize, "MaximizeButton"));
            this.dockingManager1.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Restore, "RestoreButton"));
            this.dockingManager1.SetDockLabel(this.panelSkillChart, "panelSkillChart");
            this.dockingManager1.SetEnableDocking(this.panelSkillChart, true);
            ccbpanelSkillChart.MergeWith(this.dockingManager1.CaptionButtons, false);
            this.dockingManager1.SetCustomCaptionButtons(this.panelSkillChart, ccbpanelSkillChart);
            this.dockingManager1.SetDockLabel(this.panelSkillGrid, "panelSkillGrid");
            this.dockingManager1.SetEnableDocking(this.panelSkillGrid, true);
            ccbpanelSkillGrid.MergeWith(this.dockingManager1.CaptionButtons, false);
            this.dockingManager1.SetCustomCaptionButtons(this.panelSkillGrid, ccbpanelSkillGrid);
            this.dockingManager1.SetDockLabel(this.elementHostShiftEditor, "elementHostShiftEditor");
            this.dockingManager1.SetEnableDocking(this.elementHostShiftEditor, true);
            ccbelementHostShiftEditor.MergeWith(this.dockingManager1.CaptionButtons, false);
            this.dockingManager1.SetCustomCaptionButtons(this.elementHostShiftEditor, ccbelementHostShiftEditor);
            this.dockingManager1.SetDockLabel(this.elementHostDayLayerView, "elementHostDayLayerView");
            this.dockingManager1.SetEnableDocking(this.elementHostDayLayerView, true);
            ccbelementHostDayLayerView.MergeWith(this.dockingManager1.CaptionButtons, false);
            this.dockingManager1.SetCustomCaptionButtons(this.elementHostDayLayerView, ccbelementHostDayLayerView);
            this.dockingManager1.SetDockLabel(this.elementHostPinnedLayerView, "elementHostPinnedLayerView");
            this.dockingManager1.SetEnableDocking(this.elementHostPinnedLayerView, true);
            ccbelementHostPinnedLayerView.MergeWith(this.dockingManager1.CaptionButtons, false);
            this.dockingManager1.SetCustomCaptionButtons(this.elementHostPinnedLayerView, ccbelementHostPinnedLayerView);
            this.dockingManager1.SetDockLabel(this.elementHostAgentState, "elementHost1");
            this.dockingManager1.SetEnableDocking(this.elementHostAgentState, true);
            ccbelementHostAgentState.MergeWith(this.dockingManager1.CaptionButtons, false);
            this.dockingManager1.SetCustomCaptionButtons(this.elementHostAgentState, ccbelementHostAgentState);
            this.dockingManager1.SetDockLabel(this.elementHostStaffingEffect, "elementHostStaffingEffect");
            this.dockingManager1.SetEnableDocking(this.elementHostStaffingEffect, true);
            ccbelementHostStaffingEffect.MergeWith(this.dockingManager1.CaptionButtons, false);
            this.dockingManager1.SetCustomCaptionButtons(this.elementHostStaffingEffect, ccbelementHostStaffingEffect);
            // 
            // panelSkillChart
            // 
            this.panelSkillChart.Controls.Add(this.chartControlSkillData);
            this.panelSkillChart.Location = new System.Drawing.Point(3, 23);
            this.panelSkillChart.Margin = new System.Windows.Forms.Padding(0);
            this.panelSkillChart.Name = "panelSkillChart";
            this.panelSkillChart.Size = new System.Drawing.Size(686, 123);
            this.panelSkillChart.TabIndex = 19;
            // 
            // chartControlSkillData
            // 
            this.chartControlSkillData.BackInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Control);
            this.chartControlSkillData.ChartArea.BackInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
            this.chartControlSkillData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartControlSkillData.ForeColor = System.Drawing.SystemColors.ControlText;
            // 
            // 
            // 
            this.chartControlSkillData.Legend.Location = new System.Drawing.Point(582, 75);
            this.chartControlSkillData.Location = new System.Drawing.Point(0, 0);
            this.chartControlSkillData.Name = "chartControlSkillData";
            this.chartControlSkillData.PrimaryYAxis.ForceZero = true;
            chartSeries1.Name = "Default";
            chartSeries1.Points.Add(0D, ((double)(54D)), ((double)(319D)), ((double)(249D)), ((double)(127D)));
            chartSeries1.Points.Add(1D, ((double)(68D)), ((double)(305D)), ((double)(159D)), ((double)(222D)));
            chartSeries1.Points.Add(2D, ((double)(94D)), ((double)(194D)), ((double)(138D)), ((double)(128D)));
            chartSeries1.Points.Add(3D, ((double)(60D)), ((double)(113D)), ((double)(82D)), ((double)(110D)));
            chartSeries1.Points.Add(4D, ((double)(85D)), ((double)(94D)), ((double)(86D)), ((double)(88D)));
            chartSeries1.Style.Border.Width = 2F;
            chartSeries1.Style.DisplayShadow = true;
            chartSeries1.Text = "Default";
            chartSeries1.Type = Syncfusion.Windows.Forms.Chart.ChartSeriesType.Line;
            this.chartControlSkillData.Series.Add(chartSeries1);
            this.chartControlSkillData.Size = new System.Drawing.Size(686, 123);
            this.chartControlSkillData.TabIndex = 0;
            this.chartControlSkillData.Text = "Skill";
            // 
            // 
            // 
            this.chartControlSkillData.Title.ForeColor = System.Drawing.SystemColors.ControlText;
            this.chartControlSkillData.Title.Name = "Default";
            this.chartControlSkillData.Titles.Add(this.chartControlSkillData.Title);
            this.chartControlSkillData.Visible = false;
            this.chartControlSkillData.ChartRegionClick += new Syncfusion.Windows.Forms.Chart.ChartRegionMouseEventHandler(this.chartControlSkillData_ChartRegionClick);
            this.chartControlSkillData.ChartRegionMouseEnter += new Syncfusion.Windows.Forms.Chart.ChartRegionMouseEventHandler(this.chartControlSkillData_ChartRegionMouseEnter);
            // 
            // panelSkillGrid
            // 
            this.panelSkillGrid.Controls.Add(this.tabSkillData);
            this.panelSkillGrid.Location = new System.Drawing.Point(3, 23);
            this.panelSkillGrid.Margin = new System.Windows.Forms.Padding(0);
            this.panelSkillGrid.Name = "panelSkillGrid";
            this.panelSkillGrid.Size = new System.Drawing.Size(686, 128);
            this.panelSkillGrid.TabIndex = 18;
            // 
            // tabSkillData
            // 
            this.tabSkillData.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabSkillData.BackColor = System.Drawing.SystemColors.Control;
            this.tabSkillData.Controls.Add(this.tabPageAdv1);
            this.tabSkillData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSkillData.Location = new System.Drawing.Point(0, 0);
            this.tabSkillData.Name = "tabSkillData";
            this.tabSkillData.Size = new System.Drawing.Size(686, 128);
            this.tabSkillData.TabGap = 10;
            this.tabSkillData.TabIndex = 6;
            this.tabSkillData.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
            this.tabSkillData.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
            // 
            // tabPageAdv1
            // 
            this.tabPageAdv1.BackColor = System.Drawing.Color.Transparent;
            this.tabPageAdv1.Image = null;
            this.tabPageAdv1.ImageSize = new System.Drawing.Size(16, 16);
            this.tabPageAdv1.IsTransparent = true;
            this.tabPageAdv1.Location = new System.Drawing.Point(1, 22);
            this.tabPageAdv1.Name = "tabPageAdv1";
            this.tabPageAdv1.Size = new System.Drawing.Size(683, 104);
            this.tabPageAdv1.TabIndex = 1;
            this.tabPageAdv1.ThemesEnabled = false;
            // 
            // elementHostShiftEditor
            // 
            this.elementHostShiftEditor.BackColor = System.Drawing.Color.Transparent;
            this.elementHostShiftEditor.BackColorTransparent = true;
            this.elementHostShiftEditor.Location = new System.Drawing.Point(3, 23);
            this.elementHostShiftEditor.Name = "elementHostShiftEditor";
            this.elementHostShiftEditor.Size = new System.Drawing.Size(686, 69);
            this.elementHostShiftEditor.TabIndex = 13;
            this.elementHostShiftEditor.Text = "yyelementHost1";
            this.elementHostShiftEditor.Child = null;
            // 
            // elementHostDayLayerView
            // 
            this.elementHostDayLayerView.BackColor = System.Drawing.Color.Transparent;
            this.elementHostDayLayerView.Location = new System.Drawing.Point(3, 23);
            this.elementHostDayLayerView.Name = "elementHostDayLayerView";
            this.elementHostDayLayerView.Size = new System.Drawing.Size(686, 190);
            this.elementHostDayLayerView.TabIndex = 44;
            this.elementHostDayLayerView.Text = "elementHost1";
            this.elementHostDayLayerView.Child = this.dayLayerView1;
            // 
            // elementHostPinnedLayerView
            // 
            this.elementHostPinnedLayerView.BackColor = System.Drawing.Color.Transparent;
            this.elementHostPinnedLayerView.BackColorTransparent = true;
            this.elementHostPinnedLayerView.Location = new System.Drawing.Point(3, 23);
            this.elementHostPinnedLayerView.Name = "elementHostPinnedLayerView";
            this.elementHostPinnedLayerView.Size = new System.Drawing.Size(227, 215);
            this.elementHostPinnedLayerView.TabIndex = 49;
            this.elementHostPinnedLayerView.Text = "elementHostPinnedLayerView";
            this.elementHostPinnedLayerView.Child = null;
            // 
            // elementHostAgentState
            // 
            this.elementHostAgentState.BackColor = System.Drawing.Color.Transparent;
            this.elementHostAgentState.Location = new System.Drawing.Point(3, 23);
            this.elementHostAgentState.Name = "elementHostAgentState";
            this.elementHostAgentState.Size = new System.Drawing.Size(227, 158);
            this.elementHostAgentState.TabIndex = 49;
            this.elementHostAgentState.Text = "elementHost1";
            this.elementHostAgentState.Child = this.agentStateLayerView1;
            // 
            // elementHostStaffingEffect
            // 
            this.elementHostStaffingEffect.BackColor = System.Drawing.Color.Transparent;
            this.elementHostStaffingEffect.Location = new System.Drawing.Point(3, 23);
            this.elementHostStaffingEffect.Name = "elementHostStaffingEffect";
            this.elementHostStaffingEffect.Size = new System.Drawing.Size(227, 166);
            this.elementHostStaffingEffect.TabIndex = 51;
            this.elementHostStaffingEffect.Text = "xxStaffingEffect";
            this.elementHostStaffingEffect.Child = this.staffingEffectView1;
            // 
            // timerRefreshSchedule
            // 
            this.timerRefreshSchedule.Interval = 1000;
            this.timerRefreshSchedule.Tick += new System.EventHandler(this.timerRefreshSchedule_Tick);
            // 
            // backgroundWorkerFetchData
            // 
            this.backgroundWorkerFetchData.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerFetchData_DoWork);
            this.backgroundWorkerFetchData.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerFetchData_RunWorkerCompleted);
            // 
            // IntradayViewContent
            // 
            this.BackColor = System.Drawing.Color.Transparent;
            this.Name = "IntradayViewContent";
            this.Size = new System.Drawing.Size(928, 623);
            this.Load += new System.EventHandler(this.IntradayViewContent_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dockingManager1)).EndInit();
            this.panelSkillChart.ResumeLayout(false);
            this.panelSkillGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tabSkillData)).EndInit();
            this.tabSkillData.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Chart.ChartControl chartControlSkillData;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabSkillData;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdv1;
        private Syncfusion.Windows.Forms.Tools.DockingManager dockingManager1;
        private System.Windows.Forms.Integration.ElementHost elementHostShiftEditor;
        private System.Windows.Forms.Timer timerRefreshSchedule;
        private System.Windows.Forms.Integration.ElementHost elementHostDayLayerView;
        private RealTimeScheduleControl dayLayerView1;
        private System.Windows.Forms.Integration.ElementHost elementHostPinnedLayerView;
        private System.Windows.Forms.Integration.ElementHost elementHostAgentState;
        private AgentStateLayerView agentStateLayerView1;
        private System.Windows.Forms.Integration.ElementHost elementHostStaffingEffect;
        private StaffingEffectView staffingEffectView1;
        private System.ComponentModel.BackgroundWorker backgroundWorkerFetchData;
        private System.Windows.Forms.Panel panelSkillGrid;
        private System.Windows.Forms.Panel panelSkillChart;
    }
}
