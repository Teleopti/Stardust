using DateSelectionComposite=Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionComposite;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SeasonPages
{
    partial class WorkflowDataSummaryView
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
            this.splitContainerAdv1 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.xpTaskBar1 = new Syncfusion.Windows.Forms.Tools.XPTaskBar();
            this.xpTaskBarBoxHistoricalDepth = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
            this.gradientPanel2 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.gradientPanel3 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.dateSelectionCompositeHistoricalPeriod = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionComposite();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).BeginInit();
            this.splitContainerAdv1.Panel2.SuspendLayout();
            this.splitContainerAdv1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).BeginInit();
            this.xpTaskBar1.SuspendLayout();
            this.xpTaskBarBoxHistoricalDepth.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).BeginInit();
            this.gradientPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel3)).BeginInit();
            this.gradientPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerAdv1
            // 
            this.splitContainerAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerAdv1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerAdv1.Name = "splitContainerAdv1";
            // 
            // splitContainerAdv1.Panel1
            // 
            this.splitContainerAdv1.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            // 
            // splitContainerAdv1.Panel2
            // 
            this.splitContainerAdv1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdv1.Panel2.Controls.Add(this.xpTaskBar1);
            this.splitContainerAdv1.Size = new System.Drawing.Size(1060, 600);
            this.splitContainerAdv1.SplitterDistance = 885;
            this.splitContainerAdv1.SplitterWidth = 5;
            this.splitContainerAdv1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.splitContainerAdv1.TabIndex = 0;
            this.splitContainerAdv1.Text = "yysplitContainerAdv1";
            this.splitContainerAdv1.DoubleClick += new System.EventHandler(this.splitContainerAdv1_DoubleClick);
            // 
            // xpTaskBar1
            // 
            this.xpTaskBar1.AutoSize = true;
            this.xpTaskBar1.Controls.Add(this.xpTaskBarBoxHistoricalDepth);
            this.xpTaskBar1.Dock = System.Windows.Forms.DockStyle.Top;
            this.xpTaskBar1.Location = new System.Drawing.Point(0, 0);
            this.xpTaskBar1.MinimumSize = new System.Drawing.Size(0, 0);
            this.xpTaskBar1.Name = "xpTaskBar1";
            this.xpTaskBar1.Size = new System.Drawing.Size(170, 396);
            this.xpTaskBar1.TabIndex = 0;
            this.xpTaskBar1.ThemesEnabled = true;
            // 
            // xpTaskBarBoxHistoricalDepth
            // 
            this.xpTaskBarBoxHistoricalDepth.Controls.Add(this.gradientPanel2);
            this.xpTaskBarBoxHistoricalDepth.HeaderBackColor = System.Drawing.SystemColors.Control;
            this.xpTaskBarBoxHistoricalDepth.HeaderImageIndex = -1;
            this.xpTaskBarBoxHistoricalDepth.HitTaskBoxArea = false;
            this.xpTaskBarBoxHistoricalDepth.ItemBackColor = System.Drawing.SystemColors.Control;
            this.xpTaskBarBoxHistoricalDepth.Location = new System.Drawing.Point(0, 0);
            this.xpTaskBarBoxHistoricalDepth.Name = "xpTaskBarBoxHistoricalDepth";
            this.xpTaskBarBoxHistoricalDepth.PreferredChildPanelHeight = 360;
            this.xpTaskBarBoxHistoricalDepth.Size = new System.Drawing.Size(170, 393);
            this.xpTaskBarBoxHistoricalDepth.TabIndex = 0;
            this.xpTaskBarBoxHistoricalDepth.Text = "xxSelectHistoricalData";
            // 
            // gradientPanel2
            // 
            this.gradientPanel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanel2.Controls.Add(this.gradientPanel3);
            this.gradientPanel2.Location = new System.Drawing.Point(2, 31);
            this.gradientPanel2.Name = "gradientPanel2";
            this.gradientPanel2.Size = new System.Drawing.Size(166, 360);
            this.gradientPanel2.TabIndex = 0;
            // 
            // gradientPanel3
            // 
            this.gradientPanel3.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.gradientPanel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanel3.Controls.Add(this.dateSelectionCompositeHistoricalPeriod);
            this.gradientPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanel3.Location = new System.Drawing.Point(0, 0);
            this.gradientPanel3.Name = "gradientPanel3";
            this.gradientPanel3.Size = new System.Drawing.Size(166, 360);
            this.gradientPanel3.TabIndex = 2;
            // 
            // dateSelectionCompositeHistoricalPeriod
            // 
            this.dateSelectionCompositeHistoricalPeriod.BackColor = System.Drawing.SystemColors.Control;
            this.dateSelectionCompositeHistoricalPeriod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateSelectionCompositeHistoricalPeriod.Location = new System.Drawing.Point(0, 0);
            this.dateSelectionCompositeHistoricalPeriod.Name = "dateSelectionCompositeHistoricalPeriod";
            this.dateSelectionCompositeHistoricalPeriod.ShowDateSelectionCalendar = false;
            this.dateSelectionCompositeHistoricalPeriod.ShowDateSelectionFromTo = false;
            this.dateSelectionCompositeHistoricalPeriod.ShowDateSelectionRolling = false;
            this.dateSelectionCompositeHistoricalPeriod.Size = new System.Drawing.Size(166, 360);
            this.dateSelectionCompositeHistoricalPeriod.TabIndex = 0;
            this.dateSelectionCompositeHistoricalPeriod.DateRangeChanged += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.DateSelection.DateRangeChangedEventArgs>(this.dateSelectionComposite1_DateRangeChanged);
            // 
            // WorkflowDataSummaryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerAdv1);
            this.Name = "WorkflowDataSummaryView";
            this.Size = new System.Drawing.Size(1060, 600);
            this.splitContainerAdv1.Panel2.ResumeLayout(false);
            this.splitContainerAdv1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).EndInit();
            this.splitContainerAdv1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).EndInit();
            this.xpTaskBar1.ResumeLayout(false);
            this.xpTaskBarBoxHistoricalDepth.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).EndInit();
            this.gradientPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel3)).EndInit();
            this.gradientPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv1;
        private Syncfusion.Windows.Forms.Tools.XPTaskBar xpTaskBar1;
        private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpTaskBarBoxHistoricalDepth;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel2;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel3;
        private DateSelectionComposite dateSelectionCompositeHistoricalPeriod;

    }
}