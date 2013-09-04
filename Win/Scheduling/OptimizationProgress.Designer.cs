namespace Teleopti.Ccc.Win.Scheduling
{
    partial class OptimizationProgress
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chartControl1 = new Syncfusion.Windows.Forms.Chart.ChartControl();
            this.SuspendLayout();
            // 
            // chartControl1
            // 
            this.chartControl1.BorderAppearance.BaseColor = System.Drawing.Color.Teal;
            this.chartControl1.ChartArea.XAxesLayoutMode = Syncfusion.Windows.Forms.Chart.ChartAxesLayoutMode.SideBySide;
            this.chartControl1.ChartArea.YAxesLayoutMode = Syncfusion.Windows.Forms.Chart.ChartAxesLayoutMode.SideBySide;
            this.chartControl1.ChartAreaMargins = new Syncfusion.Windows.Forms.Chart.ChartMargins(10, 10, 20, 10);
            this.chartControl1.ChartInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Black);
            this.chartControl1.Depth = 1F;
            this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartControl1.ForeColor = System.Drawing.Color.MidnightBlue;
            // 
            // 
            // 
            this.chartControl1.Legend.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.chartControl1.Legend.ForeColor = System.Drawing.Color.MidnightBlue;
            this.chartControl1.Legend.Location = new System.Drawing.Point(85, 419);
            this.chartControl1.Legend.Orientation = Syncfusion.Windows.Forms.Chart.ChartOrientation.Horizontal;
            this.chartControl1.Legend.Position = Syncfusion.Windows.Forms.Chart.ChartDock.Bottom;
            this.chartControl1.Legend.Visible = false;
            this.chartControl1.Location = new System.Drawing.Point(0, 0);
            this.chartControl1.Name = "chartControl1";
            this.chartControl1.PrimaryXAxis.ForeColor = System.Drawing.Color.MidnightBlue;
            this.chartControl1.PrimaryXAxis.GridLineType.ForeColor = System.Drawing.Color.Gray;
            this.chartControl1.PrimaryXAxis.LineType.ForeColor = System.Drawing.Color.MidnightBlue;
            this.chartControl1.PrimaryXAxis.TitleColor = System.Drawing.Color.MidnightBlue;
            this.chartControl1.PrimaryYAxis.ForeColor = System.Drawing.Color.MidnightBlue;
            this.chartControl1.PrimaryYAxis.LineType.BackColor = System.Drawing.Color.MidnightBlue;
            this.chartControl1.PrimaryYAxis.LineType.ForeColor = System.Drawing.Color.MidnightBlue;
            this.chartControl1.PrimaryYAxis.TitleColor = System.Drawing.Color.MidnightBlue;
            this.chartControl1.Rotation = 0.1F;
            this.chartControl1.Size = new System.Drawing.Size(408, 322);
            this.chartControl1.TabIndex = 1;
            this.chartControl1.Text = "Progress";
            this.chartControl1.Tilt = 0.1F;
            // 
            // 
            // 
            this.chartControl1.Title.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.chartControl1.Title.ForeColor = System.Drawing.Color.MidnightBlue;
            this.chartControl1.Title.Name = "Def_title";
            this.chartControl1.Title.Text = "Progress";
            this.chartControl1.Titles.Add(this.chartControl1.Title);
            // 
            // OptimizationProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 322);
            this.Controls.Add(this.chartControl1);
            this.Name = "OptimizationProgress";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.OptimizationProgress_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Chart.ChartControl chartControl1;
    }
}