using DateSelectionComposite=Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionComposite;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SeasonPages
{
    partial class WorkflowSeasonView
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
			this.splitContainerAdv2 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.xpTaskBar1 = new Syncfusion.Windows.Forms.Tools.XPTaskBar();
			this.xpTaskBarBox1 = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
			this.gradientPanel2 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.gradientPanel3 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.dateSelectionComposite1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionComposite();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).BeginInit();
			this.splitContainerAdv1.Panel1.SuspendLayout();
			this.splitContainerAdv1.Panel2.SuspendLayout();
			this.splitContainerAdv1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv2)).BeginInit();
			this.splitContainerAdv2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).BeginInit();
			this.xpTaskBar1.SuspendLayout();
			this.xpTaskBarBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).BeginInit();
			this.gradientPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel3)).BeginInit();
			this.gradientPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainerAdv1
			// 
			this.splitContainerAdv1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.DarkGray);
			this.splitContainerAdv1.BeforeTouchSize = 3;
			this.splitContainerAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdv1.HotBackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
			this.splitContainerAdv1.Location = new System.Drawing.Point(0, 0);
			this.splitContainerAdv1.Name = "splitContainerAdv1";
			// 
			// splitContainerAdv1.Panel1
			// 
			this.splitContainerAdv1.Panel1.BackColor = System.Drawing.Color.White;
			this.splitContainerAdv1.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv1.Panel1.Controls.Add(this.splitContainerAdv2);
			// 
			// splitContainerAdv1.Panel2
			// 
			this.splitContainerAdv1.Panel2.BackColor = System.Drawing.Color.White;
			this.splitContainerAdv1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv1.Panel2.Controls.Add(this.xpTaskBar1);
			this.splitContainerAdv1.Panel2.MinimumSize = new System.Drawing.Size(175, 0);
			this.splitContainerAdv1.Size = new System.Drawing.Size(1060, 600);
			this.splitContainerAdv1.SplitterDistance = 885;
			this.splitContainerAdv1.SplitterWidth = 3;
			this.splitContainerAdv1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdv1.TabIndex = 0;
			this.splitContainerAdv1.Text = "splitContainerAdv1";
			this.splitContainerAdv1.DoubleClick += new System.EventHandler(this.splitContainerAdv1_DoubleClick);
			// 
			// splitContainerAdv2
			// 
			this.splitContainerAdv2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.DarkGray);
			this.splitContainerAdv2.BeforeTouchSize = 3;
			this.splitContainerAdv2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdv2.Location = new System.Drawing.Point(0, 0);
			this.splitContainerAdv2.Name = "splitContainerAdv2";
			this.splitContainerAdv2.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// splitContainerAdv2.Panel1
			// 
			this.splitContainerAdv2.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			// 
			// splitContainerAdv2.Panel2
			// 
			this.splitContainerAdv2.Panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.splitContainerAdv2.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv2.Size = new System.Drawing.Size(885, 600);
			this.splitContainerAdv2.SplitterDistance = 450;
			this.splitContainerAdv2.SplitterWidth = 3;
			this.splitContainerAdv2.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdv2.TabIndex = 0;
			this.splitContainerAdv2.Text = "splitContainerAdv2";
			// 
			// xpTaskBar1
			// 
			this.xpTaskBar1.BackColor = System.Drawing.Color.White;
			this.xpTaskBar1.BeforeTouchSize = new System.Drawing.Size(175, 600);
			this.xpTaskBar1.BorderColor = System.Drawing.Color.Black;
			this.xpTaskBar1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.xpTaskBar1.Controls.Add(this.xpTaskBarBox1);
			this.xpTaskBar1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.xpTaskBar1.Location = new System.Drawing.Point(0, 0);
			this.xpTaskBar1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.xpTaskBar1.MinimumSize = new System.Drawing.Size(0, 0);
			this.xpTaskBar1.Name = "xpTaskBar1";
			this.xpTaskBar1.Size = new System.Drawing.Size(175, 600);
			this.xpTaskBar1.Style = Syncfusion.Windows.Forms.Tools.XPTaskBarStyle.Metro;
			this.xpTaskBar1.TabIndex = 0;
			// 
			// xpTaskBarBox1
			// 
			this.xpTaskBarBox1.Controls.Add(this.gradientPanel2);
			this.xpTaskBarBox1.ForeColor = System.Drawing.Color.White;
			this.xpTaskBarBox1.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.xpTaskBarBox1.HeaderForeColor = System.Drawing.Color.White;
			this.xpTaskBarBox1.HeaderImageIndex = -1;
			this.xpTaskBarBox1.HitTaskBoxArea = false;
			this.xpTaskBarBox1.HotTrackColor = System.Drawing.Color.White;
			this.xpTaskBarBox1.ItemBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(191)))), ((int)(((byte)(237)))));
			this.xpTaskBarBox1.Location = new System.Drawing.Point(0, 0);
			this.xpTaskBarBox1.Name = "xpTaskBarBox1";
			this.xpTaskBarBox1.PADY = 2;
			this.xpTaskBarBox1.PreferredChildPanelHeight = 360;
			this.xpTaskBarBox1.Size = new System.Drawing.Size(173, 385);
			this.xpTaskBarBox1.TabIndex = 0;
			this.xpTaskBarBox1.Text = "xxSelectHistoricalData";
			// 
			// gradientPanel2
			// 
			this.gradientPanel2.BorderColor = System.Drawing.Color.White;
			this.gradientPanel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel2.Controls.Add(this.gradientPanel3);
			this.gradientPanel2.Location = new System.Drawing.Point(2, 23);
			this.gradientPanel2.Name = "gradientPanel2";
			this.gradientPanel2.Size = new System.Drawing.Size(169, 360);
			this.gradientPanel2.TabIndex = 0;
			// 
			// gradientPanel3
			// 
			this.gradientPanel3.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.White, System.Drawing.Color.White);
			this.gradientPanel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel3.Controls.Add(this.dateSelectionComposite1);
			this.gradientPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel3.Location = new System.Drawing.Point(0, 0);
			this.gradientPanel3.Name = "gradientPanel3";
			this.gradientPanel3.Size = new System.Drawing.Size(169, 360);
			this.gradientPanel3.TabIndex = 2;
			// 
			// dateSelectionComposite1
			// 
			this.dateSelectionComposite1.AutoSize = true;
			this.dateSelectionComposite1.BackColor = System.Drawing.Color.White;
			this.dateSelectionComposite1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateSelectionComposite1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateSelectionComposite1.Location = new System.Drawing.Point(0, 0);
			this.dateSelectionComposite1.Name = "dateSelectionComposite1";
			this.dateSelectionComposite1.Size = new System.Drawing.Size(169, 360);
			this.dateSelectionComposite1.TabIndex = 0;
			this.dateSelectionComposite1.DateRangeChanged += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.DateSelection.DateRangeChangedEventArgs>(this.dateSelectionCompositeHistoricalPeriod_DateRangeChanged);
			// 
			// WorkflowSeasonView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.splitContainerAdv1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "WorkflowSeasonView";
			this.Size = new System.Drawing.Size(1060, 600);
			this.splitContainerAdv1.Panel1.ResumeLayout(false);
			this.splitContainerAdv1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).EndInit();
			this.splitContainerAdv1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv2)).EndInit();
			this.splitContainerAdv2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).EndInit();
			this.xpTaskBar1.ResumeLayout(false);
			this.xpTaskBarBox1.ResumeLayout(false);
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
        private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpTaskBarBox1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel2;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv2;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel3;
        private DateSelectionComposite dateSelectionComposite1;

    }
}