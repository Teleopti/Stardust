using System.Windows.Forms;

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
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PersistConflictView));
			this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnUndo = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnOverWrite = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelInfo = new System.Windows.Forms.Label();
			this.gridControlConflict = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.gridControlConflict)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(733, 407);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.btnCancel.Size = new System.Drawing.Size(125, 28);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "xxCancel";
			this.btnCancel.UseVisualStyle = true;
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnUndo
			// 
			this.btnUndo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnUndo.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.btnUndo.Location = new System.Drawing.Point(584, 407);
			this.btnUndo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnUndo.Name = "btnUndo";
			this.btnUndo.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.btnUndo.Size = new System.Drawing.Size(125, 28);
			this.btnUndo.TabIndex = 2;
			this.btnUndo.Text = "xxOk";
			this.btnUndo.UseVisualStyle = true;
			this.btnUndo.UseVisualStyleBackColor = true;
			this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
			// 
			// btnOverWrite
			// 
			this.btnOverWrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOverWrite.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.btnOverWrite.Location = new System.Drawing.Point(435, 407);
			this.btnOverWrite.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.btnOverWrite.Name = "btnOverWrite";
			this.btnOverWrite.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.btnOverWrite.Size = new System.Drawing.Size(125, 28);
			this.btnOverWrite.TabIndex = 3;
			this.btnOverWrite.Text = "xxOverWrite";
			this.btnOverWrite.UseVisualStyle = true;
			this.btnOverWrite.UseVisualStyleBackColor = true;
			this.btnOverWrite.Click += new System.EventHandler(this.btnOverWrite_Click);
			// 
			// labelInfo
			// 
			this.labelInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelInfo.Location = new System.Drawing.Point(4, 0);
			this.labelInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 10);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(831, 110);
			this.labelInfo.TabIndex = 5;
			this.labelInfo.Text = "xxInfoTextConflict";
			// 
			// gridControlConflict
			// 
			this.gridControlConflict.ColCount = 4;
			this.gridControlConflict.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlConflict.GridLineColor = System.Drawing.SystemColors.GrayText;
			this.gridControlConflict.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridControlConflict.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridControlConflict.Location = new System.Drawing.Point(4, 124);
			this.gridControlConflict.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.gridControlConflict.Name = "gridControlConflict";
			this.gridControlConflict.Office2007ScrollBars = true;
			this.gridControlConflict.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
			this.gridControlConflict.Properties.BackgroundColor = System.Drawing.Color.White;
			this.gridControlConflict.Properties.RowHeaders = false;
			this.gridControlConflict.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlConflict.ShowRowHeaders = false;
			this.gridControlConflict.Size = new System.Drawing.Size(831, 259);
			this.gridControlConflict.SmartSizeBox = false;
			this.gridControlConflict.TabIndex = 0;
			this.gridControlConflict.Text = "gridControlConflict";
			this.gridControlConflict.ThemesEnabled = true;
			this.gridControlConflict.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.gridControlConflict_QueryCellInfo);
			this.gridControlConflict.Resize += new System.EventHandler(this.gridControlConflict_Resize);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.gridControlConflict, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelInfo, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(19, 13);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(839, 387);
			this.tableLayoutPanel1.TabIndex = 7;
			// 
			// PersistConflictView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(871, 454);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.btnOverWrite);
			this.Controls.Add(this.btnUndo);
			this.Controls.Add(this.btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "PersistConflictView";
			this.Text = "xxConflict";
			this.Load += new System.EventHandler(this.PersistConflict_Load);
			((System.ComponentModel.ISupportInitialize)(this.gridControlConflict)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
        private Syncfusion.Windows.Forms.ButtonAdv btnUndo;
        private Syncfusion.Windows.Forms.ButtonAdv btnOverWrite;
		private System.Windows.Forms.Label labelInfo;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlConflict;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

    }
}