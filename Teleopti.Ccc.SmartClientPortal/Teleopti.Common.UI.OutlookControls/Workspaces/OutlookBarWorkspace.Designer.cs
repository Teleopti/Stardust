using BaseStackStrip=Teleopti.Common.UI.OutlookControls.Workspaces.BaseStackStrip;
using HeaderStrip=Teleopti.Common.UI.OutlookControls.Workspaces.HeaderStrip;

namespace Teleopti.Common.UI.OutlookControls.Workspaces
{
    partial class OutlookBarWorkspace
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutlookBarWorkspace));
            this.stackStripSplitter = new System.Windows.Forms.SplitContainer();
            this.ContentPanel = new System.Windows.Forms.Panel();
            this.headerStrip2 = new Teleopti.Common.UI.OutlookControls.Workspaces.HeaderStrip();
            this._headerLabel = new System.Windows.Forms.ToolStripLabel();
            this.stackStrip = new Teleopti.Common.UI.OutlookControls.Workspaces.StackStrip();
            this.overflowStrip = new Teleopti.Common.UI.OutlookControls.Workspaces.BaseStackStrip();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.showMoreButtonsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.showFewerButtonsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stackStripSplitter.Panel1.SuspendLayout();
            this.stackStripSplitter.Panel2.SuspendLayout();
            this.stackStripSplitter.SuspendLayout();
            this.headerStrip2.SuspendLayout();
            this.overflowStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // stackStripSplitter
            // 
            this.stackStripSplitter.AllowDrop = true;
            this.stackStripSplitter.BackColor = System.Drawing.Color.Transparent;
            this.stackStripSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stackStripSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.stackStripSplitter.Location = new System.Drawing.Point(0, 0);
            this.stackStripSplitter.Name = "stackStripSplitter";
            this.stackStripSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // stackStripSplitter.Panel1
            // 
            this.stackStripSplitter.Panel1.Controls.Add(this.ContentPanel);
            this.stackStripSplitter.Panel1.Controls.Add(this.headerStrip2);
            // 
            // stackStripSplitter.Panel2
            // 
            this.stackStripSplitter.Panel2.Controls.Add(this.stackStrip);
            this.stackStripSplitter.Panel2.Controls.Add(this.overflowStrip);
            this.stackStripSplitter.Size = new System.Drawing.Size(341, 505);
            this.stackStripSplitter.SplitterDistance = 291;
            this.stackStripSplitter.SplitterWidth = 7;
            this.stackStripSplitter.TabIndex = 0;
            this.stackStripSplitter.TabStop = false;
            this.stackStripSplitter.Text = "yysplitContainer1";
            this.stackStripSplitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.OnStackStripSplitterMoved);
            this.stackStripSplitter.Paint += new System.Windows.Forms.PaintEventHandler(this.OnStackStripSplitterPaint);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContentPanel.Location = new System.Drawing.Point(0, 25);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(341, 266);
            this.ContentPanel.TabIndex = 2;
            // 
            // headerStrip2
            // 
            this.headerStrip2.AutoSize = false;
            this.headerStrip2.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold);
            this.headerStrip2.ForeColor = System.Drawing.Color.White;
            this.headerStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.headerStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._headerLabel});
            this.headerStrip2.Location = new System.Drawing.Point(0, 0);
            this.headerStrip2.Name = "headerStrip2";
            this.headerStrip2.Size = new System.Drawing.Size(341, 25);
            this.headerStrip2.TabIndex = 1;
            this.headerStrip2.Text = "yyheaderStrip2";
            // 
            // _headerLabel
            // 
            this._headerLabel.Name = "_headerLabel";
            this._headerLabel.Size = new System.Drawing.Size(63, 22);
            this._headerLabel.Text = "Header";
            // 
            // stackStrip
            // 
            this.stackStrip.AutoSize = false;
            this.stackStrip.CanOverflow = false;
            this.stackStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stackStrip.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
            this.stackStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stackStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.stackStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.stackStrip.Location = new System.Drawing.Point(0, 0);
            this.stackStrip.Name = "stackStrip";
            this.stackStrip.Padding = new System.Windows.Forms.Padding(0);
            this.stackStrip.Size = new System.Drawing.Size(341, 9);
            this.stackStrip.TabIndex = 0;
            this.stackStrip.Tag = "Read";
            this.stackStrip.Text = "yystackStrip1";
            // 
            // overflowStrip
            // 
            this.overflowStrip.AutoSize = false;
            this.overflowStrip.CanOverflow = false;
            this.overflowStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.overflowStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.overflowStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton2});
            this.overflowStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.overflowStrip.Location = new System.Drawing.Point(0, 9);
            this.overflowStrip.Name = "overflowStrip";
            this.overflowStrip.Size = new System.Drawing.Size(341, 198);
            this.overflowStrip.TabIndex = 1;
            this.overflowStrip.Text = "yyoverflowStrip";
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showMoreButtonsToolStripMenuItem1,
            this.showFewerButtonsToolStripMenuItem});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Padding = new System.Windows.Forms.Padding(3);
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(19, 198);
            // 
            // showMoreButtonsToolStripMenuItem1
            // 
            this.showMoreButtonsToolStripMenuItem1.Image = global::Teleopti.Common.UI.OutlookControls.Properties.Resources.More;
            this.showMoreButtonsToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Black;
            this.showMoreButtonsToolStripMenuItem1.Name = "showMoreButtonsToolStripMenuItem1";
            this.showMoreButtonsToolStripMenuItem1.Size = new System.Drawing.Size(175, 22);
            this.showMoreButtonsToolStripMenuItem1.Text = "xxShowMorePanels";
            this.showMoreButtonsToolStripMenuItem1.Click += new System.EventHandler(this.showMoreButtonsToolStripMenuItem_Click);
            // 
            // showFewerButtonsToolStripMenuItem
            // 
            this.showFewerButtonsToolStripMenuItem.Image = global::Teleopti.Common.UI.OutlookControls.Properties.Resources.Few;
            this.showFewerButtonsToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.showFewerButtonsToolStripMenuItem.Name = "showFewerButtonsToolStripMenuItem";
            this.showFewerButtonsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.showFewerButtonsToolStripMenuItem.Text = "xxShowLessPanels";
            this.showFewerButtonsToolStripMenuItem.Click += new System.EventHandler(this.showFewerButtonsToolStripMenuItem_Click);
            // 
            // OutlookBarWorkspace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.stackStripSplitter);
            this.Name = "OutlookBarWorkspace";
            this.Size = new System.Drawing.Size(341, 505);
            this.stackStripSplitter.Panel1.ResumeLayout(false);
            this.stackStripSplitter.Panel2.ResumeLayout(false);
            this.stackStripSplitter.ResumeLayout(false);
            this.headerStrip2.ResumeLayout(false);
            this.headerStrip2.PerformLayout();
            this.overflowStrip.ResumeLayout(false);
            this.overflowStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private StackStrip stackStrip;
        private BaseStackStrip overflowStrip;
        private HeaderStrip headerStrip2;
        private System.Windows.Forms.ToolStripLabel _headerLabel;
        private System.Windows.Forms.SplitContainer stackStripSplitter;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripMenuItem showMoreButtonsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem showFewerButtonsToolStripMenuItem;
        private System.Windows.Forms.Panel ContentPanel;


    }
}