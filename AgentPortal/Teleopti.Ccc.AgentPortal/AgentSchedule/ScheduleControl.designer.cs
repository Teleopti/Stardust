using Teleopti.Ccc.AgentPortal.Main;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
    partial class ScheduleControl
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu")]
        private void InitializeComponent()
        {
            this.contextmenuStripExScheduleControl = new Syncfusion.Windows.Forms.Tools.ContextMenuStripEx();
            this.toolStripMenuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNewAbsenceRequest = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNewShiftTrade = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNewTextRequest = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorDelete = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorActivityColor = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemActivityColor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDefaultColor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSystemColor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorResolutions = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem60Mins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem30Mins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem15Mins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10Mins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6Mins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5Mins = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.panelSchedule = new System.Windows.Forms.Panel();
            this.scheduleControlMain = new Teleopti.Ccc.AgentPortal.Common.Controls.CustomScheduleControl();
            this.scheduleTeamView = new Teleopti.Ccc.AgentPortal.AgentSchedule.ScheduleTeamView();
            this.tableLayoutPanelTabs = new System.Windows.Forms.TableLayoutPanel();
            this.tabControlAdvMainTab = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
            this.tabPageAdvDayView = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.tabPageAdvWeekView = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.tabPageAdvMonthView = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.tabPageAdvTeamView = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.contextmenuStripExScheduleControl.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            this.panelSchedule.SuspendLayout();
            this.tableLayoutPanelTabs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdvMainTab)).BeginInit();
            this.tabControlAdvMainTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextmenuStripExScheduleControl
            // 
            this.contextmenuStripExScheduleControl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemOpen,
            this.toolStripMenuItemNewAbsenceRequest,
            this.toolStripMenuItemNewShiftTrade,
            this.toolStripMenuItemNewTextRequest,
            this.toolStripSeparatorDelete,
            this.toolStripMenuItemDelete,
            this.toolStripSeparatorActivityColor,
            this.toolStripMenuItemActivityColor,
            this.toolStripSeparatorResolutions,
            this.toolStripMenuItem60Mins,
            this.toolStripMenuItem30Mins,
            this.toolStripMenuItem15Mins,
            this.toolStripMenuItem10Mins,
            this.toolStripMenuItem6Mins,
            this.toolStripMenuItem5Mins});
            this.contextmenuStripExScheduleControl.Name = "contextMenuStripEx1";
            this.contextmenuStripExScheduleControl.ShowCheckMargin = true;
            this.contextmenuStripExScheduleControl.ShowImageMargin = false;
            this.contextmenuStripExScheduleControl.Size = new System.Drawing.Size(205, 308);
            this.contextmenuStripExScheduleControl.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuStrip_ItemClick);
            // 
            // toolStripMenuItemOpen
            // 
            this.toolStripMenuItemOpen.Name = "toolStripMenuItemOpen";
            this.toolStripMenuItemOpen.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemOpen.Text = "xxOpen";
            this.toolStripMenuItemOpen.Visible = false;
            this.toolStripMenuItemOpen.Click += new System.EventHandler(this.toolStripMenuItemOpen_Click);
            // 
            // toolStripMenuItemNewAbsenceRequest
            // 
            this.toolStripMenuItemNewAbsenceRequest.Name = "toolStripMenuItemNewAbsenceRequest";
            this.toolStripMenuItemNewAbsenceRequest.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemNewAbsenceRequest.Text = "xxNewAbsenceRequest";
            this.toolStripMenuItemNewAbsenceRequest.Visible = false;
            this.toolStripMenuItemNewAbsenceRequest.Click += new System.EventHandler(this.toolStripMenuItemNewAbsenceRequest_Click);
            // 
            // toolStripMenuItemNewShiftTrade
            // 
            this.toolStripMenuItemNewShiftTrade.Name = "toolStripMenuItemNewShiftTrade";
            this.toolStripMenuItemNewShiftTrade.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemNewShiftTrade.Text = "xxNewShiftTradeRequest";
            this.toolStripMenuItemNewShiftTrade.Click += new System.EventHandler(this.toolStripMenuItemNewShiftTrade_Click);
            // 
            // toolStripMenuItemNewTextRequest
            // 
            this.toolStripMenuItemNewTextRequest.Name = "toolStripMenuItemNewTextRequest";
            this.toolStripMenuItemNewTextRequest.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemNewTextRequest.Text = "xxNewTextRequest";
            this.toolStripMenuItemNewTextRequest.Click += new System.EventHandler(this.toolStripMenuItemNewTextRequest_Click);
            // 
            // toolStripSeparatorDelete
            // 
            this.toolStripSeparatorDelete.Name = "toolStripSeparatorDelete";
            this.toolStripSeparatorDelete.Size = new System.Drawing.Size(201, 6);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemDelete.Text = "xxDelete";
            this.toolStripMenuItemDelete.Visible = false;
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
            // 
            // toolStripSeparatorActivityColor
            // 
            this.toolStripSeparatorActivityColor.Name = "toolStripSeparatorActivityColor";
            this.toolStripSeparatorActivityColor.Size = new System.Drawing.Size(201, 6);
            // 
            // toolStripMenuItemActivityColor
            // 
            this.toolStripMenuItemActivityColor.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemDefaultColor,
            this.toolStripMenuItemSystemColor});
            this.toolStripMenuItemActivityColor.Name = "toolStripMenuItemActivityColor";
            this.toolStripMenuItemActivityColor.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemActivityColor.Text = "xxActivityColor";
            // 
            // toolStripMenuItemDefaultColor
            // 
            this.toolStripMenuItemDefaultColor.Name = "toolStripMenuItemDefaultColor";
            this.toolStripMenuItemDefaultColor.Size = new System.Drawing.Size(151, 22);
            this.toolStripMenuItemDefaultColor.Text = "xxDefaultColor";
            this.toolStripMenuItemDefaultColor.Click += new System.EventHandler(this.toolStripMenuItemDefaultColor_Click);
            // 
            // toolStripMenuItemSystemColor
            // 
            this.toolStripMenuItemSystemColor.Name = "toolStripMenuItemSystemColor";
            this.toolStripMenuItemSystemColor.Size = new System.Drawing.Size(151, 22);
            this.toolStripMenuItemSystemColor.Text = "xxSystemColor";
            this.toolStripMenuItemSystemColor.Click += new System.EventHandler(this.toolStripMenuItemSysColor_Click);
            // 
            // toolStripSeparatorResolutions
            // 
            this.toolStripSeparatorResolutions.Name = "toolStripSeparatorResolutions";
            this.toolStripSeparatorResolutions.Size = new System.Drawing.Size(201, 6);
            // 
            // toolStripMenuItem60Mins
            // 
            this.toolStripMenuItem60Mins.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem60Mins.Name = "toolStripMenuItem60Mins";
            this.toolStripMenuItem60Mins.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItem60Mins.Text = "xxSixtyMinutes";
            // 
            // toolStripMenuItem30Mins
            // 
            this.toolStripMenuItem30Mins.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem30Mins.Name = "toolStripMenuItem30Mins";
            this.toolStripMenuItem30Mins.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItem30Mins.Text = "xxThirtyMinutes";
            // 
            // toolStripMenuItem15Mins
            // 
            this.toolStripMenuItem15Mins.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem15Mins.Name = "toolStripMenuItem15Mins";
            this.toolStripMenuItem15Mins.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItem15Mins.Text = "xxFifteenMinutes";
            // 
            // toolStripMenuItem10Mins
            // 
            this.toolStripMenuItem10Mins.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem10Mins.Name = "toolStripMenuItem10Mins";
            this.toolStripMenuItem10Mins.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItem10Mins.Text = "xxTenMinutes";
            // 
            // toolStripMenuItem6Mins
            // 
            this.toolStripMenuItem6Mins.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem6Mins.Name = "toolStripMenuItem6Mins";
            this.toolStripMenuItem6Mins.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItem6Mins.Text = "xxSixMinutes";
            // 
            // toolStripMenuItem5Mins
            // 
            this.toolStripMenuItem5Mins.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem5Mins.Name = "toolStripMenuItem5Mins";
            this.toolStripMenuItem5Mins.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItem5Mins.Text = "xxFiveMinutes";
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.panelSchedule, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelTabs, 0, 0);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 2;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(875, 618);
            this.tableLayoutPanelMain.TabIndex = 7;
            // 
            // panelSchedule
            // 
            this.panelSchedule.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
            this.panelSchedule.Controls.Add(this.scheduleControlMain);
            this.panelSchedule.Controls.Add(this.scheduleTeamView);
            this.panelSchedule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSchedule.Location = new System.Drawing.Point(0, 30);
            this.panelSchedule.Margin = new System.Windows.Forms.Padding(0);
            this.panelSchedule.Name = "panelSchedule";
            this.panelSchedule.Size = new System.Drawing.Size(875, 588);
            this.panelSchedule.TabIndex = 8;
            // 
            // scheduleControlMain
            // 
            this.scheduleControlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(201)))), ((int)(((byte)(219)))));
            this.scheduleControlMain.ClickedDate = new System.DateTime(((long)(0)));
            this.scheduleControlMain.ClickedScheduleAppointment = null;
            this.scheduleControlMain.Culture = new System.Globalization.CultureInfo("");
            this.scheduleControlMain.DataSource = null;
            this.scheduleControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scheduleControlMain.ISO8601CalenderFormat = true;
            this.scheduleControlMain.Location = new System.Drawing.Point(0, 0);
            this.scheduleControlMain.Name = "scheduleControlMain";
            this.scheduleControlMain.NavigationPanelFillWithCalendar = true;
            this.scheduleControlMain.NavigationPanelPosition = Syncfusion.Schedule.CalendarNavigationPanelPosition.Left;
            this.scheduleControlMain.ShowRoundedCorners = true;
            this.scheduleControlMain.Size = new System.Drawing.Size(875, 588);
            this.scheduleControlMain.TabIndex = 3;
            // 
            // scheduleTeamView
            // 
            this.scheduleTeamView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scheduleTeamView.Location = new System.Drawing.Point(0, 0);
            this.scheduleTeamView.Margin = new System.Windows.Forms.Padding(0);
            this.scheduleTeamView.Name = "scheduleTeamView";
            this.scheduleTeamView.Size = new System.Drawing.Size(875, 588);
            this.scheduleTeamView.TabIndex = 2;
            // 
            // tableLayoutPanelTabs
            // 
            this.tableLayoutPanelTabs.ColumnCount = 1;
            this.tableLayoutPanelTabs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTabs.Controls.Add(this.tabControlAdvMainTab, 0, 0);
            this.tableLayoutPanelTabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelTabs.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelTabs.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelTabs.Name = "tableLayoutPanelTabs";
            this.tableLayoutPanelTabs.RowCount = 1;
            this.tableLayoutPanelTabs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTabs.Size = new System.Drawing.Size(875, 30);
            this.tableLayoutPanelTabs.TabIndex = 7;
            // 
            // tabControlAdvMainTab
            // 
            this.tabControlAdvMainTab.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabControlAdvMainTab.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tabControlAdvMainTab.Controls.Add(this.tabPageAdvDayView);
            this.tabControlAdvMainTab.Controls.Add(this.tabPageAdvWeekView);
            this.tabControlAdvMainTab.Controls.Add(this.tabPageAdvMonthView);
            this.tabControlAdvMainTab.Controls.Add(this.tabPageAdvTeamView);
            this.tabControlAdvMainTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlAdvMainTab.Location = new System.Drawing.Point(0, 0);
            this.tabControlAdvMainTab.Margin = new System.Windows.Forms.Padding(0);
            this.tabControlAdvMainTab.Name = "tabControlAdvMainTab";
            this.tabControlAdvMainTab.Padding = new System.Drawing.Point(0, 7);
            this.tabControlAdvMainTab.Size = new System.Drawing.Size(875, 30);
            this.tabControlAdvMainTab.TabGap = 15;
            this.tabControlAdvMainTab.TabIndex = 4;
            this.tabControlAdvMainTab.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
            this.tabControlAdvMainTab.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
            this.tabControlAdvMainTab.ThemesEnabled = true;
            this.tabControlAdvMainTab.SelectedIndexChanged += new System.EventHandler(this.tabControlAdvMainTab_SelectedIndexChanged);
            // 
            // tabPageAdvDayView
            // 
            this.tabPageAdvDayView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(246)))));
            this.tabPageAdvDayView.Image = null;
            this.tabPageAdvDayView.ImageSize = new System.Drawing.Size(16, 16);
            this.tabPageAdvDayView.Location = new System.Drawing.Point(2, 31);
            this.tabPageAdvDayView.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageAdvDayView.Name = "tabPageAdvDayView";
            this.tabPageAdvDayView.Size = new System.Drawing.Size(871, 0);
            this.tabPageAdvDayView.TabIndex = 1;
            this.tabPageAdvDayView.Text = "xxDayView";
            this.tabPageAdvDayView.ThemesEnabled = true;
            // 
            // tabPageAdvWeekView
            // 
            this.tabPageAdvWeekView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(246)))));
            this.tabPageAdvWeekView.Image = null;
            this.tabPageAdvWeekView.ImageSize = new System.Drawing.Size(16, 16);
            this.tabPageAdvWeekView.Location = new System.Drawing.Point(2, 31);
            this.tabPageAdvWeekView.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageAdvWeekView.Name = "tabPageAdvWeekView";
            this.tabPageAdvWeekView.Size = new System.Drawing.Size(871, 0);
            this.tabPageAdvWeekView.TabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabPageAdvWeekView.TabIndex = 1;
            this.tabPageAdvWeekView.Text = "xxWeekView";
            this.tabPageAdvWeekView.ThemesEnabled = true;
            // 
            // tabPageAdvMonthView
            // 
            this.tabPageAdvMonthView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(246)))));
            this.tabPageAdvMonthView.Image = null;
            this.tabPageAdvMonthView.ImageSize = new System.Drawing.Size(16, 16);
            this.tabPageAdvMonthView.Location = new System.Drawing.Point(2, 31);
            this.tabPageAdvMonthView.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageAdvMonthView.Name = "tabPageAdvMonthView";
            this.tabPageAdvMonthView.Size = new System.Drawing.Size(871, 0);
            this.tabPageAdvMonthView.TabIndex = 2;
            this.tabPageAdvMonthView.Text = "xxMonthView";
            this.tabPageAdvMonthView.ThemesEnabled = true;
            // 
            // tabPageAdvTeamView
            // 
            this.tabPageAdvTeamView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(246)))));
            this.tabPageAdvTeamView.Image = null;
            this.tabPageAdvTeamView.ImageSize = new System.Drawing.Size(16, 16);
            this.tabPageAdvTeamView.Location = new System.Drawing.Point(2, 31);
            this.tabPageAdvTeamView.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageAdvTeamView.Name = "tabPageAdvTeamView";
            this.tabPageAdvTeamView.Size = new System.Drawing.Size(871, 0);
            this.tabPageAdvTeamView.TabIndex = 3;
            this.tabPageAdvTeamView.Text = "xxTeamView";
            this.tabPageAdvTeamView.ThemesEnabled = true;
            // 
            // ScheduleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Name = "ScheduleControl";
            this.Size = new System.Drawing.Size(875, 618);
            this.Load += new System.EventHandler(this.ScheduleControl_Load);
            this.contextmenuStripExScheduleControl.ResumeLayout(false);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.panelSchedule.ResumeLayout(false);
            this.tableLayoutPanelTabs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdvMainTab)).EndInit();
            this.tabControlAdvMainTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.ContextMenuStripEx contextmenuStripExScheduleControl;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActivityColor;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDefaultColor;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSystemColor;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorResolutions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem60Mins;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem30Mins;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem15Mins;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10Mins;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6Mins;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5Mins;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewAbsenceRequest;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorDelete;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Panel panelSchedule;
        private ScheduleTeamView scheduleTeamView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTabs;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdvMainTab;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvDayView;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvWeekView;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvMonthView;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvTeamView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpen;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorActivityColor;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewShiftTrade;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewTextRequest;
        private Teleopti.Ccc.AgentPortal.Common.Controls.CustomScheduleControl scheduleControlMain;
    }
}
