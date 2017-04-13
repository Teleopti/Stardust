using Teleopti.Ccc.Win.Forecasting.Forms;

namespace Teleopti.Ccc.Win.Common.Controls.Chart
{
	partial class GridRowInChartSettingButtons
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
				this.checkBox1.CheckedChanged -= new System.EventHandler(this.checkBox1CheckedChanged);
				this.buttonAdvRightAxis.Click -= new System.EventHandler(this.buttonAdvRightAxisClick);
				this.buttonAdvBar.Click -= new System.EventHandler(this.buttonAdvBarClick);
				this.buttonAdvLine.Click -= new System.EventHandler(this.buttonAdvLineClick);
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridRowInChartSettingButtons));
			this.buttonAdvLine = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvBar = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvLeftAxis = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvRightAxis = new Syncfusion.Windows.Forms.ButtonAdv();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.pickColorControl1 = new Teleopti.Ccc.Win.Forecasting.Forms.PickColorControl();
			this.SuspendLayout();
			// 
			// buttonAdvLine
			// 
			this.buttonAdvLine.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvLine.BackColor = System.Drawing.Color.White;
			this.buttonAdvLine.BeforeTouchSize = new System.Drawing.Size(32, 32);
			this.buttonAdvLine.ForeColor = System.Drawing.Color.White;
			this.buttonAdvLine.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_LineGraph;
			this.buttonAdvLine.IsBackStageButton = false;
			this.buttonAdvLine.Location = new System.Drawing.Point(42, 2);
			this.buttonAdvLine.Name = "buttonAdvLine";
			this.buttonAdvLine.Size = new System.Drawing.Size(32, 32);
			this.buttonAdvLine.TabIndex = 3;
			this.buttonAdvLine.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.buttonAdvLine.UseVisualStyle = true;
			this.buttonAdvLine.Click += new System.EventHandler(this.buttonAdvLineClick);
			// 
			// buttonAdvBar
			// 
			this.buttonAdvBar.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvBar.BackColor = System.Drawing.Color.White;
			this.buttonAdvBar.BeforeTouchSize = new System.Drawing.Size(32, 32);
			this.buttonAdvBar.ForeColor = System.Drawing.Color.White;
			this.buttonAdvBar.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_BarGraph;
			this.buttonAdvBar.IsBackStageButton = false;
			this.buttonAdvBar.Location = new System.Drawing.Point(76, 2);
			this.buttonAdvBar.Name = "buttonAdvBar";
			this.buttonAdvBar.Size = new System.Drawing.Size(32, 32);
			this.buttonAdvBar.TabIndex = 2;
			this.buttonAdvBar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.buttonAdvBar.UseVisualStyle = true;
			this.buttonAdvBar.Click += new System.EventHandler(this.buttonAdvBarClick);
			// 
			// buttonAdvLeftAxis
			// 
			this.buttonAdvLeftAxis.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvLeftAxis.BackColor = System.Drawing.Color.White;
			this.buttonAdvLeftAxis.BeforeTouchSize = new System.Drawing.Size(32, 32);
			this.buttonAdvLeftAxis.ForeColor = System.Drawing.Color.White;
			this.buttonAdvLeftAxis.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_ChartLeftAxis2;
			this.buttonAdvLeftAxis.IsBackStageButton = false;
			this.buttonAdvLeftAxis.Location = new System.Drawing.Point(42, 36);
			this.buttonAdvLeftAxis.Name = "buttonAdvLeftAxis";
			this.buttonAdvLeftAxis.Size = new System.Drawing.Size(32, 32);
			this.buttonAdvLeftAxis.TabIndex = 1;
			this.buttonAdvLeftAxis.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.buttonAdvLeftAxis.UseVisualStyle = true;
			this.buttonAdvLeftAxis.Click += new System.EventHandler(this.buttonAdvLeftAxisClick);
			// 
			// buttonAdvRightAxis
			// 
			this.buttonAdvRightAxis.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvRightAxis.BackColor = System.Drawing.Color.White;
			this.buttonAdvRightAxis.BeforeTouchSize = new System.Drawing.Size(32, 32);
			this.buttonAdvRightAxis.ForeColor = System.Drawing.Color.White;
			this.buttonAdvRightAxis.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_ChartRightAxis2;
			this.buttonAdvRightAxis.IsBackStageButton = false;
			this.buttonAdvRightAxis.Location = new System.Drawing.Point(76, 36);
			this.buttonAdvRightAxis.Name = "buttonAdvRightAxis";
			this.buttonAdvRightAxis.Size = new System.Drawing.Size(32, 32);
			this.buttonAdvRightAxis.TabIndex = 0;
			this.buttonAdvRightAxis.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.buttonAdvRightAxis.UseVisualStyle = true;
			this.buttonAdvRightAxis.Click += new System.EventHandler(this.buttonAdvRightAxisClick);
			// 
			// checkBox1
			// 
			this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox1.ImageIndex = 0;
			this.checkBox1.ImageList = this.imageList1;
			this.checkBox1.Location = new System.Drawing.Point(0, 3);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(32, 32);
			this.checkBox1.TabIndex = 5;
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1CheckedChanged);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ccc_DisableChart.png");
			this.imageList1.Images.SetKeyName(1, "ccc_EnableGraph.png");
			// 
			// pickColorControl1
			// 
			this.pickColorControl1.BackColor = System.Drawing.Color.Transparent;
			this.pickColorControl1.Location = new System.Drawing.Point(115, 2);
			this.pickColorControl1.Name = "pickColorControl1";
			this.pickColorControl1.Size = new System.Drawing.Size(100, 66);
			this.pickColorControl1.TabIndex = 4;
			this.pickColorControl1.ThisColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
			// 
			// GridRowInChartSettingButtons
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.pickColorControl1);
			this.Controls.Add(this.buttonAdvLine);
			this.Controls.Add(this.buttonAdvBar);
			this.Controls.Add(this.buttonAdvLeftAxis);
			this.Controls.Add(this.buttonAdvRightAxis);
			this.Name = "GridRowInChartSettingButtons";
			this.Size = new System.Drawing.Size(217, 71);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRightAxis;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvLeftAxis;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvBar;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvLine;
		private PickColorControl pickColorControl1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.ImageList imageList1;
	}
}