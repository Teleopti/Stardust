
namespace Teleopti.Ccc.Win.Shifts
{
    partial class NavigationView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NavigationView));
			this.tabControlShiftCreator = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageWorkShiftRule = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabPageRuleSetBag = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.imageListTreeView = new System.Windows.Forms.ImageList(this.components);
			((System.ComponentModel.ISupportInitialize)(this.tabControlShiftCreator)).BeginInit();
			this.tabControlShiftCreator.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlShiftCreator
			// 
			this.tabControlShiftCreator.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabControlShiftCreator.AdjustTopGap = 5;
			this.tabControlShiftCreator.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlShiftCreator.BorderWidth = 0;
			this.tabControlShiftCreator.Controls.Add(this.tabPageWorkShiftRule);
			this.tabControlShiftCreator.Controls.Add(this.tabPageRuleSetBag);
			this.tabControlShiftCreator.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlShiftCreator.FocusOnTabClick = false;
			this.tabControlShiftCreator.HotTrack = true;
			this.tabControlShiftCreator.ImageList = this.imageList1;
			this.tabControlShiftCreator.ImageOffset = 3;
			this.tabControlShiftCreator.LevelTextAndImage = true;
			this.tabControlShiftCreator.Location = new System.Drawing.Point(0, 0);
			this.tabControlShiftCreator.Name = "tabControlShiftCreator";
			this.tabControlShiftCreator.Padding = new System.Drawing.Point(3, 6);
			this.tabControlShiftCreator.ShowToolTips = true;
			this.tabControlShiftCreator.Size = new System.Drawing.Size(504, 601);
			this.tabControlShiftCreator.TabGap = 10;
			this.tabControlShiftCreator.TabIndex = 11;
			this.tabControlShiftCreator.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
			this.tabControlShiftCreator.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
			this.tabControlShiftCreator.TextLineAlignment = System.Drawing.StringAlignment.Far;
			this.tabControlShiftCreator.ThemesEnabled = true;
			this.tabControlShiftCreator.SelectedIndexChanged += new System.EventHandler(this.tabControlShiftCreatorSelectedIndexChanged);
			// 
			// tabPageWorkShiftRule
			// 
			this.tabPageWorkShiftRule.Image = null;
			this.tabPageWorkShiftRule.ImageIndex = 0;
			this.tabPageWorkShiftRule.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageWorkShiftRule.Location = new System.Drawing.Point(2, 41);
			this.tabPageWorkShiftRule.Name = "tabPageWorkShiftRule";
			this.tabPageWorkShiftRule.Padding = new System.Windows.Forms.Padding(5);
			this.tabPageWorkShiftRule.Size = new System.Drawing.Size(500, 558);
			this.tabPageWorkShiftRule.TabIndex = 1;
			this.tabPageWorkShiftRule.Text = "xxRuleSets";
			this.tabPageWorkShiftRule.ThemesEnabled = true;
			// 
			// tabPageRuleSetBag
			// 
			this.tabPageRuleSetBag.Image = null;
			this.tabPageRuleSetBag.ImageIndex = 1;
			this.tabPageRuleSetBag.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageRuleSetBag.Location = new System.Drawing.Point(2, 41);
			this.tabPageRuleSetBag.Name = "tabPageRuleSetBag";
			this.tabPageRuleSetBag.Padding = new System.Windows.Forms.Padding(5);
			this.tabPageRuleSetBag.Size = new System.Drawing.Size(500, 558);
			this.tabPageRuleSetBag.TabIndex = 1;
			this.tabPageRuleSetBag.Text = "xxBags";
			this.tabPageRuleSetBag.ThemesEnabled = true;
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ccc_ShiftRuleSet.png");
			this.imageList1.Images.SetKeyName(1, "ccc_ShiftBag.png");
			// 
			// imageListTreeView
			// 
			this.imageListTreeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTreeView.ImageStream")));
			this.imageListTreeView.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListTreeView.Images.SetKeyName(0, "bullet_black.png");
			this.imageListTreeView.Images.SetKeyName(1, "bullet_white.png");
			// 
			// NavigationView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tabControlShiftCreator);
			this.Name = "NavigationView";
			this.Size = new System.Drawing.Size(504, 601);
			((System.ComponentModel.ISupportInitialize)(this.tabControlShiftCreator)).EndInit();
			this.tabControlShiftCreator.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlShiftCreator;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageWorkShiftRule;
        private System.Windows.Forms.ImageList imageList1;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageRuleSetBag;
		private System.Windows.Forms.ImageList imageListTreeView;
    }
}
