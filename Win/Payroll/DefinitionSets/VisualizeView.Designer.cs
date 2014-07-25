namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    partial class VisualizeView
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

                if (_format != null)
                    _format.Dispose();
				
				if(_visualizeToolTip != null)
					_visualizeToolTip.Dispose();

                if (gcProjection != null)
                    gcProjection.Dispose();
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
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.labelMultiplicatorVisualizer = new System.Windows.Forms.Label();
            this.dateNavigateControlThinLayout1 = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateNavigateControlThinLayout();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gcProjection = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcProjection)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel4.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.85714F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.14286F));
            this.tableLayoutPanel4.Controls.Add(this.labelMultiplicatorVisualizer, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.dateNavigateControlThinLayout1, 3, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(814, 29);
            this.tableLayoutPanel4.TabIndex = 19;
            // 
            // labelMultiplicatorVisualizer
            // 
            this.labelMultiplicatorVisualizer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMultiplicatorVisualizer.AutoSize = true;
            this.labelMultiplicatorVisualizer.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMultiplicatorVisualizer.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelMultiplicatorVisualizer.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelMultiplicatorVisualizer.Location = new System.Drawing.Point(3, 7);
            this.labelMultiplicatorVisualizer.Name = "labelMultiplicatorVisualizer";
            this.labelMultiplicatorVisualizer.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelMultiplicatorVisualizer.Size = new System.Drawing.Size(168, 17);
            this.labelMultiplicatorVisualizer.TabIndex = 21;
            this.labelMultiplicatorVisualizer.Text = "xxMultiplicatorVisualizer";
            // 
            // dateNavigateControlThinLayout1
            // 
            this.dateNavigateControlThinLayout1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateNavigateControlThinLayout1.BackColor = System.Drawing.Color.Transparent;
            this.dateNavigateControlThinLayout1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateNavigateControlThinLayout1.Location = new System.Drawing.Point(286, 1);
            this.dateNavigateControlThinLayout1.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.dateNavigateControlThinLayout1.Name = "dateNavigateControlThinLayout1";
            this.dateNavigateControlThinLayout1.Size = new System.Drawing.Size(345, 27);
            this.dateNavigateControlThinLayout1.TabIndex = 22;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.gcProjection, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(820, 127);
            this.tableLayoutPanel1.TabIndex = 20;
            // 
            // gcProjection
            // 
            this.gcProjection.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.None;
            this.gcProjection.AllowSelection = Syncfusion.Windows.Forms.Grid.GridSelectionFlags.None;
            this.gcProjection.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.NotSet;
            this.gcProjection.DefaultRowHeight = 22;
            this.gcProjection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcProjection.Location = new System.Drawing.Point(3, 38);
            this.gcProjection.Name = "gcProjection";
            this.gcProjection.NumberedRowHeaders = false;
            this.gcProjection.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gcProjection.Properties.ForceImmediateRepaint = false;
            this.gcProjection.Properties.MarkColHeader = false;
            this.gcProjection.Properties.MarkRowHeader = false;
            this.gcProjection.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gcProjection.Size = new System.Drawing.Size(814, 86);
            this.gcProjection.SmartSizeBox = false;
            this.gcProjection.TabIndex = 20;
            this.gcProjection.Text = "gridControl1";
            this.gcProjection.UseRightToLeftCompatibleTextBox = true;
            // 
            // VisualizeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "VisualizeView";
            this.Size = new System.Drawing.Size(820, 127);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcProjection)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label labelMultiplicatorVisualizer;
        private Teleopti.Ccc.Win.Common.Controls.DateSelection.DateNavigateControlThinLayout dateNavigateControlThinLayout1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Grid.GridControl gcProjection;
    }
}
