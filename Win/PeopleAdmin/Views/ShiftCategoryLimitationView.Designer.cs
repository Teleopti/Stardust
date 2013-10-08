namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
    partial class ShiftCategoryLimitationView
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
                if (components != null)
                    components.Dispose();

                ReleaseManagedResources();
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
            this.tabControlAdv1 = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
            this.tabPageAdv1 = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.gridLimitatation = new Syncfusion.Windows.Forms.Grid.GridControl();
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdv1)).BeginInit();
            this.tabControlAdv1.SuspendLayout();
            this.tabPageAdv1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLimitatation)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlAdv1
            // 
            this.tabControlAdv1.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabControlAdv1.Controls.Add(this.tabPageAdv1);
            this.tabControlAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlAdv1.Location = new System.Drawing.Point(0, 0);
            this.tabControlAdv1.Name = "tabControlAdv1";
            this.tabControlAdv1.Size = new System.Drawing.Size(365, 497);
            this.tabControlAdv1.TabGap = 10;
            this.tabControlAdv1.TabIndex = 1;
            this.tabControlAdv1.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
            this.tabControlAdv1.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
            // 
            // tabPageAdv1
            // 
            this.tabPageAdv1.BackColor = System.Drawing.SystemColors.Window;
            this.tabPageAdv1.Controls.Add(this.gridLimitatation);
            this.tabPageAdv1.Image = null;
            this.tabPageAdv1.ImageSize = new System.Drawing.Size(16, 16);
            this.tabPageAdv1.Location = new System.Drawing.Point(1, 22);
            this.tabPageAdv1.Name = "tabPageAdv1";
            this.tabPageAdv1.Size = new System.Drawing.Size(362, 473);
            this.tabPageAdv1.TabIndex = 1;
            this.tabPageAdv1.Text = "xxShiftCategoryLimitations";
            this.tabPageAdv1.ThemesEnabled = false;
            // 
            // gridLimitatation
            // 
            this.gridLimitatation.ColCount = 4;
            this.gridLimitatation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLimitatation.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridLimitatation.Location = new System.Drawing.Point(0, 0);
            this.gridLimitatation.Name = "gridLimitatation";
            this.gridLimitatation.Office2007ScrollBars = true;
            this.gridLimitatation.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridLimitatation.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridLimitatation.Size = new System.Drawing.Size(362, 473);
            this.gridLimitatation.SmartSizeBox = false;
            this.gridLimitatation.TabIndex = 1;
            this.gridLimitatation.Text = "gridControl1";
            this.gridLimitatation.ThemesEnabled = true;
            this.gridLimitatation.UseRightToLeftCompatibleTextBox = true;
            this.gridLimitatation.SaveCellInfo += new Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventHandler(this.gridLimitatation_SaveCellInfo);
            this.gridLimitatation.CheckBoxClick += new Syncfusion.Windows.Forms.Grid.GridCellClickEventHandler(this.gridLimitatation_CheckBoxClick);
            // 
            // ShiftCategoryLimitationView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlAdv1);
            this.Name = "ShiftCategoryLimitationView";
            this.Size = new System.Drawing.Size(365, 497);
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdv1)).EndInit();
            this.tabControlAdv1.ResumeLayout(false);
            this.tabPageAdv1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridLimitatation)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdv1;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdv1;
        private Syncfusion.Windows.Forms.Grid.GridControl gridLimitatation;
    }
}
