namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class RotationPage
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
                if (components != null) components.Dispose();
                if (_gridHelper!=null) _gridHelper.Dispose();
                if (_messageBroker != null)
                {
                    _messageBroker.UnregisterEventSubscription(messageBrokerDayOffMessage);
                    _messageBroker.UnregisterEventSubscription(messageBrokerShiftCategoryMessage);
                }
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
            this.autoLabelWeeks = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelSubHeader2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvOvernight = new Syncfusion.Windows.Forms.ButtonAdv();
            this.labelSubHeader2 = new System.Windows.Forms.Label();
            this.tableLayoutPanelControlContainer = new System.Windows.Forms.TableLayoutPanel();
            this.autoLabelInfoAboutChanges = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.autoLabelRotation = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.comboBoxAdvRotations = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.textBoxDescription = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.autoLabelDescription = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.numericUpDownWeek = new Syncfusion.Windows.Forms.Tools.NumericUpDownExt();
            this.autoLabelChangeInfo = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonDelete = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonNew = new Syncfusion.Windows.Forms.ButtonAdv();
            this.labelSubHeader1 = new System.Windows.Forms.Label();
            this.gridControlRotation = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
            this.labelHeader = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanelBody.SuspendLayout();
            this.tableLayoutPanelSubHeader2.SuspendLayout();
            this.tableLayoutPanelControlContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvRotations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxDescription)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWeek)).BeginInit();
            this.tableLayoutPanelSubHeader1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlRotation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
            this.gradientPanelHeader.SuspendLayout();
            this.tableLayoutPanelHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // autoLabelWeeks
            // 
            this.autoLabelWeeks.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelWeeks.AutoSize = false;
            this.autoLabelWeeks.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.autoLabelWeeks.Location = new System.Drawing.Point(3, 75);
            this.autoLabelWeeks.Name = "autoLabelWeeks";
            this.autoLabelWeeks.Size = new System.Drawing.Size(121, 24);
            this.autoLabelWeeks.TabIndex = 4;
            this.autoLabelWeeks.Text = "xxWeeksColon";
            this.autoLabelWeeks.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanelBody
            // 
            this.tableLayoutPanelBody.ColumnCount = 1;
            this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader2, 0, 2);
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelControlContainer, 0, 1);
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
            this.tableLayoutPanelBody.Controls.Add(this.gridControlRotation, 0, 3);
            this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 62);
            this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
            this.tableLayoutPanelBody.RowCount = 4;
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBody.Size = new System.Drawing.Size(877, 550);
            this.tableLayoutPanelBody.TabIndex = 59;
            // 
            // tableLayoutPanelSubHeader2
            // 
            this.tableLayoutPanelSubHeader2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanelSubHeader2.ColumnCount = 2;
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelSubHeader2.Controls.Add(this.buttonAdvOvernight, 1, 0);
            this.tableLayoutPanelSubHeader2.Controls.Add(this.labelSubHeader2, 0, 0);
            this.tableLayoutPanelSubHeader2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSubHeader2.Location = new System.Drawing.Point(3, 193);
            this.tableLayoutPanelSubHeader2.Name = "tableLayoutPanelSubHeader2";
            this.tableLayoutPanelSubHeader2.RowCount = 1;
            this.tableLayoutPanelSubHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSubHeader2.Size = new System.Drawing.Size(871, 34);
            this.tableLayoutPanelSubHeader2.TabIndex = 60;
            // 
            // buttonAdvOvernight
            // 
            this.buttonAdvOvernight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdvOvernight.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvOvernight.BackColor = System.Drawing.Color.White;
            this.buttonAdvOvernight.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonAdvOvernight.ForeColor = System.Drawing.Color.White;
            this.buttonAdvOvernight.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillTime_16x16;
            this.buttonAdvOvernight.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdvOvernight.IsBackStageButton = false;
            this.buttonAdvOvernight.Location = new System.Drawing.Point(836, 3);
            this.buttonAdvOvernight.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.buttonAdvOvernight.Name = "buttonAdvOvernight";
            this.buttonAdvOvernight.Size = new System.Drawing.Size(28, 28);
            this.buttonAdvOvernight.TabIndex = 1;
            this.buttonAdvOvernight.TabStop = false;
            this.buttonAdvOvernight.UseVisualStyle = true;
            this.buttonAdvOvernight.Click += new System.EventHandler(this.buttonAdvOvernightClick);
            // 
            // labelSubHeader2
            // 
            this.labelSubHeader2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSubHeader2.AutoSize = true;
            this.labelSubHeader2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubHeader2.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelSubHeader2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSubHeader2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelSubHeader2.Location = new System.Drawing.Point(3, 8);
            this.labelSubHeader2.Name = "labelSubHeader2";
            this.labelSubHeader2.Size = new System.Drawing.Size(186, 17);
            this.labelSubHeader2.TabIndex = 0;
            this.labelSubHeader2.Text = "xxEnterSettingsForEachWeek";
            this.labelSubHeader2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanelControlContainer
            // 
            this.tableLayoutPanelControlContainer.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanelControlContainer.ColumnCount = 3;
            this.tableLayoutPanelControlContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 146F));
            this.tableLayoutPanelControlContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tableLayoutPanelControlContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 430F));
            this.tableLayoutPanelControlContainer.Controls.Add(this.autoLabelInfoAboutChanges, 0, 3);
            this.tableLayoutPanelControlContainer.Controls.Add(this.autoLabelWeeks, 0, 2);
            this.tableLayoutPanelControlContainer.Controls.Add(this.autoLabelRotation, 0, 0);
            this.tableLayoutPanelControlContainer.Controls.Add(this.comboBoxAdvRotations, 1, 0);
            this.tableLayoutPanelControlContainer.Controls.Add(this.textBoxDescription, 1, 1);
            this.tableLayoutPanelControlContainer.Controls.Add(this.autoLabelDescription, 0, 1);
            this.tableLayoutPanelControlContainer.Controls.Add(this.numericUpDownWeek, 1, 2);
            this.tableLayoutPanelControlContainer.Controls.Add(this.autoLabelChangeInfo, 0, 3);
            this.tableLayoutPanelControlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelControlContainer.Location = new System.Drawing.Point(0, 40);
            this.tableLayoutPanelControlContainer.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelControlContainer.Name = "tableLayoutPanelControlContainer";
            this.tableLayoutPanelControlContainer.RowCount = 5;
            this.tableLayoutPanelControlContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelControlContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelControlContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelControlContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelControlContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelControlContainer.Size = new System.Drawing.Size(877, 150);
            this.tableLayoutPanelControlContainer.TabIndex = 3;
            // 
            // autoLabelInfoAboutChanges
            // 
            this.autoLabelInfoAboutChanges.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelInfoAboutChanges.AutoSize = false;
            this.tableLayoutPanelControlContainer.SetColumnSpan(this.autoLabelInfoAboutChanges, 2);
            this.autoLabelInfoAboutChanges.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.autoLabelInfoAboutChanges.Location = new System.Drawing.Point(149, 110);
            this.autoLabelInfoAboutChanges.Margin = new System.Windows.Forms.Padding(3);
            this.autoLabelInfoAboutChanges.Name = "autoLabelInfoAboutChanges";
            this.autoLabelInfoAboutChanges.Size = new System.Drawing.Size(713, 24);
            this.autoLabelInfoAboutChanges.TabIndex = 8;
            this.autoLabelInfoAboutChanges.Text = "xxInfoAboutChanges";
            this.autoLabelInfoAboutChanges.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // autoLabelRotation
            // 
            this.autoLabelRotation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelRotation.AutoSize = false;
            this.autoLabelRotation.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.autoLabelRotation.Location = new System.Drawing.Point(3, 5);
            this.autoLabelRotation.Name = "autoLabelRotation";
            this.autoLabelRotation.Size = new System.Drawing.Size(139, 24);
            this.autoLabelRotation.TabIndex = 0;
            this.autoLabelRotation.Text = "xxSelectRotationColon";
            this.autoLabelRotation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxAdvRotations
            // 
            this.comboBoxAdvRotations.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBoxAdvRotations.BackColor = System.Drawing.Color.White;
            this.comboBoxAdvRotations.BeforeTouchSize = new System.Drawing.Size(289, 23);
            this.comboBoxAdvRotations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvRotations.ItemsImageIndexes.Add(new Syncfusion.Windows.Forms.Tools.ComboBoxAdv.ImageIndexItem(this.comboBoxAdvRotations, "New Availability"));
            this.comboBoxAdvRotations.Location = new System.Drawing.Point(149, 7);
            this.comboBoxAdvRotations.Name = "comboBoxAdvRotations";
            this.comboBoxAdvRotations.Size = new System.Drawing.Size(289, 23);
            this.comboBoxAdvRotations.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
            this.comboBoxAdvRotations.TabIndex = 1;
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxDescription.BeforeTouchSize = new System.Drawing.Size(289, 23);
            this.textBoxDescription.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxDescription.Location = new System.Drawing.Point(149, 41);
            this.textBoxDescription.MaxLength = 50;
            this.textBoxDescription.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.OverflowIndicatorToolTipText = null;
            this.textBoxDescription.Size = new System.Drawing.Size(289, 23);
            this.textBoxDescription.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
            this.textBoxDescription.TabIndex = 3;
            this.textBoxDescription.WordWrap = false;
            // 
            // autoLabelDescription
            // 
            this.autoLabelDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelDescription.AutoSize = false;
            this.autoLabelDescription.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.autoLabelDescription.Location = new System.Drawing.Point(3, 40);
            this.autoLabelDescription.Name = "autoLabelDescription";
            this.autoLabelDescription.Size = new System.Drawing.Size(121, 24);
            this.autoLabelDescription.TabIndex = 2;
            this.autoLabelDescription.Text = "xxDescriptionColon";
            this.autoLabelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDownWeek
            // 
            this.numericUpDownWeek.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numericUpDownWeek.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.numericUpDownWeek.BeforeTouchSize = new System.Drawing.Size(66, 23);
            this.numericUpDownWeek.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
            this.numericUpDownWeek.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericUpDownWeek.Location = new System.Drawing.Point(149, 76);
            this.numericUpDownWeek.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.numericUpDownWeek.MaxLength = 2;
            this.numericUpDownWeek.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
            this.numericUpDownWeek.Name = "numericUpDownWeek";
            this.numericUpDownWeek.Size = new System.Drawing.Size(66, 23);
            this.numericUpDownWeek.TabIndex = 5;
            this.numericUpDownWeek.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.Metro;
            // 
            // autoLabelChangeInfo
            // 
            this.autoLabelChangeInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelChangeInfo.AutoSize = false;
            this.autoLabelChangeInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.autoLabelChangeInfo.Location = new System.Drawing.Point(3, 110);
            this.autoLabelChangeInfo.Name = "autoLabelChangeInfo";
            this.autoLabelChangeInfo.Size = new System.Drawing.Size(121, 24);
            this.autoLabelChangeInfo.TabIndex = 6;
            this.autoLabelChangeInfo.Text = "xxChangeInfoColon";
            this.autoLabelChangeInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanelSubHeader1
            // 
            this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanelSubHeader1.ColumnCount = 3;
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonDelete, 2, 0);
            this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonNew, 1, 0);
            this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
            this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
            this.tableLayoutPanelSubHeader1.RowCount = 1;
            this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(871, 34);
            this.tableLayoutPanelSubHeader1.TabIndex = 2;
            // 
            // buttonDelete
            // 
            this.buttonDelete.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonDelete.BackColor = System.Drawing.Color.White;
            this.buttonDelete.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonDelete.ForeColor = System.Drawing.Color.White;
            this.buttonDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_temp_DeleteGroup10;
            this.buttonDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDelete.IsBackStageButton = false;
            this.buttonDelete.Location = new System.Drawing.Point(836, 3);
            this.buttonDelete.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(28, 28);
            this.buttonDelete.TabIndex = 2;
            this.buttonDelete.TabStop = false;
            this.buttonDelete.UseVisualStyle = true;
            // 
            // buttonNew
            // 
            this.buttonNew.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonNew.BackColor = System.Drawing.Color.White;
            this.buttonNew.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonNew.ForeColor = System.Drawing.Color.White;
            this.buttonNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.buttonNew.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonNew.IsBackStageButton = false;
            this.buttonNew.Location = new System.Drawing.Point(801, 3);
            this.buttonNew.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Padding = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.buttonNew.Size = new System.Drawing.Size(28, 28);
            this.buttonNew.TabIndex = 1;
            this.buttonNew.UseVisualStyle = true;
            this.buttonNew.UseVisualStyleBackColor = false;
            // 
            // labelSubHeader1
            // 
            this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSubHeader1.AutoSize = true;
            this.labelSubHeader1.BackColor = System.Drawing.Color.Transparent;
            this.labelSubHeader1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubHeader1.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelSubHeader1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelSubHeader1.Location = new System.Drawing.Point(3, 8);
            this.labelSubHeader1.Name = "labelSubHeader1";
            this.labelSubHeader1.Size = new System.Drawing.Size(81, 17);
            this.labelSubHeader1.TabIndex = 0;
            this.labelSubHeader1.Text = "xxRotations";
            this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gridControlRotation
            // 
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
            gridBaseStyle3.Name = "Column Header";
            gridBaseStyle3.StyleInfo.BaseStyle = "Header";
            gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
            gridBaseStyle4.Name = "Row Header";
            gridBaseStyle4.StyleInfo.BaseStyle = "Header";
            gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
            gridBaseStyle4.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
            this.gridControlRotation.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
            this.gridControlRotation.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
            this.gridControlRotation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlRotation.Location = new System.Drawing.Point(3, 233);
            this.gridControlRotation.Name = "gridControlRotation";
            this.gridControlRotation.NumberedRowHeaders = false;
            this.gridControlRotation.Properties.BackgroundColor = System.Drawing.Color.White;
            this.gridControlRotation.Properties.ForceImmediateRepaint = false;
            this.gridControlRotation.Properties.MarkColHeader = false;
            this.gridControlRotation.Properties.MarkRowHeader = false;
            this.gridControlRotation.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 25)});
            this.gridControlRotation.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlRotation.Size = new System.Drawing.Size(871, 344);
            this.gridControlRotation.SmartSizeBox = false;
            this.gridControlRotation.TabIndex = 0;
            this.gridControlRotation.Text = "gridControl1";
            this.gridControlRotation.UseRightToLeftCompatibleTextBox = true;
            // 
            // gradientPanelHeader
            // 
            this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gradientPanelHeader.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
            this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
            this.gradientPanelHeader.Name = "gradientPanelHeader";
            this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(12);
            this.gradientPanelHeader.Size = new System.Drawing.Size(877, 62);
            this.gradientPanelHeader.TabIndex = 58;
            // 
            // tableLayoutPanelHeader
            // 
            this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelHeader.ColumnCount = 1;
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 854F));
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
            this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelHeader.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
            this.tableLayoutPanelHeader.RowCount = 1;
            this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHeader.Size = new System.Drawing.Size(853, 38);
            this.tableLayoutPanelHeader.TabIndex = 0;
            // 
            // labelHeader
            // 
            this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
            this.labelHeader.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelHeader.Location = new System.Drawing.Point(3, 6);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelHeader.Size = new System.Drawing.Size(234, 25);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "xxChangeYourRotations";
            // 
            // RotationPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelBody);
            this.Controls.Add(this.gradientPanelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "RotationPage";
            this.Size = new System.Drawing.Size(877, 612);
            this.tableLayoutPanelBody.ResumeLayout(false);
            this.tableLayoutPanelSubHeader2.ResumeLayout(false);
            this.tableLayoutPanelSubHeader2.PerformLayout();
            this.tableLayoutPanelControlContainer.ResumeLayout(false);
            this.tableLayoutPanelControlContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvRotations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxDescription)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWeek)).EndInit();
            this.tableLayoutPanelSubHeader1.ResumeLayout(false);
            this.tableLayoutPanelSubHeader1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlRotation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
            this.gradientPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelWeeks;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelControlContainer;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelRotation;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvRotations;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxDescription;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelDescription;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonDelete;
        private Syncfusion.Windows.Forms.ButtonAdv buttonNew;
        private System.Windows.Forms.Label labelSubHeader1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private Syncfusion.Windows.Forms.Tools.NumericUpDownExt numericUpDownWeek;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlRotation;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOvernight;
        private System.Windows.Forms.Label labelSubHeader2;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelChangeInfo;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelInfoAboutChanges;
    }
}