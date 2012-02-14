namespace Teleopti.Ccc.Win.Scheduling
{
	partial class FindMatching
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
			this.labelInfo = new System.Windows.Forms.Label();
			this.listViewResult = new System.Windows.Forms.ListView();
			this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderMatching = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderStartTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderEnd = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderWorkTimeMin = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderWorkTimeMax = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.buttonAssign = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelInfo
			// 
			this.labelInfo.AutoSize = true;
			this.labelInfo.Location = new System.Drawing.Point(12, 9);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(35, 13);
			this.labelInfo.TabIndex = 0;
			this.labelInfo.Text = "label1";
			// 
			// listViewResult
			// 
			this.listViewResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listViewResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderMatching,
            this.columnHeaderStartTime,
            this.columnHeaderEnd,
            this.columnHeaderWorkTimeMin,
            this.columnHeaderWorkTimeMax});
			this.listViewResult.FullRowSelect = true;
			this.listViewResult.Location = new System.Drawing.Point(12, 25);
			this.listViewResult.MultiSelect = false;
			this.listViewResult.Name = "listViewResult";
			this.listViewResult.Size = new System.Drawing.Size(666, 399);
			this.listViewResult.TabIndex = 1;
			this.listViewResult.UseCompatibleStateImageBehavior = false;
			this.listViewResult.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderName
			// 
			this.columnHeaderName.Text = "xxName";
			this.columnHeaderName.Width = 180;
			// 
			// columnHeaderMatching
			// 
			this.columnHeaderMatching.Text = "xxMatch";
			// 
			// columnHeaderStartTime
			// 
			this.columnHeaderStartTime.Text = "xxStart";
			// 
			// columnHeaderEnd
			// 
			this.columnHeaderEnd.Text = "xxEnd";
			// 
			// columnHeaderWorkTimeMin
			// 
			this.columnHeaderWorkTimeMin.Text = "xxWorkTimeMin";
			// 
			// columnHeaderWorkTimeMax
			// 
			this.columnHeaderWorkTimeMax.Text = "xxWorkTimeMax";
			// 
			// buttonAssign
			// 
			this.buttonAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAssign.Location = new System.Drawing.Point(603, 430);
			this.buttonAssign.Name = "buttonAssign";
			this.buttonAssign.Size = new System.Drawing.Size(75, 23);
			this.buttonAssign.TabIndex = 2;
			this.buttonAssign.Text = "xxAssign";
			this.buttonAssign.UseVisualStyleBackColor = true;
			this.buttonAssign.Click += new System.EventHandler(this.buttonAssign_Click);
			// 
			// FindMatching
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(690, 465);
			this.Controls.Add(this.buttonAssign);
			this.Controls.Add(this.listViewResult);
			this.Controls.Add(this.labelInfo);
			this.MinimizeBox = false;
			this.Name = "FindMatching";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "FindMatching";
			this.Load += new System.EventHandler(this.FindMatching_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelInfo;
		private System.Windows.Forms.ListView listViewResult;
		private System.Windows.Forms.ColumnHeader columnHeaderName;
		private System.Windows.Forms.ColumnHeader columnHeaderStartTime;
		private System.Windows.Forms.ColumnHeader columnHeaderEnd;
		private System.Windows.Forms.ColumnHeader columnHeaderWorkTimeMin;
		private System.Windows.Forms.Button buttonAssign;
		private System.Windows.Forms.ColumnHeader columnHeaderMatching;
		private System.Windows.Forms.ColumnHeader columnHeaderWorkTimeMax;
	}
}