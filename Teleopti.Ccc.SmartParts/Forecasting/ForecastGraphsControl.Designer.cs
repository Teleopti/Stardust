using System.Windows.Forms;

namespace Teleopti.Ccc.SmartParts.Forecasting
{
    partial class ForecastGraphsControl
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
            Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle1 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
            Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle2 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
            this.tableLayoutPanelBase = new System.Windows.Forms.TableLayoutPanel();
            this.gridControlProgress = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.gridControlTimeline = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.tableLayoutPanelBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlProgress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlTimeline)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanelBase
            // 
            this.tableLayoutPanelBase.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanelBase.ColumnCount = 2;
            this.tableLayoutPanelBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanelBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBase.Controls.Add(this.gridControlProgress, 0, 1);
            this.tableLayoutPanelBase.Controls.Add(this.gridControlTimeline, 1, 0);
            this.tableLayoutPanelBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelBase.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelBase.Name = "tableLayoutPanelBase";
            this.tableLayoutPanelBase.RowCount = 2;
            this.tableLayoutPanelBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanelBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBase.Size = new System.Drawing.Size(759, 199);
            this.tableLayoutPanelBase.TabIndex = 0;
            // 
            // gridControlProgress
            // 
            this.gridControlProgress.AutoSize = true;
            this.gridControlProgress.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.None;
            this.gridControlProgress.AllowIncreaseSmallChange = false;
            this.gridControlProgress.AllowSelection = Syncfusion.Windows.Forms.Grid.GridSelectionFlags.None;
            this.gridControlProgress.ColCount = 2;
            this.tableLayoutPanelBase.SetColumnSpan(this.gridControlProgress, 2);
            this.gridControlProgress.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(1, 125)});
            this.gridControlProgress.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
            this.gridControlProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlProgress.DragSelectedCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.None;
            this.gridControlProgress.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.gridControlProgress.HScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Disabled;
            this.gridControlProgress.Location = new System.Drawing.Point(0, 17);
            this.gridControlProgress.Margin = new System.Windows.Forms.Padding(0);
            this.gridControlProgress.Name = "gridControlProgress";
            this.gridControlProgress.NumberedColHeaders = false;
            this.gridControlProgress.NumberedRowHeaders = false;
            this.gridControlProgress.Properties.BackgroundColor = System.Drawing.Color.Transparent;
            this.gridControlProgress.Properties.Buttons3D = false;
            this.gridControlProgress.Properties.ColHeaders = false;
            this.gridControlProgress.Properties.DisplayVertLines = false;
            this.gridControlProgress.Properties.FixedLinesColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.gridControlProgress.Properties.GridLineColor = System.Drawing.SystemColors.ButtonFace;
            this.gridControlProgress.Properties.MarkColHeader = false;
            this.gridControlProgress.Properties.MarkRowHeader = false;
            this.gridControlProgress.Properties.PrintColHeader = false;
            this.gridControlProgress.Properties.PrintRowHeader = false;
            this.gridControlProgress.Properties.RowHeaders = false;
            gridRangeStyle1.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Table();
            gridRangeStyle1.StyleInfo.Font.Bold = true;
            gridRangeStyle1.StyleInfo.Font.Facename = "Tahoma";
            gridRangeStyle1.StyleInfo.Font.Italic = false;
            gridRangeStyle1.StyleInfo.Font.Size = 8.25F;
            gridRangeStyle1.StyleInfo.Font.Strikeout = false;
            gridRangeStyle1.StyleInfo.Font.Underline = false;
            gridRangeStyle1.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
            this.gridControlProgress.RangeStyles.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeStyle[] {
            gridRangeStyle1});
            this.gridControlProgress.SmoothControlResize = true;
            this.gridControlProgress.ReadOnly = true;
            this.gridControlProgress.RefreshCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridRefreshCurrentCellBehavior.None;
            this.gridControlProgress.ResizeColsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
            this.gridControlProgress.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
            this.gridControlProgress.RowCount = 5;
            this.gridControlProgress.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
            this.gridControlProgress.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlProgress.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.HideAlways;
            this.gridControlProgress.Size = new System.Drawing.Size(759, 182);
            this.gridControlProgress.SmartSizeBox = false;
            this.gridControlProgress.TabIndex = 12;
            this.gridControlProgress.Text = "gridControlProgress";
            this.gridControlProgress.UseRightToLeftCompatibleTextBox = true;
            this.gridControlProgress.VScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Automatic;
            this.gridControlProgress.HScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Disabled;
            this.gridControlProgress.HScroll = false;
            this.gridControlProgress.CellMouseHover += new Syncfusion.Windows.Forms.Grid.GridCellMouseEventHandler(this.gridControlProgress_CellMouseHover);
            this.gridControlProgress.CellDrawn += new Syncfusion.Windows.Forms.Grid.GridDrawCellEventHandler(this.gridControlProgress_CellDrawn);
            this.gridControlProgress.GridControlMouseDown +=new Syncfusion.Windows.Forms.CancelMouseEventHandler(gridControlProgress_GridControlMouseDown);
            this.gridControlProgress.CellMouseHoverLeave +=new Syncfusion.Windows.Forms.Grid.GridCellMouseEventHandler(gridControlProgress_CellMouseHoverLeave);
            // 
            // gridControlTimeline
            // 
            this.gridControlTimeline.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.None;
            this.gridControlTimeline.AllowIncreaseSmallChange = false;
            this.gridControlTimeline.AllowSelection = Syncfusion.Windows.Forms.Grid.GridSelectionFlags.None;
            this.gridControlTimeline.BackColor = System.Drawing.Color.White;
            this.gridControlTimeline.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridControlTimeline.ColCount = 1;
            this.gridControlTimeline.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.gridControlTimeline.DefaultRowHeight = 14;
            //this.gridControlTimeline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlTimeline.DragSelectedCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.None;
            this.gridControlTimeline.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.gridControlTimeline.HScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Disabled;
            this.gridControlTimeline.Location = new System.Drawing.Point(125, 0);
            this.gridControlTimeline.Margin = new System.Windows.Forms.Padding(0);
            this.gridControlTimeline.Name = "gridControlTimeline";
            this.gridControlTimeline.NumberedColHeaders = false;
            this.gridControlTimeline.NumberedRowHeaders = false;
            this.gridControlTimeline.Properties.BackgroundColor = System.Drawing.Color.Transparent;
            this.gridControlTimeline.Properties.Buttons3D = false;
            this.gridControlTimeline.Properties.ColHeaders = false;
            this.gridControlTimeline.Properties.DisplayHorzLines = false;
            this.gridControlTimeline.Properties.DisplayVertLines = false;
            this.gridControlTimeline.Properties.FixedLinesColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.gridControlTimeline.Properties.GridLineColor = System.Drawing.Color.White;
            this.gridControlTimeline.Properties.MarkColHeader = false;
            this.gridControlTimeline.Properties.MarkRowHeader = false;
            this.gridControlTimeline.Properties.PrintColHeader = false;
            this.gridControlTimeline.Properties.PrintRowHeader = false;
            this.gridControlTimeline.Properties.RowHeaders = false;
            gridRangeStyle2.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Table();
            gridRangeStyle2.StyleInfo.Font.Bold = true;
            gridRangeStyle2.StyleInfo.Font.Facename = "Tahoma";
            gridRangeStyle2.StyleInfo.Font.Italic = false;
            gridRangeStyle2.StyleInfo.Font.Size = 8.25F;
            gridRangeStyle2.StyleInfo.Font.Strikeout = false;
            gridRangeStyle2.StyleInfo.Font.Underline = false;
            gridRangeStyle2.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
            this.gridControlTimeline.RangeStyles.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeStyle[] {
            gridRangeStyle2});
            this.gridControlTimeline.ReadOnly = true;
            this.gridControlTimeline.RefreshCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridRefreshCurrentCellBehavior.None;
            this.gridControlTimeline.ResizeColsBehavior = ((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior)((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.ResizeAll | Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.OutlineBounds)));
            this.gridControlTimeline.ResizeRowsBehavior = ((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior)((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.ResizeAll | Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.OutlineBounds)));
            this.gridControlTimeline.RowCount = 1;
            this.gridControlTimeline.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(1, 12)});
            this.gridControlTimeline.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlTimeline.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.HideAlways;
            this.gridControlTimeline.Size = new System.Drawing.Size(634, 17);
            this.gridControlTimeline.SmartSizeBox = false;
            this.gridControlTimeline.TabIndex = 16;
            this.gridControlTimeline.Text = "gridControlTimeline";
            this.gridControlTimeline.UseRightToLeftCompatibleTextBox = true;
            this.gridControlTimeline.VScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Disabled;
            this.gridControlTimeline.HScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Disabled;
            this.gridControlTimeline.HScroll = false;
            this.gridControlTimeline.CellDrawn += new Syncfusion.Windows.Forms.Grid.GridDrawCellEventHandler(this.gridControlTimeline_CellDrawn);
            // 
            // ForecastGraphsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelBase);
            this.Name = "ForecastGraphsControl";
            this.Size = new System.Drawing.Size(759, 199);
            this.tableLayoutPanelBase.ResumeLayout(false);
            this.HScroll = false;
            this.VScroll = false;
            ((System.ComponentModel.ISupportInitialize)(this.gridControlProgress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlTimeline)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBase;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlProgress;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlTimeline;
    }
}
