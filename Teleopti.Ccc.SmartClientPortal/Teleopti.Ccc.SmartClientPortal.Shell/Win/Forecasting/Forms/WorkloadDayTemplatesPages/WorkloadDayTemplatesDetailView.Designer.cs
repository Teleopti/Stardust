using DateSelectionComposite=Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionComposite;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WorkloadDayTemplatesPages
{
    partial class WorkloadDayTemplatesDetailView
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
			this.splitContainerMain = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.xpBoxPeriodSelection = new Syncfusion.Windows.Forms.Tools.XPTaskBar();
			this.xpSelectperiod = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
			this.gradientPanel2 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.dateSelectionComposite1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionComposite();
			this.xpSmothing = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
			this.gradientPanel3 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelDeviationAfterTaskTime = new System.Windows.Forms.Label();
			this.cmbAverageAfterCallWorkRunningSmoothning = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.cmbTaskRunningSmoothning = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.cmbAverageTimeRunningSmoothning = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.labelDeviationTasks = new System.Windows.Forms.Label();
			this.labelDeviationTaskTime = new System.Windows.Forms.Label();
			this.gradientPanel5 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
			this.splitContainerMain.Panel2.SuspendLayout();
			this.splitContainerMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.xpBoxPeriodSelection)).BeginInit();
			this.xpBoxPeriodSelection.SuspendLayout();
			this.xpSelectperiod.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).BeginInit();
			this.gradientPanel2.SuspendLayout();
			this.xpSmothing.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel3)).BeginInit();
			this.gradientPanel3.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cmbAverageAfterCallWorkRunningSmoothning)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cmbTaskRunningSmoothning)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cmbAverageTimeRunningSmoothning)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel5)).BeginInit();
			this.SuspendLayout();
			// 
			// splitContainerMain
			// 
			this.splitContainerMain.BackColor = System.Drawing.Color.White;
			this.splitContainerMain.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.DarkGray);
			this.splitContainerMain.BeforeTouchSize = 3;
			this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
			this.splitContainerMain.Name = "splitContainerMain";
			// 
			// splitContainerMain.Panel1
			// 
			this.splitContainerMain.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			// 
			// splitContainerMain.Panel2
			// 
			this.splitContainerMain.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerMain.Panel2.Controls.Add(this.xpBoxPeriodSelection);
			this.splitContainerMain.Size = new System.Drawing.Size(1167, 692);
			this.splitContainerMain.SplitterDistance = 965;
			this.splitContainerMain.SplitterWidth = 3;
			this.splitContainerMain.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerMain.TabIndex = 0;
			this.splitContainerMain.DoubleClick += new System.EventHandler(this.splitContainerMain_DoubleClick);
			// 
			// xpBoxPeriodSelection
			// 
			this.xpBoxPeriodSelection.BackColor = System.Drawing.Color.White;
			this.xpBoxPeriodSelection.BeforeTouchSize = new System.Drawing.Size(199, 692);
			this.xpBoxPeriodSelection.BorderColor = System.Drawing.Color.Black;
			this.xpBoxPeriodSelection.Controls.Add(this.xpSelectperiod);
			this.xpBoxPeriodSelection.Controls.Add(this.xpSmothing);
			this.xpBoxPeriodSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.xpBoxPeriodSelection.Location = new System.Drawing.Point(0, 0);
			this.xpBoxPeriodSelection.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpBoxPeriodSelection.MinimumSize = new System.Drawing.Size(0, 0);
			this.xpBoxPeriodSelection.Name = "xpBoxPeriodSelection";
			this.xpBoxPeriodSelection.Size = new System.Drawing.Size(199, 692);
			this.xpBoxPeriodSelection.TabIndex = 1;
			// 
			// xpSelectperiod
			// 
			this.xpSelectperiod.AnimationDelay = 10;
			this.xpSelectperiod.Controls.Add(this.gradientPanel2);
			this.xpSelectperiod.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpSelectperiod.HeaderForeColor = System.Drawing.Color.White;
			this.xpSelectperiod.HeaderImageIndex = -1;
			this.xpSelectperiod.HitTaskBoxArea = false;
			this.xpSelectperiod.HotTrackColor = System.Drawing.Color.Empty;
			this.xpSelectperiod.ItemBackColor = System.Drawing.Color.White;
			this.xpSelectperiod.Location = new System.Drawing.Point(0, 0);
			this.xpSelectperiod.Name = "xpSelectperiod";
			this.xpSelectperiod.PreferredChildPanelHeight = 440;
			this.xpSelectperiod.ShowCollapseButton = false;
			this.xpSelectperiod.Size = new System.Drawing.Size(199, 472);
			this.xpSelectperiod.TabIndex = 1;
			this.xpSelectperiod.Text = "xxSelectHistoricalData";
			// 
			// gradientPanel2
			// 
			this.gradientPanel2.BackColor = System.Drawing.Color.White;
			this.gradientPanel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel2.Controls.Add(this.dateSelectionComposite1);
			this.gradientPanel2.Location = new System.Drawing.Point(2, 30);
			this.gradientPanel2.Name = "gradientPanel2";
			this.gradientPanel2.Size = new System.Drawing.Size(195, 440);
			this.gradientPanel2.TabIndex = 0;
			// 
			// dateSelectionComposite1
			// 
			this.dateSelectionComposite1.AutoSize = true;
			this.dateSelectionComposite1.BackColor = System.Drawing.Color.White;
			this.dateSelectionComposite1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateSelectionComposite1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateSelectionComposite1.Location = new System.Drawing.Point(0, 0);
			this.dateSelectionComposite1.Name = "dateSelectionComposite1";
			this.dateSelectionComposite1.Size = new System.Drawing.Size(195, 440);
			this.dateSelectionComposite1.TabIndex = 0;
			this.dateSelectionComposite1.DateRangeChanged += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.DateSelection.DateRangeChangedEventArgs>(this.dateSelectionComposite1_DateRangeChanged);
			// 
			// xpSmothing
			// 
			this.xpSmothing.Controls.Add(this.gradientPanel3);
			this.xpSmothing.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpSmothing.HeaderForeColor = System.Drawing.Color.White;
			this.xpSmothing.HeaderImageIndex = -1;
			this.xpSmothing.HitTaskBoxArea = false;
			this.xpSmothing.HotTrackColor = System.Drawing.Color.Empty;
			this.xpSmothing.ItemBackColor = System.Drawing.Color.White;
			this.xpSmothing.Location = new System.Drawing.Point(0, 472);
			this.xpSmothing.Name = "xpSmothing";
			this.xpSmothing.PreferredChildPanelHeight = 110;
			this.xpSmothing.Size = new System.Drawing.Size(199, 142);
			this.xpSmothing.TabIndex = 2;
			this.xpSmothing.Text = "xxSmoothing";
			// 
			// gradientPanel3
			// 
			this.gradientPanel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel3.Controls.Add(this.tableLayoutPanel1);
			this.gradientPanel3.Location = new System.Drawing.Point(2, 30);
			this.gradientPanel3.Name = "gradientPanel3";
			this.gradientPanel3.Size = new System.Drawing.Size(195, 110);
			this.gradientPanel3.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.labelDeviationAfterTaskTime, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.cmbAverageAfterCallWorkRunningSmoothning, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.cmbTaskRunningSmoothning, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.cmbAverageTimeRunningSmoothning, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelDeviationTasks, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelDeviationTaskTime, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(195, 110);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// labelDeviationAfterTaskTime
			// 
			this.labelDeviationAfterTaskTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDeviationAfterTaskTime.AutoSize = true;
			this.labelDeviationAfterTaskTime.Location = new System.Drawing.Point(3, 83);
			this.labelDeviationAfterTaskTime.Name = "labelDeviationAfterTaskTime";
			this.labelDeviationAfterTaskTime.Size = new System.Drawing.Size(133, 15);
			this.labelDeviationAfterTaskTime.TabIndex = 3;
			this.labelDeviationAfterTaskTime.Text = "xxForecastedACWColon";
			// 
			// cmbAverageAfterCallWorkRunningSmoothning
			// 
			this.cmbAverageAfterCallWorkRunningSmoothning.AllowNewText = false;
			this.cmbAverageAfterCallWorkRunningSmoothning.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.cmbAverageAfterCallWorkRunningSmoothning.BackColor = System.Drawing.Color.White;
			this.cmbAverageAfterCallWorkRunningSmoothning.BeforeTouchSize = new System.Drawing.Size(73, 23);
			this.cmbAverageAfterCallWorkRunningSmoothning.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAverageAfterCallWorkRunningSmoothning.Location = new System.Drawing.Point(164, 80);
			this.cmbAverageAfterCallWorkRunningSmoothning.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.cmbAverageAfterCallWorkRunningSmoothning.Name = "cmbAverageAfterCallWorkRunningSmoothning";
			this.cmbAverageAfterCallWorkRunningSmoothning.Size = new System.Drawing.Size(73, 23);
			this.cmbAverageAfterCallWorkRunningSmoothning.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.cmbAverageAfterCallWorkRunningSmoothning.TabIndex = 4;
			this.cmbAverageAfterCallWorkRunningSmoothning.SelectionChangeCommitted += new System.EventHandler(this.cmbAverageAfterCallWorkRunningSmoothning_SelectionChangeCommitted);
			// 
			// cmbTaskRunningSmoothning
			// 
			this.cmbTaskRunningSmoothning.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.cmbTaskRunningSmoothning.BackColor = System.Drawing.Color.White;
			this.cmbTaskRunningSmoothning.BeforeTouchSize = new System.Drawing.Size(73, 23);
			this.cmbTaskRunningSmoothning.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbTaskRunningSmoothning.Location = new System.Drawing.Point(164, 7);
			this.cmbTaskRunningSmoothning.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.cmbTaskRunningSmoothning.Name = "cmbTaskRunningSmoothning";
			this.cmbTaskRunningSmoothning.Size = new System.Drawing.Size(73, 23);
			this.cmbTaskRunningSmoothning.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.cmbTaskRunningSmoothning.TabIndex = 0;
			this.cmbTaskRunningSmoothning.SelectionChangeCommitted += new System.EventHandler(this.cmbTaskRunningSmoothning_SelectionChangeCommitted);
			this.cmbTaskRunningSmoothning.Click += new System.EventHandler(this.cmbTaskRunningSmoothning_Click);
			// 
			// cmbAverageTimeRunningSmoothning
			// 
			this.cmbAverageTimeRunningSmoothning.AllowNewText = false;
			this.cmbAverageTimeRunningSmoothning.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.cmbAverageTimeRunningSmoothning.BackColor = System.Drawing.Color.White;
			this.cmbAverageTimeRunningSmoothning.BeforeTouchSize = new System.Drawing.Size(73, 23);
			this.cmbAverageTimeRunningSmoothning.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAverageTimeRunningSmoothning.Location = new System.Drawing.Point(164, 43);
			this.cmbAverageTimeRunningSmoothning.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.cmbAverageTimeRunningSmoothning.Name = "cmbAverageTimeRunningSmoothning";
			this.cmbAverageTimeRunningSmoothning.Size = new System.Drawing.Size(73, 23);
			this.cmbAverageTimeRunningSmoothning.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.cmbAverageTimeRunningSmoothning.TabIndex = 4;
			this.cmbAverageTimeRunningSmoothning.SelectionChangeCommitted += new System.EventHandler(this.cmbAverageTimeRunningSmoothning_SelectionChangeCommitted);
			// 
			// labelDeviationTasks
			// 
			this.labelDeviationTasks.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDeviationTasks.AutoSize = true;
			this.labelDeviationTasks.Location = new System.Drawing.Point(3, 10);
			this.labelDeviationTasks.Name = "labelDeviationTasks";
			this.labelDeviationTasks.Size = new System.Drawing.Size(131, 15);
			this.labelDeviationTasks.TabIndex = 3;
			this.labelDeviationTasks.Text = "xxForecastedCallsColon";
			// 
			// labelDeviationTaskTime
			// 
			this.labelDeviationTaskTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDeviationTaskTime.AutoSize = true;
			this.labelDeviationTaskTime.Location = new System.Drawing.Point(3, 46);
			this.labelDeviationTaskTime.Name = "labelDeviationTaskTime";
			this.labelDeviationTaskTime.Size = new System.Drawing.Size(155, 15);
			this.labelDeviationTaskTime.TabIndex = 3;
			this.labelDeviationTaskTime.Text = "xxForecastedTalkTimeColon";
			// 
			// gradientPanel5
			// 
			this.gradientPanel5.Location = new System.Drawing.Point(0, 0);
			this.gradientPanel5.Name = "gradientPanel5";
			this.gradientPanel5.Size = new System.Drawing.Size(224, 80);
			this.gradientPanel5.TabIndex = 0;
			// 
			// WorkloadDayTemplatesDetailView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainerMain);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.MinimumSize = new System.Drawing.Size(583, 577);
			this.Name = "WorkloadDayTemplatesDetailView";
			this.Size = new System.Drawing.Size(1167, 692);
			this.splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
			this.splitContainerMain.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.xpBoxPeriodSelection)).EndInit();
			this.xpBoxPeriodSelection.ResumeLayout(false);
			this.xpSelectperiod.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).EndInit();
			this.gradientPanel2.ResumeLayout(false);
			this.gradientPanel2.PerformLayout();
			this.xpSmothing.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel3)).EndInit();
			this.gradientPanel3.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.cmbAverageAfterCallWorkRunningSmoothning)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cmbTaskRunningSmoothning)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cmbAverageTimeRunningSmoothning)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel5)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerMain;
        private Syncfusion.Windows.Forms.Tools.XPTaskBar xpBoxPeriodSelection;
        private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpSelectperiod;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel2;
        private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpSmothing;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel3;
        private System.Windows.Forms.Label labelDeviationTaskTime;
        private System.Windows.Forms.Label labelDeviationAfterTaskTime;
        private System.Windows.Forms.Label labelDeviationTasks;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv cmbTaskRunningSmoothning;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv cmbAverageAfterCallWorkRunningSmoothning;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv cmbAverageTimeRunningSmoothning;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel5;
        private DateSelectionComposite dateSelectionComposite1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
