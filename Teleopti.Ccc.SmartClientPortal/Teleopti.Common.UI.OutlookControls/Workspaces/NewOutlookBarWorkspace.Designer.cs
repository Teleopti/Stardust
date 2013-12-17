namespace Teleopti.Common.UI.OutlookControls.Workspaces
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
			this.groupBarModules = new Syncfusion.Windows.Forms.Tools.GroupBar();
			((System.ComponentModel.ISupportInitialize)(this.groupBarModules)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBarModules
			// 
			this.groupBarModules.AllowDrop = true;
			this.groupBarModules.AnimatedSelection = false;
			this.groupBarModules.BackColor = System.Drawing.Color.White;
			this.groupBarModules.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(146)))), ((int)(((byte)(206)))));
			this.groupBarModules.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.groupBarModules.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBarModules.ExpandedWidth = 359;
			this.groupBarModules.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBarModules.ForeColor = System.Drawing.Color.Black;
			this.groupBarModules.GroupBarItemCursor = System.Windows.Forms.Cursors.Hand;
			this.groupBarModules.GroupBarItemHeight = 32;
			this.groupBarModules.HeaderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(65)))), ((int)(((byte)(140)))));
			this.groupBarModules.Location = new System.Drawing.Point(0, 0);
			this.groupBarModules.Name = "groupBarModules";
			this.groupBarModules.PopupClientSize = new System.Drawing.Size(0, 0);
			this.groupBarModules.ShowItemImageInHeader = true;
			this.groupBarModules.ShowChevron = false;
			this.groupBarModules.Size = new System.Drawing.Size(359, 457);
			this.groupBarModules.StackedMode = true;
			this.groupBarModules.TabIndex = 0;
			this.groupBarModules.TextAlign = Syncfusion.Windows.Forms.Tools.TextAlignment.Left;
			this.groupBarModules.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			// 
			// NewOutlookBarWorkspace
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.groupBarModules);
			this.ForeColor = System.Drawing.Color.Black;
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
