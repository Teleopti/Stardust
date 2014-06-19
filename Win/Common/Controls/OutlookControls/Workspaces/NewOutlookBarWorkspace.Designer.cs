using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces
{
    partial class NewOutlookBarWorkspace
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewOutlookBarWorkspace));
			this.groupBarModules = new Syncfusion.Windows.Forms.Tools.GroupBar();
			((System.ComponentModel.ISupportInitialize)(this.groupBarModules)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBarModules
			// 
			this.groupBarModules.AllowDrop = true;
			this.groupBarModules.AnimatedSelection = false;
			this.groupBarModules.ApplyDefaultVisualStyleColor = false;
			this.groupBarModules.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
			this.groupBarModules.BeforeTouchSize = new System.Drawing.Size(359, 457);
			this.groupBarModules.BorderColor = System.Drawing.SystemColors.ControlDark;
			this.groupBarModules.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.groupBarModules.CollapseImage = ((System.Drawing.Image)(resources.GetObject("groupBarModules.CollapseImage")));
			this.groupBarModules.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBarModules.ExpandButtonToolTip = null;
			this.groupBarModules.ExpandedWidth = 359;
			this.groupBarModules.ExpandImage = ((System.Drawing.Image)(resources.GetObject("groupBarModules.ExpandImage")));
			this.groupBarModules.FlatLook = true;
			this.groupBarModules.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBarModules.ForeColor = System.Drawing.Color.Black;
			this.groupBarModules.GroupBarDropDownToolTip = null;
			this.groupBarModules.GroupBarItemCursor = System.Windows.Forms.Cursors.Hand;
			this.groupBarModules.GroupBarItemHeight = 40;
			this.groupBarModules.HeaderBackColor = System.Drawing.SystemColors.Control;
			this.groupBarModules.HeaderFont = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBarModules.HeaderForeColor = System.Drawing.Color.ForestGreen;
			this.groupBarModules.HeaderHeight = 40;
			this.groupBarModules.IndexOnVisibleItems = true;
			this.groupBarModules.Location = new System.Drawing.Point(0, 0);
			this.groupBarModules.MinimizeButtonToolTip = null;
			this.groupBarModules.Name = "groupBarModules";
			this.groupBarModules.NavigationPaneHeight = 34;
			this.groupBarModules.NavigationPaneTooltip = null;
			this.groupBarModules.PopupClientSize = new System.Drawing.Size(0, 0);
			this.groupBarModules.ShowItemImageInHeader = true;
			this.groupBarModules.Size = new System.Drawing.Size(359, 457);
			this.groupBarModules.StackedMode = true;
			this.groupBarModules.TabIndex = 0;
			this.groupBarModules.TextAlign = Syncfusion.Windows.Forms.Tools.TextAlignment.Left;
			this.groupBarModules.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.Default;
			// 
			// NewOutlookBarWorkspace
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.groupBarModules);
			this.ForeColor = System.Drawing.Color.Red;
			this.Name = "NewOutlookBarWorkspace";
			this.Size = new System.Drawing.Size(359, 457);
			this.Load += new System.EventHandler(this.NewOutlookBarWorkspaceLoad);
			((System.ComponentModel.ISupportInitialize)(this.groupBarModules)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private Syncfusion.Windows.Forms.Tools.GroupBar groupBarModules;


	}
}
