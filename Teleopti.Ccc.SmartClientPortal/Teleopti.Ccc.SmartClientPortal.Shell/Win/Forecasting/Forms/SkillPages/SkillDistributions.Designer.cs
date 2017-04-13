
namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
	partial class SkillDistributions
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param pageName="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
			this.panelMain = new System.Windows.Forms.Panel();
			this.tableLayoutPanelMainRtl = new System.Windows.Forms.TableLayoutPanel();
			this.labelEfficiency = new System.Windows.Forms.Label();
			this.labelShrinkage = new System.Windows.Forms.Label();
			this.labelMaximumOccupancy = new System.Windows.Forms.Label();
			this.labelServiceLevelPercentage = new System.Windows.Forms.Label();
			this.labelServiceLevelTarget = new System.Windows.Forms.Label();
			this.integerTextBoxServiceLevelSeconds = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
			this.labelMinimumOccupancy = new System.Windows.Forms.Label();
			this.panelMoreOptions = new System.Windows.Forms.Panel();
			this.tableLayoutPanelMoreOptionsRtl = new System.Windows.Forms.TableLayoutPanel();
			this.labelMinimumAgents = new System.Windows.Forms.Label();
			this.integerTextBoxMaximumAgents = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
			this.integerTextBoxMinimumAgents = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
			this.labelMaximumAgents = new System.Windows.Forms.Label();
			this.panelMoreOrLess = new System.Windows.Forms.Panel();
			this.moreOrLessButtonOptions = new Teleopti.Ccc.Win.Common.MoreOrLessButton();
			this.serviceLevelPercentTextBox = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
			this.minimumOccupancyPercentTextBox = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
			this.shrinkagePercentTextBox = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
			this.maximunOccupancyPercentTextBox = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
			this.efficiencyPercentTextBox = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
			this.panelMain.SuspendLayout();
			this.tableLayoutPanelMainRtl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.integerTextBoxServiceLevelSeconds)).BeginInit();
			this.panelMoreOptions.SuspendLayout();
			this.tableLayoutPanelMoreOptionsRtl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMaximumAgents)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMinimumAgents)).BeginInit();
			this.SuspendLayout();
			// 
			// panelMain
			// 
			this.panelMain.Controls.Add(this.tableLayoutPanelMainRtl);
			this.panelMain.Location = new System.Drawing.Point(0, 0);
			this.panelMain.Margin = new System.Windows.Forms.Padding(0);
			this.panelMain.Name = "panelMain";
			this.panelMain.Size = new System.Drawing.Size(355, 235);
			this.panelMain.TabIndex = 0;
			this.panelMain.Paint += new System.Windows.Forms.PaintEventHandler(this.panelMain_Paint);
			// 
			// tableLayoutPanelMainRtl
			// 
			this.tableLayoutPanelMainRtl.ColumnCount = 2;
			this.tableLayoutPanelMainRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.tableLayoutPanelMainRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.tableLayoutPanelMainRtl.Controls.Add(this.labelEfficiency, 0, 5);
			this.tableLayoutPanelMainRtl.Controls.Add(this.labelShrinkage, 0, 4);
			this.tableLayoutPanelMainRtl.Controls.Add(this.serviceLevelPercentTextBox, 1, 0);
			this.tableLayoutPanelMainRtl.Controls.Add(this.minimumOccupancyPercentTextBox, 1, 2);
			this.tableLayoutPanelMainRtl.Controls.Add(this.shrinkagePercentTextBox, 1, 4);
			this.tableLayoutPanelMainRtl.Controls.Add(this.maximunOccupancyPercentTextBox, 1, 3);
			this.tableLayoutPanelMainRtl.Controls.Add(this.labelMaximumOccupancy, 0, 3);
			this.tableLayoutPanelMainRtl.Controls.Add(this.labelServiceLevelPercentage, 0, 0);
			this.tableLayoutPanelMainRtl.Controls.Add(this.labelServiceLevelTarget, 0, 1);
			this.tableLayoutPanelMainRtl.Controls.Add(this.integerTextBoxServiceLevelSeconds, 1, 1);
			this.tableLayoutPanelMainRtl.Controls.Add(this.labelMinimumOccupancy, 0, 2);
			this.tableLayoutPanelMainRtl.Controls.Add(this.efficiencyPercentTextBox, 1, 5);
			this.tableLayoutPanelMainRtl.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelMainRtl.Name = "tableLayoutPanelMainRtl";
			this.tableLayoutPanelMainRtl.RowCount = 6;
			this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanelMainRtl.Size = new System.Drawing.Size(339, 223);
			this.tableLayoutPanelMainRtl.TabIndex = 12;
			// 
			// labelEfficiency
			// 
			this.labelEfficiency.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelEfficiency.AutoSize = true;
			this.labelEfficiency.Location = new System.Drawing.Point(3, 196);
			this.labelEfficiency.Margin = new System.Windows.Forms.Padding(3);
			this.labelEfficiency.Name = "labelEfficiency";
			this.labelEfficiency.Size = new System.Drawing.Size(68, 15);
			this.labelEfficiency.TabIndex = 20;
			this.labelEfficiency.Text = "xxEfficiency";
			// 
			// labelShrinkage
			// 
			this.labelShrinkage.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelShrinkage.AutoSize = true;
			this.labelShrinkage.Location = new System.Drawing.Point(3, 159);
			this.labelShrinkage.Margin = new System.Windows.Forms.Padding(3);
			this.labelShrinkage.Name = "labelShrinkage";
			this.labelShrinkage.Size = new System.Drawing.Size(69, 15);
			this.labelShrinkage.TabIndex = 18;
			this.labelShrinkage.Text = "xxShrinkage";
			this.labelShrinkage.Click += new System.EventHandler(this.labelShrinkage_Click);
			// 
			// labelMaximumOccupancy
			// 
			this.labelMaximumOccupancy.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelMaximumOccupancy.AutoSize = true;
			this.labelMaximumOccupancy.Location = new System.Drawing.Point(3, 122);
			this.labelMaximumOccupancy.Margin = new System.Windows.Forms.Padding(3);
			this.labelMaximumOccupancy.Name = "labelMaximumOccupancy";
			this.labelMaximumOccupancy.Size = new System.Drawing.Size(131, 15);
			this.labelMaximumOccupancy.TabIndex = 16;
			this.labelMaximumOccupancy.Text = "xxMaximumOccupancy";
			this.labelMaximumOccupancy.Click += new System.EventHandler(this.labelMaximumOccupancy_Click);
			// 
			// labelServiceLevelPercentage
			// 
			this.labelServiceLevelPercentage.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelServiceLevelPercentage.AutoSize = true;
			this.labelServiceLevelPercentage.Location = new System.Drawing.Point(3, 11);
			this.labelServiceLevelPercentage.Margin = new System.Windows.Forms.Padding(3);
			this.labelServiceLevelPercentage.Name = "labelServiceLevelPercentage";
			this.labelServiceLevelPercentage.Size = new System.Drawing.Size(204, 15);
			this.labelServiceLevelPercentage.TabIndex = 4;
			this.labelServiceLevelPercentage.Text = "xxServiceLevelParenthesisPercentSign";
			this.labelServiceLevelPercentage.Click += new System.EventHandler(this.labelServiceLevelPercentage_Click);
			// 
			// labelServiceLevelTarget
			// 
			this.labelServiceLevelTarget.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelServiceLevelTarget.AutoSize = true;
			this.labelServiceLevelTarget.Location = new System.Drawing.Point(3, 48);
			this.labelServiceLevelTarget.Margin = new System.Windows.Forms.Padding(3);
			this.labelServiceLevelTarget.Name = "labelServiceLevelTarget";
			this.labelServiceLevelTarget.Size = new System.Drawing.Size(147, 15);
			this.labelServiceLevelTarget.TabIndex = 6;
			this.labelServiceLevelTarget.Text = "xxServiceLevelSParenthesis";
			this.labelServiceLevelTarget.Click += new System.EventHandler(this.labelServiceLevelTarget_Click);
			// 
			// integerTextBoxServiceLevelSeconds
			// 
			this.integerTextBoxServiceLevelSeconds.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.integerTextBoxServiceLevelSeconds.BackGroundColor = System.Drawing.SystemColors.Window;
			this.integerTextBoxServiceLevelSeconds.BeforeTouchSize = new System.Drawing.Size(59, 20);
			this.integerTextBoxServiceLevelSeconds.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.integerTextBoxServiceLevelSeconds.IntegerValue = ((long)(1));
			this.integerTextBoxServiceLevelSeconds.Location = new System.Drawing.Point(230, 44);
			this.integerTextBoxServiceLevelSeconds.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.integerTextBoxServiceLevelSeconds.MaxValue = ((long)(36000));
			this.integerTextBoxServiceLevelSeconds.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.integerTextBoxServiceLevelSeconds.MinValue = ((long)(1));
			this.integerTextBoxServiceLevelSeconds.Name = "integerTextBoxServiceLevelSeconds";
			this.integerTextBoxServiceLevelSeconds.NullString = "0";
			this.integerTextBoxServiceLevelSeconds.OnValidationFailed = Syncfusion.Windows.Forms.Tools.OnValidationFailed.KeepFocus;
			this.integerTextBoxServiceLevelSeconds.OverflowIndicatorToolTipText = null;
			this.integerTextBoxServiceLevelSeconds.Size = new System.Drawing.Size(68, 23);
			this.integerTextBoxServiceLevelSeconds.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.integerTextBoxServiceLevelSeconds.TabIndex = 15;
			this.integerTextBoxServiceLevelSeconds.Text = "1";
			// 
			// labelMinimumOccupancy
			// 
			this.labelMinimumOccupancy.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelMinimumOccupancy.AutoSize = true;
			this.labelMinimumOccupancy.Location = new System.Drawing.Point(3, 85);
			this.labelMinimumOccupancy.Margin = new System.Windows.Forms.Padding(3);
			this.labelMinimumOccupancy.Name = "labelMinimumOccupancy";
			this.labelMinimumOccupancy.Size = new System.Drawing.Size(130, 15);
			this.labelMinimumOccupancy.TabIndex = 12;
			this.labelMinimumOccupancy.Text = "xxMinimumOccupancy";
			this.labelMinimumOccupancy.Click += new System.EventHandler(this.labelMinimumOccupancy_Click);
			// 
			// panelMoreOptions
			// 
			this.panelMoreOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelMoreOptions.Controls.Add(this.tableLayoutPanelMoreOptionsRtl);
			this.panelMoreOptions.Location = new System.Drawing.Point(0, 238);
			this.panelMoreOptions.Margin = new System.Windows.Forms.Padding(0);
			this.panelMoreOptions.Name = "panelMoreOptions";
			this.panelMoreOptions.Size = new System.Drawing.Size(20, 95);
			this.panelMoreOptions.TabIndex = 16;
			this.panelMoreOptions.Visible = false;
			// 
			// tableLayoutPanelMoreOptionsRtl
			// 
			this.tableLayoutPanelMoreOptionsRtl.ColumnCount = 2;
			this.tableLayoutPanelMoreOptionsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.tableLayoutPanelMoreOptionsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.tableLayoutPanelMoreOptionsRtl.Controls.Add(this.labelMinimumAgents, 0, 0);
			this.tableLayoutPanelMoreOptionsRtl.Controls.Add(this.integerTextBoxMaximumAgents, 1, 1);
			this.tableLayoutPanelMoreOptionsRtl.Controls.Add(this.integerTextBoxMinimumAgents, 1, 0);
			this.tableLayoutPanelMoreOptionsRtl.Controls.Add(this.labelMaximumAgents, 0, 1);
			this.tableLayoutPanelMoreOptionsRtl.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelMoreOptionsRtl.Name = "tableLayoutPanelMoreOptionsRtl";
			this.tableLayoutPanelMoreOptionsRtl.RowCount = 2;
			this.tableLayoutPanelMoreOptionsRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanelMoreOptionsRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanelMoreOptionsRtl.Size = new System.Drawing.Size(327, 81);
			this.tableLayoutPanelMoreOptionsRtl.TabIndex = 18;
			// 
			// labelMinimumAgents
			// 
			this.labelMinimumAgents.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelMinimumAgents.AutoSize = true;
			this.labelMinimumAgents.Location = new System.Drawing.Point(17, 12);
			this.labelMinimumAgents.Margin = new System.Windows.Forms.Padding(17, 0, 3, 0);
			this.labelMinimumAgents.Name = "labelMinimumAgents";
			this.labelMinimumAgents.Size = new System.Drawing.Size(107, 15);
			this.labelMinimumAgents.TabIndex = 8;
			this.labelMinimumAgents.Text = "xxMinimumAgents";
			// 
			// integerTextBoxMaximumAgents
			// 
			this.integerTextBoxMaximumAgents.BackGroundColor = System.Drawing.SystemColors.Window;
			this.integerTextBoxMaximumAgents.BeforeTouchSize = new System.Drawing.Size(59, 20);
			this.integerTextBoxMaximumAgents.IntegerValue = ((long)(1));
			this.integerTextBoxMaximumAgents.Location = new System.Drawing.Point(222, 46);
			this.integerTextBoxMaximumAgents.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
			this.integerTextBoxMaximumAgents.MaxValue = ((long)(100000000));
			this.integerTextBoxMaximumAgents.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.integerTextBoxMaximumAgents.MinValue = ((long)(0));
			this.integerTextBoxMaximumAgents.Name = "integerTextBoxMaximumAgents";
			this.integerTextBoxMaximumAgents.NullString = "0";
			this.integerTextBoxMaximumAgents.OnValidationFailed = Syncfusion.Windows.Forms.Tools.OnValidationFailed.KeepFocus;
			this.integerTextBoxMaximumAgents.OverflowIndicatorToolTipText = null;
			this.integerTextBoxMaximumAgents.Size = new System.Drawing.Size(68, 23);
			this.integerTextBoxMaximumAgents.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.integerTextBoxMaximumAgents.TabIndex = 14;
			this.integerTextBoxMaximumAgents.Text = "1";
			// 
			// integerTextBoxMinimumAgents
			// 
			this.integerTextBoxMinimumAgents.BackGroundColor = System.Drawing.SystemColors.Window;
			this.integerTextBoxMinimumAgents.BeforeTouchSize = new System.Drawing.Size(59, 20);
			this.integerTextBoxMinimumAgents.IntegerValue = ((long)(1));
			this.integerTextBoxMinimumAgents.Location = new System.Drawing.Point(222, 6);
			this.integerTextBoxMinimumAgents.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
			this.integerTextBoxMinimumAgents.MaxValue = ((long)(100000000));
			this.integerTextBoxMinimumAgents.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.integerTextBoxMinimumAgents.MinValue = ((long)(0));
			this.integerTextBoxMinimumAgents.Name = "integerTextBoxMinimumAgents";
			this.integerTextBoxMinimumAgents.NullString = "0";
			this.integerTextBoxMinimumAgents.OnValidationFailed = Syncfusion.Windows.Forms.Tools.OnValidationFailed.KeepFocus;
			this.integerTextBoxMinimumAgents.OverflowIndicatorToolTipText = null;
			this.integerTextBoxMinimumAgents.Size = new System.Drawing.Size(68, 23);
			this.integerTextBoxMinimumAgents.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.integerTextBoxMinimumAgents.TabIndex = 13;
			this.integerTextBoxMinimumAgents.Text = "1";
			// 
			// labelMaximumAgents
			// 
			this.labelMaximumAgents.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelMaximumAgents.AutoSize = true;
			this.labelMaximumAgents.Location = new System.Drawing.Point(17, 53);
			this.labelMaximumAgents.Margin = new System.Windows.Forms.Padding(17, 0, 3, 0);
			this.labelMaximumAgents.Name = "labelMaximumAgents";
			this.labelMaximumAgents.Size = new System.Drawing.Size(108, 15);
			this.labelMaximumAgents.TabIndex = 10;
			this.labelMaximumAgents.Text = "xxMaximumAgents";
			// 
			// panelMoreOrLess
			// 
			this.panelMoreOrLess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelMoreOrLess.Location = new System.Drawing.Point(0, 340);
			this.panelMoreOrLess.Margin = new System.Windows.Forms.Padding(0);
			this.panelMoreOrLess.Name = "panelMoreOrLess";
			this.panelMoreOrLess.Size = new System.Drawing.Size(20, 53);
			this.panelMoreOrLess.TabIndex = 15;
			// 
			// moreOrLessButtonOptions
			// 
			this.moreOrLessButtonOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.moreOrLessButtonOptions.Location = new System.Drawing.Point(-458, 276);
			this.moreOrLessButtonOptions.Margin = new System.Windows.Forms.Padding(0);
			this.moreOrLessButtonOptions.Name = "moreOrLessButtonOptions";
			this.moreOrLessButtonOptions.Size = new System.Drawing.Size(87, 22);
			this.moreOrLessButtonOptions.State = Teleopti.Ccc.Win.Common.MoreOrLessState.Less;
			this.moreOrLessButtonOptions.StateAsBoolean = false;
			this.moreOrLessButtonOptions.TabIndex = 12;
			this.moreOrLessButtonOptions.Visible = false;
			this.moreOrLessButtonOptions.StateChanged += new System.EventHandler<System.EventArgs>(this.moreOrLessButtonOptions_StateChanged);
			// 
			// serviceLevelPercentTextBox
			// 
			this.serviceLevelPercentTextBox.AllowNegativePercentage = false;
			this.serviceLevelPercentTextBox.DefaultValue = 0D;
			this.serviceLevelPercentTextBox.DoubleValue = 0D;
			this.serviceLevelPercentTextBox.ForeColor = System.Drawing.Color.Black;
			this.serviceLevelPercentTextBox.Location = new System.Drawing.Point(230, 6);
			this.serviceLevelPercentTextBox.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
			this.serviceLevelPercentTextBox.Maximum = 100D;
			this.serviceLevelPercentTextBox.Minimum = 1D;
			this.serviceLevelPercentTextBox.Name = "serviceLevelPercentTextBox";
			this.serviceLevelPercentTextBox.Size = new System.Drawing.Size(68, 23);
			this.serviceLevelPercentTextBox.TabIndex = 13;
			this.serviceLevelPercentTextBox.Text = "0%";
			// 
			// minimumOccupancyPercentTextBox
			// 
			this.minimumOccupancyPercentTextBox.AllowNegativePercentage = false;
			this.minimumOccupancyPercentTextBox.DefaultValue = 0D;
			this.minimumOccupancyPercentTextBox.DoubleValue = 0D;
			this.minimumOccupancyPercentTextBox.ForeColor = System.Drawing.Color.Black;
			this.minimumOccupancyPercentTextBox.Location = new System.Drawing.Point(230, 80);
			this.minimumOccupancyPercentTextBox.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
			this.minimumOccupancyPercentTextBox.Maximum = 100D;
			this.minimumOccupancyPercentTextBox.Minimum = 0D;
			this.minimumOccupancyPercentTextBox.Name = "minimumOccupancyPercentTextBox";
			this.minimumOccupancyPercentTextBox.Size = new System.Drawing.Size(68, 23);
			this.minimumOccupancyPercentTextBox.TabIndex = 16;
			this.minimumOccupancyPercentTextBox.Text = "0%";
			// 
			// shrinkagePercentTextBox
			// 
			this.shrinkagePercentTextBox.AcceptsReturn = true;
			this.shrinkagePercentTextBox.AllowNegativePercentage = false;
			this.shrinkagePercentTextBox.DefaultValue = 0D;
			this.shrinkagePercentTextBox.DoubleValue = 0D;
			this.shrinkagePercentTextBox.ForeColor = System.Drawing.Color.Black;
			this.shrinkagePercentTextBox.Location = new System.Drawing.Point(230, 154);
			this.shrinkagePercentTextBox.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
			this.shrinkagePercentTextBox.Maximum = 99D;
			this.shrinkagePercentTextBox.Minimum = 0D;
			this.shrinkagePercentTextBox.Name = "shrinkagePercentTextBox";
			this.shrinkagePercentTextBox.Size = new System.Drawing.Size(68, 23);
			this.shrinkagePercentTextBox.TabIndex = 18;
			this.shrinkagePercentTextBox.Text = "0%";
			// 
			// maximunOccupancyPercentTextBox
			// 
			this.maximunOccupancyPercentTextBox.AllowNegativePercentage = false;
			this.maximunOccupancyPercentTextBox.DefaultValue = 0D;
			this.maximunOccupancyPercentTextBox.DoubleValue = 0D;
			this.maximunOccupancyPercentTextBox.ForeColor = System.Drawing.Color.Black;
			this.maximunOccupancyPercentTextBox.Location = new System.Drawing.Point(230, 117);
			this.maximunOccupancyPercentTextBox.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
			this.maximunOccupancyPercentTextBox.Maximum = 100D;
			this.maximunOccupancyPercentTextBox.Minimum = 0D;
			this.maximunOccupancyPercentTextBox.Name = "maximunOccupancyPercentTextBox";
			this.maximunOccupancyPercentTextBox.Size = new System.Drawing.Size(68, 23);
			this.maximunOccupancyPercentTextBox.TabIndex = 17;
			this.maximunOccupancyPercentTextBox.Text = "0%";
			// 
			// efficiencyPercentTextBox
			// 
			this.efficiencyPercentTextBox.AcceptsReturn = true;
			this.efficiencyPercentTextBox.AllowNegativePercentage = false;
			this.efficiencyPercentTextBox.DefaultValue = 0D;
			this.efficiencyPercentTextBox.DoubleValue = 1D;
			this.efficiencyPercentTextBox.ForeColor = System.Drawing.Color.Black;
			this.efficiencyPercentTextBox.Location = new System.Drawing.Point(230, 191);
			this.efficiencyPercentTextBox.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
			this.efficiencyPercentTextBox.Maximum = 300D;
			this.efficiencyPercentTextBox.Minimum = 1D;
			this.efficiencyPercentTextBox.Name = "efficiencyPercentTextBox";
			this.efficiencyPercentTextBox.Size = new System.Drawing.Size(68, 23);
			this.efficiencyPercentTextBox.TabIndex = 19;
			this.efficiencyPercentTextBox.Text = "100%";
			// 
			// SkillDistributions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.moreOrLessButtonOptions);
			this.Controls.Add(this.panelMain);
			this.Controls.Add(this.panelMoreOptions);
			this.Controls.Add(this.panelMoreOrLess);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "SkillDistributions";
			this.Size = new System.Drawing.Size(359, 400);
			this.panelMain.ResumeLayout(false);
			this.tableLayoutPanelMainRtl.ResumeLayout(false);
			this.tableLayoutPanelMainRtl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.integerTextBoxServiceLevelSeconds)).EndInit();
			this.panelMoreOptions.ResumeLayout(false);
			this.tableLayoutPanelMoreOptionsRtl.ResumeLayout(false);
			this.tableLayoutPanelMoreOptionsRtl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMaximumAgents)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMinimumAgents)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.Label labelServiceLevelTarget;
		private System.Windows.Forms.Label labelServiceLevelPercentage;
		private System.Windows.Forms.Panel panelMoreOrLess;
		private System.Windows.Forms.Panel panelMoreOptions;
		private System.Windows.Forms.Label labelMaximumAgents;
		private System.Windows.Forms.Label labelMinimumAgents;
		private System.Windows.Forms.Label labelMinimumOccupancy;
		private Teleopti.Ccc.Win.Common.MoreOrLessButton moreOrLessButtonOptions;
		private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBoxServiceLevelSeconds;
		private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBoxMaximumAgents;
		private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBoxMinimumAgents;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMainRtl;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMoreOptionsRtl;
		private System.Windows.Forms.Label labelMaximumOccupancy;
		private System.Windows.Forms.Label labelShrinkage;
		private Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox serviceLevelPercentTextBox;
		private Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox minimumOccupancyPercentTextBox;
		private Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox maximunOccupancyPercentTextBox;
		private Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox shrinkagePercentTextBox;
		private System.Windows.Forms.Label labelEfficiency;
		private Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox efficiencyPercentTextBox;
	}
}