namespace Teleopti.Ccc.Win.Scheduling
{
    partial class PersistConflictView
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu")]
		private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PersistConflictView));
			this.btnUndo = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnOverWrite = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelInfo = new System.Windows.Forms.Label();
			this.gridControlConflict = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.gridControlConflict)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnUndo
			// 
			this.btnUndo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnUndo.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnUndo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnUndo.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnUndo.ForeColor = System.Drawing.Color.White;
			this.btnUndo.IsBackStageButton = false;
			this.btnUndo.Location = new System.Drawing.Point(603, 459);
			this.btnUndo.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.btnUndo.Name = "btnUndo";
			this.btnUndo.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.btnUndo.Size = new System.Drawing.Size(87, 27);
			this.btnUndo.TabIndex = 2;
			this.btnUndo.Text = "xxOk";
			this.btnUndo.UseVisualStyle = true;
			this.btnUndo.UseVisualStyleBackColor = true;
			this.btnUndo.Click += new System.EventHandler(this.btnUndoClick);
			// 
			// btnOverWrite
			// 
			this.btnOverWrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOverWrite.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnOverWrite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnOverWrite.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnOverWrite.ForeColor = System.Drawing.Color.White;
			this.btnOverWrite.IsBackStageButton = false;
			this.btnOverWrite.Location = new System.Drawing.Point(483, 459);
			this.btnOverWrite.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.btnOverWrite.Name = "btnOverWrite";
			this.btnOverWrite.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.btnOverWrite.Size = new System.Drawing.Size(87, 27);
			this.btnOverWrite.TabIndex = 3;
			this.btnOverWrite.Text = "xxOverWrite";
			this.btnOverWrite.UseVisualStyle = true;
			this.btnOverWrite.UseVisualStyleBackColor = true;
			this.btnOverWrite.Click += new System.EventHandler(this.btnOverWriteClick);
			// 
			// labelInfo
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.labelInfo, 2);
			this.labelInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelInfo.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelInfo.Location = new System.Drawing.Point(3, 0);
			this.labelInfo.Margin = new System.Windows.Forms.Padding(3, 0, 3, 9);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(694, 104);
			this.labelInfo.TabIndex = 5;
			this.labelInfo.Text = "xxInfoTextConflict";
			// 
			// gridControlConflict
			// 
			this.gridControlConflict.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			this.gridControlConflict.ColCount = 4;
			this.tableLayoutPanel1.SetColumnSpan(this.gridControlConflict, 2);
			this.gridControlConflict.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridControlConflict.DefaultRowHeight = 20;
			this.gridControlConflict.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlConflict.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.gridControlConflict.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlConflict.Location = new System.Drawing.Point(3, 116);
			this.gridControlConflict.MetroScrollBars = true;
			this.gridControlConflict.Name = "gridControlConflict";
			this.gridControlConflict.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
			this.gridControlConflict.Properties.BackgroundColor = System.Drawing.Color.White;
			this.gridControlConflict.Properties.ForceImmediateRepaint = false;
			this.gridControlConflict.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridControlConflict.Properties.MarkColHeader = false;
			this.gridControlConflict.Properties.MarkRowHeader = false;
			this.gridControlConflict.Properties.RowHeaders = false;
			this.gridControlConflict.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridControlConflict.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlConflict.Size = new System.Drawing.Size(694, 327);
			this.gridControlConflict.SmartSizeBox = false;
			this.gridControlConflict.TabIndex = 0;
			this.gridControlConflict.Text = "gridControlConflict";
			this.gridControlConflict.ThemesEnabled = true;
			this.gridControlConflict.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.gridControlConflictQueryCellInfo);
			this.gridControlConflict.Resize += new System.EventHandler(this.gridControlConflictResize);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.Controls.Add(this.gridControlConflict, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelInfo, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.btnOverWrite, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnUndo, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 113F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(700, 496);
			this.tableLayoutPanel1.TabIndex = 7;
			// 
			// PersistConflictView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(700, 496);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(204, 40);
			this.Name = "PersistConflictView";
			this.ShowIcon = false;
			this.Text = "xxConflict";
			this.Load += new System.EventHandler(this.persistConflictLoad);
			((System.ComponentModel.ISupportInitialize)(this.gridControlConflict)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

		private Syncfusion.Windows.Forms.ButtonAdv btnUndo;
        private Syncfusion.Windows.Forms.ButtonAdv btnOverWrite;
		private System.Windows.Forms.Label labelInfo;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlConflict;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

    }
}