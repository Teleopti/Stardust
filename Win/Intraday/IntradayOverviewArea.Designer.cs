namespace Teleopti.Ccc.Win.Intraday
{
    partial class IntradayOverviewArea
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntradayOverviewArea));
            this.dockingManagerOverview = new Syncfusion.Windows.Forms.Tools.DockingManager(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dockingManagerOverview)).BeginInit();
            this.SuspendLayout();
            // 
            // dockingManagerOverview
            // 
            this.dockingManagerOverview.ActiveCaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.dockingManagerOverview.CloseEnabled = false;
            this.dockingManagerOverview.DockLayoutStream = ((System.IO.MemoryStream)(resources.GetObject("dockingManagerOverview.DockLayoutStream")));
            this.dockingManagerOverview.DragProviderStyle = Syncfusion.Windows.Forms.Tools.DragProviderStyle.VS2008;
            this.dockingManagerOverview.HostControl = this;
            this.dockingManagerOverview.InActiveCaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.dockingManagerOverview.ThemesEnabled = true;
            this.dockingManagerOverview.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dockingManagerOverview.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Close, "CloseButton"));
            this.dockingManagerOverview.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Pin, "PinButton"));
            this.dockingManagerOverview.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Menu, "MenuButton"));
            this.dockingManagerOverview.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Maximize, "MaximizeButton"));
            this.dockingManagerOverview.CaptionButtons.Add(new Syncfusion.Windows.Forms.Tools.CaptionButton(Syncfusion.Windows.Forms.Tools.CaptionButtonType.Restore, "RestoreButton"));

  
            // 
            // IntradayOverviewArea
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "IntradayOverviewArea";
            this.Size = new System.Drawing.Size(743, 317);
            ((System.ComponentModel.ISupportInitialize)(this.dockingManagerOverview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.DockingManager dockingManagerOverview;
 
    }
}
