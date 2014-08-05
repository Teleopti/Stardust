namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
    partial class FilterGridToChart
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
                if (components!=null)
                    components.Dispose();
                if (_cachedChartSeries!=null)
                    _cachedChartSeries.Clear();
                if (_gridControl!=null)
                {
					_gridControl.FilterDataToChart -= _gridControl_FilterDataToChart;
					_gridControl.FilterDataSelectionChanged -= GridControl_FilterDataSelectionChanged;
                    _gridControl = null;
                }
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
			this.chartControl1 = new Syncfusion.Windows.Forms.Chart.ChartControl();
			this.splitContainerAdv1 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).BeginInit();
			this.splitContainerAdv1.Panel1.SuspendLayout();
			this.splitContainerAdv1.SuspendLayout();
			this.SuspendLayout();
			// 
			// chartControl1
			// 
			this.chartControl1.BackInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.None, System.Drawing.Color.White, System.Drawing.Color.White);
			this.chartControl1.BorderAppearance.BaseColor = System.Drawing.Color.White;
			this.chartControl1.BorderAppearance.Interior.ForeColor = System.Drawing.Color.White;
			this.chartControl1.ChartArea.CursorLocation = new System.Drawing.Point(0, 0);
			this.chartControl1.ChartArea.CursorReDraw = false;
			this.chartControl1.DataSourceName = "";
			this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chartControl1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.chartControl1.IsWindowLess = false;
			// 
			// 
			// 
			this.chartControl1.Legend.BackInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.None, System.Drawing.Color.White, System.Drawing.Color.White);
			this.chartControl1.Legend.ItemsShadowColor = System.Drawing.Color.White;
			this.chartControl1.Legend.Location = new System.Drawing.Point(58, 31);
			this.chartControl1.Legend.Orientation = Syncfusion.Windows.Forms.Chart.ChartOrientation.Horizontal;
			this.chartControl1.Legend.Position = Syncfusion.Windows.Forms.Chart.ChartDock.Top;
			this.chartControl1.Localize = null;
			this.chartControl1.Location = new System.Drawing.Point(0, 0);
			this.chartControl1.Name = "chartControl1";
			this.chartControl1.PrimaryXAxis.Crossing = double.NaN;
			this.chartControl1.PrimaryXAxis.Margin = true;
			this.chartControl1.PrimaryXAxis.ValueType = Syncfusion.Windows.Forms.Chart.ChartValueType.DateTime;
			this.chartControl1.PrimaryYAxis.Crossing = double.NaN;
			this.chartControl1.PrimaryYAxis.Margin = true;
			this.chartControl1.Size = new System.Drawing.Size(1042, 451);
			this.chartControl1.TabIndex = 0;
			this.chartControl1.TabStop = false;
			// 
			// 
			// 
			this.chartControl1.Title.Name = "Def_title";
			this.chartControl1.Title.Text = "";
			this.chartControl1.Titles.Add(this.chartControl1.Title);
			this.chartControl1.LayoutCompleted += new System.EventHandler(this.chartControl1_LayoutCompleted);
			this.chartControl1.ChartRegionClick += new Syncfusion.Windows.Forms.Chart.ChartRegionMouseEventHandler(this.chartControl1_ChartRegionClick);
			// 
			// splitContainerAdv1
			// 
			this.splitContainerAdv1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.LightGray);
			this.splitContainerAdv1.BeforeTouchSize = 3;
			this.splitContainerAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdv1.Location = new System.Drawing.Point(0, 0);
			this.splitContainerAdv1.Name = "splitContainerAdv1";
			this.splitContainerAdv1.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// splitContainerAdv1.Panel1
			// 
			this.splitContainerAdv1.Panel1.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainerAdv1.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv1.Panel1.Controls.Add(this.chartControl1);
			// 
			// splitContainerAdv1.Panel2
			// 
			this.splitContainerAdv1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
			this.splitContainerAdv1.Size = new System.Drawing.Size(1042, 665);
			this.splitContainerAdv1.SplitterDistance = 451;
			this.splitContainerAdv1.SplitterWidth = 3;
			this.splitContainerAdv1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdv1.TabIndex = 0;
			this.splitContainerAdv1.Text = "splitContainerAdv5";
			this.splitContainerAdv1.ThemesEnabled = true;
			// 
			// FilterGridToChart
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainerAdv1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FilterGridToChart";
			this.Size = new System.Drawing.Size(1042, 665);
			this.SizeChanged += new System.EventHandler(this.GridToChart_SizeChanged);
			this.splitContainerAdv1.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).EndInit();
			this.splitContainerAdv1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Chart.ChartControl chartControl1;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv1;
    }
}