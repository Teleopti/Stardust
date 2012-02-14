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
            Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo treeNodeAdvStyleInfo1 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo();
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
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvFrom)).BeginInit();
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
            this.dateTimePickerAdvTo.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.dateTimePickerAdvTo.BorderColor = System.Drawing.Color.Empty;
            this.dateTimePickerAdvTo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.dateTimePickerAdvTo.Calendar.AllowMultipleSelection = false;
            this.dateTimePickerAdvTo.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerAdvTo.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdvTo.Calendar.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dateTimePickerAdvTo.Calendar.Dock")));
            this.dateTimePickerAdvTo.Calendar.Font = ((System.Drawing.Font)(resources.GetObject("dateTimePickerAdvTo.Calendar.Font")));
            this.dateTimePickerAdvTo.Calendar.ForeColor = System.Drawing.Color.Black;
            this.dateTimePickerAdvTo.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.dateTimePickerAdvTo.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.dateTimePickerAdvTo.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.dateTimePickerAdvTo.Calendar.HeaderHeight = 20;
            this.dateTimePickerAdvTo.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdvTo.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvTo.Calendar.HeadGradient = true;
            this.dateTimePickerAdvTo.Calendar.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvTo.Calendar.Location")));
            this.dateTimePickerAdvTo.Calendar.Name = "monthCalendar";
            this.dateTimePickerAdvTo.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.dateTimePickerAdvTo.Calendar.SelectedDates = new System.DateTime[0];
            this.dateTimePickerAdvTo.Calendar.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvTo.Calendar.Size")));
            this.dateTimePickerAdvTo.Calendar.SizeToFit = true;
            this.dateTimePickerAdvTo.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdvTo.Calendar.TabIndex = ((int)(resources.GetObject("dateTimePickerAdvTo.Calendar.TabIndex")));
            this.dateTimePickerAdvTo.Calendar.ThemedEnabledGrid = true;
            this.dateTimePickerAdvTo.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdvTo.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.dateTimePickerAdvTo.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdvTo.Calendar.NoneButton.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvTo.Calendar.NoneButton.Location")));
            this.dateTimePickerAdvTo.Calendar.NoneButton.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvTo.Calendar.NoneButton.Size")));
            this.dateTimePickerAdvTo.Calendar.NoneButton.Text = resources.GetString("dateTimePickerAdvTo.Calendar.NoneButton.Text");
            this.dateTimePickerAdvTo.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.dateTimePickerAdvTo.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdvTo.Calendar.TodayButton.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvTo.Calendar.TodayButton.Location")));
            this.dateTimePickerAdvTo.Calendar.TodayButton.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvTo.Calendar.TodayButton.Size")));
            this.dateTimePickerAdvTo.Calendar.TodayButton.Text = resources.GetString("dateTimePickerAdvTo.Calendar.TodayButton.Text");
            this.dateTimePickerAdvTo.Calendar.TodayButton.UseVisualStyle = true;
            this.dateTimePickerAdvTo.CalendarForeColor = System.Drawing.Color.Black;
            this.dateTimePickerAdvTo.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdvTo.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvTo.ClipboardFormat = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerAdvTo.Culture = new System.Globalization.CultureInfo("sv-SE");
            resources.ApplyResources(this.dateTimePickerAdvTo, "dateTimePickerAdvTo");
            this.dateTimePickerAdvTo.DropDownImage = null;
            this.dateTimePickerAdvTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerAdvTo.MinValue = new System.DateTime(((long)(0)));
            this.dateTimePickerAdvTo.Name = "dateTimePickerAdvTo";
            this.dateTimePickerAdvTo.ShowCheckBox = false;
            this.dateTimePickerAdvTo.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdvTo.ThemedChildControls = true;
            this.dateTimePickerAdvTo.ThemesEnabled = true;
            this.dateTimePickerAdvTo.UseCurrentCulture = true;
            this.dateTimePickerAdvTo.Value = new System.DateTime(2011, 6, 15, 13, 21, 32, 852);
            this.dateTimePickerAdvTo.ValueChanged += new System.EventHandler(this.dateTimePickerAdvTo_ValueChanged);
            // 
            // dateTimePickerAdvFrom
            // 
            this.dateTimePickerAdvFrom.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.dateTimePickerAdvFrom.BorderColor = System.Drawing.Color.Empty;
            this.dateTimePickerAdvFrom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.dateTimePickerAdvFrom.Calendar.AllowMultipleSelection = false;
            this.dateTimePickerAdvFrom.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerAdvFrom.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdvFrom.Calendar.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dateTimePickerAdvFrom.Calendar.Dock")));
            this.dateTimePickerAdvFrom.Calendar.Font = ((System.Drawing.Font)(resources.GetObject("dateTimePickerAdvFrom.Calendar.Font")));
            this.dateTimePickerAdvFrom.Calendar.ForeColor = System.Drawing.Color.Black;
            this.dateTimePickerAdvFrom.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.dateTimePickerAdvFrom.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.dateTimePickerAdvFrom.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.dateTimePickerAdvFrom.Calendar.HeaderHeight = 20;
            this.dateTimePickerAdvFrom.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdvFrom.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvFrom.Calendar.HeadGradient = true;
            this.dateTimePickerAdvFrom.Calendar.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvFrom.Calendar.Location")));
            this.dateTimePickerAdvFrom.Calendar.Name = "monthCalendar";
            this.dateTimePickerAdvFrom.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.dateTimePickerAdvFrom.Calendar.SelectedDates = new System.DateTime[0];
            this.dateTimePickerAdvFrom.Calendar.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvFrom.Calendar.Size")));
            this.dateTimePickerAdvFrom.Calendar.SizeToFit = true;
            this.dateTimePickerAdvFrom.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdvFrom.Calendar.TabIndex = ((int)(resources.GetObject("dateTimePickerAdvFrom.Calendar.TabIndex")));
            this.dateTimePickerAdvFrom.Calendar.ThemedEnabledGrid = true;
            this.dateTimePickerAdvFrom.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdvFrom.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.dateTimePickerAdvFrom.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdvFrom.Calendar.NoneButton.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvFrom.Calendar.NoneButton.Location")));
            this.dateTimePickerAdvFrom.Calendar.NoneButton.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvFrom.Calendar.NoneButton.Size")));
            this.dateTimePickerAdvFrom.Calendar.NoneButton.Text = resources.GetString("dateTimePickerAdvFrom.Calendar.NoneButton.Text");
            this.dateTimePickerAdvFrom.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.dateTimePickerAdvFrom.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdvFrom.Calendar.TodayButton.Location = ((System.Drawing.Point)(resources.GetObject("dateTimePickerAdvFrom.Calendar.TodayButton.Location")));
            this.dateTimePickerAdvFrom.Calendar.TodayButton.Size = ((System.Drawing.Size)(resources.GetObject("dateTimePickerAdvFrom.Calendar.TodayButton.Size")));
            this.dateTimePickerAdvFrom.Calendar.TodayButton.Text = resources.GetString("dateTimePickerAdvFrom.Calendar.TodayButton.Text");
            this.dateTimePickerAdvFrom.Calendar.TodayButton.UseVisualStyle = true;
            this.dateTimePickerAdvFrom.CalendarForeColor = System.Drawing.Color.Black;
            this.dateTimePickerAdvFrom.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdvFrom.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvFrom.ClipboardFormat = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerAdvFrom.Culture = new System.Globalization.CultureInfo("sv-SE");
            resources.ApplyResources(this.dateTimePickerAdvFrom, "dateTimePickerAdvFrom");
            this.dateTimePickerAdvFrom.DropDownImage = null;
            this.dateTimePickerAdvFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerAdvFrom.MinValue = new System.DateTime(((long)(0)));
            this.dateTimePickerAdvFrom.Name = "dateTimePickerAdvFrom";
            this.dateTimePickerAdvFrom.ShowCheckBox = false;
            this.dateTimePickerAdvFrom.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdvFrom.ThemedChildControls = true;
            this.dateTimePickerAdvFrom.ThemesEnabled = true;
            this.dateTimePickerAdvFrom.UseCurrentCulture = true;
            this.dateTimePickerAdvFrom.Value = new System.DateTime(2011, 6, 15, 13, 21, 32, 852);
            this.dateTimePickerAdvFrom.ValueChanged += new System.EventHandler(this.dateTimePickerAdvFrom_ValueChanged);
            // 
            // textBoxExtFind
            // 
            resources.ApplyResources(this.textBoxExtFind, "textBoxExtFind");
            this.textBoxExtFind.Name = "textBoxExtFind";
            this.textBoxExtFind.TextChanged += new System.EventHandler(this.textBoxExtFind_TextChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // treeViewAdvResult
            // 
            treeNodeAdvStyleInfo1.EnsureDefaultOptionedChild = true;
            this.treeViewAdvResult.BaseStylePairs.AddRange(new Syncfusion.Windows.Forms.Tools.StyleNamePair[] {
            new Syncfusion.Windows.Forms.Tools.StyleNamePair("Standard", treeNodeAdvStyleInfo1)});
            this.tableLayoutPanel1.SetColumnSpan(this.treeViewAdvResult, 2);
            this.treeViewAdvResult.ContextMenuStrip = this.contextMenuStripTreeActions;
            resources.ApplyResources(this.treeViewAdvResult, "treeViewAdvResult");
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
            this.treeViewAdvResult.Name = "treeViewAdvResult";
            this.treeViewAdvResult.Office2007ScrollBars = true;
            this.treeViewAdvResult.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectSameLevel;
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
            this.treeViewAdvResult.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewAdvPreviewTree_ItemDrag);
            this.treeViewAdvResult.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeViewAdvResult_KeyDown);
            // 
            // contextMenuStripTreeActions
            // 
            this.contextMenuStripTreeActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCut});
            this.contextMenuStripTreeActions.Name = "contextMenuStripTreeActions";
            this.contextMenuStripTreeActions.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            resources.ApplyResources(this.contextMenuStripTreeActions, "contextMenuStripTreeActions");
            this.contextMenuStripTreeActions.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripTreeActions_Opening);
            // 
            // toolStripMenuItemCut
            // 
            this.toolStripMenuItemCut.Name = "toolStripMenuItemCut";
            resources.ApplyResources(this.toolStripMenuItemCut, "toolStripMenuItemCut");
            this.toolStripMenuItemCut.Click += new System.EventHandler(this.toolStripMenuItemCut_Click);
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
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvTo)).EndInit();
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
