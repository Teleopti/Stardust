using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
    partial class ContractControl
    {
        private const int MaxHoursPerWeek = 168;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAdjustTimeBankWithPartTimePercentage")]
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelSubHeader1 = new System.Windows.Forms.Label();
			this.buttonDeleteContract = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonNew = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelChangeInfo = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel5 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.comboBoxAdvContracts = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.textBoxDescription = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.autoLabel1 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelInfoAboutChanges = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelSubHeader2 = new System.Windows.Forms.TableLayoutPanel();
			this.labelSubHeader2 = new System.Windows.Forms.Label();
			this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
			this.checkBoxAdjustTimeBankWithPartTimePercentage = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdjustTimeBankWithSeasonality = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.autoLabelAdjustTimeBankWithPartTimePercentage = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelAdjustTimeBankWithSeasonality = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.timeSpanTextBoxPlanningMax = new TimeSpanTextBox();
			this.timeSpanTextBoxPlanningMin = new TimeSpanTextBox();
			this.labelTimeBankMax = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.labelTimeBankMin = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel7 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel6 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel3 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.comboBoxAdvEmpTypes = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel2 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel11 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel10 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel8 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel4 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.textBoxExtMaxTimePerWeek = new TimeSpanTextBox();
			this.textBoxExtNightlyRestTime = new TimeSpanTextBox();
			this.textBoxExtWeeklyRestTime = new TimeSpanTextBox();
			this.textBoxExMinTimeSchedulePeriod = new TimeSpanTextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.timeSpanTextBoxNegativeTolerance = new TimeSpanTextBox();
			this.timeSpanTextBoxPositiveTolerance = new TimeSpanTextBox();
			this.autoLabel9 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabel12 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.numericUpDownNegativeDayOff = new Syncfusion.Windows.Forms.Tools.NumericUpDownExt();
			this.numericUpDownPositiveDayOff = new Syncfusion.Windows.Forms.Tools.NumericUpDownExt();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.textBoxExtAvgWorkTimePerDay = new TimeSpanTextBox();
			this.AutoLabelFullDayAbsence = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
			this.radioButtonFromSchedule = new System.Windows.Forms.RadioButton();
			this.radioButtonFromContract = new System.Windows.Forms.RadioButton();
			this.autoLabel13 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.textBoxExtMinTimePerWeek = new TimeSpanTextBox();
			this.tableLayoutPanelSubHeader3 = new System.Windows.Forms.TableLayoutPanel();
			this.labelSubHeader3 = new System.Windows.Forms.Label();
			this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
			this.checkedListBoxMultiplicatorDefenitionSets = new System.Windows.Forms.CheckedListBox();
			this.labelConnectedMultiplicatorDefinitionSets = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
			this.labelHeader = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanelBody.SuspendLayout();
			this.tableLayoutPanelSubHeader1.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvContracts)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxDescription)).BeginInit();
			this.tableLayoutPanel5.SuspendLayout();
			this.tableLayoutPanelSubHeader2.SuspendLayout();
			this.tableLayoutPanel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdjustTimeBankWithPartTimePercentage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdjustTimeBankWithSeasonality)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvEmpTypes)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNegativeDayOff)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPositiveDayOff)).BeginInit();
			this.tableLayoutPanel4.SuspendLayout();
			this.tableLayoutPanel7.SuspendLayout();
			this.tableLayoutPanelSubHeader3.SuspendLayout();
			this.tableLayoutPanel9.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
			this.gradientPanelHeader.SuspendLayout();
			this.tableLayoutPanelHeader.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanelBody
			// 
			this.tableLayoutPanelBody.ColumnCount = 1;
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanel3, 0, 1);
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanel5, 0, 2);
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader3, 0, 3);
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanel9, 0, 4);
			this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 62);
			this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
			this.tableLayoutPanelBody.RowCount = 5;
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelBody.Size = new System.Drawing.Size(944, 780);
			this.tableLayoutPanelBody.TabIndex = 55;
			// 
			// tableLayoutPanelSubHeader1
			// 
			this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader1.ColumnCount = 3;
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonDeleteContract, 2, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonNew, 1, 0);
			this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
			this.tableLayoutPanelSubHeader1.RowCount = 1;
			this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(938, 34);
			this.tableLayoutPanelSubHeader1.TabIndex = 0;
			// 
			// labelSubHeader1
			// 
			this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSubHeader1.AutoSize = true;
			this.labelSubHeader1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSubHeader1.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelSubHeader1.Location = new System.Drawing.Point(3, 7);
			this.labelSubHeader1.Name = "labelSubHeader1";
			this.labelSubHeader1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelSubHeader1.Size = new System.Drawing.Size(180, 20);
			this.labelSubHeader1.TabIndex = 0;
			this.labelSubHeader1.Text = "xxChooseContractToChange";
			this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonDeleteContract
			// 
			this.buttonDeleteContract.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonDeleteContract.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonDeleteContract.BackColor = System.Drawing.Color.White;
			this.buttonDeleteContract.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonDeleteContract.ForeColor = System.Drawing.Color.White;
			this.buttonDeleteContract.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_temp_DeleteGroup4;
			this.buttonDeleteContract.IsBackStageButton = false;
			this.buttonDeleteContract.Location = new System.Drawing.Point(903, 3);
			this.buttonDeleteContract.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
			this.buttonDeleteContract.Name = "buttonDeleteContract";
			this.buttonDeleteContract.Size = new System.Drawing.Size(28, 28);
			this.buttonDeleteContract.TabIndex = 7;
			this.buttonDeleteContract.TabStop = false;
			this.buttonDeleteContract.UseVisualStyle = true;
			// 
			// buttonNew
			// 
			this.buttonNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonNew.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonNew.BackColor = System.Drawing.Color.White;
			this.buttonNew.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonNew.Font = new System.Drawing.Font("Tahoma", 8F);
			this.buttonNew.ForeColor = System.Drawing.Color.White;
			this.buttonNew.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.test_add2;
			this.buttonNew.IsBackStageButton = false;
			this.buttonNew.Location = new System.Drawing.Point(868, 3);
			this.buttonNew.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
			this.buttonNew.Name = "buttonNew";
			this.buttonNew.Size = new System.Drawing.Size(28, 28);
			this.buttonNew.TabIndex = 0;
			this.buttonNew.UseVisualStyle = true;
			this.buttonNew.UseVisualStyleBackColor = false;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.autoLabelChangeInfo, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.autoLabel5, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.comboBoxAdvContracts, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.textBoxDescription, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.autoLabel1, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.autoLabelInfoAboutChanges, 1, 2);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 40);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 3;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(944, 110);
			this.tableLayoutPanel3.TabIndex = 1;
			// 
			// autoLabelChangeInfo
			// 
			this.autoLabelChangeInfo.AutoSize = false;
			this.autoLabelChangeInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.autoLabelChangeInfo.Location = new System.Drawing.Point(3, 70);
			this.autoLabelChangeInfo.Name = "autoLabelChangeInfo";
			this.autoLabelChangeInfo.Size = new System.Drawing.Size(195, 35);
			this.autoLabelChangeInfo.TabIndex = 10;
			this.autoLabelChangeInfo.Text = "xxChangeInfoColon";
			this.autoLabelChangeInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel5
			// 
			this.autoLabel5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel5.AutoSize = false;
			this.autoLabel5.Location = new System.Drawing.Point(3, 5);
			this.autoLabel5.Name = "autoLabel5";
			this.autoLabel5.Size = new System.Drawing.Size(197, 24);
			this.autoLabel5.TabIndex = 8;
			this.autoLabel5.Text = "xxContractColon";
			this.autoLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxAdvContracts
			// 
			this.comboBoxAdvContracts.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvContracts.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvContracts.BeforeTouchSize = new System.Drawing.Size(252, 23);
			this.comboBoxAdvContracts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvContracts.Location = new System.Drawing.Point(353, 7);
			this.comboBoxAdvContracts.Name = "comboBoxAdvContracts";
			this.comboBoxAdvContracts.Size = new System.Drawing.Size(252, 23);
			this.comboBoxAdvContracts.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvContracts.TabIndex = 0;
			// 
			// textBoxDescription
			// 
			this.textBoxDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxDescription.BeforeTouchSize = new System.Drawing.Size(115, 23);
			this.textBoxDescription.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxDescription.Location = new System.Drawing.Point(353, 41);
			this.textBoxDescription.MaxLength = 50;
			this.textBoxDescription.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxDescription.Name = "textBoxDescription";
			this.textBoxDescription.OverflowIndicatorToolTipText = null;
			this.textBoxDescription.Size = new System.Drawing.Size(251, 23);
			this.textBoxDescription.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.textBoxDescription.TabIndex = 1;
			this.textBoxDescription.WordWrap = false;
			// 
			// autoLabel1
			// 
			this.autoLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel1.AutoSize = false;
			this.autoLabel1.Location = new System.Drawing.Point(3, 40);
			this.autoLabel1.Name = "autoLabel1";
			this.autoLabel1.Size = new System.Drawing.Size(197, 24);
			this.autoLabel1.TabIndex = 4;
			this.autoLabel1.Text = "xxDescriptionColon";
			this.autoLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabelInfoAboutChanges
			// 
			this.autoLabelInfoAboutChanges.AutoSize = false;
			this.autoLabelInfoAboutChanges.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabelInfoAboutChanges.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.autoLabelInfoAboutChanges.Location = new System.Drawing.Point(350, 70);
			this.autoLabelInfoAboutChanges.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.autoLabelInfoAboutChanges.Name = "autoLabelInfoAboutChanges";
			this.autoLabelInfoAboutChanges.Size = new System.Drawing.Size(591, 40);
			this.autoLabelInfoAboutChanges.TabIndex = 9;
			this.autoLabelInfoAboutChanges.Text = "xxInfoAboutChanges";
			this.autoLabelInfoAboutChanges.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.AutoScroll = true;
			this.tableLayoutPanel5.AutoScrollMinSize = new System.Drawing.Size(0, 550);
			this.tableLayoutPanel5.ColumnCount = 1;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanelSubHeader2, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 1);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 150);
			this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 2;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 450F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(944, 470);
			this.tableLayoutPanel5.TabIndex = 2;
			// 
			// tableLayoutPanelSubHeader2
			// 
			this.tableLayoutPanelSubHeader2.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader2.ColumnCount = 1;
			this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelSubHeader2.Controls.Add(this.labelSubHeader2, 0, 0);
			this.tableLayoutPanelSubHeader2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSubHeader2.Name = "tableLayoutPanelSubHeader2";
			this.tableLayoutPanelSubHeader2.RowCount = 1;
			this.tableLayoutPanelSubHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelSubHeader2.Size = new System.Drawing.Size(938, 34);
			this.tableLayoutPanelSubHeader2.TabIndex = 1;
			// 
			// labelSubHeader2
			// 
			this.labelSubHeader2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSubHeader2.AutoSize = true;
			this.labelSubHeader2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSubHeader2.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelSubHeader2.Location = new System.Drawing.Point(3, 8);
			this.labelSubHeader2.Name = "labelSubHeader2";
			this.labelSubHeader2.Size = new System.Drawing.Size(149, 17);
			this.labelSubHeader2.TabIndex = 0;
			this.labelSubHeader2.Text = "xxEnterContractDetails";
			this.labelSubHeader2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// tableLayoutPanel6
			// 
			this.tableLayoutPanel6.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel6.ColumnCount = 2;
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F));
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel6.Controls.Add(this.checkBoxAdjustTimeBankWithPartTimePercentage, 1, 12);
			this.tableLayoutPanel6.Controls.Add(this.checkBoxAdjustTimeBankWithSeasonality, 1, 11);
			this.tableLayoutPanel6.Controls.Add(this.autoLabelAdjustTimeBankWithPartTimePercentage, 0, 12);
			this.tableLayoutPanel6.Controls.Add(this.autoLabelAdjustTimeBankWithSeasonality, 0, 11);
			this.tableLayoutPanel6.Controls.Add(this.timeSpanTextBoxPlanningMax, 1, 10);
			this.tableLayoutPanel6.Controls.Add(this.timeSpanTextBoxPlanningMin, 1, 9);
			this.tableLayoutPanel6.Controls.Add(this.labelTimeBankMax, 0, 10);
			this.tableLayoutPanel6.Controls.Add(this.labelTimeBankMin, 0, 9);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel7, 0, 8);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel6, 0, 3);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel3, 0, 2);
			this.tableLayoutPanel6.Controls.Add(this.comboBoxAdvEmpTypes, 1, 0);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel2, 0, 0);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel11, 0, 7);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel10, 0, 6);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel8, 0, 4);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel4, 0, 1);
			this.tableLayoutPanel6.Controls.Add(this.textBoxExtMaxTimePerWeek, 1, 4);
			this.tableLayoutPanel6.Controls.Add(this.textBoxExtNightlyRestTime, 1, 6);
			this.tableLayoutPanel6.Controls.Add(this.textBoxExtWeeklyRestTime, 1, 7);
			this.tableLayoutPanel6.Controls.Add(this.textBoxExMinTimeSchedulePeriod, 1, 8);
			this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel1, 1, 2);
			this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel2, 1, 3);
			this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel4, 1, 1);
			this.tableLayoutPanel6.Controls.Add(this.AutoLabelFullDayAbsence, 0, 13);
			this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel7, 1, 13);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel13, 0, 5);
			this.tableLayoutPanel6.Controls.Add(this.textBoxExtMinTimePerWeek, 1, 5);
			this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 43);
			this.tableLayoutPanel6.MinimumSize = new System.Drawing.Size(0, 200);
			this.tableLayoutPanel6.Name = "tableLayoutPanel6";
			this.tableLayoutPanel6.RowCount = 14;
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel6.Size = new System.Drawing.Size(938, 504);
			this.tableLayoutPanel6.TabIndex = 0;
			// 
			// checkBoxAdjustTimeBankWithPartTimePercentage
			// 
			this.checkBoxAdjustTimeBankWithPartTimePercentage.BeforeTouchSize = new System.Drawing.Size(20, 22);
			this.checkBoxAdjustTimeBankWithPartTimePercentage.DrawFocusRectangle = false;
			this.checkBoxAdjustTimeBankWithPartTimePercentage.Location = new System.Drawing.Point(353, 427);
			this.checkBoxAdjustTimeBankWithPartTimePercentage.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
			this.checkBoxAdjustTimeBankWithPartTimePercentage.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdjustTimeBankWithPartTimePercentage.Name = "checkBoxAdjustTimeBankWithPartTimePercentage";
			this.checkBoxAdjustTimeBankWithPartTimePercentage.Size = new System.Drawing.Size(20, 22);
			this.checkBoxAdjustTimeBankWithPartTimePercentage.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdjustTimeBankWithPartTimePercentage.TabIndex = 13;
			this.checkBoxAdjustTimeBankWithPartTimePercentage.ThemesEnabled = false;
			this.checkBoxAdjustTimeBankWithPartTimePercentage.CheckStateChanged += new System.EventHandler(this.checkBoxAdjustTimeBankWithPartTimePercentageCheckStateChanged);
			// 
			// checkBoxAdjustTimeBankWithSeasonality
			// 
			this.checkBoxAdjustTimeBankWithSeasonality.BeforeTouchSize = new System.Drawing.Size(20, 22);
			this.checkBoxAdjustTimeBankWithSeasonality.DrawFocusRectangle = false;
			this.checkBoxAdjustTimeBankWithSeasonality.Location = new System.Drawing.Point(353, 392);
			this.checkBoxAdjustTimeBankWithSeasonality.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
			this.checkBoxAdjustTimeBankWithSeasonality.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdjustTimeBankWithSeasonality.Name = "checkBoxAdjustTimeBankWithSeasonality";
			this.checkBoxAdjustTimeBankWithSeasonality.Size = new System.Drawing.Size(20, 22);
			this.checkBoxAdjustTimeBankWithSeasonality.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdjustTimeBankWithSeasonality.TabIndex = 12;
			this.checkBoxAdjustTimeBankWithSeasonality.ThemesEnabled = false;
			this.checkBoxAdjustTimeBankWithSeasonality.CheckStateChanged += new System.EventHandler(this.checkBoxAdjustTimeBankWithSeasonalityCheckStateChanged);
			// 
			// autoLabelAdjustTimeBankWithPartTimePercentage
			// 
			this.autoLabelAdjustTimeBankWithPartTimePercentage.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabelAdjustTimeBankWithPartTimePercentage.AutoSize = false;
			this.autoLabelAdjustTimeBankWithPartTimePercentage.Location = new System.Drawing.Point(3, 425);
			this.autoLabelAdjustTimeBankWithPartTimePercentage.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabelAdjustTimeBankWithPartTimePercentage.Name = "autoLabelAdjustTimeBankWithPartTimePercentage";
			this.autoLabelAdjustTimeBankWithPartTimePercentage.Size = new System.Drawing.Size(344, 24);
			this.autoLabelAdjustTimeBankWithPartTimePercentage.TabIndex = 65;
			this.autoLabelAdjustTimeBankWithPartTimePercentage.Text = "xxAdjustTimeBankWithPartTimePercentage";
			this.autoLabelAdjustTimeBankWithPartTimePercentage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabelAdjustTimeBankWithSeasonality
			// 
			this.autoLabelAdjustTimeBankWithSeasonality.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabelAdjustTimeBankWithSeasonality.AutoSize = false;
			this.autoLabelAdjustTimeBankWithSeasonality.Location = new System.Drawing.Point(3, 390);
			this.autoLabelAdjustTimeBankWithSeasonality.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabelAdjustTimeBankWithSeasonality.Name = "autoLabelAdjustTimeBankWithSeasonality";
			this.autoLabelAdjustTimeBankWithSeasonality.Size = new System.Drawing.Size(344, 24);
			this.autoLabelAdjustTimeBankWithSeasonality.TabIndex = 64;
			this.autoLabelAdjustTimeBankWithSeasonality.Text = "xxAdjustTimeBankWithSeasonality";
			this.autoLabelAdjustTimeBankWithSeasonality.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// timeSpanTextBoxPlanningMax
			// 
			this.timeSpanTextBoxPlanningMax.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxPlanningMax.AllowNegativeValues = false;
			this.timeSpanTextBoxPlanningMax.DefaultInterpretAsMinutes = false;
			this.timeSpanTextBoxPlanningMax.Location = new System.Drawing.Point(353, 355);
			this.timeSpanTextBoxPlanningMax.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.timeSpanTextBoxPlanningMax.MaximumValue = System.TimeSpan.Parse("4.03:59:00");
			this.timeSpanTextBoxPlanningMax.Name = "timeSpanTextBoxPlanningMax";
			this.timeSpanTextBoxPlanningMax.Size = new System.Drawing.Size(585, 25);
			this.timeSpanTextBoxPlanningMax.TabIndex = 11;
			this.timeSpanTextBoxPlanningMax.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.timeSpanTextBoxPlanningMax.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxPlanningMax.TimeSpanBoxWidth = 4932;
			// 
			// timeSpanTextBoxPlanningMin
			// 
			this.timeSpanTextBoxPlanningMin.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxPlanningMin.AllowNegativeValues = true;
			this.timeSpanTextBoxPlanningMin.DefaultInterpretAsMinutes = false;
			this.timeSpanTextBoxPlanningMin.Location = new System.Drawing.Point(353, 320);
			this.timeSpanTextBoxPlanningMin.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.timeSpanTextBoxPlanningMin.MaximumValue = System.TimeSpan.Parse("00:00:00");
			this.timeSpanTextBoxPlanningMin.Name = "timeSpanTextBoxPlanningMin";
			this.timeSpanTextBoxPlanningMin.Size = new System.Drawing.Size(585, 25);
			this.timeSpanTextBoxPlanningMin.TabIndex = 10;
			this.timeSpanTextBoxPlanningMin.TimeFormat = TimeFormatsType.HoursMinutes;
			this.timeSpanTextBoxPlanningMin.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxPlanningMin.TimeSpanBoxWidth = 4932;
			// 
			// labelTimeBankMax
			// 
			this.labelTimeBankMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTimeBankMax.AutoSize = false;
			this.labelTimeBankMax.Location = new System.Drawing.Point(3, 355);
			this.labelTimeBankMax.Margin = new System.Windows.Forms.Padding(3);
			this.labelTimeBankMax.Name = "labelTimeBankMax";
			this.labelTimeBankMax.Size = new System.Drawing.Size(344, 24);
			this.labelTimeBankMax.TabIndex = 61;
			this.labelTimeBankMax.Text = "xxPlanningTimeBankMax";
			this.labelTimeBankMax.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelTimeBankMin
			// 
			this.labelTimeBankMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTimeBankMin.AutoSize = false;
			this.labelTimeBankMin.Location = new System.Drawing.Point(3, 320);
			this.labelTimeBankMin.Margin = new System.Windows.Forms.Padding(3);
			this.labelTimeBankMin.Name = "labelTimeBankMin";
			this.labelTimeBankMin.Size = new System.Drawing.Size(344, 24);
			this.labelTimeBankMin.TabIndex = 60;
			this.labelTimeBankMin.Text = "xxPlanningTimeBankMin";
			this.labelTimeBankMin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel7
			// 
			this.autoLabel7.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel7.AutoSize = false;
			this.autoLabel7.Location = new System.Drawing.Point(3, 285);
			this.autoLabel7.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel7.Name = "autoLabel7";
			this.autoLabel7.Size = new System.Drawing.Size(344, 24);
			this.autoLabel7.TabIndex = 58;
			this.autoLabel7.Text = "xxMinTimeSchedulePeriodColon";
			this.autoLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel6
			// 
			this.autoLabel6.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel6.AutoSize = false;
			this.autoLabel6.Location = new System.Drawing.Point(3, 110);
			this.autoLabel6.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel6.Name = "autoLabel6";
			this.autoLabel6.Size = new System.Drawing.Size(344, 24);
			this.autoLabel6.TabIndex = 57;
			this.autoLabel6.Text = "xxDayOffTolerancMinus";
			this.autoLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel3
			// 
			this.autoLabel3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel3.AutoSize = false;
			this.autoLabel3.Location = new System.Drawing.Point(3, 75);
			this.autoLabel3.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel3.Name = "autoLabel3";
			this.autoLabel3.Size = new System.Drawing.Size(344, 24);
			this.autoLabel3.TabIndex = 55;
			this.autoLabel3.Text = "xxTargetToleranceMinus";
			this.autoLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxAdvEmpTypes
			// 
			this.comboBoxAdvEmpTypes.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvEmpTypes.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvEmpTypes.BeforeTouchSize = new System.Drawing.Size(252, 23);
			this.comboBoxAdvEmpTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvEmpTypes.Location = new System.Drawing.Point(353, 7);
			this.comboBoxAdvEmpTypes.Name = "comboBoxAdvEmpTypes";
			this.comboBoxAdvEmpTypes.Size = new System.Drawing.Size(252, 23);
			this.comboBoxAdvEmpTypes.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvEmpTypes.TabIndex = 0;
			this.comboBoxAdvEmpTypes.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvEmpTypesSelectedIndexChanged);
			// 
			// autoLabel2
			// 
			this.autoLabel2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel2.AutoSize = false;
			this.autoLabel2.Location = new System.Drawing.Point(3, 5);
			this.autoLabel2.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel2.Name = "autoLabel2";
			this.autoLabel2.Size = new System.Drawing.Size(344, 24);
			this.autoLabel2.TabIndex = 6;
			this.autoLabel2.Text = "xxEmploymentTypeColon";
			this.autoLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel11
			// 
			this.autoLabel11.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel11.AutoSize = false;
			this.autoLabel11.Location = new System.Drawing.Point(3, 250);
			this.autoLabel11.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel11.Name = "autoLabel11";
			this.autoLabel11.Size = new System.Drawing.Size(344, 24);
			this.autoLabel11.TabIndex = 54;
			this.autoLabel11.Text = "xxWeeklyRestTimeColon";
			this.autoLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel10
			// 
			this.autoLabel10.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel10.AutoSize = false;
			this.autoLabel10.Location = new System.Drawing.Point(3, 215);
			this.autoLabel10.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel10.Name = "autoLabel10";
			this.autoLabel10.Size = new System.Drawing.Size(344, 24);
			this.autoLabel10.TabIndex = 12;
			this.autoLabel10.Text = "xxNightlyRestTimeColon";
			this.autoLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel8
			// 
			this.autoLabel8.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel8.AutoSize = false;
			this.autoLabel8.Location = new System.Drawing.Point(3, 145);
			this.autoLabel8.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel8.Name = "autoLabel8";
			this.autoLabel8.Size = new System.Drawing.Size(344, 24);
			this.autoLabel8.TabIndex = 9;
			this.autoLabel8.Text = "xxMaximumTimePerWeekColon";
			this.autoLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel4
			// 
			this.autoLabel4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel4.AutoSize = false;
			this.autoLabel4.Location = new System.Drawing.Point(3, 40);
			this.autoLabel4.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel4.Name = "autoLabel4";
			this.autoLabel4.Size = new System.Drawing.Size(344, 24);
			this.autoLabel4.TabIndex = 16;
			this.autoLabel4.Text = "xxAverageWorkTimePerDayColon";
			this.autoLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxExtMaxTimePerWeek
			// 
			this.textBoxExtMaxTimePerWeek.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.textBoxExtMaxTimePerWeek.AllowNegativeValues = true;
			this.textBoxExtMaxTimePerWeek.DefaultInterpretAsMinutes = false;
			this.textBoxExtMaxTimePerWeek.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.textBoxExtMaxTimePerWeek.Location = new System.Drawing.Point(353, 145);
			this.textBoxExtMaxTimePerWeek.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.textBoxExtMaxTimePerWeek.MaximumValue = System.TimeSpan.Parse("3.12:00:00");
			this.textBoxExtMaxTimePerWeek.Name = "textBoxExtMaxTimePerWeek";
			this.textBoxExtMaxTimePerWeek.Size = new System.Drawing.Size(585, 25);
			this.textBoxExtMaxTimePerWeek.TabIndex = 6;
			this.textBoxExtMaxTimePerWeek.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.textBoxExtMaxTimePerWeek.TimeSpanBoxHeight = 23;
			this.textBoxExtMaxTimePerWeek.TimeSpanBoxWidth = 4932;
			// 
			// textBoxExtNightlyRestTime
			// 
			this.textBoxExtNightlyRestTime.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.textBoxExtNightlyRestTime.AllowNegativeValues = true;
			this.textBoxExtNightlyRestTime.DefaultInterpretAsMinutes = false;
			this.textBoxExtNightlyRestTime.Location = new System.Drawing.Point(353, 215);
			this.textBoxExtNightlyRestTime.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.textBoxExtNightlyRestTime.MaximumValue = System.TimeSpan.Parse("1.00:00:00");
			this.textBoxExtNightlyRestTime.Name = "textBoxExtNightlyRestTime";
			this.textBoxExtNightlyRestTime.Size = new System.Drawing.Size(585, 25);
			this.textBoxExtNightlyRestTime.TabIndex = 7;
			this.textBoxExtNightlyRestTime.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.textBoxExtNightlyRestTime.TimeSpanBoxHeight = 23;
			this.textBoxExtNightlyRestTime.TimeSpanBoxWidth = 4932;
			// 
			// textBoxExtWeeklyRestTime
			// 
			this.textBoxExtWeeklyRestTime.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.textBoxExtWeeklyRestTime.AllowNegativeValues = true;
			this.textBoxExtWeeklyRestTime.DefaultInterpretAsMinutes = false;
			this.textBoxExtWeeklyRestTime.Location = new System.Drawing.Point(353, 250);
			this.textBoxExtWeeklyRestTime.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.textBoxExtWeeklyRestTime.MaximumValue = System.TimeSpan.Parse("2.08:00:00");
			this.textBoxExtWeeklyRestTime.Name = "textBoxExtWeeklyRestTime";
			this.textBoxExtWeeklyRestTime.Size = new System.Drawing.Size(585, 25);
			this.textBoxExtWeeklyRestTime.TabIndex = 8;
			this.textBoxExtWeeklyRestTime.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.textBoxExtWeeklyRestTime.TimeSpanBoxHeight = 23;
			this.textBoxExtWeeklyRestTime.TimeSpanBoxWidth = 4932;
			// 
			// textBoxExMinTimeSchedulePeriod
			// 
			this.textBoxExMinTimeSchedulePeriod.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.textBoxExMinTimeSchedulePeriod.AllowNegativeValues = true;
			this.textBoxExMinTimeSchedulePeriod.DefaultInterpretAsMinutes = false;
			this.textBoxExMinTimeSchedulePeriod.Location = new System.Drawing.Point(353, 285);
			this.textBoxExMinTimeSchedulePeriod.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.textBoxExMinTimeSchedulePeriod.MaximumValue = System.TimeSpan.Parse("83.08:00:00");
			this.textBoxExMinTimeSchedulePeriod.Name = "textBoxExMinTimeSchedulePeriod";
			this.textBoxExMinTimeSchedulePeriod.Size = new System.Drawing.Size(585, 25);
			this.textBoxExMinTimeSchedulePeriod.TabIndex = 9;
			this.textBoxExMinTimeSchedulePeriod.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.textBoxExMinTimeSchedulePeriod.TimeSpanBoxHeight = 23;
			this.textBoxExMinTimeSchedulePeriod.TimeSpanBoxWidth = 4932;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 139F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 13F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 218F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 356F));
			this.tableLayoutPanel1.Controls.Add(this.timeSpanTextBoxNegativeTolerance, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.timeSpanTextBoxPositiveTolerance, 3, 0);
			this.tableLayoutPanel1.Controls.Add(this.autoLabel9, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(350, 70);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(588, 35);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// timeSpanTextBoxNegativeTolerance
			// 
			this.timeSpanTextBoxNegativeTolerance.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxNegativeTolerance.AllowNegativeValues = true;
			this.timeSpanTextBoxNegativeTolerance.DefaultInterpretAsMinutes = false;
			this.timeSpanTextBoxNegativeTolerance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.timeSpanTextBoxNegativeTolerance.Location = new System.Drawing.Point(3, 5);
			this.timeSpanTextBoxNegativeTolerance.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.timeSpanTextBoxNegativeTolerance.MaximumValue = System.TimeSpan.Parse("12.00:00:00");
			this.timeSpanTextBoxNegativeTolerance.Name = "timeSpanTextBoxNegativeTolerance";
			this.timeSpanTextBoxNegativeTolerance.Size = new System.Drawing.Size(136, 25);
			this.timeSpanTextBoxNegativeTolerance.TabIndex = 2;
			this.timeSpanTextBoxNegativeTolerance.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.timeSpanTextBoxNegativeTolerance.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxNegativeTolerance.TimeSpanBoxWidth = 4932;
			// 
			// timeSpanTextBoxPositiveTolerance
			// 
			this.timeSpanTextBoxPositiveTolerance.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxPositiveTolerance.AllowNegativeValues = true;
			this.timeSpanTextBoxPositiveTolerance.DefaultInterpretAsMinutes = false;
			this.timeSpanTextBoxPositiveTolerance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.timeSpanTextBoxPositiveTolerance.Location = new System.Drawing.Point(373, 5);
			this.timeSpanTextBoxPositiveTolerance.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.timeSpanTextBoxPositiveTolerance.MaximumValue = System.TimeSpan.Parse("12.00:00:00");
			this.timeSpanTextBoxPositiveTolerance.Name = "timeSpanTextBoxPositiveTolerance";
			this.timeSpanTextBoxPositiveTolerance.Size = new System.Drawing.Size(353, 25);
			this.timeSpanTextBoxPositiveTolerance.TabIndex = 3;
			this.timeSpanTextBoxPositiveTolerance.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.timeSpanTextBoxPositiveTolerance.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxPositiveTolerance.TimeSpanBoxWidth = 4932;
			// 
			// autoLabel9
			// 
			this.autoLabel9.AutoSize = false;
			this.autoLabel9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabel9.Location = new System.Drawing.Point(155, 0);
			this.autoLabel9.Name = "autoLabel9";
			this.autoLabel9.Size = new System.Drawing.Size(212, 35);
			this.autoLabel9.TabIndex = 58;
			this.autoLabel9.Text = "xxTargetTolerancePlus";
			this.autoLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 4;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 135F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 17F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 217F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 357F));
			this.tableLayoutPanel2.Controls.Add(this.autoLabel12, 2, 0);
			this.tableLayoutPanel2.Controls.Add(this.numericUpDownNegativeDayOff, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.numericUpDownPositiveDayOff, 3, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(350, 105);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(588, 35);
			this.tableLayoutPanel2.TabIndex = 3;
			// 
			// autoLabel12
			// 
			this.autoLabel12.AutoSize = false;
			this.autoLabel12.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabel12.Location = new System.Drawing.Point(155, 0);
			this.autoLabel12.Name = "autoLabel12";
			this.autoLabel12.Size = new System.Drawing.Size(211, 35);
			this.autoLabel12.TabIndex = 58;
			this.autoLabel12.Text = "xxDayOffTolerancePlus";
			this.autoLabel12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numericUpDownNegativeDayOff
			// 
			this.numericUpDownNegativeDayOff.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.numericUpDownNegativeDayOff.BeforeTouchSize = new System.Drawing.Size(52, 23);
			this.numericUpDownNegativeDayOff.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.numericUpDownNegativeDayOff.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDownNegativeDayOff.Location = new System.Drawing.Point(3, 5);
			this.numericUpDownNegativeDayOff.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.numericUpDownNegativeDayOff.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.numericUpDownNegativeDayOff.Name = "numericUpDownNegativeDayOff";
			this.numericUpDownNegativeDayOff.Size = new System.Drawing.Size(52, 23);
			this.numericUpDownNegativeDayOff.TabIndex = 4;
			this.numericUpDownNegativeDayOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDownNegativeDayOff.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.numericUpDownNegativeDayOff.ValueChanged += new System.EventHandler(this.numericUpDownNegativeDayOffValueChanged);
			// 
			// numericUpDownPositiveDayOff
			// 
			this.numericUpDownPositiveDayOff.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.numericUpDownPositiveDayOff.BeforeTouchSize = new System.Drawing.Size(52, 23);
			this.numericUpDownPositiveDayOff.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.numericUpDownPositiveDayOff.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDownPositiveDayOff.Location = new System.Drawing.Point(372, 5);
			this.numericUpDownPositiveDayOff.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.numericUpDownPositiveDayOff.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.numericUpDownPositiveDayOff.Name = "numericUpDownPositiveDayOff";
			this.numericUpDownPositiveDayOff.Size = new System.Drawing.Size(52, 23);
			this.numericUpDownPositiveDayOff.TabIndex = 5;
			this.numericUpDownPositiveDayOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDownPositiveDayOff.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.numericUpDownPositiveDayOff.ValueChanged += new System.EventHandler(this.numericUpDownPositiveDayOffValueChanged);
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.ColumnCount = 3;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 152F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.Controls.Add(this.textBoxExtAvgWorkTimePerDay, 0, 0);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(350, 35);
			this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 1;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(588, 35);
			this.tableLayoutPanel4.TabIndex = 1;
			// 
			// textBoxExtAvgWorkTimePerDay
			// 
			this.textBoxExtAvgWorkTimePerDay.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.textBoxExtAvgWorkTimePerDay.AllowNegativeValues = true;
			this.textBoxExtAvgWorkTimePerDay.DefaultInterpretAsMinutes = false;
			this.textBoxExtAvgWorkTimePerDay.Location = new System.Drawing.Point(3, 5);
			this.textBoxExtAvgWorkTimePerDay.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.textBoxExtAvgWorkTimePerDay.MaximumValue = System.TimeSpan.Parse("1.00:00:00");
			this.textBoxExtAvgWorkTimePerDay.Name = "textBoxExtAvgWorkTimePerDay";
			this.textBoxExtAvgWorkTimePerDay.Size = new System.Drawing.Size(149, 26);
			this.textBoxExtAvgWorkTimePerDay.TabIndex = 1;
			this.textBoxExtAvgWorkTimePerDay.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.textBoxExtAvgWorkTimePerDay.TimeSpanBoxHeight = 23;
			this.textBoxExtAvgWorkTimePerDay.TimeSpanBoxWidth = 4932;
			// 
			// AutoLabelFullDayAbsence
			// 
			this.AutoLabelFullDayAbsence.AutoSize = false;
			this.AutoLabelFullDayAbsence.Location = new System.Drawing.Point(3, 458);
			this.AutoLabelFullDayAbsence.Margin = new System.Windows.Forms.Padding(3);
			this.AutoLabelFullDayAbsence.Name = "AutoLabelFullDayAbsence";
			this.AutoLabelFullDayAbsence.Size = new System.Drawing.Size(344, 24);
			this.AutoLabelFullDayAbsence.TabIndex = 69;
			this.AutoLabelFullDayAbsence.Text = "xxFullDayAbsence";
			this.AutoLabelFullDayAbsence.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanel7
			// 
			this.tableLayoutPanel7.ColumnCount = 2;
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel7.Controls.Add(this.radioButtonFromSchedule, 1, 0);
			this.tableLayoutPanel7.Controls.Add(this.radioButtonFromContract, 0, 0);
			this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel7.Location = new System.Drawing.Point(350, 455);
			this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel7.Name = "tableLayoutPanel7";
			this.tableLayoutPanel7.RowCount = 1;
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel7.Size = new System.Drawing.Size(588, 49);
			this.tableLayoutPanel7.TabIndex = 70;
			// 
			// radioButtonFromSchedule
			// 
			this.radioButtonFromSchedule.AutoSize = true;
			this.radioButtonFromSchedule.Location = new System.Drawing.Point(178, 7);
			this.radioButtonFromSchedule.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
			this.radioButtonFromSchedule.Name = "radioButtonFromSchedule";
			this.radioButtonFromSchedule.Size = new System.Drawing.Size(145, 19);
			this.radioButtonFromSchedule.TabIndex = 15;
			this.radioButtonFromSchedule.Text = "xxFromSchedulePeriod";
			this.radioButtonFromSchedule.UseVisualStyleBackColor = true;
			this.radioButtonFromSchedule.CheckedChanged += new System.EventHandler(this.radioButtonFromScheduleCheckedChanged);
			// 
			// radioButtonFromContract
			// 
			this.radioButtonFromContract.AutoSize = true;
			this.radioButtonFromContract.Checked = true;
			this.radioButtonFromContract.Location = new System.Drawing.Point(3, 7);
			this.radioButtonFromContract.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
			this.radioButtonFromContract.Name = "radioButtonFromContract";
			this.radioButtonFromContract.Size = new System.Drawing.Size(109, 19);
			this.radioButtonFromContract.TabIndex = 14;
			this.radioButtonFromContract.TabStop = true;
			this.radioButtonFromContract.Text = "xxFromContract";
			this.radioButtonFromContract.UseVisualStyleBackColor = true;
			this.radioButtonFromContract.CheckedChanged += new System.EventHandler(this.radioButtonFromContractCheckedChanged);
			// 
			// autoLabel13
			// 
			this.autoLabel13.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel13.AutoSize = false;
			this.autoLabel13.Location = new System.Drawing.Point(3, 180);
			this.autoLabel13.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel13.Name = "autoLabel13";
			this.autoLabel13.Size = new System.Drawing.Size(344, 24);
			this.autoLabel13.TabIndex = 71;
			this.autoLabel13.Text = "xxMinimumTimePerWeekColon";
			this.autoLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxExtMinTimePerWeek
			// 
			this.textBoxExtMinTimePerWeek.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.textBoxExtMinTimePerWeek.AllowNegativeValues = true;
			this.textBoxExtMinTimePerWeek.DefaultInterpretAsMinutes = false;
			this.textBoxExtMinTimePerWeek.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.textBoxExtMinTimePerWeek.Location = new System.Drawing.Point(353, 181);
			this.textBoxExtMinTimePerWeek.Margin = new System.Windows.Forms.Padding(3, 6, 0, 0);
			this.textBoxExtMinTimePerWeek.MaximumValue = System.TimeSpan.Parse("3.12:00:00");
			this.textBoxExtMinTimePerWeek.Name = "textBoxExtMinTimePerWeek";
			this.textBoxExtMinTimePerWeek.Size = new System.Drawing.Size(585, 29);
			this.textBoxExtMinTimePerWeek.TabIndex = 72;
			this.textBoxExtMinTimePerWeek.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.textBoxExtMinTimePerWeek.TimeSpanBoxHeight = 23;
			this.textBoxExtMinTimePerWeek.TimeSpanBoxWidth = 4932;
			// 
			// tableLayoutPanelSubHeader3
			// 
			this.tableLayoutPanelSubHeader3.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader3.ColumnCount = 1;
			this.tableLayoutPanelSubHeader3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader3.Controls.Add(this.labelSubHeader3, 0, 0);
			this.tableLayoutPanelSubHeader3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader3.Location = new System.Drawing.Point(3, 623);
			this.tableLayoutPanelSubHeader3.Name = "tableLayoutPanelSubHeader3";
			this.tableLayoutPanelSubHeader3.RowCount = 1;
			this.tableLayoutPanelSubHeader3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
			this.tableLayoutPanelSubHeader3.Size = new System.Drawing.Size(938, 34);
			this.tableLayoutPanelSubHeader3.TabIndex = 3;
			// 
			// labelSubHeader3
			// 
			this.labelSubHeader3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSubHeader3.AutoSize = true;
			this.labelSubHeader3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSubHeader3.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelSubHeader3.Location = new System.Drawing.Point(3, 8);
			this.labelSubHeader3.Name = "labelSubHeader3";
			this.labelSubHeader3.Size = new System.Drawing.Size(190, 17);
			this.labelSubHeader3.TabIndex = 0;
			this.labelSubHeader3.Text = "xxMultiplicatorDefinitionSets";
			this.labelSubHeader3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// tableLayoutPanel9
			// 
			this.tableLayoutPanel9.AutoScroll = true;
			this.tableLayoutPanel9.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel9.ColumnCount = 2;
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F));
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel9.Controls.Add(this.checkedListBoxMultiplicatorDefenitionSets, 1, 0);
			this.tableLayoutPanel9.Controls.Add(this.labelConnectedMultiplicatorDefinitionSets, 0, 0);
			this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel9.Location = new System.Drawing.Point(0, 660);
			this.tableLayoutPanel9.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel9.Name = "tableLayoutPanel9";
			this.tableLayoutPanel9.RowCount = 1;
			this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel9.Size = new System.Drawing.Size(944, 120);
			this.tableLayoutPanel9.TabIndex = 1;
			// 
			// checkedListBoxMultiplicatorDefenitionSets
			// 
			this.checkedListBoxMultiplicatorDefenitionSets.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.checkedListBoxMultiplicatorDefenitionSets.CheckOnClick = true;
			this.checkedListBoxMultiplicatorDefenitionSets.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkedListBoxMultiplicatorDefenitionSets.FormattingEnabled = true;
			this.checkedListBoxMultiplicatorDefenitionSets.Location = new System.Drawing.Point(357, 3);
			this.checkedListBoxMultiplicatorDefenitionSets.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
			this.checkedListBoxMultiplicatorDefenitionSets.Name = "checkedListBoxMultiplicatorDefenitionSets";
			this.checkedListBoxMultiplicatorDefenitionSets.Size = new System.Drawing.Size(584, 114);
			this.checkedListBoxMultiplicatorDefenitionSets.TabIndex = 0;
			this.checkedListBoxMultiplicatorDefenitionSets.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxMultiplicatorDefenitionSetsItemCheck);
			// 
			// labelConnectedMultiplicatorDefinitionSets
			// 
			this.labelConnectedMultiplicatorDefinitionSets.Location = new System.Drawing.Point(3, 3);
			this.labelConnectedMultiplicatorDefinitionSets.Margin = new System.Windows.Forms.Padding(3);
			this.labelConnectedMultiplicatorDefinitionSets.Name = "labelConnectedMultiplicatorDefinitionSets";
			this.labelConnectedMultiplicatorDefinitionSets.Size = new System.Drawing.Size(148, 15);
			this.labelConnectedMultiplicatorDefinitionSets.TabIndex = 1;
			this.labelConnectedMultiplicatorDefinitionSets.Text = "xxConnectedDefinitionSets";
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
			this.gradientPanelHeader.Size = new System.Drawing.Size(944, 62);
			this.gradientPanelHeader.TabIndex = 54;
			// 
			// tableLayoutPanelHeader
			// 
			this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelHeader.ColumnCount = 1;
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 920F));
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
			this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelHeader.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tableLayoutPanelHeader.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
			this.tableLayoutPanelHeader.RowCount = 1;
			this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Size = new System.Drawing.Size(920, 38);
			this.tableLayoutPanelHeader.TabIndex = 0;
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHeader.AutoSize = true;
			this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelHeader.Location = new System.Drawing.Point(3, 6);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
			this.labelHeader.Size = new System.Drawing.Size(197, 25);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "xxManageContracts";
			// 
			// ContractControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.tableLayoutPanelBody);
			this.Controls.Add(this.gradientPanelHeader);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ContractControl";
			this.Size = new System.Drawing.Size(944, 842);
			this.tableLayoutPanelBody.ResumeLayout(false);
			this.tableLayoutPanelSubHeader1.ResumeLayout(false);
			this.tableLayoutPanelSubHeader1.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvContracts)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxDescription)).EndInit();
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanelSubHeader2.ResumeLayout(false);
			this.tableLayoutPanelSubHeader2.PerformLayout();
			this.tableLayoutPanel6.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdjustTimeBankWithPartTimePercentage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdjustTimeBankWithSeasonality)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvEmpTypes)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNegativeDayOff)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownPositiveDayOff)).EndInit();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.tableLayoutPanel7.ResumeLayout(false);
			this.tableLayoutPanel7.PerformLayout();
			this.tableLayoutPanelSubHeader3.ResumeLayout(false);
			this.tableLayoutPanelSubHeader3.PerformLayout();
			this.tableLayoutPanel9.ResumeLayout(false);
			this.tableLayoutPanel9.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
			this.gradientPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxDescription;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel4;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel8;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel2;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel1;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvContracts;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel10;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel5;
        private System.Windows.Forms.Label labelSubHeader1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader2;
        private System.Windows.Forms.Label labelSubHeader2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonNew;
        private Syncfusion.Windows.Forms.ButtonAdv buttonDeleteContract;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel11;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvEmpTypes;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader3;
        private System.Windows.Forms.Label labelSubHeader3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.CheckedListBox checkedListBoxMultiplicatorDefenitionSets;
        private Syncfusion.Windows.Forms.Tools.AutoLabel labelConnectedMultiplicatorDefinitionSets;
        private TimeSpanTextBox textBoxExtAvgWorkTimePerDay;
        private TimeSpanTextBox textBoxExtMaxTimePerWeek;
        private TimeSpanTextBox textBoxExtNightlyRestTime;
        private TimeSpanTextBox textBoxExtWeeklyRestTime;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelInfoAboutChanges;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelChangeInfo;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel6;
        private TimeSpanTextBox timeSpanTextBoxPositiveTolerance;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel3;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel7;
        private TimeSpanTextBox textBoxExMinTimeSchedulePeriod;
        private Controls.TimeSpanTextBox timeSpanTextBoxPlanningMax;
        private Controls.TimeSpanTextBox timeSpanTextBoxPlanningMin;
        private Syncfusion.Windows.Forms.Tools.AutoLabel labelTimeBankMax;
        private Syncfusion.Windows.Forms.Tools.AutoLabel labelTimeBankMin;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelAdjustTimeBankWithPartTimePercentage;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelAdjustTimeBankWithSeasonality;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdjustTimeBankWithPartTimePercentage;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdjustTimeBankWithSeasonality;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel9;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel12;
        private Syncfusion.Windows.Forms.Tools.NumericUpDownExt numericUpDownPositiveDayOff;
        private Syncfusion.Windows.Forms.Tools.NumericUpDownExt numericUpDownNegativeDayOff;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.RadioButton radioButtonFromSchedule;
        private System.Windows.Forms.RadioButton radioButtonFromContract;
        private Controls.TimeSpanTextBox timeSpanTextBoxNegativeTolerance;
        private Syncfusion.Windows.Forms.Tools.AutoLabel AutoLabelFullDayAbsence;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel13;
		private Controls.TimeSpanTextBox textBoxExtMinTimePerWeek;

    }
}
