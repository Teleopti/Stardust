using System.Drawing.Printing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
    partial class ScheduleTeamView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summaryScheduleTeamView_Resize 
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleTeamView));
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            this.splitContainerAdvTeamView = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.navigationMonthCalendarTeamView = new Teleopti.Ccc.AgentPortal.Common.Controls.NavigationMonthCalendar();
            this.panelBetseenCalendarAndRibbon = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gridControlTeamSchedules = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.panelBetweenRibbonAndGridRight = new System.Windows.Forms.Panel();
            this.comboBoxAdvGroup = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.comboSiteAndTeam = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.buttonNextDate = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonPreviousDate = new Syncfusion.Windows.Forms.ButtonAdv();
            this.autoLabelSelectedDate = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvTeamView)).BeginInit();
            this.splitContainerAdvTeamView.Panel1.SuspendLayout();
            this.splitContainerAdvTeamView.Panel2.SuspendLayout();
            this.splitContainerAdvTeamView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.navigationMonthCalendarTeamView)).BeginInit();
            this.panelBetseenCalendarAndRibbon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlTeamSchedules)).BeginInit();
            this.panelBetweenRibbonAndGridRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvGroup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboSiteAndTeam)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerAdvTeamView
            // 
            this.splitContainerAdvTeamView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerAdvTeamView.FixedPanel = Syncfusion.Windows.Forms.Tools.Enums.FixedPanel.Panel1;
            this.splitContainerAdvTeamView.Location = new System.Drawing.Point(0, 0);
            this.splitContainerAdvTeamView.Name = "splitContainerAdvTeamView";
            // 
            // splitContainerAdvTeamView.Panel1
            // 
            this.splitContainerAdvTeamView.Panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.splitContainerAdvTeamView.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(209)))), ((int)(((byte)(255)))));
            this.splitContainerAdvTeamView.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdvTeamView.Panel1.Controls.Add(this.navigationMonthCalendarTeamView);
            this.splitContainerAdvTeamView.Panel1.Controls.Add(this.panelBetseenCalendarAndRibbon);
            // 
            // splitContainerAdvTeamView.Panel2
            // 
            this.splitContainerAdvTeamView.Panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.splitContainerAdvTeamView.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(209)))), ((int)(((byte)(255)))));
            this.splitContainerAdvTeamView.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdvTeamView.Panel2.Controls.Add(this.gridControlTeamSchedules);
            this.splitContainerAdvTeamView.Panel2.Controls.Add(this.panelBetweenRibbonAndGridRight);
            this.splitContainerAdvTeamView.Panel2MinSize = 175;
            this.splitContainerAdvTeamView.Size = new System.Drawing.Size(1466, 546);
            this.splitContainerAdvTeamView.SplitterDistance = 175;
            this.splitContainerAdvTeamView.SplitterWidth = 4;
            this.splitContainerAdvTeamView.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.splitContainerAdvTeamView.TabIndex = 6;
            this.splitContainerAdvTeamView.Text = "splitContainerAdv1";
            // 
            // navigationMonthCalendarTeamView
            // 
            this.navigationMonthCalendarTeamView.AllowedDateInterval = ((Teleopti.Interfaces.Domain.MinMax<System.DateTime>)(resources.GetObject("navigationMonthCalendarTeamView.AllowedDateInterval")));
            this.navigationMonthCalendarTeamView.BackColor = System.Drawing.SystemColors.Window;
            this.navigationMonthCalendarTeamView.CurrentCulture = new System.Globalization.CultureInfo("sv-SE");
            this.navigationMonthCalendarTeamView.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            this.navigationMonthCalendarTeamView.DateValue = new System.DateTime(2008, 3, 28, 0, 0, 0, 0);
            this.navigationMonthCalendarTeamView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navigationMonthCalendarTeamView.Location = new System.Drawing.Point(0, 32);
            this.navigationMonthCalendarTeamView.Margin = new System.Windows.Forms.Padding(0);
            this.navigationMonthCalendarTeamView.Name = "navigationMonthCalendarTeamView";
            this.navigationMonthCalendarTeamView.Padding = new System.Windows.Forms.Padding(3);
            this.navigationMonthCalendarTeamView.ScheduleType = Syncfusion.Windows.Forms.Schedule.ScheduleViewType.Day;
            this.navigationMonthCalendarTeamView.Size = new System.Drawing.Size(175, 514);
            this.navigationMonthCalendarTeamView.TabIndex = 2;
            // 
            // panelBetseenCalendarAndRibbon
            // 
            this.panelBetseenCalendarAndRibbon.Controls.Add(this.panel1);
            this.panelBetseenCalendarAndRibbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBetseenCalendarAndRibbon.Location = new System.Drawing.Point(0, 0);
            this.panelBetseenCalendarAndRibbon.Name = "panelBetseenCalendarAndRibbon";
            this.panelBetseenCalendarAndRibbon.Size = new System.Drawing.Size(175, 32);
            this.panelBetseenCalendarAndRibbon.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 29);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(175, 3);
            this.panel1.TabIndex = 1;
            // 
            // gridControlTeamSchedules
            // 
            this.gridControlTeamSchedules.AccelerateScrolling = Syncfusion.Windows.Forms.AccelerateScrollingBehavior.Fast;
            this.gridControlTeamSchedules.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.DblClickOnCell;
            this.gridControlTeamSchedules.BackColor = System.Drawing.Color.White;
            gridBaseStyle1.Name = "Header";
            gridBaseStyle1.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.CellType = "Header";
            gridBaseStyle1.StyleInfo.Font.Bold = true;
            gridBaseStyle1.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
            gridBaseStyle1.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
            gridBaseStyle2.Name = "Standard";
            gridBaseStyle2.StyleInfo.Font.Facename = "Tahoma";
            gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
            gridBaseStyle3.Name = "Row Header";
            gridBaseStyle3.StyleInfo.BaseStyle = "Header";
            gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
            gridBaseStyle3.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
            gridBaseStyle4.Name = "Column Header";
            gridBaseStyle4.StyleInfo.BaseStyle = "Header";
            gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
            this.gridControlTeamSchedules.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
            this.gridControlTeamSchedules.ColCount = 0;
            this.gridControlTeamSchedules.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
            this.gridControlTeamSchedules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlTeamSchedules.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gridControlTeamSchedules.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
            this.gridControlTeamSchedules.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridControlTeamSchedules.Location = new System.Drawing.Point(0, 28);
            this.gridControlTeamSchedules.Margin = new System.Windows.Forms.Padding(10);
            this.gridControlTeamSchedules.Name = "gridControlTeamSchedules";
            this.gridControlTeamSchedules.NumberedColHeaders = false;
            this.gridControlTeamSchedules.NumberedRowHeaders = false;
            this.gridControlTeamSchedules.Office2007ScrollBars = true;
            this.gridControlTeamSchedules.Properties.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.gridControlTeamSchedules.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.gridControlTeamSchedules.RowCount = 0;
            this.gridControlTeamSchedules.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
            this.gridControlTeamSchedules.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlTeamSchedules.Size = new System.Drawing.Size(1287, 518);
            this.gridControlTeamSchedules.SmartSizeBox = false;
            this.gridControlTeamSchedules.TabIndex = 0;
            this.gridControlTeamSchedules.ThemesEnabled = true;
            this.gridControlTeamSchedules.UseRightToLeftCompatibleTextBox = true;
            this.gridControlTeamSchedules.ResizingColumns += new Syncfusion.Windows.Forms.Grid.GridResizingColumnsEventHandler(this.gridControlTeamSchedules_ResizingColumns);
            this.gridControlTeamSchedules.ResizingRows += new Syncfusion.Windows.Forms.Grid.GridResizingRowsEventHandler(this.gridControlTeamSchedules_ResizingRows);
            this.gridControlTeamSchedules.CurrentCellActivating += new Syncfusion.Windows.Forms.Grid.GridCurrentCellActivatingEventHandler(this.gridControlTeamSchedules_CurrentCellActivating);
            this.gridControlTeamSchedules.CellClick += new Syncfusion.Windows.Forms.Grid.GridCellClickEventHandler(this.gridControlTeamSchedules_CellClick);
            this.gridControlTeamSchedules.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gridControlTeamSchedules_MouseDown);
            this.gridControlTeamSchedules.Resize += new System.EventHandler(this.gridControlTeamSchedules_Resize);
            // 
            // panelBetweenRibbonAndGridRight
            // 
            this.panelBetweenRibbonAndGridRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(209)))), ((int)(((byte)(255)))));
            this.panelBetweenRibbonAndGridRight.Controls.Add(this.comboBoxAdvGroup);
            this.panelBetweenRibbonAndGridRight.Controls.Add(this.comboSiteAndTeam);
            this.panelBetweenRibbonAndGridRight.Controls.Add(this.buttonNextDate);
            this.panelBetweenRibbonAndGridRight.Controls.Add(this.buttonPreviousDate);
            this.panelBetweenRibbonAndGridRight.Controls.Add(this.autoLabelSelectedDate);
            this.panelBetweenRibbonAndGridRight.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBetweenRibbonAndGridRight.Location = new System.Drawing.Point(0, 0);
            this.panelBetweenRibbonAndGridRight.Name = "panelBetweenRibbonAndGridRight";
            this.panelBetweenRibbonAndGridRight.Size = new System.Drawing.Size(1287, 28);
            this.panelBetweenRibbonAndGridRight.TabIndex = 1;
            this.panelBetweenRibbonAndGridRight.Resize += new System.EventHandler(this.panelBetweenRibbonAndGridRightResize);
            // 
            // comboBoxAdvGroup
            // 
            this.comboBoxAdvGroup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdvGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvGroup.Location = new System.Drawing.Point(44, 3);
            this.comboBoxAdvGroup.Name = "comboBoxAdvGroup";
            this.comboBoxAdvGroup.Size = new System.Drawing.Size(224, 21);
            this.comboBoxAdvGroup.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvGroup.TabIndex = 2;
            this.comboBoxAdvGroup.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvGroup_SelectedIndexChanged);
            // 
            // comboSiteAndTeam
            // 
            this.comboSiteAndTeam.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboSiteAndTeam.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSiteAndTeam.Location = new System.Drawing.Point(274, 3);
            this.comboSiteAndTeam.Name = "comboSiteAndTeam";
            this.comboSiteAndTeam.Size = new System.Drawing.Size(224, 21);
            this.comboSiteAndTeam.Sorted = true;
            this.comboSiteAndTeam.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboSiteAndTeam.TabIndex = 2;
            this.comboSiteAndTeam.SelectedIndexChanged += new System.EventHandler(this.comboSiteAndTeam_SelectedIndexChanged);
            // 
            // buttonNextDate
            // 
            this.buttonNextDate.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonNextDate.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonNextDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(209)))), ((int)(((byte)(255)))));
            this.buttonNextDate.Location = new System.Drawing.Point(1243, 0);
            this.buttonNextDate.Name = "buttonNextDate";
            this.buttonNextDate.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonNextDate.Size = new System.Drawing.Size(35, 26);
            this.buttonNextDate.TabIndex = 1;
            this.buttonNextDate.Text = ">";
            this.buttonNextDate.UseVisualStyle = true;
            this.buttonNextDate.Click += new System.EventHandler(this.buttonNextDate_Click);
            // 
            // buttonPreviousDate
            // 
            this.buttonPreviousDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonPreviousDate.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonPreviousDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(209)))), ((int)(((byte)(255)))));
            this.buttonPreviousDate.Location = new System.Drawing.Point(3, 0);
            this.buttonPreviousDate.Name = "buttonPreviousDate";
            this.buttonPreviousDate.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonPreviousDate.Size = new System.Drawing.Size(35, 26);
            this.buttonPreviousDate.TabIndex = 0;
            this.buttonPreviousDate.Text = "<";
            this.buttonPreviousDate.UseVisualStyle = true;
            this.buttonPreviousDate.Click += new System.EventHandler(this.buttonPreviousDate_Click);
            // 
            // autoLabelSelectedDate
            // 
            this.autoLabelSelectedDate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.autoLabelSelectedDate.AutoSize = false;
            this.autoLabelSelectedDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autoLabelSelectedDate.Location = new System.Drawing.Point(495, 0);
            this.autoLabelSelectedDate.Name = "autoLabelSelectedDate";
            this.autoLabelSelectedDate.Size = new System.Drawing.Size(742, 28);
            this.autoLabelSelectedDate.TabIndex = 2;
            this.autoLabelSelectedDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ScheduleTeamView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerAdvTeamView);
            this.Name = "ScheduleTeamView";
            this.Size = new System.Drawing.Size(1466, 546);
            this.splitContainerAdvTeamView.Panel1.ResumeLayout(false);
            this.splitContainerAdvTeamView.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvTeamView)).EndInit();
            this.splitContainerAdvTeamView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.navigationMonthCalendarTeamView)).EndInit();
            this.panelBetseenCalendarAndRibbon.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlTeamSchedules)).EndInit();
            this.panelBetweenRibbonAndGridRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvGroup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboSiteAndTeam)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvTeamView;
        //private Syncfusion.Windows.Forms.Grid.GridControl gridControlActivityColourCodes;
        private Teleopti.Ccc.AgentPortal.Common.Controls.NavigationMonthCalendar navigationMonthCalendarTeamView;
        private GridControl gridControlTeamSchedules;
        private Panel panelBetweenRibbonAndGridRight;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboSiteAndTeam;
        private Syncfusion.Windows.Forms.ButtonAdv buttonNextDate;
        private Syncfusion.Windows.Forms.ButtonAdv buttonPreviousDate;
        private Panel panelBetseenCalendarAndRibbon;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelSelectedDate;
        private Panel panel1;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvGroup;
        
    }
}
