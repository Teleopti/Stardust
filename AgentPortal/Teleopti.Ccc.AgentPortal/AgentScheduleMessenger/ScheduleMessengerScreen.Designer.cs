namespace Teleopti.Ccc.AgentPortal.AgentScheduleMessenger
{
    partial class ScheduleMessengerScreen
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
                if (components != null)
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleMessengerScreen));
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.mailButton1 = new Teleopti.Ccc.AgentPortal.Common.Controls.MailButton();
			this.nextActivityControl1 = new Teleopti.Ccc.AgentPortal.AgentScheduleMessenger.NextActivityControl();
			this.layerVisualizer1 = new Teleopti.Ccc.AgentPortal.AgentScheduleMessenger.LayerVisualizer();
			this.notifyIconScheduleMessenger = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStripScheduleMessenger = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemRestoreASM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRestoreAgentPortal = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.timerCurrentTime = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.tableLayoutPanelMain.SuspendLayout();
			this.contextMenuStripScheduleMessenger.SuspendLayout();
			this.SuspendLayout();
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.MenuButtonImage = ((System.Drawing.Image)(resources.GetObject("ribbonControlAdv1.MenuButtonImage")));
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = true;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(558, 33);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "startMenu";
			this.ribbonControlAdv1.TabIndex = 0;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			this.ribbonControlAdv1.TitleColor = System.Drawing.Color.Black;
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanelMain.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.tableLayoutPanelMain.ColumnCount = 3;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 66F));
			this.tableLayoutPanelMain.Controls.Add(this.mailButton1, 2, 0);
			this.tableLayoutPanelMain.Controls.Add(this.nextActivityControl1, 1, 0);
			this.tableLayoutPanelMain.Controls.Add(this.layerVisualizer1, 0, 0);
			this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelMain.ForeColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(3, 34);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 1;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(554, 45);
			this.tableLayoutPanelMain.TabIndex = 1;
			// 
			// mailButton1
			// 
			this.mailButton1.AutoSize = true;
			this.mailButton1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mailButton1.Enabled = false;
			this.mailButton1.Location = new System.Drawing.Point(490, 3);
			this.mailButton1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 3);
			this.mailButton1.Name = "mailButton1";
			this.mailButton1.Size = new System.Drawing.Size(60, 38);
			this.mailButton1.TabIndex = 0;
			// 
			// nextActivityControl1
			// 
			this.nextActivityControl1.BackColor = System.Drawing.Color.White;
			this.nextActivityControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nextActivityControl1.Location = new System.Drawing.Point(374, 1);
			this.nextActivityControl1.Margin = new System.Windows.Forms.Padding(0);
			this.nextActivityControl1.Name = "nextActivityControl1";
			this.nextActivityControl1.Size = new System.Drawing.Size(112, 43);
			this.nextActivityControl1.TabIndex = 1;
			// 
			// layerVisualizer1
			// 
			this.layerVisualizer1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.layerVisualizer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.layerVisualizer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layerVisualizer1.Location = new System.Drawing.Point(1, 1);
			this.layerVisualizer1.Margin = new System.Windows.Forms.Padding(0);
			this.layerVisualizer1.Name = "layerVisualizer1";
			this.layerVisualizer1.Size = new System.Drawing.Size(372, 43);
			this.layerVisualizer1.TabIndex = 2;
			// 
			// notifyIconScheduleMessenger
			// 
			this.notifyIconScheduleMessenger.ContextMenuStrip = this.contextMenuStripScheduleMessenger;
			this.notifyIconScheduleMessenger.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconScheduleMessenger.Icon")));
			this.notifyIconScheduleMessenger.Text = "Agent Schedule Messenger";
			this.notifyIconScheduleMessenger.Visible = true;
			this.notifyIconScheduleMessenger.BalloonTipClicked += new System.EventHandler(this.notifyIconScheduleMessenger_BallonTipClick);
			this.notifyIconScheduleMessenger.DoubleClick += new System.EventHandler(this.notifyIconScheduleMessenger_DoubleClick);
			// 
			// contextMenuStripScheduleMessenger
			// 
			this.contextMenuStripScheduleMessenger.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRestoreASM,
            this.toolStripMenuItemRestoreAgentPortal,
            this.toolStripSeparator1,
            this.toolStripMenuItemExit});
			this.contextMenuStripScheduleMessenger.Name = "contextMenuStripScheduleMessenger";
			this.contextMenuStripScheduleMessenger.ShowImageMargin = false;
			this.contextMenuStripScheduleMessenger.Size = new System.Drawing.Size(162, 76);
			// 
			// toolStripMenuItemRestoreASM
			// 
			this.toolStripMenuItemRestoreASM.Name = "toolStripMenuItemRestoreASM";
			this.SetShortcut(this.toolStripMenuItemRestoreASM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRestoreASM.Size = new System.Drawing.Size(161, 22);
			this.toolStripMenuItemRestoreASM.Text = "xxRestoreASM";
			this.toolStripMenuItemRestoreASM.Click += new System.EventHandler(this.toolStripMenuItemRestoreASM_Click);
			// 
			// toolStripMenuItemRestoreAgentPortal
			// 
			this.toolStripMenuItemRestoreAgentPortal.Name = "toolStripMenuItemRestoreAgentPortal";
			this.SetShortcut(this.toolStripMenuItemRestoreAgentPortal, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRestoreAgentPortal.Size = new System.Drawing.Size(161, 22);
			this.toolStripMenuItemRestoreAgentPortal.Text = "xxRestoreAgentPortal";
			this.toolStripMenuItemRestoreAgentPortal.Click += new System.EventHandler(this.toolStripMenuItemRestoreAgentPortal_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.SetShortcut(this.toolStripSeparator1, System.Windows.Forms.Keys.None);
			this.toolStripSeparator1.Size = new System.Drawing.Size(158, 6);
			// 
			// toolStripMenuItemExit
			// 
			this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
			this.SetShortcut(this.toolStripMenuItemExit, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemExit.Size = new System.Drawing.Size(161, 22);
			this.toolStripMenuItemExit.Text = "xxExit";
			this.toolStripMenuItemExit.Click += new System.EventHandler(this.toolStripMenuItemExit_Click);
			// 
			// timerCurrentTime
			// 
			this.timerCurrentTime.Interval = 30000;
			this.timerCurrentTime.Tick += new System.EventHandler(this.timerCurrentTime_Tick);
			// 
			// ScheduleMessengerScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Borders = new System.Windows.Forms.Padding(3, 1, 3, 3);
			this.ClientSize = new System.Drawing.Size(560, 82);
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Controls.Add(this.ribbonControlAdv1);
			this.HelpButtonImage = ((System.Drawing.Image)(resources.GetObject("$this.HelpButtonImage")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Location = new System.Drawing.Point(10, 10);
			this.MaximumSize = new System.Drawing.Size(2000, 82);
			this.MinimumSize = new System.Drawing.Size(560, 82);
			this.Name = "ScheduleMessengerScreen";
			this.Opacity = 0.8D;
			this.Text = "xxAgentScheduleMessenger";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelMain.PerformLayout();
			this.contextMenuStripScheduleMessenger.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.NotifyIcon notifyIconScheduleMessenger;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripScheduleMessenger;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRestoreASM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRestoreAgentPortal;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Teleopti.Ccc.AgentPortal.Common.Controls.MailButton mailButton1;
        private NextActivityControl nextActivityControl1;
        private LayerVisualizer layerVisualizer1;
        private System.Windows.Forms.Timer timerCurrentTime;
    }
}

