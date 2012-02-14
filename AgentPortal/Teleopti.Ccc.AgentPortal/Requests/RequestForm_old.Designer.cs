namespace Teleopti.Ccc.AgentPortal.Requests
{
    partial class RequestForm
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu")]
        private void InitializeComponent()
        {
            this.ribbonControlAdvMain = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.toolStripTabItemResponse = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.toolStripExMain = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonAccept = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDeny = new System.Windows.Forms.ToolStripButton();
            this.toolStripExDelete = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
            this.gradientPanelButtons = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.gradientPanelMain = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).BeginInit();
            this.ribbonControlAdvMain.SuspendLayout();
            this.toolStripTabItemResponse.Panel.SuspendLayout();
            this.toolStripExMain.SuspendLayout();
            this.toolStripExDelete.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelButtons)).BeginInit();
            this.gradientPanelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonControlAdvMain
            // 
            this.ribbonControlAdvMain.AllowCollapse = false;
            this.ribbonControlAdvMain.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Top;
            this.ribbonControlAdvMain.Header.AddMainItem(toolStripTabItemResponse);
            this.ribbonControlAdvMain.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdvMain.MenuButtonVisible = false;
            this.ribbonControlAdvMain.MenuButtonWidth = 20;
            this.ribbonControlAdvMain.Name = "ribbonControlAdvMain";
            // 
            // ribbonControlAdvMain.OfficeMenu
            // 
            this.ribbonControlAdvMain.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdvMain.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdvMain.QuickPanelVisible = false;
            this.ribbonControlAdvMain.ShowCaption = false;
            this.ribbonControlAdvMain.ShowLauncher = false;
            this.ribbonControlAdvMain.Size = new System.Drawing.Size(428, 128);
            this.ribbonControlAdvMain.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlAdvMain.TabIndex = 1;
            this.ribbonControlAdvMain.Text = "ribbonControlAdv1";
            // 
            // toolStripTabItemResponse
            // 
            this.toolStripTabItemResponse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripTabItemResponse.Name = "toolStripTabItemResponse";
            // 
            // ribbonControlAdvMain.ribbonPanel1
            // 
            this.toolStripTabItemResponse.Panel.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Bottom;
            this.toolStripTabItemResponse.Panel.Controls.Add(this.toolStripExMain);
            this.toolStripTabItemResponse.Panel.Controls.Add(this.toolStripExDelete);
            this.toolStripTabItemResponse.Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripTabItemResponse.Panel.Name = "ribbonPanel1";
            this.toolStripTabItemResponse.Panel.ScrollPosition = 0;
            this.toolStripTabItemResponse.Panel.TabIndex = 2;
            this.toolStripTabItemResponse.Panel.Text = "xxRequestResponseAction";
            this.SetShortcut(this.toolStripTabItemResponse, System.Windows.Forms.Keys.None);
            this.toolStripTabItemResponse.Size = new System.Drawing.Size(141, 19);
            this.toolStripTabItemResponse.Text = "xxRequestResponseAction";
            // 
            // toolStripExMain
            // 
            this.toolStripExMain.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripExMain.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExMain.Image = null;
            this.toolStripExMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAccept,
            this.toolStripButtonDeny});
            this.toolStripExMain.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripExMain.Location = new System.Drawing.Point(0, 1);
            this.toolStripExMain.Name = "toolStripExMain";
            this.toolStripExMain.ShowCaption = false;
            this.toolStripExMain.Size = new System.Drawing.Size(108, 66);
            this.toolStripExMain.TabIndex = 0;
            // 
            // toolStripButtonAccept
            // 
            this.toolStripButtonAccept.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_MeetingPlanner;
            this.toolStripButtonAccept.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolStripButtonAccept.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonAccept.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAccept.Name = "toolStripButtonAccept";
            this.SetShortcut(this.toolStripButtonAccept, System.Windows.Forms.Keys.None);
            this.toolStripButtonAccept.Size = new System.Drawing.Size(55, 59);
            this.toolStripButtonAccept.Text = "xxAccept";
            this.toolStripButtonAccept.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolStripButtonAccept.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonAccept.Click += new System.EventHandler(this.toolStripButtonAccept_Click);
            // 
            // toolStripButtonDeny
            // 
            this.toolStripButtonDeny.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Delete;
            this.toolStripButtonDeny.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolStripButtonDeny.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonDeny.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonDeny.Name = "toolStripButtonDeny";
            this.SetShortcut(this.toolStripButtonDeny, System.Windows.Forms.Keys.None);
            this.toolStripButtonDeny.Size = new System.Drawing.Size(46, 59);
            this.toolStripButtonDeny.Text = "xxDeny";
            this.toolStripButtonDeny.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolStripButtonDeny.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonDeny.Click += new System.EventHandler(this.toolStripButtonDeny_Click);
            // 
            // toolStripExDelete
            // 
            this.toolStripExDelete.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripExDelete.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExDelete.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExDelete.Image = null;
            this.toolStripExDelete.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonDelete});
            this.toolStripExDelete.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripExDelete.Location = new System.Drawing.Point(108, 1);
            this.toolStripExDelete.Name = "toolStripExDelete";
            this.toolStripExDelete.Size = new System.Drawing.Size(59, 66);
            this.toolStripExDelete.TabIndex = 1;
            // 
            // toolStripButtonDelete
            // 
            this.toolStripButtonDelete.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Delete;
            this.toolStripButtonDelete.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonDelete.Name = "toolStripButtonDelete";
            this.SetShortcut(this.toolStripButtonDelete, System.Windows.Forms.Keys.None);
            this.toolStripButtonDelete.Size = new System.Drawing.Size(52, 59);
            this.toolStripButtonDelete.Text = "xxDelete";
            this.toolStripButtonDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonDelete.Click += new System.EventHandler(this.toolStripButtonDelete_Click);
            // 
            // gradientPanelButtons
            // 
            this.gradientPanelButtons.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.White, System.Drawing.Color.LightSteelBlue);
            this.gradientPanelButtons.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.gradientPanelButtons.BorderColor = System.Drawing.Color.Black;
            this.gradientPanelButtons.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanelButtons.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelButtons.Controls.Add(this.buttonAdvCancel);
            this.gradientPanelButtons.Controls.Add(this.buttonAdvOK);
            this.gradientPanelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gradientPanelButtons.Location = new System.Drawing.Point(6, 312);
            this.gradientPanelButtons.Name = "gradientPanelButtons";
            this.gradientPanelButtons.Size = new System.Drawing.Size(418, 32);
            this.gradientPanelButtons.TabIndex = 2;
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvCancel.Location = new System.Drawing.Point(330, 3);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Size = new System.Drawing.Size(76, 26);
            this.buttonAdvCancel.TabIndex = 1;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
            // 
            // buttonAdvOK
            // 
            this.buttonAdvOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvOK.Location = new System.Drawing.Point(248, 3);
            this.buttonAdvOK.Name = "buttonAdvOK";
            this.buttonAdvOK.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvOK.Size = new System.Drawing.Size(76, 26);
            this.buttonAdvOK.TabIndex = 0;
            this.buttonAdvOK.Text = "xxOk";
            this.buttonAdvOK.UseVisualStyle = true;
            this.buttonAdvOK.Click += new System.EventHandler(this.buttonAdvOK_Click);
            // 
            // gradientPanelMain
            // 
            this.gradientPanelMain.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
            this.gradientPanelMain.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.gradientPanelMain.BorderColor = System.Drawing.Color.Black;
            this.gradientPanelMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanelMain.Location = new System.Drawing.Point(6, 129);
            this.gradientPanelMain.Name = "gradientPanelMain";
            this.gradientPanelMain.Size = new System.Drawing.Size(418, 183);
            this.gradientPanelMain.TabIndex = 3;
            // 
            // RequestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 350);
            this.Controls.Add(this.gradientPanelMain);
            this.Controls.Add(this.gradientPanelButtons);
            this.Controls.Add(this.ribbonControlAdvMain);
            this.MaximizeBox = false;
            this.Name = "RequestForm";
            this.Text = "xxRequestForm";
            this.Load += new System.EventHandler(this.RequestForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).EndInit();
            this.ribbonControlAdvMain.ResumeLayout(false);
            this.ribbonControlAdvMain.PerformLayout();
            this.toolStripTabItemResponse.Panel.ResumeLayout(false);
            this.toolStripTabItemResponse.Panel.PerformLayout();
            this.toolStripExMain.ResumeLayout(false);
            this.toolStripExMain.PerformLayout();
            this.toolStripExDelete.ResumeLayout(false);
            this.toolStripExDelete.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelButtons)).EndInit();
            this.gradientPanelButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdvMain;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelButtons;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelMain;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemResponse;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExMain;
        private System.Windows.Forms.ToolStripButton toolStripButtonAccept;
        private System.Windows.Forms.ToolStripButton toolStripButtonDeny;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExDelete;
        private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
    }
}