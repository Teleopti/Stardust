namespace Teleopti.Ccc.Win.Meetings.Overview
{
    partial class ExportMeetingView
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportMeetingView));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.panelExport = new System.Windows.Forms.Panel();
			this.comboBoxScenario = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.label2 = new System.Windows.Forms.Label();
			this.progressBarExporting = new System.Windows.Forms.ProgressBar();
			this.labelExportResult = new System.Windows.Forms.Label();
			this.dateSelectionFromTo1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonClose = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonExport = new Syncfusion.Windows.Forms.ButtonAdv();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.panelExport.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxScenario)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.gradientPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43.84615F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56.15385F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.panelExport, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.progressBarExporting, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelExportResult, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.dateSelectionFromTo1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 26.34731F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 73.65269F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(396, 252);
			this.tableLayoutPanel1.TabIndex = 10;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.label1, 2);
			this.label1.Location = new System.Drawing.Point(10, 10);
			this.label1.Margin = new System.Windows.Forms.Padding(10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(202, 13);
			this.label1.TabIndex = 11;
			this.label1.Text = "xxSelectPeriodAndScenarioToExportTo";
			// 
			// panelExport
			// 
			this.panelExport.BackColor = System.Drawing.Color.Transparent;
			this.panelExport.Controls.Add(this.comboBoxScenario);
			this.panelExport.Controls.Add(this.label2);
			this.panelExport.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelExport.Location = new System.Drawing.Point(173, 39);
			this.panelExport.Margin = new System.Windows.Forms.Padding(0);
			this.panelExport.Name = "panelExport";
			this.panelExport.Size = new System.Drawing.Size(223, 111);
			this.panelExport.TabIndex = 12;
			// 
			// comboBoxScenario
			// 
			this.comboBoxScenario.BackColor = System.Drawing.Color.White;
			this.comboBoxScenario.BeforeTouchSize = new System.Drawing.Size(178, 21);
			this.comboBoxScenario.Location = new System.Drawing.Point(13, 31);
			this.comboBoxScenario.Name = "comboBoxScenario";
			this.comboBoxScenario.Size = new System.Drawing.Size(178, 21);
			this.comboBoxScenario.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxScenario.TabIndex = 9;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 11);
			this.label2.Margin = new System.Windows.Forms.Padding(10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(61, 13);
			this.label2.TabIndex = 11;
			this.label2.Text = "xxScenario";
			// 
			// progressBarExporting
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.progressBarExporting, 2);
			this.progressBarExporting.Dock = System.Windows.Forms.DockStyle.Fill;
			this.progressBarExporting.Location = new System.Drawing.Point(3, 153);
			this.progressBarExporting.Name = "progressBarExporting";
			this.progressBarExporting.Size = new System.Drawing.Size(390, 17);
			this.progressBarExporting.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBarExporting.TabIndex = 13;
			// 
			// labelExportResult
			// 
			this.labelExportResult.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.labelExportResult, 2);
			this.labelExportResult.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelExportResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.labelExportResult.Location = new System.Drawing.Point(0, 173);
			this.labelExportResult.Margin = new System.Windows.Forms.Padding(0);
			this.labelExportResult.Name = "labelExportResult";
			this.labelExportResult.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.labelExportResult.Size = new System.Drawing.Size(396, 29);
			this.labelExportResult.TabIndex = 11;
			// 
			// dateSelectionFromTo1
			// 
			this.dateSelectionFromTo1.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromTo1.ButtonApplyText = "xxApply";
			this.dateSelectionFromTo1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateSelectionFromTo1.HideNoneButtons = false;
			this.dateSelectionFromTo1.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromTo1.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromTo1.Location = new System.Drawing.Point(3, 42);
			this.dateSelectionFromTo1.Name = "dateSelectionFromTo1";
			this.dateSelectionFromTo1.NoneButtonText = "xxNone";
			this.dateSelectionFromTo1.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromTo1.ShowApplyButton = false;
			this.dateSelectionFromTo1.Size = new System.Drawing.Size(167, 105);
			this.dateSelectionFromTo1.TabIndex = 14;
			this.dateSelectionFromTo1.TodayButtonText = "xxToday";
			this.dateSelectionFromTo1.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromTo1.WorkPeriodEnd")));
			this.dateSelectionFromTo1.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromTo1.WorkPeriodStart")));
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55.14019F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.85981F));
			this.tableLayoutPanel2.Controls.Add(this.buttonClose, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonExport, 0, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(176, 205);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(214, 44);
			this.tableLayoutPanel2.TabIndex = 15;
			// 
			// buttonClose
			// 
			this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.buttonClose.BeforeTouchSize = new System.Drawing.Size(76, 24);
			this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonClose.ForeColor = System.Drawing.Color.White;
			this.buttonClose.IsBackStageButton = false;
			this.buttonClose.Location = new System.Drawing.Point(126, 10);
			this.buttonClose.Margin = new System.Windows.Forms.Padding(8, 10, 10, 10);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(76, 24);
			this.buttonClose.TabIndex = 8;
			this.buttonClose.Text = "xxClose";
			this.buttonClose.UseVisualStyle = true;
			this.buttonClose.UseVisualStyleBackColor = true;
			this.buttonClose.Click += new System.EventHandler(this.buttonCloseClick);
			// 
			// buttonExport
			// 
			this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExport.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.buttonExport.BeforeTouchSize = new System.Drawing.Size(76, 24);
			this.buttonExport.ForeColor = System.Drawing.Color.White;
			this.buttonExport.IsBackStageButton = false;
			this.buttonExport.Location = new System.Drawing.Point(37, 10);
			this.buttonExport.Margin = new System.Windows.Forms.Padding(10, 10, 5, 10);
			this.buttonExport.Name = "buttonExport";
			this.buttonExport.Size = new System.Drawing.Size(76, 24);
			this.buttonExport.TabIndex = 9;
			this.buttonExport.Text = "xxExport";
			this.buttonExport.UseVisualStyle = true;
			this.buttonExport.UseVisualStyleBackColor = true;
			this.buttonExport.Click += new System.EventHandler(this.buttonExportClick);
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowContextMenu = false;
			this.ribbonControlAdv1.ShowLauncher = false;
			this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = true;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(398, 33);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "xxStart menu";
			this.ribbonControlAdv1.TabIndex = 7;
			this.ribbonControlAdv1.Text = "xxOpen";
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(250)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(228)))), ((int)(((byte)(242))))));
			this.gradientPanel1.Border3DStyle = System.Windows.Forms.Border3DStyle.Adjust;
			this.gradientPanel1.Controls.Add(this.tableLayoutPanel1);
			this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Size = new System.Drawing.Size(400, 256);
			this.gradientPanel1.TabIndex = 11;
			// 
			// ExportMeetingView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(400, 256);
			this.Controls.Add(this.gradientPanel1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExportMeetingView";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxExportMeetings";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panelExport.ResumeLayout(false);
			this.panelExport.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxScenario)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.gradientPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonClose;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelExport;
		  private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxScenario;
        private System.Windows.Forms.ProgressBar progressBarExporting;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelExportResult;
        private Common.Controls.DateSelection.DateSelectionFromTo dateSelectionFromTo1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonExport;
    }
}