namespace Teleopti.Ccc.Win.BaseFormsAndControls
{
	partial class BaseRibbonFormWithRibbon
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseRibbonFormWithRibbon));
			this.ribbonControlAdvMain = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
			this.toolStripTabItemMain = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).BeginInit();
			this.ribbonControlAdvMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// ribbonControlAdvMain
			// 
			this.ribbonControlAdvMain.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvMain.Header.AddMainItem(toolStripTabItemMain);
			this.ribbonControlAdvMain.HideMenuButtonToolTip = false;
			this.ribbonControlAdvMain.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.ribbonControlAdvMain.Location = new System.Drawing.Point(1, 1);
			this.ribbonControlAdvMain.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdvMain.MenuButtonEnabled = true;
			this.ribbonControlAdvMain.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvMain.MenuButtonText = "xxFile";
			this.ribbonControlAdvMain.MenuButtonWidth = 56;
			this.ribbonControlAdvMain.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdvMain.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdvMain.Name = "ribbonControlAdvMain";
			this.ribbonControlAdvMain.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			this.ribbonControlAdvMain.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			// 
			// ribbonControlAdvMain.OfficeMenu
			// 
			this.ribbonControlAdvMain.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdvMain.OfficeMenu.ShowItemToolTips = true;
			this.ribbonControlAdvMain.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdvMain.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdvMain.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdvMain.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdvMain.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdvMain.SelectedTab = this.toolStripTabItemMain;
			this.ribbonControlAdvMain.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdvMain.ShowRibbonDisplayOptionButton = true;
			this.ribbonControlAdvMain.Size = new System.Drawing.Size(429, 56);
			this.ribbonControlAdvMain.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdvMain.TabIndex = 0;
			this.ribbonControlAdvMain.Text = "ribbonControlAdv1";
			this.ribbonControlAdvMain.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdvMain.TitleColor = System.Drawing.Color.Black;
			// 
			// toolStripTabItemMain
			// 
			this.toolStripTabItemMain.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripTabItemMain.Name = "toolStripTabItemMain";
			// 
			// ribbonControlAdvMain.ribbonPanelHome
			// 
			this.toolStripTabItemMain.Panel.Name = "ribbonPanelHome";
			this.toolStripTabItemMain.Panel.ScrollPosition = 0;
			this.toolStripTabItemMain.Panel.TabIndex = 2;
			this.toolStripTabItemMain.Panel.Text = "xxHome";
			this.toolStripTabItemMain.Position = 0;
			this.toolStripTabItemMain.Size = new System.Drawing.Size(67, 25);
			this.toolStripTabItemMain.Tag = "1";
			this.toolStripTabItemMain.Text = "xxHome";
			// 
			// BaseRibbonFormWithRibbon
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Borders = new System.Windows.Forms.Padding(0);
			this.ClientSize = new System.Drawing.Size(427, 343);
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.ribbonControlAdvMain);
			this.HelpButtonImage = ((System.Drawing.Image)(resources.GetObject("$this.HelpButtonImage")));
			this.Name = "BaseRibbonFormWithRibbon";
			this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.Text = "BaseRibbonFormWithRibbon";
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).EndInit();
			this.ribbonControlAdvMain.ResumeLayout(false);
			this.ribbonControlAdvMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdvMain;
		private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemMain;
	}
}