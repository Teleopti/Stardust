namespace Teleopti.Ccc.Win.Shifts
{
    partial class ShiftsNavigationPanel
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
			this.toolStripEx1 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripEx1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripEx1
			// 
			this.toolStripEx1.BackColor = System.Drawing.Color.White;
			this.toolStripEx1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.toolStripEx1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx1.Image = null;
			this.toolStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripButton1});
			this.toolStripEx1.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripEx1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripEx1.Location = new System.Drawing.Point(0, 0);
			this.toolStripEx1.Margin = new System.Windows.Forms.Padding(3);
			this.toolStripEx1.Name = "toolStripEx1";
			this.toolStripEx1.Office12Mode = false;
			this.toolStripEx1.Padding = new System.Windows.Forms.Padding(12, 0, 1, 0);
			this.toolStripEx1.ShowCaption = false;
			this.toolStripEx1.ShowLauncher = false;
			this.toolStripEx1.Size = new System.Drawing.Size(299, 73);
			this.toolStripEx1.TabIndex = 0;
			this.toolStripEx1.Text = "toolStripEx1";
			this.toolStripEx1.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(285, 17);
			this.toolStripLabel1.Text = "xxActions";
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Open_small;
			this.toolStripButton1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButton1.Size = new System.Drawing.Size(285, 29);
			this.toolStripButton1.Text = "xxOpenShiftsTreeDots";
			this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// ShiftsNavigationPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.toolStripEx1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.Name = "ShiftsNavigationPanel";
			this.Size = new System.Drawing.Size(299, 732);
			this.toolStripEx1.ResumeLayout(false);
			this.toolStripEx1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx1;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripButton toolStripButton1;


	}
}