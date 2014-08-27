﻿namespace Teleopti.Ccc.Win.Meetings
{
	partial class AddressBookView
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
				if (_gridHelper != null) _gridHelper.Dispose();
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
			this.tableLayoutPanelForm = new System.Windows.Forms.TableLayoutPanel();
			this.gridControlPeople = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanelSelection = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvOptional = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvRequired = new Syncfusion.Windows.Forms.ButtonAdv();
			this.textBoxExtOptionalParticipant = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.contextMenuStripEx1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.textBoxExtRequiredParticipant = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.tableLayoutPanelFilter = new System.Windows.Forms.TableLayoutPanel();
			this.dateTimePickerAdvtDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.buttonAdvGo = new Syncfusion.Windows.Forms.ButtonAdv();
			this.autoLabelFilterPeople = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.textBoxExtFilterCriteria = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.tableLayoutPanelConfirmButtons = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.statusLabelMessage = new System.Windows.Forms.Label();
			this.tableLayoutPanelForm.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlPeople)).BeginInit();
			this.tableLayoutPanelSelection.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtOptionalParticipant)).BeginInit();
			this.contextMenuStripEx1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtRequiredParticipant)).BeginInit();
			this.tableLayoutPanelFilter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvtDate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvtDate.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtFilterCriteria)).BeginInit();
			this.tableLayoutPanelConfirmButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanelForm
			// 
			this.tableLayoutPanelForm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanelForm.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelForm.ColumnCount = 1;
			this.tableLayoutPanelForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelForm.Controls.Add(this.gridControlPeople, 0, 1);
			this.tableLayoutPanelForm.Controls.Add(this.tableLayoutPanelSelection, 0, 2);
			this.tableLayoutPanelForm.Controls.Add(this.tableLayoutPanelFilter, 0, 0);
			this.tableLayoutPanelForm.Controls.Add(this.tableLayoutPanelConfirmButtons, 0, 3);
			this.tableLayoutPanelForm.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelForm.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelForm.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelForm.Name = "tableLayoutPanelForm";
			this.tableLayoutPanelForm.Padding = new System.Windows.Forms.Padding(9, 5, 9, 5);
			this.tableLayoutPanelForm.RowCount = 4;
			this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelForm.Size = new System.Drawing.Size(669, 533);
			this.tableLayoutPanelForm.TabIndex = 6;
			// 
			// gridControlPeople
			// 
			this.gridControlPeople.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.None;
			this.gridControlPeople.AllowSelection = ((Syncfusion.Windows.Forms.Grid.GridSelectionFlags)((((((Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Row | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Table) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Multiple) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Shift) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Keyboard) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.MixRangeType)));
			this.gridControlPeople.BackColor = System.Drawing.Color.White;
			this.gridControlPeople.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.gridControlPeople.ColCount = 1;
			this.gridControlPeople.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 0),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(1, 250),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(2, 225),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(3, 150),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(4, 150)});
			this.gridControlPeople.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridControlPeople.DefaultRowHeight = 20;
			this.gridControlPeople.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlPeople.ExcelLikeCurrentCell = true;
			this.gridControlPeople.ExcelLikeSelectionFrame = true;
			this.gridControlPeople.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.gridControlPeople.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlPeople.Location = new System.Drawing.Point(12, 46);
			this.gridControlPeople.MetroScrollBars = true;
			this.gridControlPeople.Name = "gridControlPeople";
			this.gridControlPeople.Properties.DisplayVertLines = false;
			this.gridControlPeople.Properties.ForceImmediateRepaint = false;
			this.gridControlPeople.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridControlPeople.Properties.MarkColHeader = false;
			this.gridControlPeople.Properties.MarkRowHeader = false;
			this.gridControlPeople.ReadOnly = true;
			this.gridControlPeople.RowCount = 1;
			this.gridControlPeople.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridControlPeople.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlPeople.Size = new System.Drawing.Size(645, 370);
			this.gridControlPeople.SmartSizeBox = false;
			this.gridControlPeople.TabIndex = 10;
			this.gridControlPeople.ThemesEnabled = true;
			this.gridControlPeople.UseRightToLeftCompatibleTextBox = true;
			this.gridControlPeople.CellClick += new Syncfusion.Windows.Forms.Grid.GridCellClickEventHandler(this.gridControlPeopleCellClick);
			this.gridControlPeople.CellDoubleClick += new Syncfusion.Windows.Forms.Grid.GridCellClickEventHandler(this.gridControlPeople_CellDoubleClick);
			// 
			// tableLayoutPanelSelection
			// 
			this.tableLayoutPanelSelection.AutoSize = true;
			this.tableLayoutPanelSelection.ColumnCount = 2;
			this.tableLayoutPanelSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSelection.Controls.Add(this.buttonAdvOptional, 0, 1);
			this.tableLayoutPanelSelection.Controls.Add(this.buttonAdvRequired, 0, 0);
			this.tableLayoutPanelSelection.Controls.Add(this.textBoxExtOptionalParticipant, 0, 1);
			this.tableLayoutPanelSelection.Controls.Add(this.textBoxExtRequiredParticipant, 1, 0);
			this.tableLayoutPanelSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSelection.Location = new System.Drawing.Point(9, 419);
			this.tableLayoutPanelSelection.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelSelection.Name = "tableLayoutPanelSelection";
			this.tableLayoutPanelSelection.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.tableLayoutPanelSelection.RowCount = 2;
			this.tableLayoutPanelSelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelSelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelSelection.Size = new System.Drawing.Size(651, 71);
			this.tableLayoutPanelSelection.TabIndex = 1;
			// 
			// buttonAdvOptional
			// 
			this.buttonAdvOptional.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOptional.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOptional.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOptional.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOptional.IsBackStageButton = false;
			this.buttonAdvOptional.Location = new System.Drawing.Point(3, 41);
			this.buttonAdvOptional.Name = "buttonAdvOptional";
			this.buttonAdvOptional.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOptional.TabIndex = 1;
			this.buttonAdvOptional.Text = "xxOptional";
			this.buttonAdvOptional.UseVisualStyle = true;
			this.buttonAdvOptional.Click += new System.EventHandler(this.buttonAdvOptionalClick);
			// 
			// buttonAdvRequired
			// 
			this.buttonAdvRequired.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvRequired.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvRequired.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvRequired.ForeColor = System.Drawing.Color.White;
			this.buttonAdvRequired.IsBackStageButton = false;
			this.buttonAdvRequired.Location = new System.Drawing.Point(3, 8);
			this.buttonAdvRequired.Name = "buttonAdvRequired";
			this.buttonAdvRequired.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvRequired.TabIndex = 0;
			this.buttonAdvRequired.Text = "xxRequired";
			this.buttonAdvRequired.UseVisualStyle = true;
			this.buttonAdvRequired.Click += new System.EventHandler(this.buttonAdvRequiredClick);
			// 
			// textBoxExtOptionalParticipant
			// 
			this.textBoxExtOptionalParticipant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxExtOptionalParticipant.BackColor = System.Drawing.Color.White;
			this.textBoxExtOptionalParticipant.BeforeTouchSize = new System.Drawing.Size(886, 23);
			this.textBoxExtOptionalParticipant.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtOptionalParticipant.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExtOptionalParticipant.ContextMenuStrip = this.contextMenuStripEx1;
			this.textBoxExtOptionalParticipant.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExtOptionalParticipant.Location = new System.Drawing.Point(96, 41);
			this.textBoxExtOptionalParticipant.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtOptionalParticipant.Multiline = true;
			this.textBoxExtOptionalParticipant.Name = "textBoxExtOptionalParticipant";
			this.textBoxExtOptionalParticipant.OverflowIndicatorToolTipText = null;
			this.textBoxExtOptionalParticipant.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxExtOptionalParticipant.Size = new System.Drawing.Size(552, 24);
			this.textBoxExtOptionalParticipant.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExtOptionalParticipant.TabIndex = 3;
			this.textBoxExtOptionalParticipant.TextChanged += new System.EventHandler(this.textBoxExtOptionalParticipant_TextChanged);
			this.textBoxExtOptionalParticipant.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxExtOptionalParticipant_KeyDown);
			this.textBoxExtOptionalParticipant.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textBoxExtOptionalParticipant_MouseUp);
			this.textBoxExtOptionalParticipant.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBoxExtOptionalParticipant_MouseUp);
			// 
			// contextMenuStripEx1
			// 
			this.contextMenuStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteToolStripMenuItem});
			this.contextMenuStripEx1.Name = "contextMenuStripEx1";
			this.contextMenuStripEx1.Size = new System.Drawing.Size(108, 98);
			this.contextMenuStripEx1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripEx1_Opening);
			// 
			// cutToolStripMenuItem
			// 
			this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			this.cutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.cutToolStripMenuItem.Text = "Cut";
			this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.copyToolStripMenuItem.Text = "Copy";
			this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
			// 
			// pasteToolStripMenuItem
			// 
			this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			this.pasteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.pasteToolStripMenuItem.Text = "Paste";
			this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(104, 6);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.deleteToolStripMenuItem.Text = "Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// textBoxExtRequiredParticipant
			// 
			this.textBoxExtRequiredParticipant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxExtRequiredParticipant.BackColor = System.Drawing.Color.White;
			this.textBoxExtRequiredParticipant.BeforeTouchSize = new System.Drawing.Size(886, 23);
			this.textBoxExtRequiredParticipant.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtRequiredParticipant.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExtRequiredParticipant.ContextMenuStrip = this.contextMenuStripEx1;
			this.textBoxExtRequiredParticipant.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExtRequiredParticipant.Location = new System.Drawing.Point(96, 8);
			this.textBoxExtRequiredParticipant.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtRequiredParticipant.Multiline = true;
			this.textBoxExtRequiredParticipant.Name = "textBoxExtRequiredParticipant";
			this.textBoxExtRequiredParticipant.OverflowIndicatorToolTipText = null;
			this.textBoxExtRequiredParticipant.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxExtRequiredParticipant.Size = new System.Drawing.Size(552, 24);
			this.textBoxExtRequiredParticipant.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExtRequiredParticipant.TabIndex = 2;
			this.textBoxExtRequiredParticipant.TextChanged += new System.EventHandler(this.textBoxExtRequiredParticipant_TextChanged);
			this.textBoxExtRequiredParticipant.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxExtRequiredParticipant_KeyDown);
			this.textBoxExtRequiredParticipant.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textBoxExtRequiredParticipant_MouseUp);
			this.textBoxExtRequiredParticipant.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBoxExtRequiredParticipant_MouseUp);
			// 
			// tableLayoutPanelFilter
			// 
			this.tableLayoutPanelFilter.AutoSize = true;
			this.tableLayoutPanelFilter.ColumnCount = 4;
			this.tableLayoutPanelFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelFilter.Controls.Add(this.dateTimePickerAdvtDate, 3, 0);
			this.tableLayoutPanelFilter.Controls.Add(this.buttonAdvGo, 1, 0);
			this.tableLayoutPanelFilter.Controls.Add(this.autoLabelFilterPeople, 0, 0);
			this.tableLayoutPanelFilter.Controls.Add(this.textBoxExtFilterCriteria, 1, 0);
			this.tableLayoutPanelFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelFilter.Location = new System.Drawing.Point(9, 5);
			this.tableLayoutPanelFilter.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelFilter.Name = "tableLayoutPanelFilter";
			this.tableLayoutPanelFilter.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
			this.tableLayoutPanelFilter.RowCount = 1;
			this.tableLayoutPanelFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelFilter.Size = new System.Drawing.Size(651, 38);
			this.tableLayoutPanelFilter.TabIndex = 6;
			// 
			// dateTimePickerAdvtDate
			// 
			this.dateTimePickerAdvtDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvtDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvtDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvtDate.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvtDate.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvtDate.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvtDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvtDate.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvtDate.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvtDate.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvtDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvtDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvtDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvtDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvtDate.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvtDate.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvtDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvtDate.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvtDate.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvtDate.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvtDate.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvtDate.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvtDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvtDate.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvtDate.Calendar.Size = new System.Drawing.Size(119, 174);
			this.dateTimePickerAdvtDate.Calendar.SizeToFit = true;
			this.dateTimePickerAdvtDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvtDate.Calendar.TabIndex = 0;
			this.dateTimePickerAdvtDate.Calendar.ThemedEnabledScrollButtons = false;
			this.dateTimePickerAdvtDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvtDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvtDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvtDate.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvtDate.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvtDate.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvtDate.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvtDate.Calendar.NoneButton.Location = new System.Drawing.Point(125, 0);
			this.dateTimePickerAdvtDate.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvtDate.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvtDate.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvtDate.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvtDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvtDate.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvtDate.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvtDate.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvtDate.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvtDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvtDate.Calendar.TodayButton.Size = new System.Drawing.Size(119, 25);
			this.dateTimePickerAdvtDate.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvtDate.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvtDate.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvtDate.CalendarForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvtDate.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvtDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvtDate.ClipboardFormat = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvtDate.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvtDate.DropDownImage = null;
			this.dateTimePickerAdvtDate.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvtDate.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvtDate.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvtDate.EnableNullDate = false;
			this.dateTimePickerAdvtDate.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvtDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvtDate.Location = new System.Drawing.Point(527, 3);
			this.dateTimePickerAdvtDate.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvtDate.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvtDate.Name = "dateTimePickerAdvtDate";
			this.dateTimePickerAdvtDate.NoneButtonVisible = false;
			this.dateTimePickerAdvtDate.ShowCheckBox = false;
			this.dateTimePickerAdvtDate.Size = new System.Drawing.Size(121, 21);
			this.dateTimePickerAdvtDate.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvtDate.TabIndex = 5;
			this.dateTimePickerAdvtDate.ThemesEnabled = true;
			this.dateTimePickerAdvtDate.Value = new System.DateTime(2008, 8, 5, 8, 1, 24, 984);
			// 
			// buttonAdvGo
			// 
			this.buttonAdvGo.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvGo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvGo.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvGo.ForeColor = System.Drawing.Color.White;
			this.buttonAdvGo.IsBackStageButton = false;
			this.buttonAdvGo.Location = new System.Drawing.Point(378, 3);
			this.buttonAdvGo.Name = "buttonAdvGo";
			this.buttonAdvGo.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvGo.TabIndex = 4;
			this.buttonAdvGo.Text = "xxGo";
			this.buttonAdvGo.UseVisualStyle = true;
			this.buttonAdvGo.Click += new System.EventHandler(this.buttonAdvGoClick);
			// 
			// autoLabelFilterPeople
			// 
			this.autoLabelFilterPeople.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabelFilterPeople.Location = new System.Drawing.Point(3, 9);
			this.autoLabelFilterPeople.Name = "autoLabelFilterPeople";
			this.autoLabelFilterPeople.Size = new System.Drawing.Size(111, 15);
			this.autoLabelFilterPeople.TabIndex = 2;
			this.autoLabelFilterPeople.Text = "xxFilterPeopleColon";
			this.autoLabelFilterPeople.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textBoxExtFilterCriteria
			// 
			this.textBoxExtFilterCriteria.BackColor = System.Drawing.Color.White;
			this.textBoxExtFilterCriteria.BeforeTouchSize = new System.Drawing.Size(886, 23);
			this.textBoxExtFilterCriteria.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtFilterCriteria.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExtFilterCriteria.ContextMenuStrip = this.contextMenuStripEx1;
			this.textBoxExtFilterCriteria.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExtFilterCriteria.Location = new System.Drawing.Point(120, 3);
			this.textBoxExtFilterCriteria.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtFilterCriteria.Name = "textBoxExtFilterCriteria";
			this.textBoxExtFilterCriteria.OverflowIndicatorToolTipText = null;
			this.textBoxExtFilterCriteria.Size = new System.Drawing.Size(252, 23);
			this.textBoxExtFilterCriteria.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExtFilterCriteria.TabIndex = 3;
			this.textBoxExtFilterCriteria.Enter += new System.EventHandler(this.textBoxExtFilterCriteriaEnter);
			this.textBoxExtFilterCriteria.Leave += new System.EventHandler(this.textBoxExtFilterCriteriaLeave);
			this.textBoxExtFilterCriteria.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBoxExtFilterCriteria_MouseUp);
			// 
			// tableLayoutPanelConfirmButtons
			// 
			this.tableLayoutPanelConfirmButtons.AutoSize = true;
			this.tableLayoutPanelConfirmButtons.ColumnCount = 3;
			this.tableLayoutPanelConfirmButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelConfirmButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelConfirmButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelConfirmButtons.Controls.Add(this.buttonAdvCancel, 2, 0);
			this.tableLayoutPanelConfirmButtons.Controls.Add(this.buttonAdvOK, 1, 0);
			this.tableLayoutPanelConfirmButtons.Controls.Add(this.statusLabelMessage, 0, 0);
			this.tableLayoutPanelConfirmButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelConfirmButtons.Location = new System.Drawing.Point(9, 490);
			this.tableLayoutPanelConfirmButtons.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelConfirmButtons.Name = "tableLayoutPanelConfirmButtons";
			this.tableLayoutPanelConfirmButtons.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.tableLayoutPanelConfirmButtons.RowCount = 1;
			this.tableLayoutPanelConfirmButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelConfirmButtons.Size = new System.Drawing.Size(651, 38);
			this.tableLayoutPanelConfirmButtons.TabIndex = 2;
			this.tableLayoutPanelConfirmButtons.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanelConfirmButtons_Paint);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(561, 8);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 1;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			// 
			// buttonAdvOK
			// 
			this.buttonAdvOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOK.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvOK.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOK.IsBackStageButton = false;
			this.buttonAdvOK.Location = new System.Drawing.Point(441, 8);
			this.buttonAdvOK.Name = "buttonAdvOK";
			this.buttonAdvOK.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOK.TabIndex = 0;
			this.buttonAdvOK.Text = "xxOK";
			this.buttonAdvOK.UseVisualStyle = true;
			this.buttonAdvOK.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// statusLabelMessage
			// 
			this.statusLabelMessage.AutoSize = true;
			this.statusLabelMessage.Location = new System.Drawing.Point(3, 10);
			this.statusLabelMessage.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
			this.statusLabelMessage.Name = "statusLabelMessage";
			this.statusLabelMessage.Size = new System.Drawing.Size(0, 15);
			this.statusLabelMessage.TabIndex = 2;
			// 
			// AddressBookView
			// 
			this.AcceptButton = this.buttonAdvOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(669, 533);
			this.Controls.Add(this.tableLayoutPanelForm);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(581, 456);
			this.Name = "AddressBookView";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxSelectAttendees";
			this.tableLayoutPanelForm.ResumeLayout(false);
			this.tableLayoutPanelForm.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlPeople)).EndInit();
			this.tableLayoutPanelSelection.ResumeLayout(false);
			this.tableLayoutPanelSelection.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtOptionalParticipant)).EndInit();
			this.contextMenuStripEx1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtRequiredParticipant)).EndInit();
			this.tableLayoutPanelFilter.ResumeLayout(false);
			this.tableLayoutPanelFilter.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvtDate.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvtDate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtFilterCriteria)).EndInit();
			this.tableLayoutPanelConfirmButtons.ResumeLayout(false);
			this.tableLayoutPanelConfirmButtons.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForm;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelConfirmButtons;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFilter;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelFilterPeople;
		private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtFilterCriteria;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSelection;
		private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtRequiredParticipant;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOptional;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRequired;
		private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtOptionalParticipant;
		private Syncfusion.Windows.Forms.Grid.GridControl gridControlPeople;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvGo;
		private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvtDate;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripEx1;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.Label statusLabelMessage;
	}
}

