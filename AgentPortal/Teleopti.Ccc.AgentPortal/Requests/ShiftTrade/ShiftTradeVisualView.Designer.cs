namespace Teleopti.Ccc.AgentPortal.Requests.ShiftTrade
{
    partial class ShiftTradeVisualView
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
            this.gridControlVisual = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.contextMenuStripExVisualGrid = new Syncfusion.Windows.Forms.Tools.ContextMenuStripEx();
            this.toolStripMenuItemRemoveDays = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlVisual)).BeginInit();
            this.contextMenuStripExVisualGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridControlVisual
            // 
            this.gridControlVisual.ColCount = 2;
            this.gridControlVisual.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 85),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(1, 120)});
            this.gridControlVisual.ContextMenuStrip = this.contextMenuStripExVisualGrid;
            this.gridControlVisual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlVisual.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridControlVisual.Location = new System.Drawing.Point(0, 0);
            this.gridControlVisual.Name = "gridControlVisual";
            this.gridControlVisual.NumberedColHeaders = false;
            this.gridControlVisual.NumberedRowHeaders = false;
            this.gridControlVisual.ResizeColsBehavior = ((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior)((((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.ResizeSingle | Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.OutlineHeaders)
                        | Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.OutlineBounds)
                        | Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.AllowDragOutside)));
            this.gridControlVisual.RowCount = 1;
            this.gridControlVisual.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlVisual.Size = new System.Drawing.Size(512, 150);
            this.gridControlVisual.SmartSizeBox = false;
            this.gridControlVisual.TabIndex = 0;
            this.gridControlVisual.Text = "gridControlVisual";
            this.gridControlVisual.UseRightToLeftCompatibleTextBox = true;
            // 
            // contextMenuStripExVisualGrid
            // 
            this.contextMenuStripExVisualGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRemoveDays});
            this.contextMenuStripExVisualGrid.Name = "contextMenuStripExVisualGrid";
            this.contextMenuStripExVisualGrid.Size = new System.Drawing.Size(153, 26);
            // 
            // toolStripMenuItemRemoveDays
            // 
            this.toolStripMenuItemRemoveDays.Name = "toolStripMenuItemRemoveDays";
            this.toolStripMenuItemRemoveDays.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItemRemoveDays.Text = "xxRemoveDays";
            this.toolStripMenuItemRemoveDays.Click += new System.EventHandler(this.toolStripMenuItemRemoveDays_Click);
            // 
            // ShiftTradeVisualView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControlVisual);
            this.Name = "ShiftTradeVisualView";
            this.Size = new System.Drawing.Size(512, 150);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlVisual)).EndInit();
            this.contextMenuStripExVisualGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Grid.GridControl gridControlVisual;
        private Syncfusion.Windows.Forms.Tools.ContextMenuStripEx contextMenuStripExVisualGrid;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveDays;
    }
}
