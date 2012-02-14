using Syncfusion.Windows.Forms;

namespace Teleopti.Ccc.AgentPortal.Requests.ShiftTrade
{
    partial class ShiftTradeView
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
                toolStripExAddDates.Dispose();
                _shiftTradeVisualView.Dispose();
                _popupControlAddDates.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShiftTradeView));
            this.ribbonControlAdvMain = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.toolStripTabItemResponse = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.toolStripExActions = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonSaveAndClose = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripExMain = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonAccept = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDeny = new System.Windows.Forms.ToolStripButton();
            this.toolStripExAddDates = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonAddDays = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRemoveDate = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonClose = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxExtName = new System.Windows.Forms.Label();
            this.textBoxExtMessage = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxExtSubject = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelUserStatus = new System.Windows.Forms.Label();
            this.labelSubject = new System.Windows.Forms.Label();
            this.textBoxExtStatus = new System.Windows.Forms.Label();
            this.gradientPanelScheduleHasChanged = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.labelReturnMessage = new System.Windows.Forms.Label();
            this.addDatesView1 = new Teleopti.Ccc.AgentPortal.Requests.ShiftTrade.AddDatesView();
            this.officeButtonSave = new Syncfusion.Windows.Forms.Tools.OfficeButton();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).BeginInit();
            this.ribbonControlAdvMain.SuspendLayout();
            this.toolStripTabItemResponse.Panel.SuspendLayout();
            this.toolStripExActions.SuspendLayout();
            this.toolStripExMain.SuspendLayout();
            this.toolStripExAddDates.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtMessage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtSubject)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelScheduleHasChanged)).BeginInit();
            this.gradientPanelScheduleHasChanged.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbonControlAdvMain
            // 
            this.ribbonControlAdvMain.AllowCollapse = false;
            this.ribbonControlAdvMain.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Top;
            this.ribbonControlAdvMain.Header.AddMainItem(toolStripTabItemResponse);
            this.ribbonControlAdvMain.Header.AddQuickItem(new Syncfusion.Windows.Forms.Tools.QuickButtonReflectable(toolStripButtonSaveAndClose));
            resources.ApplyResources(this.ribbonControlAdvMain, "ribbonControlAdvMain");
            this.ribbonControlAdvMain.MenuButtonImage = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Menu;
            this.ribbonControlAdvMain.Name = "ribbonControlAdvMain";
            // 
            // ribbonControlAdvMain.OfficeMenu
            // 
            this.ribbonControlAdvMain.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonClose});
            this.ribbonControlAdvMain.OfficeMenu.Name = "OfficeMenu";
            resources.ApplyResources(this.ribbonControlAdvMain.OfficeMenu, "ribbonControlAdvMain.OfficeMenu");
            this.ribbonControlAdvMain.ShowLauncher = false;
            this.ribbonControlAdvMain.SystemText.QuickAccessDialogDropDownName = "xxStartMenu";
            // 
            // toolStripTabItemResponse
            // 
            this.toolStripTabItemResponse.Name = "toolStripTabItemResponse";
            // 
            // ribbonControlAdvMain.ribbonPanel1
            // 
            this.toolStripTabItemResponse.Panel.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Bottom;
            this.toolStripTabItemResponse.Panel.Controls.Add(this.toolStripExActions);
            this.toolStripTabItemResponse.Panel.Controls.Add(this.toolStripExMain);
            this.toolStripTabItemResponse.Panel.Controls.Add(this.toolStripExAddDates);
            this.toolStripTabItemResponse.Panel.Name = "ribbonPanel1";
            this.toolStripTabItemResponse.Panel.ScrollPosition = 0;
            this.toolStripTabItemResponse.Panel.ShowCaption = true;
            resources.ApplyResources(this.toolStripTabItemResponse.Panel, "ribbonControlAdvMain.ribbonPanel1");
            this.toolStripTabItemResponse.Position = 0;
            resources.ApplyResources(this.toolStripTabItemResponse, "toolStripTabItemResponse");
            // 
            // toolStripExActions
            // 
            resources.ApplyResources(this.toolStripExActions, "toolStripExActions");
            this.toolStripExActions.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExActions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExActions.Image = null;
            this.toolStripExActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSaveAndClose,
            this.toolStripButtonDelete});
            this.toolStripExActions.Name = "toolStripExActions";
            // 
            // toolStripButtonSaveAndClose
            // 
            this.toolStripButtonSaveAndClose.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_SkillEmail;
            resources.ApplyResources(this.toolStripButtonSaveAndClose, "toolStripButtonSaveAndClose");
            this.toolStripButtonSaveAndClose.Name = "toolStripButtonSaveAndClose";
            this.ribbonControlAdvMain.SetUseInQuickAccessMenu(this.toolStripButtonSaveAndClose, true);
            this.toolStripButtonSaveAndClose.Click += new System.EventHandler(this.toolStripButtonSaveAndClose_Click);
            // 
            // toolStripButtonDelete
            // 
            this.toolStripButtonDelete.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Delete;
            resources.ApplyResources(this.toolStripButtonDelete, "toolStripButtonDelete");
            this.toolStripButtonDelete.Name = "toolStripButtonDelete";
            this.toolStripButtonDelete.Click += new System.EventHandler(this.toolStripButtonDelete_Click);
            // 
            // toolStripExMain
            // 
            resources.ApplyResources(this.toolStripExMain, "toolStripExMain");
            this.toolStripExMain.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExMain.Image = null;
            this.toolStripExMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAccept,
            this.toolStripButtonDeny});
            this.toolStripExMain.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripExMain.Name = "toolStripExMain";
            this.toolStripExMain.ShowCaption = true;
            this.toolStripExMain.ShowLauncher = false;
            // 
            // toolStripButtonAccept
            // 
            this.toolStripButtonAccept.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_MeetingPlanner;
            resources.ApplyResources(this.toolStripButtonAccept, "toolStripButtonAccept");
            this.toolStripButtonAccept.Name = "toolStripButtonAccept";
            this.toolStripButtonAccept.Click += new System.EventHandler(this.toolStripButtonAccept_Click);
            // 
            // toolStripButtonDeny
            // 
            this.toolStripButtonDeny.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Agent_denyRequest_32x32;
            resources.ApplyResources(this.toolStripButtonDeny, "toolStripButtonDeny");
            this.toolStripButtonDeny.Name = "toolStripButtonDeny";
            this.toolStripButtonDeny.Click += new System.EventHandler(this.toolStripButtonDeny_Click);
            // 
            // toolStripExAddDates
            // 
            resources.ApplyResources(this.toolStripExAddDates, "toolStripExAddDates");
            this.toolStripExAddDates.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExAddDates.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExAddDates.Image = null;
            this.toolStripExAddDates.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAddDays,
            this.toolStripButtonRemoveDate});
            this.toolStripExAddDates.Name = "toolStripExAddDates";
            // 
            // toolStripButtonAddDays
            // 
            this.toolStripButtonAddDays.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Add;
            resources.ApplyResources(this.toolStripButtonAddDays, "toolStripButtonAddDays");
            this.toolStripButtonAddDays.Name = "toolStripButtonAddDays";
            this.toolStripButtonAddDays.Click += new System.EventHandler(this.toolStripButtonAddDays_Click);
            // 
            // toolStripButtonRemoveDate
            // 
            this.toolStripButtonRemoveDate.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Remove;
            resources.ApplyResources(this.toolStripButtonRemoveDate, "toolStripButtonRemoveDate");
            this.toolStripButtonRemoveDate.Name = "toolStripButtonRemoveDate";
            this.toolStripButtonRemoveDate.Click += new System.EventHandler(this.toolStripButtonRemoveDate_Click);
            // 
            // toolStripButtonClose
            // 
            this.toolStripButtonClose.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Close;
            resources.ApplyResources(this.toolStripButtonClose, "toolStripButtonClose");
            this.toolStripButtonClose.Name = "toolStripButtonClose";
            this.toolStripButtonClose.Click += new System.EventHandler(this.toolStripButtonClose_Click);
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.tableLayoutPanelMain, "tableLayoutPanelMain");
            this.tableLayoutPanelMain.Controls.Add(this.textBoxExtName, 1, 1);
            this.tableLayoutPanelMain.Controls.Add(this.textBoxExtMessage, 1, 3);
            this.tableLayoutPanelMain.Controls.Add(this.labelName, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.textBoxExtSubject, 1, 2);
            this.tableLayoutPanelMain.Controls.Add(this.labelMessage, 0, 3);
            this.tableLayoutPanelMain.Controls.Add(this.labelUserStatus, 0, 4);
            this.tableLayoutPanelMain.Controls.Add(this.labelSubject, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.textBoxExtStatus, 1, 4);
            this.tableLayoutPanelMain.Controls.Add(this.gradientPanelScheduleHasChanged, 0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            // 
            // textBoxExtName
            // 
            resources.ApplyResources(this.textBoxExtName, "textBoxExtName");
            this.textBoxExtName.Name = "textBoxExtName";
            // 
            // textBoxExtMessage
            // 
            this.textBoxExtMessage.AcceptsReturn = true;
            resources.ApplyResources(this.textBoxExtMessage, "textBoxExtMessage");
            this.textBoxExtMessage.Name = "textBoxExtMessage";
            this.textBoxExtMessage.OverflowIndicatorToolTipText = null;
            // 
            // labelName
            // 
            resources.ApplyResources(this.labelName, "labelName");
            this.labelName.Name = "labelName";
            // 
            // textBoxExtSubject
            // 
            resources.ApplyResources(this.textBoxExtSubject, "textBoxExtSubject");
            this.textBoxExtSubject.Name = "textBoxExtSubject";
            this.textBoxExtSubject.OverflowIndicatorToolTipText = null;
            // 
            // labelMessage
            // 
            resources.ApplyResources(this.labelMessage, "labelMessage");
            this.labelMessage.Name = "labelMessage";
            // 
            // labelUserStatus
            // 
            resources.ApplyResources(this.labelUserStatus, "labelUserStatus");
            this.labelUserStatus.Name = "labelUserStatus";
            // 
            // labelSubject
            // 
            resources.ApplyResources(this.labelSubject, "labelSubject");
            this.labelSubject.Name = "labelSubject";
            // 
            // textBoxExtStatus
            // 
            resources.ApplyResources(this.textBoxExtStatus, "textBoxExtStatus");
            this.textBoxExtStatus.Name = "textBoxExtStatus";
            // 
            // gradientPanelScheduleHasChanged
            // 
            this.gradientPanelScheduleHasChanged.BackColor = System.Drawing.SystemColors.Info;
            this.gradientPanelScheduleHasChanged.BorderColor = System.Drawing.SystemColors.ControlDarkDark;
            this.gradientPanelScheduleHasChanged.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanelMain.SetColumnSpan(this.gradientPanelScheduleHasChanged, 2);
            this.gradientPanelScheduleHasChanged.Controls.Add(this.labelReturnMessage);
            resources.ApplyResources(this.gradientPanelScheduleHasChanged, "gradientPanelScheduleHasChanged");
            this.gradientPanelScheduleHasChanged.Name = "gradientPanelScheduleHasChanged";
            // 
            // labelReturnMessage
            // 
            resources.ApplyResources(this.labelReturnMessage, "labelReturnMessage");
            this.labelReturnMessage.Name = "labelReturnMessage";
            // 
            // addDatesView1
            // 
            resources.ApplyResources(this.addDatesView1, "addDatesView1");
            this.addDatesView1.Name = "addDatesView1";
            this.addDatesView1.DatesSelected += new System.EventHandler<Teleopti.Ccc.AgentPortal.Requests.ShiftTrade.DateRangeSelectionEventArgs>(this.addDatesView1_DatesSelected);
            this.addDatesView1.BeforePopup += new System.EventHandler(this.addDatesView1_BeforePopup);
            this.addDatesView1.PopupClosed += new System.EventHandler(this.addDatesView1_PopupClosed);
            // 
            // officeButtonSave
            // 
            this.officeButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            this.officeButtonSave.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Save;
            resources.ApplyResources(this.officeButtonSave, "officeButtonSave");
            this.officeButtonSave.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.officeButtonSave.Name = "officeButtonSave";
            // 
            // ShiftTradeView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Controls.Add(this.ribbonControlAdvMain);
            this.HelpButtonImage = ((System.Drawing.Image)(resources.GetObject("$this.HelpButtonImage")));
            this.Name = "ShiftTradeView";
            this.Load += new System.EventHandler(this.ShiftTradeForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).EndInit();
            this.ribbonControlAdvMain.ResumeLayout(false);
            this.ribbonControlAdvMain.PerformLayout();
            this.toolStripTabItemResponse.Panel.ResumeLayout(false);
            this.toolStripTabItemResponse.Panel.PerformLayout();
            this.toolStripExActions.ResumeLayout(false);
            this.toolStripExActions.PerformLayout();
            this.toolStripExMain.ResumeLayout(false);
            this.toolStripExMain.PerformLayout();
            this.toolStripExAddDates.ResumeLayout(false);
            this.toolStripExAddDates.PerformLayout();
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtMessage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtSubject)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelScheduleHasChanged)).EndInit();
            this.gradientPanelScheduleHasChanged.ResumeLayout(false);
            this.gradientPanelScheduleHasChanged.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdvMain;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemResponse;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExMain;
        private System.Windows.Forms.ToolStripButton toolStripButtonAccept;
        private System.Windows.Forms.ToolStripButton toolStripButtonDeny;
        private System.Windows.Forms.Label textBoxExtName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtMessage;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtSubject;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelUserStatus;
        private System.Windows.Forms.Label labelSubject;
        private System.Windows.Forms.Label textBoxExtStatus;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExAddDates;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemoveDate;
        private System.Windows.Forms.ToolStripButton toolStripButtonAddDays;
        private AddDatesView addDatesView1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExActions;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAndClose;
        private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
        private Syncfusion.Windows.Forms.Tools.OfficeButton officeButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonClose;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelScheduleHasChanged;
        private System.Windows.Forms.Label labelReturnMessage;


    }
}
