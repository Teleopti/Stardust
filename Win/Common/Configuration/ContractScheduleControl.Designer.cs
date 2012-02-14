using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class ContractScheduleControl
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
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            this.gridControlContractSchedule = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.textBoxExtDescription = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.autoLabel2 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.comboBoxAdvScheduleCollection = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelSubHeader2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvDeleteWeek = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvAddWeek = new System.Windows.Forms.Button();
            this.labelSubHeader2 = new System.Windows.Forms.Label();
            this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvDeleteContractSchedule = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAddNewContractSchedule = new System.Windows.Forms.Button();
            this.labelSubHeader1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.autoLabel5 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.autoLabelChangeInfo = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.autoLabelInfoAboutChanges = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
            this.labelHeader = new System.Windows.Forms.Label();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdv1 = new Syncfusion.Windows.Forms.ButtonAdv();
            this.button1 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlContractSchedule)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtDescription)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvScheduleCollection)).BeginInit();
            this.tableLayoutPanelBody.SuspendLayout();
            this.tableLayoutPanelSubHeader2.SuspendLayout();
            this.tableLayoutPanelSubHeader1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
            this.gradientPanelHeader.SuspendLayout();
            this.tableLayoutPanelHeader.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridControlContractSchedule
            // 
            gridBaseStyle1.Name = "Header";
            gridBaseStyle1.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.CellType = "Header";
            gridBaseStyle1.StyleInfo.CellValueType = typeof(string);
            gridBaseStyle1.StyleInfo.Font.Bold = true;
            gridBaseStyle1.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
            gridBaseStyle1.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
            gridBaseStyle2.Name = "Standard";
            gridBaseStyle2.StyleInfo.CellType = "CheckBox";
            gridBaseStyle2.StyleInfo.CellValueType = typeof(bool);
            gridBaseStyle2.StyleInfo.CheckBoxOptions.CheckedValue = "True";
            gridBaseStyle2.StyleInfo.CheckBoxOptions.UncheckedValue = "False";
            gridBaseStyle2.StyleInfo.DataSource = null;
            gridBaseStyle2.StyleInfo.Font.Facename = "Tahoma";
            gridBaseStyle2.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
            gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
            gridBaseStyle3.Name = "Row Header";
            gridBaseStyle3.StyleInfo.BaseStyle = "Header";
            gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
            gridBaseStyle3.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
            gridBaseStyle4.Name = "Column Header";
            gridBaseStyle4.StyleInfo.BaseStyle = "Header";
            gridBaseStyle4.StyleInfo.CellValueType = typeof(string);
            gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
            this.gridControlContractSchedule.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
            this.gridControlContractSchedule.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridControlContractSchedule.ColCount = 7;
            this.gridControlContractSchedule.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
            this.gridControlContractSchedule.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
            this.gridControlContractSchedule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlContractSchedule.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gridControlContractSchedule.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridControlContractSchedule.Location = new System.Drawing.Point(2, 3);
            this.gridControlContractSchedule.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gridControlContractSchedule.Name = "gridControlContractSchedule";
            this.gridControlContractSchedule.NumberedRowHeaders = false;
            this.gridControlContractSchedule.Office2007ScrollBars = true;
            this.gridControlContractSchedule.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridControlContractSchedule.RowCount = 1;
            this.gridControlContractSchedule.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
            this.gridControlContractSchedule.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlContractSchedule.Size = new System.Drawing.Size(836, 238);
            this.gridControlContractSchedule.SmartSizeBox = false;
            this.gridControlContractSchedule.TabIndex = 37;
            this.gridControlContractSchedule.ThemesEnabled = true;
            this.gridControlContractSchedule.UseRightToLeftCompatibleTextBox = true;
            this.gridControlContractSchedule.CurrentCellChanged += new System.EventHandler(this.ContractScheduleGridSelectionChanged);
            this.gridControlContractSchedule.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ContractScheduleGridKeyUp);
            // 
            // textBoxExtDescription
            // 
            this.textBoxExtDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxExtDescription.Location = new System.Drawing.Point(178, 30);
            this.textBoxExtDescription.MaxLength = 50;
            this.textBoxExtDescription.Name = "textBoxExtDescription";
            this.textBoxExtDescription.OverflowIndicatorToolTipText = null;
            this.textBoxExtDescription.Size = new System.Drawing.Size(216, 21);
            this.textBoxExtDescription.TabIndex = 35;
            this.textBoxExtDescription.WordWrap = false;
            this.textBoxExtDescription.TextChanged += new System.EventHandler(this.TextBoxExtDescriptionTextChanged);
            this.textBoxExtDescription.Leave += new System.EventHandler(this.TextBoxExtDescriptionLeave);
            this.textBoxExtDescription.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxExtDescriptionValidating);
            this.textBoxExtDescription.Validated += new System.EventHandler(this.TextBoxExtDescriptionValidated);
            // 
            // autoLabel2
            // 
            this.autoLabel2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabel2.AutoSize = false;
            this.autoLabel2.Location = new System.Drawing.Point(3, 30);
            this.autoLabel2.Name = "autoLabel2";
            this.autoLabel2.Size = new System.Drawing.Size(119, 21);
            this.autoLabel2.TabIndex = 34;
            this.autoLabel2.Text = "xxNameColon";
            this.autoLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxAdvScheduleCollection
            // 
            this.comboBoxAdvScheduleCollection.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBoxAdvScheduleCollection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdvScheduleCollection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvScheduleCollection.Location = new System.Drawing.Point(178, 3);
            this.comboBoxAdvScheduleCollection.Name = "comboBoxAdvScheduleCollection";
            this.comboBoxAdvScheduleCollection.Size = new System.Drawing.Size(216, 21);
            this.comboBoxAdvScheduleCollection.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvScheduleCollection.TabIndex = 33;
            this.comboBoxAdvScheduleCollection.SelectedIndexChanged += new System.EventHandler(this.ComboBoxAdvScheduleCollectionSelectedIndexChanged);
            // 
            // tableLayoutPanelBody
            // 
            this.tableLayoutPanelBody.ColumnCount = 1;
            this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader2, 0, 2);
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanel5, 0, 3);
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 55);
            this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
            this.tableLayoutPanelBody.RowCount = 4;
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBody.Size = new System.Drawing.Size(846, 405);
            this.tableLayoutPanelBody.TabIndex = 41;
            // 
            // tableLayoutPanelSubHeader2
            // 
            this.tableLayoutPanelSubHeader2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanelSubHeader2.ColumnCount = 3;
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanelSubHeader2.Controls.Add(this.buttonAdvDeleteWeek, 0, 0);
            this.tableLayoutPanelSubHeader2.Controls.Add(this.buttonAdvAddWeek, 0, 0);
            this.tableLayoutPanelSubHeader2.Controls.Add(this.labelSubHeader2, 0, 0);
            this.tableLayoutPanelSubHeader2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSubHeader2.Location = new System.Drawing.Point(3, 128);
            this.tableLayoutPanelSubHeader2.Name = "tableLayoutPanelSubHeader2";
            this.tableLayoutPanelSubHeader2.RowCount = 1;
            this.tableLayoutPanelSubHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSubHeader2.Size = new System.Drawing.Size(840, 27);
            this.tableLayoutPanelSubHeader2.TabIndex = 3;
            // 
            // buttonAdvDeleteWeek
            // 
            this.buttonAdvDeleteWeek.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvDeleteWeek.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_delete_32x32;
            this.buttonAdvDeleteWeek.Location = new System.Drawing.Point(814, 1);
            this.buttonAdvDeleteWeek.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
            this.buttonAdvDeleteWeek.Name = "buttonAdvDeleteWeek";
            this.buttonAdvDeleteWeek.Size = new System.Drawing.Size(26, 24);
            this.buttonAdvDeleteWeek.TabIndex = 8;
            this.buttonAdvDeleteWeek.TabStop = false;
            this.buttonAdvDeleteWeek.Click += new System.EventHandler(this.ButtonAdvDeleteWeekClick);
            // 
            // buttonAdvAddWeek
            // 
            this.buttonAdvAddWeek.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvAddWeek.BackColor = System.Drawing.Color.White;
            this.buttonAdvAddWeek.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.buttonAdvAddWeek.Location = new System.Drawing.Point(787, 1);
            this.buttonAdvAddWeek.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
            this.buttonAdvAddWeek.Name = "buttonAdvAddWeek";
            this.buttonAdvAddWeek.Size = new System.Drawing.Size(24, 24);
            this.buttonAdvAddWeek.TabIndex = 7;
            this.buttonAdvAddWeek.UseVisualStyleBackColor = false;
            this.buttonAdvAddWeek.Click += new System.EventHandler(this.ButtonAdvAddNewContractScheduleWeekClick);
            // 
            // labelSubHeader2
            // 
            this.labelSubHeader2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSubHeader2.AutoSize = true;
            this.labelSubHeader2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelSubHeader2.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelSubHeader2.Location = new System.Drawing.Point(3, 7);
            this.labelSubHeader2.Name = "labelSubHeader2";
            this.labelSubHeader2.Size = new System.Drawing.Size(271, 13);
            this.labelSubHeader2.TabIndex = 0;
            this.labelSubHeader2.Text = "xxSelectWorkingdaysForThisContractSchedule";
            this.labelSubHeader2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tableLayoutPanelSubHeader1
            // 
            this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanelSubHeader1.ColumnCount = 3;
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonAdvDeleteContractSchedule, 2, 0);
            this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonAddNewContractSchedule, 1, 0);
            this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
            this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
            this.tableLayoutPanelSubHeader1.RowCount = 1;
            this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(840, 27);
            this.tableLayoutPanelSubHeader1.TabIndex = 1;
            // 
            // buttonAdvDeleteContractSchedule
            // 
            this.buttonAdvDeleteContractSchedule.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvDeleteContractSchedule.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_delete_32x32;
            this.buttonAdvDeleteContractSchedule.Location = new System.Drawing.Point(816, 1);
            this.buttonAdvDeleteContractSchedule.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
            this.buttonAdvDeleteContractSchedule.Name = "buttonAdvDeleteContractSchedule";
            this.buttonAdvDeleteContractSchedule.Size = new System.Drawing.Size(24, 24);
            this.buttonAdvDeleteContractSchedule.TabIndex = 7;
            this.buttonAdvDeleteContractSchedule.TabStop = false;
            this.buttonAdvDeleteContractSchedule.Click += new System.EventHandler(this.ButtonAdvDeleteContractScheduleClick);
            // 
            // buttonAddNewContractSchedule
            // 
            this.buttonAddNewContractSchedule.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddNewContractSchedule.BackColor = System.Drawing.Color.White;
            this.buttonAddNewContractSchedule.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.buttonAddNewContractSchedule.Location = new System.Drawing.Point(789, 1);
            this.buttonAddNewContractSchedule.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
            this.buttonAddNewContractSchedule.Name = "buttonAddNewContractSchedule";
            this.buttonAddNewContractSchedule.Size = new System.Drawing.Size(24, 24);
            this.buttonAddNewContractSchedule.TabIndex = 6;
            this.buttonAddNewContractSchedule.UseVisualStyleBackColor = false;
            this.buttonAddNewContractSchedule.Click += new System.EventHandler(this.ButtonNewContractScheduleClick);
            // 
            // labelSubHeader1
            // 
            this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSubHeader1.AutoSize = true;
            this.labelSubHeader1.BackColor = System.Drawing.Color.Transparent;
            this.labelSubHeader1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelSubHeader1.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelSubHeader1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSubHeader1.Location = new System.Drawing.Point(3, 7);
            this.labelSubHeader1.Name = "labelSubHeader1";
            this.labelSubHeader1.Size = new System.Drawing.Size(218, 13);
            this.labelSubHeader1.TabIndex = 0;
            this.labelSubHeader1.Text = "xxChooseContractScheduleToChange";
            this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.gridControlContractSchedule, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 161);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 244F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(840, 241);
            this.tableLayoutPanel5.TabIndex = 4;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.Controls.Add(this.autoLabel5, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.comboBoxAdvScheduleCollection, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.autoLabel2, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.autoLabelChangeInfo, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.textBoxExtDescription, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.autoLabelInfoAboutChanges, 1, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 36);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(840, 86);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // autoLabel5
            // 
            this.autoLabel5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabel5.AutoSize = false;
            this.autoLabel5.Location = new System.Drawing.Point(3, 3);
            this.autoLabel5.Name = "autoLabel5";
            this.autoLabel5.Size = new System.Drawing.Size(112, 21);
            this.autoLabel5.TabIndex = 36;
            this.autoLabel5.Text = "xxScheduleColon";
            this.autoLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // autoLabelChangeInfo
            // 
            this.autoLabelChangeInfo.AutoSize = false;
            this.autoLabelChangeInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.autoLabelChangeInfo.Location = new System.Drawing.Point(3, 54);
            this.autoLabelChangeInfo.Name = "autoLabelChangeInfo";
            this.autoLabelChangeInfo.Size = new System.Drawing.Size(169, 32);
            this.autoLabelChangeInfo.TabIndex = 37;
            this.autoLabelChangeInfo.Text = "xxChangeInfoColon";
            this.autoLabelChangeInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // autoLabelInfoAboutChanges
            // 
            this.autoLabelInfoAboutChanges.AutoSize = false;
            this.autoLabelInfoAboutChanges.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.autoLabelInfoAboutChanges.Location = new System.Drawing.Point(175, 54);
            this.autoLabelInfoAboutChanges.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.autoLabelInfoAboutChanges.Name = "autoLabelInfoAboutChanges";
            this.autoLabelInfoAboutChanges.Size = new System.Drawing.Size(635, 32);
            this.autoLabelInfoAboutChanges.TabIndex = 38;
            this.autoLabelInfoAboutChanges.Text = "xxInfoAboutChanges";
            this.autoLabelInfoAboutChanges.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gradientPanelHeader
            // 
            this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gradientPanelHeader.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.gradientPanelHeader.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.White);
            this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
            this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
            this.gradientPanelHeader.Name = "gradientPanelHeader";
            this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(10);
            this.gradientPanelHeader.Size = new System.Drawing.Size(846, 55);
            this.gradientPanelHeader.TabIndex = 55;
            // 
            // tableLayoutPanelHeader
            // 
            this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelHeader.ColumnCount = 1;
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 826F));
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
            this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelHeader.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
            this.tableLayoutPanelHeader.RowCount = 1;
            this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHeader.Size = new System.Drawing.Size(826, 35);
            this.tableLayoutPanelHeader.TabIndex = 0;
            // 
            // labelHeader
            // 
            this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
            this.labelHeader.Location = new System.Drawing.Point(3, 8);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.labelHeader.Size = new System.Drawing.Size(200, 18);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "xxManageContractSchedules";
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel6.ColumnCount = 3;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.Controls.Add(this.buttonAdv1, 2, 0);
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // buttonAdv1
            // 
            this.buttonAdv1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdv1.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_delete_32x32;
            this.buttonAdv1.Location = new System.Drawing.Point(174, 0);
            this.buttonAdv1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 4);
            this.buttonAdv1.Name = "buttonAdv1";
            this.buttonAdv1.Size = new System.Drawing.Size(24, 96);
            this.buttonAdv1.TabIndex = 7;
            this.buttonAdv1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.BackColor = System.Drawing.Color.White;
            this.button1.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.button1.Location = new System.Drawing.Point(146, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(24, 96);
            this.button1.TabIndex = 6;
            this.button1.UseVisualStyleBackColor = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(128, 26);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Visible = false;
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.ButtonAdvDeleteWeekClick);
            // 
            // ContractScheduleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanelBody);
            this.Controls.Add(this.gradientPanelHeader);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.Name = "ContractScheduleControl";
            this.Size = new System.Drawing.Size(846, 460);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlContractSchedule)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtDescription)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvScheduleCollection)).EndInit();
            this.tableLayoutPanelBody.ResumeLayout(false);
            this.tableLayoutPanelSubHeader2.ResumeLayout(false);
            this.tableLayoutPanelSubHeader2.PerformLayout();
            this.tableLayoutPanelSubHeader1.ResumeLayout(false);
            this.tableLayoutPanelSubHeader1.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
            this.gradientPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvScheduleCollection;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtDescription;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel2;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlContractSchedule;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDeleteContractSchedule;
        private System.Windows.Forms.Button buttonAddNewContractSchedule;
        private System.Windows.Forms.Label labelSubHeader1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader2;
        private System.Windows.Forms.Label labelSubHeader2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonAdvAddWeek;
        private System.Windows.Forms.ToolTip toolTip1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDeleteWeek;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private AutoLabel autoLabelChangeInfo;
        private AutoLabel autoLabelInfoAboutChanges;
    	
    }
}
