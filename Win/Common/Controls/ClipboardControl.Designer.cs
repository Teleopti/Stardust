namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class ClipboardControl
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
			this.toolStripPanelItem1 = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripSplitButtonCut = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripSplitButtonCopy = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripSplitButtonPaste = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripEx1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripEx1
			// 
			this.toolStripEx1.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripEx1.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx1.Image = null;
			this.toolStripEx1.ImageScalingSize = new System.Drawing.Size(25, 25);
			this.toolStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItem1});
			this.toolStripEx1.Location = new System.Drawing.Point(0, 0);
			this.toolStripEx1.Margin = new System.Windows.Forms.Padding(0);
			this.toolStripEx1.Name = "toolStripEx1";
			this.toolStripEx1.Office12Mode = false;
			this.toolStripEx1.Padding = new System.Windows.Forms.Padding(0);
			this.toolStripEx1.ShowCaption = false;
			this.toolStripEx1.ShowLauncher = false;
			this.toolStripEx1.Size = new System.Drawing.Size(117, 65);
			this.toolStripEx1.TabIndex = 3;
			this.toolStripEx1.Text = "xxClipboard";
			this.toolStripEx1.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripPanelItem1
			// 
			this.toolStripPanelItem1.BackColor = System.Drawing.Color.Transparent;
			this.toolStripPanelItem1.CausesValidation = false;
			this.toolStripPanelItem1.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItem1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButtonCut,
            this.toolStripSplitButtonCopy,
            this.toolStripSplitButtonPaste});
			this.toolStripPanelItem1.Margin = new System.Windows.Forms.Padding(0);
			this.toolStripPanelItem1.Name = "toolStripPanelItem1";
			this.toolStripPanelItem1.Padding = new System.Windows.Forms.Padding(0);
			this.toolStripPanelItem1.Size = new System.Drawing.Size(84, 65);
			this.toolStripPanelItem1.Text = "yytoolStripPanelItem1";
			this.toolStripPanelItem1.Transparent = true;
			// 
			// toolStripSplitButtonCut
			// 
			this.toolStripSplitButtonCut.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Cut_16x16;
			this.toolStripSplitButtonCut.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonCut.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripSplitButtonCut.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonCut.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
			this.toolStripSplitButtonCut.Name = "toolStripSplitButtonCut";
			this.toolStripSplitButtonCut.Size = new System.Drawing.Size(68, 20);
			this.toolStripSplitButtonCut.Text = "xxCut";
			this.toolStripSplitButtonCut.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonCut.TextChanged += new System.EventHandler(this.toolStripSplitButtonCut_TextChanged);
			// 
			// toolStripSplitButtonCopy
			// 
			this.toolStripSplitButtonCopy.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Copy_16x16;
			this.toolStripSplitButtonCopy.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonCopy.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripSplitButtonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonCopy.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
			this.toolStripSplitButtonCopy.Name = "toolStripSplitButtonCopy";
			this.toolStripSplitButtonCopy.Size = new System.Drawing.Size(77, 20);
			this.toolStripSplitButtonCopy.Text = "xxCopy";
			this.toolStripSplitButtonCopy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonCopy.TextChanged += new System.EventHandler(this.toolStripSplitButtonCopy_TextChanged);
			// 
			// toolStripSplitButtonPaste
			// 
			this.toolStripSplitButtonPaste.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.toolStripSplitButtonPaste.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_paste_16x16;
			this.toolStripSplitButtonPaste.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonPaste.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripSplitButtonPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonPaste.Margin = new System.Windows.Forms.Padding(2, 1, 0, 2);
			this.toolStripSplitButtonPaste.Name = "toolStripSplitButtonPaste";
			this.toolStripSplitButtonPaste.Size = new System.Drawing.Size(82, 20);
			this.toolStripSplitButtonPaste.Text = "xxPaste";
			this.toolStripSplitButtonPaste.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonPaste.TextChanged += new System.EventHandler(this.toolStripSplitButtonPaste_TextChanged);
			// 
			// ClipboardControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.toolStripEx1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "ClipboardControl";
			this.Size = new System.Drawing.Size(117, 65);
			this.toolStripEx1.ResumeLayout(false);
			this.toolStripEx1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonPaste;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonCut;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonCopy;

        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx1;
        private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItem1;
    }
}
