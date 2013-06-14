namespace Teleopti.Ccc.Win.Scheduling
{
	partial class FindMatchingNew
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindMatchingNew));
			this.listViewResult = new System.Windows.Forms.ListView();
			this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderWorkedYesterday = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderAvailiableToday = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderMatching = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderWorksTomorrow = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderNightRestOK = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.buttonAssign = new System.Windows.Forms.Button();
			this.ribbonControlHeader = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlHeader)).BeginInit();
			this.SuspendLayout();
			// 
			// listViewResult
			// 
			this.listViewResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderWorkedYesterday,
            this.columnHeaderAvailiableToday,
            this.columnHeaderMatching,
            this.columnHeaderWorksTomorrow,
            this.columnHeaderNightRestOK});
			this.listViewResult.FullRowSelect = true;
			this.listViewResult.Location = new System.Drawing.Point(12, 45);
			this.listViewResult.MultiSelect = false;
			this.listViewResult.Name = "listViewResult";
			this.listViewResult.Size = new System.Drawing.Size(706, 389);
			this.listViewResult.SmallImageList = this.imageList1;
			this.listViewResult.TabIndex = 0;
			this.listViewResult.UseCompatibleStateImageBehavior = false;
			this.listViewResult.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderName
			// 
			this.columnHeaderName.Text = "xxName";
			this.columnHeaderName.Width = 184;
			// 
			// columnHeaderWorkedYesterday
			// 
			this.columnHeaderWorkedYesterday.Text = "xxWorkedYesterday";
			this.columnHeaderWorkedYesterday.Width = 109;
			// 
			// columnHeaderAvailiableToday
			// 
			this.columnHeaderAvailiableToday.Text = "xxAvaliableToday";
			this.columnHeaderAvailiableToday.Width = 107;
			// 
			// columnHeaderMatching
			// 
			this.columnHeaderMatching.Text = "xxMatching";
			this.columnHeaderMatching.Width = 77;
			// 
			// columnHeaderWorksTomorrow
			// 
			this.columnHeaderWorksTomorrow.Text = "xxWorksTomorrow";
			this.columnHeaderWorksTomorrow.Width = 104;
			// 
			// columnHeaderNightRestOK
			// 
			this.columnHeaderNightRestOK.Text = "xxNightRestOK";
			this.columnHeaderNightRestOK.Width = 89;
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ccc_ok_green _16x16.png");
			this.imageList1.Images.SetKeyName(1, "ccc_varning_yellow _16x16.png");
			this.imageList1.Images.SetKeyName(2, "ccc_not_ok_red _16x16.png");
			// 
			// buttonAssign
			// 
			this.buttonAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAssign.Location = new System.Drawing.Point(643, 440);
			this.buttonAssign.Name = "buttonAssign";
			this.buttonAssign.Size = new System.Drawing.Size(75, 23);
			this.buttonAssign.TabIndex = 1;
			this.buttonAssign.Text = "xxAssign";
			this.buttonAssign.UseVisualStyleBackColor = true;
			this.buttonAssign.Click += new System.EventHandler(this.buttonAssign_Click);
			// 
			// ribbonControlHeader
			// 
			this.ribbonControlHeader.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlHeader.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlHeader.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlHeader.MenuButtonText = "";
			this.ribbonControlHeader.MenuButtonVisible = false;
			this.ribbonControlHeader.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlHeader.Name = "ribbonControlHeader";
			// 
			// ribbonControlHeader.OfficeMenu
			// 
			this.ribbonControlHeader.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlHeader.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlHeader.QuickPanelVisible = false;
			this.ribbonControlHeader.SelectedTab = null;
			this.ribbonControlHeader.Size = new System.Drawing.Size(728, 33);
			this.ribbonControlHeader.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlHeader.TabIndex = 5;
			this.ribbonControlHeader.Text = "ribbonControlAdv1";
			this.ribbonControlHeader.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			// 
			// FindMatchingNew
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(730, 475);
			this.Controls.Add(this.ribbonControlHeader);
			this.Controls.Add(this.buttonAssign);
			this.Controls.Add(this.listViewResult);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FindMatchingNew";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "FindMatchingNew";
			this.Load += new System.EventHandler(this.FindMatching_Load);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlHeader)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listViewResult;
		private System.Windows.Forms.ColumnHeader columnHeaderName;
		private System.Windows.Forms.ColumnHeader columnHeaderWorkedYesterday;
		private System.Windows.Forms.ColumnHeader columnHeaderAvailiableToday;
		private System.Windows.Forms.ColumnHeader columnHeaderMatching;
		private System.Windows.Forms.ColumnHeader columnHeaderWorksTomorrow;
		private System.Windows.Forms.ColumnHeader columnHeaderNightRestOK;
		private System.Windows.Forms.Button buttonAssign;
		private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlHeader;
		private System.Windows.Forms.ImageList imageList1;
	}
}