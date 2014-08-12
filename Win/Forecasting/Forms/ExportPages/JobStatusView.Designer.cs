namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
	partial class JobStatusView
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
				if (components != null)
				components.Dispose();
				ReleaseManagedResources();
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
			this.components = new System.ComponentModel.Container();
			this.labelTitle = new System.Windows.Forms.Label();
			this.labelDetail = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.progressBar1 = new Syncfusion.Windows.Forms.Tools.ProgressBarAdv();
			this.buttonAdvClose = new Syncfusion.Windows.Forms.ButtonAdv();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.progressBar1)).BeginInit();
			this.SuspendLayout();
			// 
			// labelTitle
			// 
			this.labelTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTitle.AutoSize = true;
			this.labelTitle.BackColor = System.Drawing.Color.Transparent;
			this.labelTitle.Location = new System.Drawing.Point(3, 16);
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.Size = new System.Drawing.Size(157, 17);
			this.labelTitle.TabIndex = 1;
			this.labelTitle.Text = "xxRunningBackgroundJob";
			// 
			// labelDetail
			// 
			this.labelDetail.AutoSize = true;
			this.labelDetail.BackColor = System.Drawing.Color.Transparent;
			this.labelDetail.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelDetail.Location = new System.Drawing.Point(3, 49);
			this.labelDetail.Name = "labelDetail";
			this.labelDetail.Size = new System.Drawing.Size(402, 81);
			this.labelDetail.TabIndex = 3;
			this.labelDetail.Text = "label2";
			this.labelDetail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Window;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvClose, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.labelDetail, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelTitle, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.progressBar1, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(408, 220);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.BackColor = System.Drawing.Color.White;
			this.progressBar1.BackGradientEndColor = System.Drawing.Color.White;
			this.progressBar1.BackGradientStartColor = System.Drawing.Color.White;
			this.progressBar1.BackMultipleColors = new System.Drawing.Color[0];
			this.progressBar1.BackSegments = false;
			this.progressBar1.BackTubeEndColor = System.Drawing.Color.White;
			this.progressBar1.BackTubeStartColor = System.Drawing.Color.White;
			this.progressBar1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.progressBar1.CustomText = null;
			this.progressBar1.CustomWaitingRender = false;
			this.progressBar1.FontColor = System.Drawing.Color.White;
			this.progressBar1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.progressBar1.ForegroundImage = null;
			this.progressBar1.GradientEndColor = System.Drawing.Color.Lime;
			this.progressBar1.GradientStartColor = System.Drawing.Color.Red;
			this.progressBar1.Location = new System.Drawing.Point(0, 139);
			this.progressBar1.Margin = new System.Windows.Forms.Padding(0);
			this.progressBar1.MultipleColors = new System.Drawing.Color[0];
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.SegmentWidth = 12;
			this.progressBar1.Size = new System.Drawing.Size(408, 31);
			this.progressBar1.TabIndex = 0;
			this.progressBar1.ThemesEnabled = false;
			this.progressBar1.TubeEndColor = System.Drawing.Color.Black;
			this.progressBar1.TubeStartColor = System.Drawing.Color.Red;
			this.progressBar1.WaitingGradientWidth = 400;
			// 
			// buttonAdvClose
			// 
			this.buttonAdvClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvClose.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvClose.ForeColor = System.Drawing.Color.White;
			this.buttonAdvClose.IsBackStageButton = false;
			this.buttonAdvClose.Location = new System.Drawing.Point(311, 183);
			this.buttonAdvClose.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvClose.Name = "buttonAdvClose";
			this.buttonAdvClose.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvClose.TabIndex = 4;
			this.buttonAdvClose.Text = "xxHide";
			this.buttonAdvClose.UseVisualStyle = true;
			this.buttonAdvClose.Click += new System.EventHandler(this.buttonAdvClose_Click);
			// 
			// JobStatusView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(408, 220);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "JobStatusView";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxRunningBackgroundJob";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.progressBar1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelTitle;
		private System.Windows.Forms.Label labelDetail;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvClose;
		  private Syncfusion.Windows.Forms.Tools.ProgressBarAdv progressBar1;
		private System.Windows.Forms.ToolTip toolTip1;

	}
}