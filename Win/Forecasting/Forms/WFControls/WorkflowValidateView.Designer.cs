using System;
using System.Windows.Forms;
using DateSelectionFromTo=Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
    partial class WorkflowValidateView
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
                UnhookEvents();
                ReleaseManagedResources();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowValidateView));
			this.xpTaskBar1 = new Syncfusion.Windows.Forms.Tools.XPTaskBar();
			this.xpTaskBarBoxSelectPeriod = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
			this.gradientPanelSelectPeriod = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.buttonAdvCancelLoad = new Syncfusion.Windows.Forms.ButtonAdv();
			this.dateSelectionFromToHistorical = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.dateSelectionFromToTarget = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.xpTaskBarBoxDeviations = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
			this.gradientPanelDeviations = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelDeviationAfterTaskTime = new System.Windows.Forms.Label();
			this.labelDeviationTaskTime = new System.Windows.Forms.Label();
			this.percentTextBoxTasks = new Syncfusion.Windows.Forms.Tools.PercentTextBox();
			this.labelDeviationTasks = new System.Windows.Forms.Label();
			this.percentTextBoxDeviationTaskTime = new Syncfusion.Windows.Forms.Tools.PercentTextBox();
			this.percentTextBoxDeviationAfterTaskTime = new Syncfusion.Windows.Forms.Tools.PercentTextBox();
			this.xpTaskBarBoxSpecialEvents = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
			this.gradientPanelSpecialEvents = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.outlierBoxControl = new Teleopti.Ccc.Win.Common.Controls.OutlierBox();
			this.splitContainerAdv2 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.backgroundWorkerStatistics = new System.ComponentModel.BackgroundWorker();
			this.backgroundWorkerValidationPeriod = new System.ComponentModel.BackgroundWorker();
			((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).BeginInit();
			this.xpTaskBar1.SuspendLayout();
			this.xpTaskBarBoxSelectPeriod.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelSelectPeriod)).BeginInit();
			this.gradientPanelSelectPeriod.SuspendLayout();
			this.xpTaskBarBoxDeviations.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelDeviations)).BeginInit();
			this.gradientPanelDeviations.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.percentTextBoxTasks)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.percentTextBoxDeviationTaskTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.percentTextBoxDeviationAfterTaskTime)).BeginInit();
			this.xpTaskBarBoxSpecialEvents.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelSpecialEvents)).BeginInit();
			this.gradientPanelSpecialEvents.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv2)).BeginInit();
			this.splitContainerAdv2.Panel2.SuspendLayout();
			this.splitContainerAdv2.SuspendLayout();
			this.SuspendLayout();
			// 
			// xpTaskBar1
			// 
			this.xpTaskBar1.AutoSize = true;
			this.xpTaskBar1.BackColor = System.Drawing.Color.White;
			this.xpTaskBar1.BeforeTouchSize = new System.Drawing.Size(172, 600);
			this.xpTaskBar1.BorderColor = System.Drawing.Color.Black;
			this.xpTaskBar1.Controls.Add(this.xpTaskBarBoxSelectPeriod);
			this.xpTaskBar1.Controls.Add(this.xpTaskBarBoxDeviations);
			this.xpTaskBar1.Controls.Add(this.xpTaskBarBoxSpecialEvents);
			this.xpTaskBar1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.xpTaskBar1.Location = new System.Drawing.Point(0, 0);
			this.xpTaskBar1.MetroColor = System.Drawing.Color.White;
			this.xpTaskBar1.MinimumSize = new System.Drawing.Size(0, 0);
			this.xpTaskBar1.Name = "xpTaskBar1";
			this.xpTaskBar1.Size = new System.Drawing.Size(172, 600);
			this.xpTaskBar1.TabIndex = 1;
			// 
			// xpTaskBarBoxSelectPeriod
			// 
			this.xpTaskBarBoxSelectPeriod.BackColor = System.Drawing.SystemColors.Control;
			this.xpTaskBarBoxSelectPeriod.Controls.Add(this.gradientPanelSelectPeriod);
			this.xpTaskBarBoxSelectPeriod.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpTaskBarBoxSelectPeriod.HeaderForeColor = System.Drawing.Color.White;
			this.xpTaskBarBoxSelectPeriod.HeaderImageIndex = -1;
			this.xpTaskBarBoxSelectPeriod.HitTaskBoxArea = false;
			this.xpTaskBarBoxSelectPeriod.HotTrackColor = System.Drawing.Color.Empty;
			this.xpTaskBarBoxSelectPeriod.ItemBackColor = System.Drawing.SystemColors.Control;
			this.xpTaskBarBoxSelectPeriod.Location = new System.Drawing.Point(0, 0);
			this.xpTaskBarBoxSelectPeriod.Name = "xpTaskBarBoxSelectPeriod";
			this.xpTaskBarBoxSelectPeriod.PreferredChildPanelHeight = 277;
			this.xpTaskBarBoxSelectPeriod.ShowCollapseButton = false;
			this.xpTaskBarBoxSelectPeriod.Size = new System.Drawing.Size(172, 308);
			this.xpTaskBarBoxSelectPeriod.TabIndex = 0;
			this.xpTaskBarBoxSelectPeriod.TabStop = false;
			this.xpTaskBarBoxSelectPeriod.Text = "xxSelectHistoricalData";
			// 
			// gradientPanelSelectPeriod
			// 
			this.gradientPanelSelectPeriod.AutoSize = true;
			this.gradientPanelSelectPeriod.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelSelectPeriod.Controls.Add(this.buttonAdvCancelLoad);
			this.gradientPanelSelectPeriod.Controls.Add(this.dateSelectionFromToHistorical);
			this.gradientPanelSelectPeriod.Controls.Add(this.dateSelectionFromToTarget);
			this.gradientPanelSelectPeriod.Location = new System.Drawing.Point(2, 29);
			this.gradientPanelSelectPeriod.Name = "gradientPanelSelectPeriod";
			this.gradientPanelSelectPeriod.Size = new System.Drawing.Size(168, 277);
			this.gradientPanelSelectPeriod.TabIndex = 0;
			// 
			// buttonAdvCancelLoad
			// 
			this.buttonAdvCancelLoad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancelLoad.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancelLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancelLoad.BeforeTouchSize = new System.Drawing.Size(164, 26);
			this.buttonAdvCancelLoad.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancelLoad.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Cancel_16x16;
			this.buttonAdvCancelLoad.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonAdvCancelLoad.IsBackStageButton = false;
			this.buttonAdvCancelLoad.Location = new System.Drawing.Point(3, 237);
			this.buttonAdvCancelLoad.Name = "buttonAdvCancelLoad";
			this.buttonAdvCancelLoad.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancelLoad.Size = new System.Drawing.Size(164, 26);
			this.buttonAdvCancelLoad.TabIndex = 4;
			this.buttonAdvCancelLoad.Text = "xxxCancelLoading";
			this.buttonAdvCancelLoad.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.buttonAdvCancelLoad.UseVisualStyle = true;
			this.buttonAdvCancelLoad.Visible = false;
			this.buttonAdvCancelLoad.Click += new System.EventHandler(this.buttonAdvCancelLoad_Click);
			this.buttonAdvCancelLoad.MouseEnter += new System.EventHandler(this.buttonAdvCancelLoad_MouseEnter);
			this.buttonAdvCancelLoad.MouseLeave += new System.EventHandler(this.buttonAdvCancelLoad_MouseLeave);
			// 
			// dateSelectionFromToHistorical
			// 
			this.dateSelectionFromToHistorical.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToHistorical.ButtonApplyText = "xxApply";
			this.dateSelectionFromToHistorical.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.dateSelectionFromToHistorical.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.dateSelectionFromToHistorical.HideNoneButtons = false;
			this.dateSelectionFromToHistorical.LabelDateSelectionText = "xxCompareWith";
			this.dateSelectionFromToHistorical.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToHistorical.Location = new System.Drawing.Point(0, 120);
			this.dateSelectionFromToHistorical.Name = "dateSelectionFromToHistorical";
			this.dateSelectionFromToHistorical.NoneButtonText = "xxNone";
			this.dateSelectionFromToHistorical.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToHistorical.Size = new System.Drawing.Size(168, 157);
			this.dateSelectionFromToHistorical.TabIndex = 3;
			this.dateSelectionFromToHistorical.TodayButtonText = "xxToday";
			this.dateSelectionFromToHistorical.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToHistorical.WorkPeriodEnd")));
			this.dateSelectionFromToHistorical.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToHistorical.WorkPeriodStart")));
			this.dateSelectionFromToHistorical.ValueChanged += new System.EventHandler(this.dateSelectionFromToHistorical_ValueChanged);
			this.dateSelectionFromToHistorical.ButtonClickedNoValidation += new System.EventHandler(this.dateSelectionFromToHistorical_ButtonClicked_novalidation);
			// 
			// dateSelectionFromToTarget
			// 
			this.dateSelectionFromToTarget.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToTarget.ButtonApplyText = "xxApplyTargetPeriod";
			this.dateSelectionFromToTarget.Dock = System.Windows.Forms.DockStyle.Top;
			this.dateSelectionFromToTarget.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.dateSelectionFromToTarget.HideNoneButtons = false;
			this.dateSelectionFromToTarget.LabelDateSelectionText = "xxValidationPeriod";
			this.dateSelectionFromToTarget.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToTarget.Location = new System.Drawing.Point(0, 0);
			this.dateSelectionFromToTarget.Margin = new System.Windows.Forms.Padding(4);
			this.dateSelectionFromToTarget.Name = "dateSelectionFromToTarget";
			this.dateSelectionFromToTarget.NoneButtonText = "xxNone";
			this.dateSelectionFromToTarget.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToTarget.ShowApplyButton = false;
			this.dateSelectionFromToTarget.Size = new System.Drawing.Size(168, 120);
			this.dateSelectionFromToTarget.TabIndex = 2;
			this.dateSelectionFromToTarget.TodayButtonText = "xxToday";
			this.dateSelectionFromToTarget.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToTarget.WorkPeriodEnd")));
			this.dateSelectionFromToTarget.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToTarget.WorkPeriodStart")));
			this.dateSelectionFromToTarget.ValueChanged += new System.EventHandler(this.dateSelectionFromToTarget_ValueChanged);
			// 
			// xpTaskBarBoxDeviations
			// 
			this.xpTaskBarBoxDeviations.Controls.Add(this.gradientPanelDeviations);
			this.xpTaskBarBoxDeviations.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpTaskBarBoxDeviations.HeaderForeColor = System.Drawing.Color.White;
			this.xpTaskBarBoxDeviations.HeaderImageIndex = -1;
			this.xpTaskBarBoxDeviations.HitTaskBoxArea = false;
			this.xpTaskBarBoxDeviations.HotTrackColor = System.Drawing.Color.Empty;
			this.xpTaskBarBoxDeviations.ItemBackColor = System.Drawing.SystemColors.Control;
			this.xpTaskBarBoxDeviations.Location = new System.Drawing.Point(0, 308);
			this.xpTaskBarBoxDeviations.Name = "xpTaskBarBoxDeviations";
			this.xpTaskBarBoxDeviations.PreferredChildPanelHeight = 120;
			this.xpTaskBarBoxDeviations.Size = new System.Drawing.Size(172, 151);
			this.xpTaskBarBoxDeviations.TabIndex = 1;
			this.xpTaskBarBoxDeviations.TabStop = false;
			this.xpTaskBarBoxDeviations.Text = "xxDeviations";
			// 
			// gradientPanelDeviations
			// 
			this.gradientPanelDeviations.AutoSize = true;
			this.gradientPanelDeviations.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelDeviations.Controls.Add(this.tableLayoutPanel1);
			this.gradientPanelDeviations.Location = new System.Drawing.Point(2, 29);
			this.gradientPanelDeviations.Name = "gradientPanelDeviations";
			this.gradientPanelDeviations.Size = new System.Drawing.Size(168, 120);
			this.gradientPanelDeviations.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.Controls.Add(this.labelDeviationAfterTaskTime, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelDeviationTaskTime, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.percentTextBoxTasks, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelDeviationTasks, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.percentTextBoxDeviationTaskTime, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.percentTextBoxDeviationAfterTaskTime, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(168, 120);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// labelDeviationAfterTaskTime
			// 
			this.labelDeviationAfterTaskTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDeviationAfterTaskTime.AutoSize = true;
			this.labelDeviationAfterTaskTime.Location = new System.Drawing.Point(3, 87);
			this.labelDeviationAfterTaskTime.Name = "labelDeviationAfterTaskTime";
			this.labelDeviationAfterTaskTime.Size = new System.Drawing.Size(98, 26);
			this.labelDeviationAfterTaskTime.TabIndex = 5;
			this.labelDeviationAfterTaskTime.Text = "xxDeviationACWColon";
			// 
			// labelDeviationTaskTime
			// 
			this.labelDeviationTaskTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDeviationTaskTime.AutoSize = true;
			this.labelDeviationTaskTime.Location = new System.Drawing.Point(3, 47);
			this.labelDeviationTaskTime.Name = "labelDeviationTaskTime";
			this.labelDeviationTaskTime.Size = new System.Drawing.Size(103, 26);
			this.labelDeviationTaskTime.TabIndex = 3;
			this.labelDeviationTaskTime.Text = "xxDeviationTalkTimeColon";
			// 
			// percentTextBoxTasks
			// 
			this.percentTextBoxTasks.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.percentTextBoxTasks.BackGroundColor = System.Drawing.SystemColors.Window;
			this.percentTextBoxTasks.BeforeTouchSize = new System.Drawing.Size(45, 22);
			this.percentTextBoxTasks.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.percentTextBoxTasks.DoubleValue = 0D;
			this.percentTextBoxTasks.Location = new System.Drawing.Point(122, 9);
			this.percentTextBoxTasks.MaxValue = 100D;
			this.percentTextBoxTasks.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.percentTextBoxTasks.MinValue = -100D;
			this.percentTextBoxTasks.Name = "percentTextBoxTasks";
			this.percentTextBoxTasks.NullString = "0,00 %";
			this.percentTextBoxTasks.OverflowIndicatorToolTipText = null;
			this.percentTextBoxTasks.PercentDecimalDigits = 0;
			this.percentTextBoxTasks.Size = new System.Drawing.Size(45, 22);
			this.percentTextBoxTasks.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.percentTextBoxTasks.TabIndex = 5;
			this.percentTextBoxTasks.Text = "0 %";
			this.percentTextBoxTasks.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PercentTextBoxTasks_OnKeyUp);
			this.percentTextBoxTasks.Validated += new System.EventHandler(this.percentTextBoxTasks_Validated);
			// 
			// labelDeviationTasks
			// 
			this.labelDeviationTasks.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDeviationTasks.AutoSize = true;
			this.labelDeviationTasks.Location = new System.Drawing.Point(3, 7);
			this.labelDeviationTasks.MaximumSize = new System.Drawing.Size(100, 0);
			this.labelDeviationTasks.Name = "labelDeviationTasks";
			this.labelDeviationTasks.Size = new System.Drawing.Size(97, 26);
			this.labelDeviationTasks.TabIndex = 1;
			this.labelDeviationTasks.Text = "xxDeviationCallsColon";
			// 
			// percentTextBoxDeviationTaskTime
			// 
			this.percentTextBoxDeviationTaskTime.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.percentTextBoxDeviationTaskTime.BackGroundColor = System.Drawing.SystemColors.Window;
			this.percentTextBoxDeviationTaskTime.BeforeTouchSize = new System.Drawing.Size(45, 22);
			this.percentTextBoxDeviationTaskTime.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.percentTextBoxDeviationTaskTime.DoubleValue = 0D;
			this.percentTextBoxDeviationTaskTime.Location = new System.Drawing.Point(122, 49);
			this.percentTextBoxDeviationTaskTime.MaxValue = 100D;
			this.percentTextBoxDeviationTaskTime.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.percentTextBoxDeviationTaskTime.MinValue = -100D;
			this.percentTextBoxDeviationTaskTime.Name = "percentTextBoxDeviationTaskTime";
			this.percentTextBoxDeviationTaskTime.NullString = "0,00 %";
			this.percentTextBoxDeviationTaskTime.OverflowIndicatorToolTipText = null;
			this.percentTextBoxDeviationTaskTime.PercentDecimalDigits = 0;
			this.percentTextBoxDeviationTaskTime.Size = new System.Drawing.Size(45, 22);
			this.percentTextBoxDeviationTaskTime.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.percentTextBoxDeviationTaskTime.TabIndex = 6;
			this.percentTextBoxDeviationTaskTime.Text = "0 %";
			this.percentTextBoxDeviationTaskTime.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PercentTextBoxDeviationTaskTime_OnKeyUp);
			this.percentTextBoxDeviationTaskTime.Validated += new System.EventHandler(this.percentTextBoxDeviationTaskTime_Validated);
			// 
			// percentTextBoxDeviationAfterTaskTime
			// 
			this.percentTextBoxDeviationAfterTaskTime.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.percentTextBoxDeviationAfterTaskTime.BackGroundColor = System.Drawing.SystemColors.Window;
			this.percentTextBoxDeviationAfterTaskTime.BeforeTouchSize = new System.Drawing.Size(45, 22);
			this.percentTextBoxDeviationAfterTaskTime.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.percentTextBoxDeviationAfterTaskTime.DoubleValue = 0D;
			this.percentTextBoxDeviationAfterTaskTime.Location = new System.Drawing.Point(122, 89);
			this.percentTextBoxDeviationAfterTaskTime.MaxValue = 100D;
			this.percentTextBoxDeviationAfterTaskTime.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.percentTextBoxDeviationAfterTaskTime.MinValue = -100D;
			this.percentTextBoxDeviationAfterTaskTime.Name = "percentTextBoxDeviationAfterTaskTime";
			this.percentTextBoxDeviationAfterTaskTime.NullString = "0,00 %";
			this.percentTextBoxDeviationAfterTaskTime.OverflowIndicatorToolTipText = null;
			this.percentTextBoxDeviationAfterTaskTime.PercentDecimalDigits = 0;
			this.percentTextBoxDeviationAfterTaskTime.Size = new System.Drawing.Size(45, 22);
			this.percentTextBoxDeviationAfterTaskTime.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.percentTextBoxDeviationAfterTaskTime.TabIndex = 7;
			this.percentTextBoxDeviationAfterTaskTime.Text = "0 %";
			this.percentTextBoxDeviationAfterTaskTime.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PercentTextBoxDeviationAfterTaskTime_OnKeyUp);
			this.percentTextBoxDeviationAfterTaskTime.Validated += new System.EventHandler(this.percentTextBoxDeviationAfterTaskTime_Validated);
			// 
			// xpTaskBarBoxSpecialEvents
			// 
			this.xpTaskBarBoxSpecialEvents.Controls.Add(this.gradientPanelSpecialEvents);
			this.xpTaskBarBoxSpecialEvents.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpTaskBarBoxSpecialEvents.HeaderForeColor = System.Drawing.Color.White;
			this.xpTaskBarBoxSpecialEvents.HeaderImageIndex = -1;
			this.xpTaskBarBoxSpecialEvents.HitTaskBoxArea = false;
			this.xpTaskBarBoxSpecialEvents.HotTrackColor = System.Drawing.Color.Empty;
			this.xpTaskBarBoxSpecialEvents.ItemBackColor = System.Drawing.SystemColors.Control;
			this.xpTaskBarBoxSpecialEvents.Location = new System.Drawing.Point(0, 459);
			this.xpTaskBarBoxSpecialEvents.Name = "xpTaskBarBoxSpecialEvents";
			this.xpTaskBarBoxSpecialEvents.PreferredChildPanelHeight = 100;
			this.xpTaskBarBoxSpecialEvents.Size = new System.Drawing.Size(172, 131);
			this.xpTaskBarBoxSpecialEvents.TabIndex = 7;
			this.xpTaskBarBoxSpecialEvents.TabStop = false;
			this.xpTaskBarBoxSpecialEvents.Text = "xxSpecialEvents";
			// 
			// gradientPanelSpecialEvents
			// 
			this.gradientPanelSpecialEvents.AutoSize = true;
			this.gradientPanelSpecialEvents.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelSpecialEvents.Controls.Add(this.outlierBoxControl);
			this.gradientPanelSpecialEvents.Location = new System.Drawing.Point(2, 29);
			this.gradientPanelSpecialEvents.Name = "gradientPanelSpecialEvents";
			this.gradientPanelSpecialEvents.Size = new System.Drawing.Size(168, 100);
			this.gradientPanelSpecialEvents.TabIndex = 1;
			// 
			// outlierBoxControl
			// 
			this.outlierBoxControl.BackColor = System.Drawing.Color.White;
			this.outlierBoxControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outlierBoxControl.Location = new System.Drawing.Point(0, 0);
			this.outlierBoxControl.Name = "outlierBoxControl";
			this.outlierBoxControl.Size = new System.Drawing.Size(168, 100);
			this.outlierBoxControl.TabIndex = 8;
			this.outlierBoxControl.AddOutlier += new System.EventHandler<Teleopti.Ccc.Domain.Common.CustomEventArgs<Teleopti.Interfaces.Domain.DateOnly>>(this.outlierBoxControl_AddOutlier);
			this.outlierBoxControl.DeleteOutlier += new System.EventHandler<Teleopti.Ccc.Domain.Common.CustomEventArgs<Teleopti.Interfaces.Domain.IOutlier>>(this.outlierBoxControl_DeleteOutlier);
			this.outlierBoxControl.UpdateOutlier += new System.EventHandler<Teleopti.Ccc.Domain.Common.CustomEventArgs<Teleopti.Interfaces.Domain.IOutlier>>(this.outlierBoxControl_UpdateOutlier);
			// 
			// splitContainerAdv2
			// 
			this.splitContainerAdv2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.DarkGray);
			this.splitContainerAdv2.BeforeTouchSize = 3;
			this.splitContainerAdv2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdv2.Location = new System.Drawing.Point(0, 0);
			this.splitContainerAdv2.Name = "splitContainerAdv2";
			// 
			// splitContainerAdv2.Panel1
			// 
			this.splitContainerAdv2.Panel1.AutoSize = true;
			this.splitContainerAdv2.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			// 
			// splitContainerAdv2.Panel2
			// 
			this.splitContainerAdv2.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv2.Panel2.Controls.Add(this.xpTaskBar1);
			this.splitContainerAdv2.Size = new System.Drawing.Size(1060, 600);
			this.splitContainerAdv2.SplitterDistance = 885;
			this.splitContainerAdv2.SplitterWidth = 3;
			this.splitContainerAdv2.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdv2.TabIndex = 0;
			this.splitContainerAdv2.TabStop = false;
			this.splitContainerAdv2.Text = "splitContainerAdv7";
			this.splitContainerAdv2.DoubleClick += new System.EventHandler(this.splitContainerAdv2_DoubleClick);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 56F));
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(200, 100);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// backgroundWorkerStatistics
			// 
			this.backgroundWorkerStatistics.WorkerSupportsCancellation = true;
			// 
			// backgroundWorkerValidationPeriod
			// 
			this.backgroundWorkerValidationPeriod.WorkerSupportsCancellation = true;
			// 
			// WorkflowValidateView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainerAdv2);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "WorkflowValidateView";
			this.Size = new System.Drawing.Size(1060, 600);
			((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).EndInit();
			this.xpTaskBar1.ResumeLayout(false);
			this.xpTaskBarBoxSelectPeriod.ResumeLayout(false);
			this.xpTaskBarBoxSelectPeriod.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelSelectPeriod)).EndInit();
			this.gradientPanelSelectPeriod.ResumeLayout(false);
			this.xpTaskBarBoxDeviations.ResumeLayout(false);
			this.xpTaskBarBoxDeviations.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelDeviations)).EndInit();
			this.gradientPanelDeviations.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.percentTextBoxTasks)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.percentTextBoxDeviationTaskTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.percentTextBoxDeviationAfterTaskTime)).EndInit();
			this.xpTaskBarBoxSpecialEvents.ResumeLayout(false);
			this.xpTaskBarBoxSpecialEvents.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelSpecialEvents)).EndInit();
			this.gradientPanelSpecialEvents.ResumeLayout(false);
			this.splitContainerAdv2.Panel2.ResumeLayout(false);
			this.splitContainerAdv2.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv2)).EndInit();
			this.splitContainerAdv2.ResumeLayout(false);
			this.splitContainerAdv2.PerformLayout();
			this.ResumeLayout(false);

        }

	    #endregion

        private Syncfusion.Windows.Forms.Tools.XPTaskBar xpTaskBar1;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv2;
        private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpTaskBarBoxSelectPeriod;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelSelectPeriod;
        private DateSelectionFromTo dateSelectionFromToTarget;
        private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpTaskBarBoxDeviations;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelDeviations;
        private Syncfusion.Windows.Forms.Tools.PercentTextBox percentTextBoxTasks;
        private System.Windows.Forms.Label labelDeviationAfterTaskTime;
        private Syncfusion.Windows.Forms.Tools.PercentTextBox percentTextBoxDeviationAfterTaskTime;
        private System.Windows.Forms.Label labelDeviationTaskTime;
        private Syncfusion.Windows.Forms.Tools.PercentTextBox percentTextBoxDeviationTaskTime;
		  private System.Windows.Forms.Label labelDeviationTasks;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpTaskBarBoxSpecialEvents;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelSpecialEvents;
        private Common.Controls.OutlierBox outlierBoxControl;
        private System.ComponentModel.BackgroundWorker backgroundWorkerStatistics;
        private System.ComponentModel.BackgroundWorker backgroundWorkerValidationPeriod;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancelLoad;
		  private DateSelectionFromTo dateSelectionFromToHistorical;
    }
}
