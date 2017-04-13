namespace Teleopti.Ccc.Win.Grouping
{
    partial class FindPersonsView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindPersonsView));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.groupBoxDateSelection = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.dateTimePickerAdvTo = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.dateTimePickerAdvFrom = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.textBoxExtFind = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.label3 = new System.Windows.Forms.Label();
			this.treeViewAdvResult = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			this.contextMenuStripTreeActions = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemCut = new System.Windows.Forms.ToolStripMenuItem();
			this.imageListTreeView = new System.Windows.Forms.ImageList(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.groupBoxDateSelection.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvTo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvTo.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvFrom)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvFrom.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtFind)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.treeViewAdvResult)).BeginInit();
			this.contextMenuStripTreeActions.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.Controls.Add(this.groupBoxDateSelection, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxExtFind, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.treeViewAdvResult, 0, 2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// groupBoxDateSelection
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.groupBoxDateSelection, 2);
			this.groupBoxDateSelection.Controls.Add(this.tableLayoutPanel2);
			resources.ApplyResources(this.groupBoxDateSelection, "groupBoxDateSelection");
			this.groupBoxDateSelection.Name = "groupBoxDateSelection";
			this.groupBoxDateSelection.TabStop = false;
			// 
			// tableLayoutPanel2
			// 
			resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.dateTimePickerAdvTo, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.dateTimePickerAdvFrom, 1, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			// 
			// dateTimePickerAdvTo
			// 
			this.dateTimePickerAdvTo.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
			this.dateTimePickerAdvTo.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvTo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvTo.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvTo.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvTo.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvTo.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvTo.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvTo.Calendar.DayNamesColor = System.Drawing.Color.Black;
			this.dateTimePickerAdvTo.Calendar.DayNamesFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvTo.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvTo.Calendar.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dateTimePickerAdvTo.Calendar.Dock")));
			this.dateTimePickerAdvTo.Calendar.Font = ((System.Drawing.Font)(resources.GetObject("dateTimePickerAdvTo.Calendar.Font")));
			this.dateTimePickerAdvTo.Calendar.ForeColor = System.Drawing.Color.Black;
			this.dateTimePickerAdvTo.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvTo.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvTo.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvTo.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvTo.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvTo.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvTo.Calendar.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvTo.Calendar.Location")));
			this.dateTimePickerAdvTo.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvTo.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvTo.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvTo.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvTo.Calendar.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvTo.Calendar.Size")));
			this.dateTimePickerAdvTo.Calendar.SizeToFit = true;
			this.dateTimePickerAdvTo.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvTo.Calendar.TabIndex = ((int)(resources.GetObject("dateTimePickerAdvTo.Calendar.TabIndex")));
			this.dateTimePickerAdvTo.Calendar.ThemedEnabledGrid = true;
			this.dateTimePickerAdvTo.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvTo.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvTo.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvTo.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvTo.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvTo.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvTo.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvTo.Calendar.NoneButton.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvTo.Calendar.NoneButton.Location")));
			this.dateTimePickerAdvTo.Calendar.NoneButton.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvTo.Calendar.NoneButton.Size")));
			this.dateTimePickerAdvTo.Calendar.NoneButton.Text = resources.GetString("dateTimePickerAdvTo.Calendar.NoneButton.Text");
			this.dateTimePickerAdvTo.Calendar.NoneButton.UseVisualStyle = true;
			// 
			// 
			// 
			this.dateTimePickerAdvTo.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvTo.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvTo.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvTo.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvTo.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvTo.Calendar.TodayButton.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvTo.Calendar.TodayButton.Location")));
			this.dateTimePickerAdvTo.Calendar.TodayButton.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvTo.Calendar.TodayButton.Size")));
			this.dateTimePickerAdvTo.Calendar.TodayButton.Text = resources.GetString("dateTimePickerAdvTo.Calendar.TodayButton.Text");
			this.dateTimePickerAdvTo.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvTo.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvTo.CalendarForeColor = System.Drawing.Color.Black;
			this.dateTimePickerAdvTo.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvTo.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvTo.ClipboardFormat = System.Windows.Forms.DateTimePickerFormat.Short;
			resources.ApplyResources(this.dateTimePickerAdvTo, "dateTimePickerAdvTo");
			this.dateTimePickerAdvTo.DropDownImage = null;
			this.dateTimePickerAdvTo.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvTo.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvTo.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvTo.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvTo.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvTo.Name = "dateTimePickerAdvTo";
			this.dateTimePickerAdvTo.ShowCheckBox = false;
			this.dateTimePickerAdvTo.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvTo.ThemedChildControls = true;
			this.dateTimePickerAdvTo.ThemesEnabled = true;
			this.dateTimePickerAdvTo.UseCurrentCulture = true;
			this.dateTimePickerAdvTo.Value = new System.DateTime(2011, 6, 15, 13, 21, 32, 852);
			this.dateTimePickerAdvTo.ValueChanged += new System.EventHandler(this.dateTimePickerAdvToValueChanged);
			// 
			// dateTimePickerAdvFrom
			// 
			this.dateTimePickerAdvFrom.BackColor = System.Drawing.Color.White;
			this.dateTimePickerAdvFrom.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
			this.dateTimePickerAdvFrom.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvFrom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvFrom.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvFrom.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvFrom.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvFrom.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvFrom.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvFrom.Calendar.DayNamesColor = System.Drawing.Color.Black;
			this.dateTimePickerAdvFrom.Calendar.DayNamesFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvFrom.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvFrom.Calendar.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dateTimePickerAdvFrom.Calendar.Dock")));
			this.dateTimePickerAdvFrom.Calendar.Font = ((System.Drawing.Font)(resources.GetObject("dateTimePickerAdvFrom.Calendar.Font")));
			this.dateTimePickerAdvFrom.Calendar.ForeColor = System.Drawing.Color.Black;
			this.dateTimePickerAdvFrom.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvFrom.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvFrom.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvFrom.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvFrom.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvFrom.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvFrom.Calendar.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvFrom.Calendar.Location")));
			this.dateTimePickerAdvFrom.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvFrom.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvFrom.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvFrom.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvFrom.Calendar.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvFrom.Calendar.Size")));
			this.dateTimePickerAdvFrom.Calendar.SizeToFit = true;
			this.dateTimePickerAdvFrom.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvFrom.Calendar.TabIndex = ((int)(resources.GetObject("dateTimePickerAdvFrom.Calendar.TabIndex")));
			this.dateTimePickerAdvFrom.Calendar.ThemedEnabledGrid = true;
			this.dateTimePickerAdvFrom.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvFrom.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvFrom.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvFrom.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvFrom.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvFrom.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvFrom.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvFrom.Calendar.NoneButton.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvFrom.Calendar.NoneButton.Location")));
			this.dateTimePickerAdvFrom.Calendar.NoneButton.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvFrom.Calendar.NoneButton.Size")));
			this.dateTimePickerAdvFrom.Calendar.NoneButton.Text = resources.GetString("dateTimePickerAdvFrom.Calendar.NoneButton.Text");
			this.dateTimePickerAdvFrom.Calendar.NoneButton.UseVisualStyle = true;
			// 
			// 
			// 
			this.dateTimePickerAdvFrom.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvFrom.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvFrom.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvFrom.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvFrom.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvFrom.Calendar.TodayButton.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvFrom.Calendar.TodayButton.Location")));
			this.dateTimePickerAdvFrom.Calendar.TodayButton.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvFrom.Calendar.TodayButton.Size")));
			this.dateTimePickerAdvFrom.Calendar.TodayButton.Text = resources.GetString("dateTimePickerAdvFrom.Calendar.TodayButton.Text");
			this.dateTimePickerAdvFrom.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvFrom.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvFrom.CalendarForeColor = System.Drawing.Color.Black;
			this.dateTimePickerAdvFrom.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvFrom.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvFrom.ClipboardFormat = System.Windows.Forms.DateTimePickerFormat.Short;
			resources.ApplyResources(this.dateTimePickerAdvFrom, "dateTimePickerAdvFrom");
			this.dateTimePickerAdvFrom.DropDownImage = null;
			this.dateTimePickerAdvFrom.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvFrom.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvFrom.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvFrom.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvFrom.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvFrom.Name = "dateTimePickerAdvFrom";
			this.dateTimePickerAdvFrom.ShowCheckBox = false;
			this.dateTimePickerAdvFrom.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvFrom.ThemedChildControls = true;
			this.dateTimePickerAdvFrom.ThemesEnabled = true;
			this.dateTimePickerAdvFrom.UseCurrentCulture = true;
			this.dateTimePickerAdvFrom.Value = new System.DateTime(2011, 6, 15, 13, 21, 32, 852);
			this.dateTimePickerAdvFrom.ValueChanged += new System.EventHandler(this.dateTimePickerAdvFromValueChanged);
			// 
			// textBoxExtFind
			// 
			this.textBoxExtFind.BeforeTouchSize = new System.Drawing.Size(233, 23);
			this.textBoxExtFind.Cursor = System.Windows.Forms.Cursors.IBeam;
			resources.ApplyResources(this.textBoxExtFind, "textBoxExtFind");
			this.textBoxExtFind.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtFind.Name = "textBoxExtFind";
			this.textBoxExtFind.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.textBoxExtFind.TextChanged += new System.EventHandler(this.textBoxExtFindTextChanged);
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// treeViewAdvResult
			// 
			this.treeViewAdvResult.BackColor = System.Drawing.Color.White;
			this.treeViewAdvResult.BeforeTouchSize = new System.Drawing.Size(293, 428);
			this.treeViewAdvResult.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.treeViewAdvResult.BorderColor = System.Drawing.Color.Transparent;
			this.treeViewAdvResult.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.treeViewAdvResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdvResult.CanSelectDisabledNode = false;
			this.tableLayoutPanel1.SetColumnSpan(this.treeViewAdvResult, 2);
			this.treeViewAdvResult.ContextMenuStrip = this.contextMenuStripTreeActions;
			resources.ApplyResources(this.treeViewAdvResult, "treeViewAdvResult");
			this.treeViewAdvResult.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			// 
			// 
			// 
			this.treeViewAdvResult.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdvResult.HelpTextControl.Location = ((System.Drawing.Point)(resources.GetObject("treeViewAdvResult.HelpTextControl.Location")));
			this.treeViewAdvResult.HelpTextControl.Name = "helpText";
			this.treeViewAdvResult.HelpTextControl.Size = ((System.Drawing.Size)(resources.GetObject("treeViewAdvResult.HelpTextControl.Size")));
			this.treeViewAdvResult.HelpTextControl.TabIndex = ((int)(resources.GetObject("treeViewAdvResult.HelpTextControl.TabIndex")));
			this.treeViewAdvResult.HelpTextControl.Text = resources.GetString("treeViewAdvResult.HelpTextControl.Text");
			this.treeViewAdvResult.LeftImageList = this.imageListTreeView;
			this.treeViewAdvResult.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.treeViewAdvResult.Name = "treeViewAdvResult";
			this.treeViewAdvResult.Office2007ScrollBars = true;
			this.treeViewAdvResult.SelectedNodeBackground = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220))))));
			this.treeViewAdvResult.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectSameLevel;
			this.treeViewAdvResult.ShowFocusRect = false;
			this.treeViewAdvResult.Style = Syncfusion.Windows.Forms.Tools.TreeStyle.Metro;
			// 
			// 
			// 
			this.treeViewAdvResult.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
			this.treeViewAdvResult.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdvResult.ToolTipControl.Location = ((System.Drawing.Point)(resources.GetObject("treeViewAdvResult.ToolTipControl.Location")));
			this.treeViewAdvResult.ToolTipControl.Name = "toolTip";
			this.treeViewAdvResult.ToolTipControl.Size = ((System.Drawing.Size)(resources.GetObject("treeViewAdvResult.ToolTipControl.Size")));
			this.treeViewAdvResult.ToolTipControl.TabIndex = ((int)(resources.GetObject("treeViewAdvResult.ToolTipControl.TabIndex")));
			this.treeViewAdvResult.ToolTipControl.Text = resources.GetString("treeViewAdvResult.ToolTipControl.Text");
			this.treeViewAdvResult.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewAdvPreviewTreeItemDrag);
			this.treeViewAdvResult.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeViewAdvResultKeyDown);
			// 
			// contextMenuStripTreeActions
			// 
			this.contextMenuStripTreeActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCut});
			this.contextMenuStripTreeActions.Name = "contextMenuStripTreeActions";
			this.contextMenuStripTreeActions.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			resources.ApplyResources(this.contextMenuStripTreeActions, "contextMenuStripTreeActions");
			this.contextMenuStripTreeActions.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripTreeActionsOpening);
			// 
			// toolStripMenuItemCut
			// 
			this.toolStripMenuItemCut.Name = "toolStripMenuItemCut";
			resources.ApplyResources(this.toolStripMenuItemCut, "toolStripMenuItemCut");
			this.toolStripMenuItemCut.Click += new System.EventHandler(this.toolStripMenuItemCutClick);
			// 
			// imageListTreeView
			// 
			this.imageListTreeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTreeView.ImageStream")));
			this.imageListTreeView.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListTreeView.Images.SetKeyName(0, "ccc_tree_Agent.png");
			// 
			// FindPersonsView
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "FindPersonsView";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.groupBoxDateSelection.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvTo.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvTo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvFrom.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvFrom)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtFind)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.treeViewAdvResult)).EndInit();
			this.contextMenuStripTreeActions.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewAdvResult;
        private System.Windows.Forms.GroupBox groupBoxDateSelection;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtFind;
        private System.Windows.Forms.Label label3;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvTo;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvFrom;
        private System.Windows.Forms.ImageList imageListTreeView;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTreeActions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCut;
    }
}
