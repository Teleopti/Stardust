using Syncfusion.Windows.Forms;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SeasonPages
{
    partial class WorkflowTrendView
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
            this.gradientPanel2 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.gradientPanelExt1 = new Syncfusion.Windows.Forms.Tools.GradientPanelExt();
            this.trackBarExTrend = new Syncfusion.Windows.Forms.Tools.TrackBarEx(-100, 500);
            this.percentTextBoxTrend = new Syncfusion.Windows.Forms.Tools.PercentTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.xxResetYearlyTrend = new Syncfusion.Windows.Forms.ButtonAdv();
            this.checkBoxAdvUseTrend = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.label2 = new System.Windows.Forms.Label();
            this.xpTaskBarBox2 = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
            this.xpTaskBar1 = new Syncfusion.Windows.Forms.Tools.XPTaskBar();
            this.splitContainerAdv2 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).BeginInit();
            this.gradientPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelExt1)).BeginInit();
            this.gradientPanelExt1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.percentTextBoxTrend)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvUseTrend)).BeginInit();
            this.xpTaskBarBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).BeginInit();
            this.xpTaskBar1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv2)).BeginInit();
            this.splitContainerAdv2.Panel2.SuspendLayout();
            this.splitContainerAdv2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gradientPanel2
            // 
            this.gradientPanel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.InactiveCaptionText);
            this.gradientPanel2.BorderColor = System.Drawing.Color.Black;
            this.gradientPanel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanel2.Controls.Add(this.gradientPanelExt1);
            this.gradientPanel2.Controls.Add(this.checkBoxAdvUseTrend);
            this.gradientPanel2.Controls.Add(this.label2);
            this.gradientPanel2.Location = new System.Drawing.Point(2, 27);
            this.gradientPanel2.Name = "gradientPanel2";
            this.gradientPanel2.Size = new System.Drawing.Size(166, 140);
            this.gradientPanel2.TabIndex = 0;
            // 
            // gradientPanelExt1
            // 
            this.gradientPanelExt1.AccessibleDescription = "";
            this.gradientPanelExt1.AccessibleName = "";
            this.gradientPanelExt1.BackColor = System.Drawing.Color.Transparent;
            this.gradientPanelExt1.BorderColor = System.Drawing.Color.Black;
            this.gradientPanelExt1.BorderGap = 3;
            this.gradientPanelExt1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelExt1.Controls.Add(this.trackBarExTrend);
            this.gradientPanelExt1.Controls.Add(this.percentTextBoxTrend);
            this.gradientPanelExt1.Controls.Add(this.label1);
            this.gradientPanelExt1.Controls.Add(this.xxResetYearlyTrend);
            this.gradientPanelExt1.CornerRadius = 5;
            this.gradientPanelExt1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gradientPanelExt1.ExpandLocation = new System.Drawing.Point(0, 0);
            this.gradientPanelExt1.ExpandSize = new System.Drawing.Size(0, 0);
            this.gradientPanelExt1.Location = new System.Drawing.Point(0, 44);
            this.gradientPanelExt1.Name = "gradientPanelExt1";
            this.gradientPanelExt1.Size = new System.Drawing.Size(166, 96);
            this.gradientPanelExt1.TabIndex = 7;
            // 
            // trackBarExTrend
            // 
            this.trackBarExTrend.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.trackBarExTrend.Location = new System.Drawing.Point(10, 67);
            this.trackBarExTrend.Name = "trackBarExTrend";
            this.trackBarExTrend.Size = new System.Drawing.Size(145, 20);
            this.trackBarExTrend.TabIndex = 4;
            this.trackBarExTrend.TimerInterval = 100;
            this.trackBarExTrend.TrackBarGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.trackBarExTrend.TrackBarGradientStart = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(247)))), ((int)(((byte)(255)))));
            this.trackBarExTrend.Value = 0;
            this.trackBarExTrend.ValueChanged += new System.EventHandler(this.trackBarExTrend_ValueChanged);
            // 
            // percentTextBoxTrend
            // 
            this.percentTextBoxTrend.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.percentTextBoxTrend.DoubleValue = 0;
            this.percentTextBoxTrend.Location = new System.Drawing.Point(98, 43);
            this.percentTextBoxTrend.MaxValue = 500;
            this.percentTextBoxTrend.MinValue = -100;
            this.percentTextBoxTrend.Name = "percentTextBoxTrend";
            this.percentTextBoxTrend.NegativeInputPendingOnSelectAll = true;
            this.percentTextBoxTrend.NullString = "0,00 %";
            this.percentTextBoxTrend.OverflowIndicatorToolTipText = null;
            this.percentTextBoxTrend.Size = new System.Drawing.Size(58, 20);
            this.percentTextBoxTrend.TabIndex = 5;
            this.percentTextBoxTrend.KeyDown += new System.Windows.Forms.KeyEventHandler(this.percentTextBoxTrend_KeyDown);
            this.percentTextBoxTrend.Leave += new System.EventHandler(this.percentTextBoxTrend_Leave);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "xxYearlyTrend";
            // 
            // xxResetYearlyTrend
            // 
            this.xxResetYearlyTrend.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.xxResetYearlyTrend.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.xxResetYearlyTrend.Location = new System.Drawing.Point(10, 13);
            this.xxResetYearlyTrend.Name = "xxResetYearlyTrend";
            this.xxResetYearlyTrend.Size = new System.Drawing.Size(146, 19);
            this.xxResetYearlyTrend.TabIndex = 6;
            this.xxResetYearlyTrend.Text = "xxResetTrend";
            this.xxResetYearlyTrend.UseVisualStyle = true;
            this.xxResetYearlyTrend.Click += new System.EventHandler(this.xxResetYearlyTrend_Click);
            // 
            // checkBoxAdvUseTrend
            // 
            this.checkBoxAdvUseTrend.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.checkBoxAdvUseTrend.BackColor = System.Drawing.Color.Transparent;
            this.checkBoxAdvUseTrend.BorderColor = System.Drawing.SystemColors.WindowFrame;
            this.checkBoxAdvUseTrend.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxAdvUseTrend.GradientEnd = System.Drawing.SystemColors.ControlDark;
            this.checkBoxAdvUseTrend.GradientStart = System.Drawing.SystemColors.Control;
            this.checkBoxAdvUseTrend.HotBorderColor = System.Drawing.SystemColors.WindowFrame;
            this.checkBoxAdvUseTrend.ImageCheckBoxSize = new System.Drawing.Size(13, 13);
            this.checkBoxAdvUseTrend.Location = new System.Drawing.Point(3, 6);
            this.checkBoxAdvUseTrend.Name = "checkBoxAdvUseTrend";
            this.checkBoxAdvUseTrend.ShadowColor = System.Drawing.Color.Black;
            this.checkBoxAdvUseTrend.ShadowOffset = new System.Drawing.Point(2, 2);
            this.checkBoxAdvUseTrend.Size = new System.Drawing.Size(158, 21);
            this.checkBoxAdvUseTrend.TabIndex = 3;
            this.checkBoxAdvUseTrend.Text = "xxUseTrend";
            this.checkBoxAdvUseTrend.ThemesEnabled = true;
            this.checkBoxAdvUseTrend.CheckStateChanged += new System.EventHandler(this.checkBoxAdvUseTrend_CheckStateChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(189, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "%";
            // 
            // xpTaskBarBox2
            // 
            this.xpTaskBarBox2.Controls.Add(this.gradientPanel2);
            this.xpTaskBarBox2.HeaderBackColor = System.Drawing.SystemColors.ActiveCaption;
            this.xpTaskBarBox2.HeaderImageIndex = -1;
            this.xpTaskBarBox2.HitTaskBoxArea = false;
            this.xpTaskBarBox2.ItemBackColor = System.Drawing.SystemColors.Control;
            this.xpTaskBarBox2.Location = new System.Drawing.Point(0, 0);
            this.xpTaskBarBox2.Name = "xpTaskBarBox2";
            this.xpTaskBarBox2.PreferredChildPanelHeight = 140;
            this.xpTaskBarBox2.ShowCollapseButton = false;
            this.xpTaskBarBox2.Size = new System.Drawing.Size(170, 169);
            this.xpTaskBarBox2.TabIndex = 1;
            this.xpTaskBarBox2.Text = "xxSetTrendManually";
            // 
            // xpTaskBar1
            // 
            this.xpTaskBar1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.xpTaskBar1.Controls.Add(this.xpTaskBarBox2);
            this.xpTaskBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xpTaskBar1.Location = new System.Drawing.Point(0, 0);
            this.xpTaskBar1.MinimumSize = new System.Drawing.Size(0, 0);
            this.xpTaskBar1.Name = "xpTaskBar1";
            this.xpTaskBar1.Size = new System.Drawing.Size(170, 600);
            this.xpTaskBar1.TabIndex = 1;
            this.xpTaskBar1.ThemesEnabled = true;
            // 
            // splitContainerAdv2
            // 
            this.splitContainerAdv2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerAdv2.IsSplitterFixed = true;
            this.splitContainerAdv2.Location = new System.Drawing.Point(0, 0);
            this.splitContainerAdv2.Name = "splitContainerAdv2";
            // 
            // splitContainerAdv2.Panel1
            // 
            this.splitContainerAdv2.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            // 
            // splitContainerAdv2.Panel2
            // 
            this.splitContainerAdv2.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdv2.Panel2.Controls.Add(this.xpTaskBar1);
            this.splitContainerAdv2.Size = new System.Drawing.Size(1060, 600);
            this.splitContainerAdv2.SplitterDistance = 885;
            this.splitContainerAdv2.SplitterWidth = 5;
            this.splitContainerAdv2.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.splitContainerAdv2.TabIndex = 3;
            this.splitContainerAdv2.Text = "splitContainerAdv4";
            this.splitContainerAdv2.DoubleClick += new System.EventHandler(this.splitContainerAdv2_DoubleClick);
            // 
            // WorkflowTrendView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.splitContainerAdv2);
            this.Name = "WorkflowTrendView";
            this.Size = new System.Drawing.Size(1060, 600);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).EndInit();
            this.gradientPanel2.ResumeLayout(false);
            this.gradientPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelExt1)).EndInit();
            this.gradientPanelExt1.ResumeLayout(false);
            this.gradientPanelExt1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.percentTextBoxTrend)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvUseTrend)).EndInit();
            this.xpTaskBarBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).EndInit();
            this.xpTaskBar1.ResumeLayout(false);
            this.splitContainerAdv2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv2)).EndInit();
            this.splitContainerAdv2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel2;
        private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpTaskBarBox2;
        private Syncfusion.Windows.Forms.Tools.XPTaskBar xpTaskBar1;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv2;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvUseTrend;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private Syncfusion.Windows.Forms.Tools.TrackBarEx trackBarExTrend;
        private Syncfusion.Windows.Forms.Tools.PercentTextBox percentTextBoxTrend;
        private ButtonAdv xxResetYearlyTrend;
        private Syncfusion.Windows.Forms.Tools.GradientPanelExt gradientPanelExt1;
    }
}
