namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class EditControl
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
			this.toolStripButtonNew = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripButtonDelete = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripPanelItem1 = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripEx1 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripEx1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripButtonNew
			// 
			this.toolStripButtonNew.DropDownButtonWidth = 15;
			this.toolStripButtonNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Add;
			this.toolStripButtonNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonNew.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonNew.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
			this.toolStripButtonNew.Name = "toolStripButtonNew";
			this.toolStripButtonNew.Size = new System.Drawing.Size(93, 36);
			this.toolStripButtonNew.Text = "xxNew";
			this.toolStripButtonNew.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripButtonDelete
			// 
			this.toolStripButtonDelete.DropDownButtonWidth = 15;
			this.toolStripButtonDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripButtonDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonDelete.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDelete.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripButtonDelete.Name = "toolStripButtonDelete";
			this.toolStripButtonDelete.Size = new System.Drawing.Size(102, 36);
			this.toolStripButtonDelete.Text = "xxDelete";
			this.toolStripButtonDelete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripPanelItem1
			// 
			this.toolStripPanelItem1.CausesValidation = false;
			this.toolStripPanelItem1.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItem1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNew,
            this.toolStripButtonDelete});
			this.toolStripPanelItem1.Margin = new System.Windows.Forms.Padding(0);
			this.toolStripPanelItem1.Name = "toolStripPanelItem1";
			this.toolStripPanelItem1.Padding = new System.Windows.Forms.Padding(0);
			this.toolStripPanelItem1.Size = new System.Drawing.Size(102, 75);
			this.toolStripPanelItem1.Text = "toolStripPanelItem1";
			this.toolStripPanelItem1.Transparent = true;
			// 
			// toolStripEx1
			// 
			this.toolStripEx1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripEx1.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx1.Image = null;
			this.toolStripEx1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.toolStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItem1});
			this.toolStripEx1.Location = new System.Drawing.Point(0, 0);
			this.toolStripEx1.Margin = new System.Windows.Forms.Padding(0);
			this.toolStripEx1.Name = "toolStripEx1";
			this.toolStripEx1.Office12Mode = false;
			this.toolStripEx1.ShowCaption = false;
			this.toolStripEx1.ShowLauncher = false;
			this.toolStripEx1.Size = new System.Drawing.Size(136, 75);
			this.toolStripEx1.TabIndex = 0;
			this.toolStripEx1.Text = "toolStripEx1";
			this.toolStripEx1.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// EditControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.toolStripEx1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "EditControl";
			this.Size = new System.Drawing.Size(136, 75);
			this.toolStripEx1.ResumeLayout(false);
			this.toolStripEx1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripSplitButton toolStripButtonNew;
        private System.Windows.Forms.ToolStripSplitButton toolStripButtonDelete;
        private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItem1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx1;

    }
}
