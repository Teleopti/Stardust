using System;

namespace Teleopti.Ccc.AgentPortal.Requests.RequestMaster
{
    partial class RequestMasterView
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
            this.gridControlRequestMaster = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.contextMenuStripExRequestGrid = new Syncfusion.Windows.Forms.Tools.ContextMenuStripEx();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlRequestMaster)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControlRequestMaster
            // 
            this.gridControlRequestMaster.AllowSelection = ((Syncfusion.Windows.Forms.Grid.GridSelectionFlags)(((((((Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Row | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Table)
                        | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Cell)
                        | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Multiple)
                        | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Shift)
                        | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Keyboard)
                        | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.AlphaBlend)));
            this.gridControlRequestMaster.ColCount = 7;
            this.gridControlRequestMaster.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
            this.gridControlRequestMaster.ContextMenuStrip = this.contextMenuStripExRequestGrid;
            this.gridControlRequestMaster.DefaultColWidth = 106;
            this.gridControlRequestMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlRequestMaster.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridControlRequestMaster.Location = new System.Drawing.Point(0, 0);
            this.gridControlRequestMaster.Name = "gridControlRequestMaster";
            this.gridControlRequestMaster.NumberedRowHeaders = false;
            this.gridControlRequestMaster.Properties.BackgroundColor = System.Drawing.Color.White;
            this.gridControlRequestMaster.ResizeColsBehavior = ((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior)(((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.ResizeSingle | Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.InsideGrid)
                        | Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.OutlineHeaders)));
            this.gridControlRequestMaster.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
            this.gridControlRequestMaster.SelectCellsMouseButtonsMask = ((System.Windows.Forms.MouseButtons)((System.Windows.Forms.MouseButtons.Left | System.Windows.Forms.MouseButtons.Right)));
            this.gridControlRequestMaster.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlRequestMaster.Size = new System.Drawing.Size(779, 273);
            this.gridControlRequestMaster.SmartSizeBox = false;
            this.gridControlRequestMaster.TabIndex = 0;
            this.gridControlRequestMaster.ThemesEnabled = true;
            this.gridControlRequestMaster.UseRightToLeftCompatibleTextBox = true;
            this.gridControlRequestMaster.CellClick += new Syncfusion.Windows.Forms.Grid.GridCellClickEventHandler(this.gridControlRequestMaster_CellClick);
            this.gridControlRequestMaster.ResizingColumns += new Syncfusion.Windows.Forms.Grid.GridResizingColumnsEventHandler(this.gridControlRequestMaster_ResizingColumns);
            this.gridControlRequestMaster.CellDoubleClick += new Syncfusion.Windows.Forms.Grid.GridCellClickEventHandler(this.gridControlRequestMaster_CellDoubleClick);
            // 
            // contextMenuStripExRequestGrid
            // 
            this.contextMenuStripExRequestGrid.Name = "contextMenuStripExRequestGrid";
            this.contextMenuStripExRequestGrid.Size = new System.Drawing.Size(61, 4);
            this.contextMenuStripExRequestGrid.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuStripExRequestGrid_ItemClicked);
            this.contextMenuStripExRequestGrid.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripExRequestGrid_Opening);
            // 
            // RequestMasterView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControlRequestMaster);
            this.Name = "RequestMasterView";
            this.Size = new System.Drawing.Size(779, 273);
            this.Load += new System.EventHandler(this.RequestMasterView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlRequestMaster)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Grid.GridControl gridControlRequestMaster;
        private Syncfusion.Windows.Forms.Tools.ContextMenuStripEx contextMenuStripExRequestGrid;

    }
}