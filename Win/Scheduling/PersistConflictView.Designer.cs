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
            this.panel1 = new System.Windows.Forms.Panel();
            this.gridControlConflict = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlConflict)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(550, 319);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.btnCancel.Size = new System.Drawing.Size(94, 23);
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
            this.btnUndo.Location = new System.Drawing.Point(438, 319);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.btnUndo.Size = new System.Drawing.Size(94, 23);
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
            this.btnOverWrite.Location = new System.Drawing.Point(326, 319);
            this.btnOverWrite.Name = "btnOverWrite";
            this.btnOverWrite.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.btnOverWrite.Size = new System.Drawing.Size(94, 23);
            this.btnOverWrite.TabIndex = 3;
            this.btnOverWrite.Text = "xxOverWrite";
            this.btnOverWrite.UseVisualStyle = true;
            this.btnOverWrite.UseVisualStyleBackColor = true;
            this.btnOverWrite.Click += new System.EventHandler(this.btnOverWrite_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInfo.Location = new System.Drawing.Point(14, 8);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(630, 91);
            this.labelInfo.TabIndex = 5;
            this.labelInfo.Text = "xxInfoTextConflict";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.gridControlConflict);
            this.panel1.Location = new System.Drawing.Point(14, 109);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(630, 202);
            this.panel1.TabIndex = 6;
            // 
            // gridControlConflict
            // 
            this.gridControlConflict.ColCount = 4;
            this.gridControlConflict.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlConflict.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridControlConflict.Location = new System.Drawing.Point(0, 0);
            this.gridControlConflict.Name = "gridControlConflict";
            this.gridControlConflict.Office2007ScrollBars = true;
            this.gridControlConflict.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
            this.gridControlConflict.Properties.BackgroundColor = System.Drawing.Color.White;
            this.gridControlConflict.Properties.RowHeaders = false;
            this.gridControlConflict.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlConflict.Size = new System.Drawing.Size(628, 200);
            this.gridControlConflict.SmartSizeBox = false;
            this.gridControlConflict.TabIndex = 0;
            this.gridControlConflict.Text = "gridControlConflict";
            this.gridControlConflict.ThemesEnabled = true;
            this.gridControlConflict.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.gridControlConflict_QueryCellInfo);
            this.gridControlConflict.Resize += new System.EventHandler(this.gridControlConflict_Resize);
            // 
            // PersistConflictView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(658, 354);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.btnOverWrite);
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PersistConflictView";
            this.Text = "xxConflict";
            this.Load += new System.EventHandler(this.PersistConflict_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlConflict)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
        private Syncfusion.Windows.Forms.ButtonAdv btnUndo;
        private Syncfusion.Windows.Forms.ButtonAdv btnOverWrite;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Panel panel1;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlConflict;

    }
}