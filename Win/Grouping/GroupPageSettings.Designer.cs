namespace Teleopti.Ccc.Win.Grouping
{
    partial class GroupPageSettings
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
                if(components != null)
                    components.Dispose();
                releaseManagedResources();
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
            this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanel22 = new System.Windows.Forms.TableLayoutPanel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel66 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBoxAdvOptionalColumns = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.radioButtonDoNotGroup = new System.Windows.Forms.RadioButton();
            this.radioButtonOptionalColumn = new System.Windows.Forms.RadioButton();
            this.radioButtonGroupByPersonNote = new System.Windows.Forms.RadioButton();
            this.radioButtonGroupByPersonPeriodRulesetBag = new System.Windows.Forms.RadioButton();
            this.radioButtonGroupByPersonPeriodContract = new System.Windows.Forms.RadioButton();
            this.radioButtonGroupByPersonPeriodContractSchedule = new System.Windows.Forms.RadioButton();
            this.radioButtonGroupByPersonPeriodPartTimePercentage = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxExtGroupPageName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.autoLabel1 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
            this.gradientPanel1.SuspendLayout();
            this.tableLayoutPanel22.SuspendLayout();
            this.tableLayoutPanel11.SuspendLayout();
            this.tableLayoutPanel66.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvOptionalColumns)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtGroupPageName)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.SuspendLayout();
            // 
            // gradientPanel1
            // 
            this.gradientPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gradientPanel1.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.gradientPanel1.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanel1.Controls.Add(this.tableLayoutPanel22);
            this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Padding = new System.Windows.Forms.Padding(12);
            this.gradientPanel1.Size = new System.Drawing.Size(810, 63);
            this.gradientPanel1.TabIndex = 55;
            // 
            // tableLayoutPanel22
            // 
            this.tableLayoutPanel22.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel22.ColumnCount = 1;
            this.tableLayoutPanel22.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1163F));
            this.tableLayoutPanel22.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel22.Controls.Add(this.labelTitle, 1, 0);
            this.tableLayoutPanel22.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel22.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel22.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel22.Name = "tableLayoutPanel22";
            this.tableLayoutPanel22.RowCount = 1;
            this.tableLayoutPanel22.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel22.Size = new System.Drawing.Size(786, 39);
            this.tableLayoutPanel22.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTitle.Location = new System.Drawing.Point(3, 7);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelTitle.Size = new System.Drawing.Size(207, 25);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "xxManageGroupings";
            // 
            // tableLayoutPanel11
            // 
            this.tableLayoutPanel11.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel11.ColumnCount = 1;
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel11.Controls.Add(this.tableLayoutPanel66, 0, 3);
            this.tableLayoutPanel11.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel11.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel11.Controls.Add(this.tableLayoutPanel5, 0, 2);
            this.tableLayoutPanel11.Controls.Add(this.tableLayoutPanel7, 0, 4);
            this.tableLayoutPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel11.Location = new System.Drawing.Point(0, 63);
            this.tableLayoutPanel11.Name = "tableLayoutPanel11";
            this.tableLayoutPanel11.RowCount = 5;
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 231F));
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel11.Size = new System.Drawing.Size(810, 406);
            this.tableLayoutPanel11.TabIndex = 56;
            // 
            // tableLayoutPanel66
            // 
            this.tableLayoutPanel66.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel66.ColumnCount = 2;
            this.tableLayoutPanel66.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel66.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 408F));
            this.tableLayoutPanel66.Controls.Add(this.comboBoxAdvOptionalColumns, 1, 7);
            this.tableLayoutPanel66.Controls.Add(this.radioButtonDoNotGroup, 0, 1);
            this.tableLayoutPanel66.Controls.Add(this.radioButtonOptionalColumn, 0, 7);
            this.tableLayoutPanel66.Controls.Add(this.radioButtonGroupByPersonNote, 0, 2);
            this.tableLayoutPanel66.Controls.Add(this.radioButtonGroupByPersonPeriodRulesetBag, 0, 6);
            this.tableLayoutPanel66.Controls.Add(this.radioButtonGroupByPersonPeriodContract, 0, 3);
            this.tableLayoutPanel66.Controls.Add(this.radioButtonGroupByPersonPeriodContractSchedule, 0, 5);
            this.tableLayoutPanel66.Controls.Add(this.radioButtonGroupByPersonPeriodPartTimePercentage, 0, 4);
            this.tableLayoutPanel66.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel66.Location = new System.Drawing.Point(0, 125);
            this.tableLayoutPanel66.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel66.Name = "tableLayoutPanel66";
            this.tableLayoutPanel66.RowCount = 9;
            this.tableLayoutPanel66.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel66.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel66.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel66.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel66.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel66.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel66.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel66.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel66.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanel66.Size = new System.Drawing.Size(810, 231);
            this.tableLayoutPanel66.TabIndex = 68;
            // 
            // comboBoxAdvOptionalColumns
            // 
            this.comboBoxAdvOptionalColumns.BackColor = System.Drawing.Color.White;
            this.comboBoxAdvOptionalColumns.BeforeTouchSize = new System.Drawing.Size(386, 23);
            this.comboBoxAdvOptionalColumns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvOptionalColumns.Location = new System.Drawing.Point(405, 189);
            this.comboBoxAdvOptionalColumns.Name = "comboBoxAdvOptionalColumns";
            this.comboBoxAdvOptionalColumns.Size = new System.Drawing.Size(386, 23);
            this.comboBoxAdvOptionalColumns.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
            this.comboBoxAdvOptionalColumns.TabIndex = 7;
            // 
            // radioButtonDoNotGroup
            // 
            this.radioButtonDoNotGroup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonDoNotGroup.AutoSize = true;
            this.radioButtonDoNotGroup.Checked = true;
            this.radioButtonDoNotGroup.Location = new System.Drawing.Point(12, 17);
            this.radioButtonDoNotGroup.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.radioButtonDoNotGroup.Name = "radioButtonDoNotGroup";
            this.radioButtonDoNotGroup.Size = new System.Drawing.Size(103, 19);
            this.radioButtonDoNotGroup.TabIndex = 0;
            this.radioButtonDoNotGroup.TabStop = true;
            this.radioButtonDoNotGroup.Text = "xxDoNotGroup";
            this.radioButtonDoNotGroup.UseVisualStyleBackColor = true;
            // 
            // radioButtonOptionalColumn
            // 
            this.radioButtonOptionalColumn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonOptionalColumn.AutoSize = true;
            this.radioButtonOptionalColumn.Location = new System.Drawing.Point(12, 191);
            this.radioButtonOptionalColumn.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.radioButtonOptionalColumn.Name = "radioButtonOptionalColumn";
            this.radioButtonOptionalColumn.Size = new System.Drawing.Size(124, 19);
            this.radioButtonOptionalColumn.TabIndex = 6;
            this.radioButtonOptionalColumn.Text = "xxOptionalColumn";
            this.radioButtonOptionalColumn.UseVisualStyleBackColor = true;
            // 
            // radioButtonGroupByPersonNote
            // 
            this.radioButtonGroupByPersonNote.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonGroupByPersonNote.AutoSize = true;
            this.radioButtonGroupByPersonNote.Location = new System.Drawing.Point(12, 46);
            this.radioButtonGroupByPersonNote.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.radioButtonGroupByPersonNote.Name = "radioButtonGroupByPersonNote";
            this.radioButtonGroupByPersonNote.Size = new System.Drawing.Size(143, 19);
            this.radioButtonGroupByPersonNote.TabIndex = 1;
            this.radioButtonGroupByPersonNote.Text = "xxGroupByPersonNote";
            this.radioButtonGroupByPersonNote.UseVisualStyleBackColor = true;
            // 
            // radioButtonGroupByPersonPeriodRulesetBag
            // 
            this.radioButtonGroupByPersonPeriodRulesetBag.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonGroupByPersonPeriodRulesetBag.AutoSize = true;
            this.radioButtonGroupByPersonPeriodRulesetBag.Location = new System.Drawing.Point(12, 162);
            this.radioButtonGroupByPersonPeriodRulesetBag.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.radioButtonGroupByPersonPeriodRulesetBag.Name = "radioButtonGroupByPersonPeriodRulesetBag";
            this.radioButtonGroupByPersonPeriodRulesetBag.Size = new System.Drawing.Size(209, 19);
            this.radioButtonGroupByPersonPeriodRulesetBag.TabIndex = 5;
            this.radioButtonGroupByPersonPeriodRulesetBag.Text = "xxGroupByPersonPeriodRulesetBag";
            this.radioButtonGroupByPersonPeriodRulesetBag.UseVisualStyleBackColor = true;
            // 
            // radioButtonGroupByPersonPeriodContract
            // 
            this.radioButtonGroupByPersonPeriodContract.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonGroupByPersonPeriodContract.AutoSize = true;
            this.radioButtonGroupByPersonPeriodContract.Location = new System.Drawing.Point(12, 75);
            this.radioButtonGroupByPersonPeriodContract.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.radioButtonGroupByPersonPeriodContract.Name = "radioButtonGroupByPersonPeriodContract";
            this.radioButtonGroupByPersonPeriodContract.Size = new System.Drawing.Size(197, 19);
            this.radioButtonGroupByPersonPeriodContract.TabIndex = 2;
            this.radioButtonGroupByPersonPeriodContract.Text = "xxGroupByPersonPeriodContract";
            this.radioButtonGroupByPersonPeriodContract.UseVisualStyleBackColor = true;
            // 
            // radioButtonGroupByPersonPeriodContractSchedule
            // 
            this.radioButtonGroupByPersonPeriodContractSchedule.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonGroupByPersonPeriodContractSchedule.AutoSize = true;
            this.radioButtonGroupByPersonPeriodContractSchedule.Location = new System.Drawing.Point(12, 133);
            this.radioButtonGroupByPersonPeriodContractSchedule.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.radioButtonGroupByPersonPeriodContractSchedule.Name = "radioButtonGroupByPersonPeriodContractSchedule";
            this.radioButtonGroupByPersonPeriodContractSchedule.Size = new System.Drawing.Size(245, 19);
            this.radioButtonGroupByPersonPeriodContractSchedule.TabIndex = 4;
            this.radioButtonGroupByPersonPeriodContractSchedule.Text = "xxGroupByPersonPeriodContractSchedule";
            this.radioButtonGroupByPersonPeriodContractSchedule.UseVisualStyleBackColor = true;
            // 
            // radioButtonGroupByPersonPeriodPartTimePercentage
            // 
            this.radioButtonGroupByPersonPeriodPartTimePercentage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioButtonGroupByPersonPeriodPartTimePercentage.AutoSize = true;
            this.radioButtonGroupByPersonPeriodPartTimePercentage.Location = new System.Drawing.Point(12, 104);
            this.radioButtonGroupByPersonPeriodPartTimePercentage.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.radioButtonGroupByPersonPeriodPartTimePercentage.Name = "radioButtonGroupByPersonPeriodPartTimePercentage";
            this.radioButtonGroupByPersonPeriodPartTimePercentage.Size = new System.Drawing.Size(258, 19);
            this.radioButtonGroupByPersonPeriodPartTimePercentage.TabIndex = 3;
            this.radioButtonGroupByPersonPeriodPartTimePercentage.Text = "xxGroupByPersonPeriodPartTimePercentage";
            this.radioButtonGroupByPersonPeriodPartTimePercentage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(804, 34);
            this.tableLayoutPanel3.TabIndex = 57;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.GhostWhite;
            this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Location = new System.Drawing.Point(12, 8);
            this.label2.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label2.Size = new System.Drawing.Size(173, 20);
            this.label2.TabIndex = 57;
            this.label2.Text = "xxEnterNameOnNewGroup";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tableLayoutPanel4.Controls.Add(this.textBoxExtGroupPageName, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.autoLabel1, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 40);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(810, 45);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // textBoxExtGroupPageName
            // 
            this.textBoxExtGroupPageName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxExtGroupPageName.BeforeTouchSize = new System.Drawing.Size(385, 23);
            this.textBoxExtGroupPageName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxExtGroupPageName.Location = new System.Drawing.Point(286, 12);
            this.textBoxExtGroupPageName.MaxLength = 50;
            this.textBoxExtGroupPageName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
            this.textBoxExtGroupPageName.Name = "textBoxExtGroupPageName";
            this.textBoxExtGroupPageName.OverflowIndicatorToolTipText = null;
            this.textBoxExtGroupPageName.Size = new System.Drawing.Size(385, 23);
            this.textBoxExtGroupPageName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
            this.textBoxExtGroupPageName.TabIndex = 1;
            this.textBoxExtGroupPageName.WordWrap = false;
            this.textBoxExtGroupPageName.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxExtGroupPageNameValidating);
            // 
            // autoLabel1
            // 
            this.autoLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabel1.Location = new System.Drawing.Point(12, 16);
            this.autoLabel1.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
            this.autoLabel1.Name = "autoLabel1";
            this.autoLabel1.Size = new System.Drawing.Size(108, 15);
            this.autoLabel1.TabIndex = 59;
            this.autoLabel1.Text = "xxGroupPageName";
            this.autoLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel5.ColumnCount = 3;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 88);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(804, 34);
            this.tableLayoutPanel5.TabIndex = 57;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.GhostWhite;
            this.label3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label3.Location = new System.Drawing.Point(12, 8);
            this.label3.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
            this.label3.Name = "label3";
            this.label3.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label3.Size = new System.Drawing.Size(124, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "xxGroupByOptions";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel7.ColumnCount = 3;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel7.Controls.Add(this.buttonAdvCancel, 2, 0);
            this.tableLayoutPanel7.Controls.Add(this.buttonAdvOk, 1, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 359);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(804, 44);
            this.tableLayoutPanel7.TabIndex = 57;
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
            this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
            this.buttonAdvCancel.IsBackStageButton = false;
            this.buttonAdvCancel.Location = new System.Drawing.Point(714, 8);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
            this.buttonAdvCancel.TabIndex = 1;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
            // 
            // buttonAdvOk
            // 
            this.buttonAdvOk.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
            this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
            this.buttonAdvOk.IsBackStageButton = false;
            this.buttonAdvOk.Location = new System.Drawing.Point(594, 8);
            this.buttonAdvOk.Name = "buttonAdvOk";
            this.buttonAdvOk.Size = new System.Drawing.Size(87, 27);
            this.buttonAdvOk.TabIndex = 0;
            this.buttonAdvOk.Text = "xxOk";
            this.buttonAdvOk.UseVisualStyle = true;
            this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
            // 
            // GroupPageSettings
            // 
            this.AcceptButton = this.buttonAdvOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(810, 469);
            this.Controls.Add(this.tableLayoutPanel11);
            this.Controls.Add(this.gradientPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GroupPageSettings";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xxAddNewGroupPage";
            this.Load += new System.EventHandler(this.groupPageSettingsLoad);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
            this.gradientPanel1.ResumeLayout(false);
            this.tableLayoutPanel22.ResumeLayout(false);
            this.tableLayoutPanel22.PerformLayout();
            this.tableLayoutPanel11.ResumeLayout(false);
            this.tableLayoutPanel66.ResumeLayout(false);
            this.tableLayoutPanel66.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvOptionalColumns)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtGroupPageName)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tableLayoutPanel7.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel22;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel11;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel1;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtGroupPageName;
        private System.Windows.Forms.RadioButton radioButtonDoNotGroup;
        private System.Windows.Forms.RadioButton radioButtonGroupByPersonNote;
        private System.Windows.Forms.RadioButton radioButtonGroupByPersonPeriodPartTimePercentage;
        private System.Windows.Forms.RadioButton radioButtonGroupByPersonPeriodContract;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private System.Windows.Forms.RadioButton radioButtonGroupByPersonPeriodRulesetBag;
        private System.Windows.Forms.RadioButton radioButtonGroupByPersonPeriodContractSchedule;
        private System.Windows.Forms.RadioButton radioButtonOptionalColumn;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvOptionalColumns;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel66;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
    }
}