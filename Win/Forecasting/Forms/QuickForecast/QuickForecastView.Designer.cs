namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    partial class QuickForecastView
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickForecastView));
            this.checkedListBoxWorkloads = new System.Windows.Forms.CheckedListBox();
            this.comboBoxScenario = new System.Windows.Forms.ComboBox();
            this.dateSelectionFromToStatistics = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
            this.dateSelectionFromToTarget = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
            this.labelScenario = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBoxUpdateStandardTemplates = new System.Windows.Forms.CheckBox();
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.textBoxResult = new System.Windows.Forms.TextBox();
            this.backgroundWorkerAutoForecast = new System.ComponentModel.BackgroundWorker();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.SuspendLayout();
            // 
            // checkedListBoxWorkloads
            // 
            this.checkedListBoxWorkloads.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBoxWorkloads.FormattingEnabled = true;
            this.checkedListBoxWorkloads.Location = new System.Drawing.Point(3, 16);
            this.checkedListBoxWorkloads.Name = "checkedListBoxWorkloads";
            this.checkedListBoxWorkloads.Size = new System.Drawing.Size(523, 129);
            this.checkedListBoxWorkloads.TabIndex = 1;
            this.checkedListBoxWorkloads.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxWorkloads_ItemCheck);
            // 
            // comboBoxScenario
            // 
            this.comboBoxScenario.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxScenario.FormattingEnabled = true;
            this.comboBoxScenario.Location = new System.Drawing.Point(6, 36);
            this.comboBoxScenario.Name = "comboBoxScenario";
            this.comboBoxScenario.Size = new System.Drawing.Size(161, 21);
            this.comboBoxScenario.TabIndex = 2;
            this.comboBoxScenario.SelectedIndexChanged += new System.EventHandler(this.comboBoxScenario_SelectedIndexChanged);
            // 
            // dateSelectionFromToStatistics
            // 
            this.dateSelectionFromToStatistics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateSelectionFromToStatistics.BackColor = System.Drawing.Color.Transparent;
            this.dateSelectionFromToStatistics.ButtonApplyText = "xxApply";
            this.dateSelectionFromToStatistics.HideNoneButtons = false;
            this.dateSelectionFromToStatistics.LabelDateSelectionText = "xxFrom";
            this.dateSelectionFromToStatistics.LabelDateSelectionToText = "xxTo";
            this.dateSelectionFromToStatistics.Location = new System.Drawing.Point(6, 10);
            this.dateSelectionFromToStatistics.Name = "dateSelectionFromToStatistics";
            this.dateSelectionFromToStatistics.NoneButtonText = "xxNone";
            this.dateSelectionFromToStatistics.NullString = "xxNoDateIsSelected";
            this.dateSelectionFromToStatistics.ShowApplyButton = false;
            this.dateSelectionFromToStatistics.Size = new System.Drawing.Size(163, 129);
            this.dateSelectionFromToStatistics.TabIndex = 3;
            this.dateSelectionFromToStatistics.TodayButtonText = "xxToday";
            this.dateSelectionFromToStatistics.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToStatistics.WorkPeriodEnd")));
            this.dateSelectionFromToStatistics.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToStatistics.WorkPeriodStart")));
            // 
            // dateSelectionFromToTarget
            // 
            this.dateSelectionFromToTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateSelectionFromToTarget.BackColor = System.Drawing.Color.Transparent;
            this.dateSelectionFromToTarget.ButtonApplyText = "xxApply";
            this.dateSelectionFromToTarget.HideNoneButtons = false;
            this.dateSelectionFromToTarget.LabelDateSelectionText = "xxFrom";
            this.dateSelectionFromToTarget.LabelDateSelectionToText = "xxTo";
            this.dateSelectionFromToTarget.Location = new System.Drawing.Point(7, 10);
            this.dateSelectionFromToTarget.Name = "dateSelectionFromToTarget";
            this.dateSelectionFromToTarget.NoneButtonText = "xxNone";
            this.dateSelectionFromToTarget.NullString = "xxNoDateIsSelected";
            this.dateSelectionFromToTarget.ShowApplyButton = false;
            this.dateSelectionFromToTarget.Size = new System.Drawing.Size(163, 129);
            this.dateSelectionFromToTarget.TabIndex = 4;
            this.dateSelectionFromToTarget.TodayButtonText = "xxToday";
            this.dateSelectionFromToTarget.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToTarget.WorkPeriodEnd")));
            this.dateSelectionFromToTarget.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToTarget.WorkPeriodStart")));
            // 
            // labelScenario
            // 
            this.labelScenario.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelScenario.AutoSize = true;
            this.labelScenario.Location = new System.Drawing.Point(6, 17);
            this.labelScenario.Name = "labelScenario";
            this.labelScenario.Size = new System.Drawing.Size(59, 13);
            this.labelScenario.TabIndex = 3;
            this.labelScenario.Text = "xxScenario";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkedListBoxWorkloads);
            this.groupBox1.Location = new System.Drawing.Point(12, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 148);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "xxWorkloads";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dateSelectionFromToStatistics);
            this.groupBox2.Location = new System.Drawing.Point(13, 200);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(173, 117);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "xxStatisticsFromPeriod";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dateSelectionFromToTarget);
            this.groupBox3.Location = new System.Drawing.Point(192, 200);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(173, 117);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "xxTargetPeriod";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.checkBoxUpdateStandardTemplates);
            this.groupBox4.Controls.Add(this.comboBoxScenario);
            this.groupBox4.Controls.Add(this.labelScenario);
            this.groupBox4.Location = new System.Drawing.Point(371, 200);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(170, 117);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "xxOptions";
            // 
            // checkBoxUpdateStandardTemplates
            // 
            this.checkBoxUpdateStandardTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxUpdateStandardTemplates.AutoSize = true;
            this.checkBoxUpdateStandardTemplates.Location = new System.Drawing.Point(6, 64);
            this.checkBoxUpdateStandardTemplates.Name = "checkBoxUpdateStandardTemplates";
            this.checkBoxUpdateStandardTemplates.Size = new System.Drawing.Size(163, 17);
            this.checkBoxUpdateStandardTemplates.TabIndex = 4;
            this.checkBoxUpdateStandardTemplates.Text = "xxUpdateStandardTemplates";
            this.checkBoxUpdateStandardTemplates.UseVisualStyleBackColor = true;
            this.checkBoxUpdateStandardTemplates.CheckedChanged += new System.EventHandler(this.checkBoxUpdateStandardTemplates_CheckedChanged);
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Location = new System.Drawing.Point(466, 324);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 8;
            this.buttonRun.Text = "xxRun";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(385, 324);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "xxCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.textBoxResult);
            this.groupBox5.Location = new System.Drawing.Point(12, 353);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(529, 144);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "xxProgress";
            // 
            // textBoxResult
            // 
            this.textBoxResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxResult.Location = new System.Drawing.Point(3, 16);
            this.textBoxResult.Multiline = true;
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.ReadOnly = true;
            this.textBoxResult.Size = new System.Drawing.Size(523, 125);
            this.textBoxResult.TabIndex = 0;
            // 
            // backgroundWorkerAutoForecast
            // 
            this.backgroundWorkerAutoForecast.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerAutoForecast_DoWork);
            this.backgroundWorkerAutoForecast.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerAutoForecast_RunWorkerCompleted);
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(551, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "xxStartMenu";
            this.ribbonControlAdv1.TabIndex = 11;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // QuickForecastView
            // 
            this.AcceptButton = this.buttonRun;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(553, 509);
            this.Controls.Add(this.ribbonControlAdv1);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "QuickForecastView";
            this.Text = "xxQuickForecast";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBoxWorkloads;
        private System.Windows.Forms.ComboBox comboBoxScenario;
        private Common.Controls.DateSelection.DateSelectionFromTo dateSelectionFromToStatistics;
        private Common.Controls.DateSelection.DateSelectionFromTo dateSelectionFromToTarget;
        private System.Windows.Forms.Label labelScenario;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBoxUpdateStandardTemplates;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox textBoxResult;
        private System.ComponentModel.BackgroundWorker backgroundWorkerAutoForecast;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
    }
}