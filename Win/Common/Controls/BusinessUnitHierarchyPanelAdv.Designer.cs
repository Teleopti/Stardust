using Syncfusion.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class BusinessUnitHierarchyPanelAdv
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BusinessUnitHierarchyPanelAdv));
            Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo treeNodeAdvStyleInfo1 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.xbtnRefresh = new System.Windows.Forms.Button();
            this.xdtpDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.ximgBUStructure = new System.Windows.Forms.ImageList(this.components);
            this.xtreeBUStructure = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            ((System.ComponentModel.ISupportInitialize)(this.xdtpDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xdtpDate.Calendar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtreeBUStructure)).BeginInit();
            this.SuspendLayout();
            // 
            // xbtnRefresh
            // 
            this.xbtnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.xbtnRefresh.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Refresh ;
            this.xbtnRefresh.Location = new System.Drawing.Point(157, -112);
            this.xbtnRefresh.Name = "xbtnRefresh";
            this.xbtnRefresh.Size = new System.Drawing.Size(22, 22);
            this.xbtnRefresh.TabIndex = 6;
            this.toolTip1.SetToolTip(this.xbtnRefresh, "xxRefreshData");
            this.xbtnRefresh.UseVisualStyleBackColor = true;
            // 
            // xdtpDate
            // 
            this.xdtpDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.xdtpDate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this.xdtpDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.xdtpDate.Calendar.AllowMultipleSelection = false;
            this.xdtpDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.xdtpDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.xdtpDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xdtpDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xdtpDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.xdtpDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.xdtpDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.xdtpDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.xdtpDate.Calendar.HeaderHeight = 20;
            this.xdtpDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.xdtpDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.xdtpDate.Calendar.HeadGradient = true;
            this.xdtpDate.Calendar.Location = new System.Drawing.Point(0, 0);
            this.xdtpDate.Calendar.Name = "monthCalendar";
            this.xdtpDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.xdtpDate.Calendar.SelectedDates = new System.DateTime[0];
            this.xdtpDate.Calendar.Size = new System.Drawing.Size(206, 174);
            this.xdtpDate.Calendar.SizeToFit = true;
            this.xdtpDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.xdtpDate.Calendar.TabIndex = 0;
            this.xdtpDate.Calendar.ThemedEnabledGrid = true;
            this.xdtpDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.xdtpDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.xdtpDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.xdtpDate.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.xdtpDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.xdtpDate.Calendar.NoneButton.Text = "None";
            this.xdtpDate.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.xdtpDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.xdtpDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.xdtpDate.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.xdtpDate.Calendar.TodayButton.Text = "Today";
            this.xdtpDate.Calendar.TodayButton.UseVisualStyle = true;
            this.xdtpDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.xdtpDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.xdtpDate.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.xdtpDate.Dock = System.Windows.Forms.DockStyle.Top;
            this.xdtpDate.DropDownImage = null;
            this.xdtpDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.xdtpDate.Location = new System.Drawing.Point(0, 0);
            this.xdtpDate.MinValue = new System.DateTime(((long)(0)));
            this.xdtpDate.Name = "xdtpDate";
            this.xdtpDate.ShowCheckBox = false;
            this.xdtpDate.Size = new System.Drawing.Size(202, 20);
            this.xdtpDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.xdtpDate.TabIndex = 5;
            this.xdtpDate.ThemedChildControls = true;
            this.xdtpDate.ThemesEnabled = true;
            this.xdtpDate.UseCurrentCulture = true;
            this.xdtpDate.Value = new System.DateTime(2008, 8, 27, 9, 25, 32, 508);
            this.xdtpDate.ValueChanged += new System.EventHandler(this.xdtpDate_ValueChanged);
            // 
            // ximgBUStructure
            // 
            this.ximgBUStructure.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ximgBUStructure.ImageStream")));
            this.ximgBUStructure.TransparentColor = System.Drawing.Color.Transparent;
            this.ximgBUStructure.Images.SetKeyName(0, "ccc_BusinessUnit.png");
            this.ximgBUStructure.Images.SetKeyName(1, "ccc_Site.png");
            this.ximgBUStructure.Images.SetKeyName(2, "ccc_Team.png");
            this.ximgBUStructure.Images.SetKeyName(3, "ccc_Agent.png");
            this.ximgBUStructure.Images.SetKeyName(4, "ccc_User.png");
            this.ximgBUStructure.Images.SetKeyName(5, "Bu16.ico");
            this.ximgBUStructure.Images.SetKeyName(6, "Home16.ico");
            this.ximgBUStructure.Images.SetKeyName(7, "personSmall.ico");
            this.ximgBUStructure.Images.SetKeyName(8, "onePersonSmall.ico");
            this.ximgBUStructure.Images.SetKeyName(9, "Bu32.ico");
            // 
            // xtreeBUStructure
            // 
            treeNodeAdvStyleInfo1.CacheValues = false;
            treeNodeAdvStyleInfo1.EnsureDefaultOptionedChild = true;
            treeNodeAdvStyleInfo1.ShowCheckBox = false;
            treeNodeAdvStyleInfo1.ShowOptionButton = false;
            treeNodeAdvStyleInfo1.ThemesEnabled = true;
            this.xtreeBUStructure.BaseStylePairs.AddRange(new Syncfusion.Windows.Forms.Tools.StyleNamePair[] {
            new Syncfusion.Windows.Forms.Tools.StyleNamePair("Standard", treeNodeAdvStyleInfo1)});
            this.xtreeBUStructure.BorderColor = System.Drawing.Color.Transparent;
            this.xtreeBUStructure.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.xtreeBUStructure.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.xtreeBUStructure.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // 
            // 
            this.xtreeBUStructure.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.xtreeBUStructure.HelpTextControl.Location = new System.Drawing.Point(0, 0);
            this.xtreeBUStructure.HelpTextControl.Name = "helpText";
            this.xtreeBUStructure.HelpTextControl.Size = new System.Drawing.Size(62, 15);
            this.xtreeBUStructure.HelpTextControl.TabIndex = 0;
            this.xtreeBUStructure.HelpTextControl.Text = "xxHelpText";
            this.xtreeBUStructure.LeftImageList = this.ximgBUStructure;
            this.xtreeBUStructure.Location = new System.Drawing.Point(0, 20);
            this.xtreeBUStructure.Name = "xtreeBUStructure";
            this.xtreeBUStructure.Office2007ScrollBars = true;
            this.xtreeBUStructure.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
            this.xtreeBUStructure.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectAll;
            this.xtreeBUStructure.ShowCheckBoxes = false;
            this.xtreeBUStructure.Size = new System.Drawing.Size(202, 132);
            this.xtreeBUStructure.TabIndex = 9;
            this.xtreeBUStructure.ThemesEnabled = true;
            // 
            // 
            // 
            this.xtreeBUStructure.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
            this.xtreeBUStructure.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.xtreeBUStructure.ToolTipControl.Location = new System.Drawing.Point(0, 0);
            this.xtreeBUStructure.ToolTipControl.Name = "toolTip";
            this.xtreeBUStructure.ToolTipControl.Size = new System.Drawing.Size(51, 15);
            this.xtreeBUStructure.ToolTipControl.TabIndex = 1;
            this.xtreeBUStructure.ToolTipControl.Text = "yytoolTip";
            this.xtreeBUStructure.MouseClick += new System.Windows.Forms.MouseEventHandler(this.xtreeBUStructure_MouseClick);
            // 
            // BusinessUnitHierarchyPanelAdv
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.xtreeBUStructure);
            this.Controls.Add(this.xbtnRefresh);
            this.Controls.Add(this.xdtpDate);
            this.Name = "BusinessUnitHierarchyPanelAdv";
            this.Size = new System.Drawing.Size(202, 152);
            ((System.ComponentModel.ISupportInitialize)(this.xdtpDate.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xdtpDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtreeBUStructure)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button xbtnRefresh;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv xdtpDate;
        private System.Windows.Forms.ImageList ximgBUStructure;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv xtreeBUStructure;

    }
}
