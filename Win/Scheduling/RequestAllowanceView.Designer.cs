using System.Globalization;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Scheduling
{
    partial class RequestAllowanceView
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxRefresh")]
        private void InitializeComponent()
        {
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle1 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle2 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RequestAllowanceView));
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.gradientPanelBackground = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.buttonRefresh = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelBudgetGroup = new System.Windows.Forms.Label();
			this.labelAllowance = new System.Windows.Forms.Label();
			this.radioButtonTotalAllowance = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonAllowance = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.comboBoxAdvBudgetGroup = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.requestAllowanceGridControl = new Teleopti.Ccc.Win.Scheduling.RequestAllowanceGridControl();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelBackground)).BeginInit();
			this.gradientPanelBackground.SuspendLayout();
			this.tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTotalAllowance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAllowance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvBudgetGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.requestAllowanceGridControl)).BeginInit();
			this.SuspendLayout();
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.ShowMinimizeButton = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(615, 33);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Startmenu";
			this.ribbonControlAdv1.TabIndex = 1;
			this.ribbonControlAdv1.Text = "xxViewAllowance";
			// 
			// gradientPanelBackground
			// 
			this.gradientPanelBackground.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(252)))), ((int)(((byte)(252))))), System.Drawing.Color.White);
			this.gradientPanelBackground.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelBackground.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelBackground.Controls.Add(this.tableLayoutPanel);
			this.gradientPanelBackground.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanelBackground.Location = new System.Drawing.Point(6, 34);
			this.gradientPanelBackground.Name = "gradientPanelBackground";
			this.gradientPanelBackground.Size = new System.Drawing.Size(605, 220);
			this.gradientPanelBackground.TabIndex = 4;
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.AutoSize = true;
			this.tableLayoutPanel.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel.ColumnCount = 5;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 146F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 169F));
			this.tableLayoutPanel.Controls.Add(this.buttonRefresh, 4, 0);
			this.tableLayoutPanel.Controls.Add(this.labelBudgetGroup, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.labelAllowance, 2, 0);
			this.tableLayoutPanel.Controls.Add(this.radioButtonTotalAllowance, 3, 0);
			this.tableLayoutPanel.Controls.Add(this.radioButtonAllowance, 3, 1);
			this.tableLayoutPanel.Controls.Add(this.comboBoxAdvBudgetGroup, 1, 0);
			this.tableLayoutPanel.Controls.Add(this.requestAllowanceGridControl, 0, 3);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 3;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
			this.tableLayoutPanel.Size = new System.Drawing.Size(605, 220);
			this.tableLayoutPanel.TabIndex = 0;
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonRefresh.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonRefresh.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.buttonRefresh.Location = new System.Drawing.Point(457, 13);
			this.buttonRefresh.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.buttonRefresh.MinimumSize = new System.Drawing.Size(85, 24);
			this.buttonRefresh.Name = "buttonRefresh";
			this.tableLayoutPanel.SetRowSpan(this.buttonRefresh, 2);
			this.buttonRefresh.Size = new System.Drawing.Size(85, 24);
			this.buttonRefresh.TabIndex = 4;
			this.buttonRefresh.Text = "xxRefresh";
			this.buttonRefresh.UseVisualStyle = true;
			this.buttonRefresh.Click += new System.EventHandler(this.buttonRefreshClick);
			// 
			// labelBudgetGroup
			// 
			this.labelBudgetGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelBudgetGroup.AutoSize = true;
			this.labelBudgetGroup.Location = new System.Drawing.Point(3, 0);
			this.labelBudgetGroup.Name = "labelBudgetGroup";
			this.tableLayoutPanel.SetRowSpan(this.labelBudgetGroup, 2);
			this.labelBudgetGroup.Size = new System.Drawing.Size(84, 50);
			this.labelBudgetGroup.TabIndex = 0;
			this.labelBudgetGroup.Text = "xxBudgetGroupColon";
			this.labelBudgetGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAllowance
			// 
			this.labelAllowance.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelAllowance.AutoSize = true;
			this.labelAllowance.Location = new System.Drawing.Point(239, 0);
			this.labelAllowance.Name = "labelAllowance";
			this.tableLayoutPanel.SetRowSpan(this.labelAllowance, 2);
			this.labelAllowance.Size = new System.Drawing.Size(84, 50);
			this.labelAllowance.TabIndex = 3;
			this.labelAllowance.Text = "xxAllowanceColon";
			this.labelAllowance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// radioButtonTotalAllowance
			// 
			this.radioButtonTotalAllowance.Location = new System.Drawing.Point(329, 3);
			this.radioButtonTotalAllowance.Name = "radioButtonTotalAllowance";
			this.radioButtonTotalAllowance.Size = new System.Drawing.Size(122, 19);
			this.radioButtonTotalAllowance.TabIndex = 1;
			this.radioButtonTotalAllowance.Text = "xxTotalAllowance";
			this.radioButtonTotalAllowance.ThemesEnabled = false;
			this.radioButtonTotalAllowance.CheckChanged += new System.EventHandler(this.radioButtonTotalAllowanceCheckChanged);
			// 
			// radioButtonAllowance
			// 
			this.radioButtonAllowance.Location = new System.Drawing.Point(329, 28);
			this.radioButtonAllowance.Name = "radioButtonAllowance";
			this.radioButtonAllowance.Size = new System.Drawing.Size(122, 19);
			this.radioButtonAllowance.TabIndex = 2;
			this.radioButtonAllowance.Text = "xxAllowance";
			this.radioButtonAllowance.ThemesEnabled = false;
			this.radioButtonAllowance.CheckChanged += new System.EventHandler(this.radioButtonAllowanceCheckChanged);
			// 
			// comboBoxAdvBudgetGroup
			// 
			this.comboBoxAdvBudgetGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxAdvBudgetGroup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdvBudgetGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvBudgetGroup.Location = new System.Drawing.Point(93, 14);
			this.comboBoxAdvBudgetGroup.Name = "comboBoxAdvBudgetGroup";
			this.tableLayoutPanel.SetRowSpan(this.comboBoxAdvBudgetGroup, 2);
			this.comboBoxAdvBudgetGroup.Size = new System.Drawing.Size(140, 21);
			this.comboBoxAdvBudgetGroup.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdvBudgetGroup.TabIndex = 0;
			this.comboBoxAdvBudgetGroup.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvBudgetGroupSelectedIndexChanged);
			// 
			// requestAllowanceGridControl
			// 
			this.requestAllowanceGridControl.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.DblClickOnCell;
			this.requestAllowanceGridControl.BackColor = System.Drawing.Color.White;
			this.requestAllowanceGridControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tableLayoutPanel.SetColumnSpan(this.requestAllowanceGridControl, 5);
			this.requestAllowanceGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.requestAllowanceGridControl.ExcelLikeCurrentCell = true;
			this.requestAllowanceGridControl.ExcelLikeSelectionFrame = true;
			this.requestAllowanceGridControl.GridLineColor = System.Drawing.SystemColors.GrayText;
			this.requestAllowanceGridControl.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.requestAllowanceGridControl.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.requestAllowanceGridControl.HorizontalThumbTrack = true;
			this.requestAllowanceGridControl.Location = new System.Drawing.Point(3, 58);
			this.requestAllowanceGridControl.Name = "requestAllowanceGridControl";
			this.requestAllowanceGridControl.Office2007ScrollBars = true;
			this.requestAllowanceGridControl.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
			this.requestAllowanceGridControl.Properties.BackgroundColor = System.Drawing.Color.White;
			gridRangeStyle1.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cell(0, 1);
			gridRangeStyle1.StyleInfo.CellType = "PushButton";
			gridRangeStyle1.StyleInfo.Description = "<";
			gridRangeStyle2.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cell(0, 7);
			gridRangeStyle2.StyleInfo.CellType = "PushButton";
			gridRangeStyle2.StyleInfo.Description = ">";
			this.requestAllowanceGridControl.RangeStyles.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeStyle[] {
            gridRangeStyle1,
            gridRangeStyle2});
			this.requestAllowanceGridControl.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.requestAllowanceGridControl.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.requestAllowanceGridControl.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.requestAllowanceGridControl.Size = new System.Drawing.Size(617, 159);
			this.requestAllowanceGridControl.SmartSizeBox = false;
			this.requestAllowanceGridControl.TabIndex = 3;
			this.requestAllowanceGridControl.ThemesEnabled = true;
			this.requestAllowanceGridControl.UseRightToLeftCompatibleTextBox = true;
			this.requestAllowanceGridControl.VerticalThumbTrack = true;
			// 
			// RequestAllowanceView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(617, 260);
			this.Controls.Add(this.gradientPanelBackground);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "RequestAllowanceView";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxViewAllowance";
			this.Load += new System.EventHandler(this.requestAllowanceViewLoad);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelBackground)).EndInit();
			this.gradientPanelBackground.ResumeLayout(false);
			this.gradientPanelBackground.PerformLayout();
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTotalAllowance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAllowance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvBudgetGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.requestAllowanceGridControl)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelBackground;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Label labelBudgetGroup;
        private System.Windows.Forms.Label labelAllowance;
        private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonTotalAllowance;
        private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAllowance;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvBudgetGroup;
        private RequestAllowanceGridControl requestAllowanceGridControl;
        private Syncfusion.Windows.Forms.ButtonAdv buttonRefresh;

    }
}