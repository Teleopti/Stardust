using System.Windows.Forms;

namespace Teleopti.Common.UI.SmartPartControls.SmartParts
{
    partial class SmartPartBase
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
                UnregisterForMessageBrokerEvents();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SmartPartBase));
			this.toolStripExHeader = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.CloseButton = new System.Windows.Forms.ToolStripButton();
			this.FormTitle = new System.Windows.Forms.ToolStripLabel();
			this.addSmartPartsToolStripMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
			this.replaceSmartPartsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.statusStripExSmartPart = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabelIcon = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBarSmartPart = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripExHeader.SuspendLayout();
			this.statusStripExSmartPart.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripExHeader
			// 
			this.toolStripExHeader.BackColor = System.Drawing.Color.Transparent;
			this.toolStripExHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripExHeader.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExHeader.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExHeader.Image = null;
			this.toolStripExHeader.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.toolStripExHeader.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CloseButton,
            this.FormTitle,
            this.addSmartPartsToolStripMenuItem});
			this.toolStripExHeader.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripExHeader.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.toolStripExHeader.Location = new System.Drawing.Point(0, 0);
			this.toolStripExHeader.Name = "toolStripExHeader";
			this.toolStripExHeader.Office12Mode = false;
			this.toolStripExHeader.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Blue;
			this.toolStripExHeader.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.toolStripExHeader.ShowCaption = false;
			this.toolStripExHeader.ShowLauncher = false;
			this.toolStripExHeader.Size = new System.Drawing.Size(401, 25);
			this.toolStripExHeader.TabIndex = 1;
			this.toolStripExHeader.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// CloseButton
			// 
			this.CloseButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.CloseButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.CloseButton.Image = global::Teleopti.Common.UI.SmartPartControls.Properties.Resources.cancel;
			this.CloseButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(23, 22);
			this.CloseButton.Text = "Close";
			this.CloseButton.Visible = false;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// FormTitle
			// 
			this.FormTitle.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.FormTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.FormTitle.Image = global::Teleopti.Common.UI.SmartPartControls.Properties.Resources.browser;
			this.FormTitle.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.FormTitle.Margin = new System.Windows.Forms.Padding(5, 1, 0, 2);
			this.FormTitle.Name = "FormTitle";
			this.FormTitle.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FormTitle.Size = new System.Drawing.Size(116, 22);
			this.FormTitle.Text = "Smart Part  Title";
			this.FormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// addSmartPartsToolStripMenuItem
			// 
			this.addSmartPartsToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.addSmartPartsToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.addSmartPartsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.replaceSmartPartsToolStripMenuItem});
			this.addSmartPartsToolStripMenuItem.Image = global::Teleopti.Common.UI.SmartPartControls.Properties.Resources.dropdown;
			this.addSmartPartsToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.addSmartPartsToolStripMenuItem.Name = "addSmartPartsToolStripMenuItem";
			this.addSmartPartsToolStripMenuItem.Size = new System.Drawing.Size(29, 22);
			this.addSmartPartsToolStripMenuItem.Visible = false;
			// 
			// replaceSmartPartsToolStripMenuItem
			// 
			this.replaceSmartPartsToolStripMenuItem.Image = global::Teleopti.Common.UI.SmartPartControls.Properties.Resources.undo;
			this.replaceSmartPartsToolStripMenuItem.Name = "replaceSmartPartsToolStripMenuItem";
			this.replaceSmartPartsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.replaceSmartPartsToolStripMenuItem.Text = "Replace Smart Parts";
			// 
			// statusStripExSmartPart
			// 
			this.statusStripExSmartPart.AllowItemReorder = true;
			this.statusStripExSmartPart.BackColor = System.Drawing.Color.White;
			this.statusStripExSmartPart.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelIcon});
			this.statusStripExSmartPart.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.statusStripExSmartPart.Location = new System.Drawing.Point(0, 249);
			this.statusStripExSmartPart.Name = "statusStripExSmartPart";
			this.statusStripExSmartPart.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.statusStripExSmartPart.ShowItemToolTips = true;
			this.statusStripExSmartPart.Size = new System.Drawing.Size(401, 21);
			this.statusStripExSmartPart.SizingGrip = false;
			this.statusStripExSmartPart.TabIndex = 16;
			this.statusStripExSmartPart.Text = "statusStripEx4";
			// 
			// toolStripStatusLabelIcon
			// 
			this.toolStripStatusLabelIcon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripStatusLabelIcon.Image = ((System.Drawing.Image)(resources.GetObject("toolStripStatusLabelIcon.Image")));
			this.toolStripStatusLabelIcon.Name = "toolStripStatusLabelIcon";
			this.toolStripStatusLabelIcon.Size = new System.Drawing.Size(16, 16);
			// 
			// toolStripProgressBarSmartPart
			// 
			this.toolStripProgressBarSmartPart.Name = "toolStripProgressBarSmartPart";
			this.toolStripProgressBarSmartPart.Size = new System.Drawing.Size(100, 15);
			this.toolStripProgressBarSmartPart.Value = 50;
			// 
			// SmartPartBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.statusStripExSmartPart);
			this.Controls.Add(this.toolStripExHeader);
			this.Name = "SmartPartBase";
			this.Size = new System.Drawing.Size(401, 270);
			this.Load += new System.EventHandler(this.SmartPartBase_Load);
			this.toolStripExHeader.ResumeLayout(false);
			this.toolStripExHeader.PerformLayout();
			this.statusStripExSmartPart.ResumeLayout(false);
			this.statusStripExSmartPart.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExHeader;
        private System.Windows.Forms.ToolStripButton CloseButton;
        private System.Windows.Forms.ToolStripLabel FormTitle;
        private System.Windows.Forms.ToolStripDropDownButton addSmartPartsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceSmartPartsToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStripExSmartPart;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBarSmartPart;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelIcon;

        
    }
}
