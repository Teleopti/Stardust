using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using DateSelectionFromTo=Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection.DateSelectionFromTo;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SeasonPages
{
	partial class WorkflowTotalView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowTotalView));
			this.splitContainerAdv1 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.xpTaskBarTotal = new Syncfusion.Windows.Forms.Tools.XPTaskBar();
			this.xpTaskBarBoxPeriod = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.scenarioSelectorControl = new ScenarioSelector();
			this.dateSelectionFromToTarget = new DateSelectionFromTo();
			this.xpTaskBarBoxSpecialEvents = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
			this.gradientPanel2 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.outlierBox = new OutlierBox();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).BeginInit();
			this.splitContainerAdv1.Panel2.SuspendLayout();
			this.splitContainerAdv1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.xpTaskBarTotal)).BeginInit();
			this.xpTaskBarTotal.SuspendLayout();
			this.xpTaskBarBoxPeriod.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.gradientPanel1.SuspendLayout();
			this.xpTaskBarBoxSpecialEvents.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).BeginInit();
			this.gradientPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainerAdv1
			// 
			this.splitContainerAdv1.BackColor = System.Drawing.Color.White;
			this.splitContainerAdv1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.DarkGray);
			this.splitContainerAdv1.BeforeTouchSize = 3;
			this.splitContainerAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdv1.Location = new System.Drawing.Point(0, 0);
			this.splitContainerAdv1.Name = "splitContainerAdv1";
			// 
			// splitContainerAdv1.Panel1
			// 
			this.splitContainerAdv1.Panel1.BackColor = System.Drawing.Color.White;
			this.splitContainerAdv1.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			// 
			// splitContainerAdv1.Panel2
			// 
			this.splitContainerAdv1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv1.Panel2.Controls.Add(this.xpTaskBarTotal);
			this.splitContainerAdv1.Size = new System.Drawing.Size(1024, 691);
			this.splitContainerAdv1.SplitterDistance = 854;
			this.splitContainerAdv1.SplitterWidth = 3;
			this.splitContainerAdv1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdv1.TabIndex = 0;
			this.splitContainerAdv1.Text = "splitContainerAdv1";
			this.splitContainerAdv1.DoubleClick += new System.EventHandler(this.splitContainerAdv1_DoubleClick);
			// 
			// xpTaskBarTotal
			// 
			this.xpTaskBarTotal.BackColor = System.Drawing.Color.White;
			this.xpTaskBarTotal.BeforeTouchSize = new System.Drawing.Size(167, 691);
			this.xpTaskBarTotal.BorderColor = System.Drawing.Color.Black;
			this.xpTaskBarTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.xpTaskBarTotal.Controls.Add(this.xpTaskBarBoxPeriod);
			this.xpTaskBarTotal.Controls.Add(this.xpTaskBarBoxSpecialEvents);
			this.xpTaskBarTotal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.xpTaskBarTotal.Location = new System.Drawing.Point(0, 0);
			this.xpTaskBarTotal.MetroColor = System.Drawing.Color.White;
			this.xpTaskBarTotal.MinimumSize = new System.Drawing.Size(0, 0);
			this.xpTaskBarTotal.Name = "xpTaskBarTotal";
			this.xpTaskBarTotal.Size = new System.Drawing.Size(167, 691);
			this.xpTaskBarTotal.TabIndex = 0;
			// 
			// xpTaskBarBoxPeriod
			// 
			this.xpTaskBarBoxPeriod.Controls.Add(this.gradientPanel1);
			this.xpTaskBarBoxPeriod.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpTaskBarBoxPeriod.HeaderForeColor = System.Drawing.Color.White;
			this.xpTaskBarBoxPeriod.HeaderImageIndex = -1;
			this.xpTaskBarBoxPeriod.HitTaskBoxArea = false;
			this.xpTaskBarBoxPeriod.HotTrackColor = System.Drawing.Color.Empty;
			this.xpTaskBarBoxPeriod.ItemBackColor = System.Drawing.Color.White;
			this.xpTaskBarBoxPeriod.Location = new System.Drawing.Point(0, 0);
			this.xpTaskBarBoxPeriod.Name = "xpTaskBarBoxPeriod";
			this.xpTaskBarBoxPeriod.PreferredChildPanelHeight = 230;
			this.xpTaskBarBoxPeriod.Size = new System.Drawing.Size(165, 262);
			this.xpTaskBarBoxPeriod.TabIndex = 0;
			this.xpTaskBarBoxPeriod.Text = "xxCreateForecastFor";
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.BackColor = System.Drawing.Color.White;
			this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel1.Controls.Add(this.scenarioSelectorControl);
			this.gradientPanel1.Controls.Add(this.dateSelectionFromToTarget);
			this.gradientPanel1.Location = new System.Drawing.Point(2, 30);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Size = new System.Drawing.Size(161, 230);
			this.gradientPanel1.TabIndex = 0;
			// 
			// scenarioSelectorControl
			// 
			this.scenarioSelectorControl.BackColor = System.Drawing.Color.Transparent;
			this.scenarioSelectorControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.scenarioSelectorControl.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.scenarioSelectorControl.Location = new System.Drawing.Point(0, 0);
			this.scenarioSelectorControl.Name = "scenarioSelectorControl";
			this.scenarioSelectorControl.SelectedItem = null;
			this.scenarioSelectorControl.Size = new System.Drawing.Size(161, 56);
			this.scenarioSelectorControl.TabIndex = 1;
			// 
			// dateSelectionFromToTarget
			// 
			this.dateSelectionFromToTarget.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToTarget.ButtonApplyText = "xxApply";
			this.dateSelectionFromToTarget.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.dateSelectionFromToTarget.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateSelectionFromToTarget.HideNoneButtons = false;
			this.dateSelectionFromToTarget.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromToTarget.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToTarget.Location = new System.Drawing.Point(0, 57);
			this.dateSelectionFromToTarget.Name = "dateSelectionFromToTarget";
			this.dateSelectionFromToTarget.NoneButtonText = "xxNone";
			this.dateSelectionFromToTarget.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToTarget.Size = new System.Drawing.Size(161, 173);
			this.dateSelectionFromToTarget.TabIndex = 0;
			this.dateSelectionFromToTarget.TodayButtonText = "xxToday";
			this.dateSelectionFromToTarget.DateRangeChanged += new System.EventHandler<DateRangeChangedEventArgs>(this.dualPeriodHandler1_DateRangeChanged);
			// 
			// xpTaskBarBoxSpecialEvents
			// 
			this.xpTaskBarBoxSpecialEvents.Controls.Add(this.gradientPanel2);
			this.xpTaskBarBoxSpecialEvents.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpTaskBarBoxSpecialEvents.HeaderForeColor = System.Drawing.Color.White;
			this.xpTaskBarBoxSpecialEvents.HeaderImageIndex = -1;
			this.xpTaskBarBoxSpecialEvents.HitTaskBoxArea = false;
			this.xpTaskBarBoxSpecialEvents.HotTrackColor = System.Drawing.Color.Empty;
			this.xpTaskBarBoxSpecialEvents.ItemBackColor = System.Drawing.Color.White;
			this.xpTaskBarBoxSpecialEvents.Location = new System.Drawing.Point(0, 262);
			this.xpTaskBarBoxSpecialEvents.Name = "xpTaskBarBoxSpecialEvents";
			this.xpTaskBarBoxSpecialEvents.PreferredChildPanelHeight = 110;
			this.xpTaskBarBoxSpecialEvents.Size = new System.Drawing.Size(165, 142);
			this.xpTaskBarBoxSpecialEvents.TabIndex = 1;
			this.xpTaskBarBoxSpecialEvents.Text = "xxSpecialEvents";
			// 
			// gradientPanel2
			// 
			this.gradientPanel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel2.Controls.Add(this.outlierBox);
			this.gradientPanel2.Location = new System.Drawing.Point(2, 30);
			this.gradientPanel2.Name = "gradientPanel2";
			this.gradientPanel2.Size = new System.Drawing.Size(161, 110);
			this.gradientPanel2.TabIndex = 0;
			// 
			// outlierBox
			// 
			this.outlierBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outlierBox.Location = new System.Drawing.Point(0, 0);
			this.outlierBox.Name = "outlierBox";
			this.outlierBox.Size = new System.Drawing.Size(161, 110);
			this.outlierBox.TabIndex = 0;
			this.outlierBox.AddOutlier += new System.EventHandler<Teleopti.Ccc.Domain.Common.CustomEventArgs<Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly>>(this.outlierBox_AddOutlier);
			this.outlierBox.DeleteOutlier += new System.EventHandler<Teleopti.Ccc.Domain.Common.CustomEventArgs<IOutlier>>(this.outlierBox_DeleteOutlier);
			this.outlierBox.UpdateOutlier += new System.EventHandler<Teleopti.Ccc.Domain.Common.CustomEventArgs<IOutlier>>(this.outlierBox_UpdateOutlier);
			// 
			// WorkflowTotalView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainerAdv1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "WorkflowTotalView";
			this.Size = new System.Drawing.Size(1024, 691);
			this.splitContainerAdv1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).EndInit();
			this.splitContainerAdv1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.xpTaskBarTotal)).EndInit();
			this.xpTaskBarTotal.ResumeLayout(false);
			this.xpTaskBarBoxPeriod.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.gradientPanel1.ResumeLayout(false);
			this.xpTaskBarBoxSpecialEvents.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).EndInit();
			this.gradientPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv1;
		private Syncfusion.Windows.Forms.Tools.XPTaskBar xpTaskBarTotal;
		private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpTaskBarBoxPeriod;
		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
		private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpTaskBarBoxSpecialEvents;
		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel2;
		private DateSelectionFromTo dateSelectionFromToTarget;
		private OutlierBox outlierBox;
		private ScenarioSelector scenarioSelectorControl;

	}
}
