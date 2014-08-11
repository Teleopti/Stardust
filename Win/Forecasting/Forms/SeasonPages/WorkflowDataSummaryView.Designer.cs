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
			this.components = new System.ComponentModel.Container();
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
			this.splitContainerAdv1.Panel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// splitContainerAdv1.Panel2
			// 
			this.splitContainerAdv1.Panel2.BackColor = System.Drawing.Color.White;
			this.splitContainerAdv1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
			this.splitContainerAdv1.Panel2.Controls.Add(this.xpTaskBar1);
			this.splitContainerAdv1.Size = new System.Drawing.Size(1060, 600);
			this.splitContainerAdv1.SplitterDistance = 885;
			this.splitContainerAdv1.SplitterWidth = 3;
			this.splitContainerAdv1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdv1.TabIndex = 0;
			this.splitContainerAdv1.Text = "yysplitContainerAdv1";
			this.splitContainerAdv1.DoubleClick += new System.EventHandler(this.splitContainerAdv1_DoubleClick);
			// 
			// xpTaskBar1
			// 
			this.xpTaskBar1.AutoSize = true;
			this.xpTaskBar1.BackColor = System.Drawing.Color.White;
			this.xpTaskBar1.BeforeTouchSize = new System.Drawing.Size(172, 393);
			this.xpTaskBar1.BorderColor = System.Drawing.Color.Black;
			this.xpTaskBar1.Controls.Add(this.xpTaskBarBoxHistoricalDepth);
			this.xpTaskBar1.Dock = System.Windows.Forms.DockStyle.Top;
			this.xpTaskBar1.Location = new System.Drawing.Point(0, 0);
			this.xpTaskBar1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpTaskBar1.MinimumSize = new System.Drawing.Size(0, 0);
			this.xpTaskBar1.Name = "xpTaskBar1";
			this.xpTaskBar1.Size = new System.Drawing.Size(172, 393);
			this.xpTaskBar1.TabIndex = 0;
			// 
			// xpTaskBarBoxHistoricalDepth
			// 
			this.xpTaskBarBoxHistoricalDepth.Controls.Add(this.gradientPanel2);
			this.xpTaskBarBoxHistoricalDepth.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.xpTaskBarBoxHistoricalDepth.HeaderForeColor = System.Drawing.Color.White;
			this.xpTaskBarBoxHistoricalDepth.HeaderImageIndex = -1;
			this.xpTaskBarBoxHistoricalDepth.HitTaskBoxArea = false;
			this.xpTaskBarBoxHistoricalDepth.HotTrackColor = System.Drawing.Color.Empty;
			this.xpTaskBarBoxHistoricalDepth.ItemBackColor = System.Drawing.Color.White;
			this.xpTaskBarBoxHistoricalDepth.Location = new System.Drawing.Point(0, 0);
			this.xpTaskBarBoxHistoricalDepth.Name = "xpTaskBarBoxHistoricalDepth";
			this.xpTaskBarBoxHistoricalDepth.PreferredChildPanelHeight = 360;
			this.xpTaskBarBoxHistoricalDepth.Size = new System.Drawing.Size(172, 390);
			this.xpTaskBarBoxHistoricalDepth.TabIndex = 0;
			this.xpTaskBarBoxHistoricalDepth.Text = "xxSelectHistoricalData";
			// 
			// gradientPanel2
			// 
			this.gradientPanel2.AutoSize = true;
			this.gradientPanel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel2.Controls.Add(this.gradientPanel3);
			this.gradientPanel2.Location = new System.Drawing.Point(2, 28);
			this.gradientPanel2.Name = "gradientPanel2";
			this.gradientPanel2.Size = new System.Drawing.Size(168, 360);
			this.gradientPanel2.TabIndex = 0;
			// 
			// gradientPanel3
			// 
			this.gradientPanel3.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.None, System.Drawing.Color.White, System.Drawing.Color.White);
			this.gradientPanel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel3.Controls.Add(this.dateSelectionCompositeHistoricalPeriod);
			this.gradientPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel3.Location = new System.Drawing.Point(0, 0);
			this.gradientPanel3.Name = "gradientPanel3";
			this.gradientPanel3.Size = new System.Drawing.Size(168, 360);
			this.gradientPanel3.TabIndex = 2;
			// 
			// dateSelectionCompositeHistoricalPeriod
			// 
			this.dateSelectionCompositeHistoricalPeriod.AutoSize = true;
			this.dateSelectionCompositeHistoricalPeriod.BackColor = System.Drawing.Color.White;
			this.dateSelectionCompositeHistoricalPeriod.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateSelectionCompositeHistoricalPeriod.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateSelectionCompositeHistoricalPeriod.Location = new System.Drawing.Point(0, 0);
			this.dateSelectionCompositeHistoricalPeriod.Name = "dateSelectionCompositeHistoricalPeriod";
			this.dateSelectionCompositeHistoricalPeriod.Size = new System.Drawing.Size(168, 360);
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
			this.xpTaskBarBoxHistoricalDepth.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).EndInit();
			this.gradientPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel3)).EndInit();
			this.gradientPanel3.ResumeLayout(false);
			this.gradientPanel3.PerformLayout();
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