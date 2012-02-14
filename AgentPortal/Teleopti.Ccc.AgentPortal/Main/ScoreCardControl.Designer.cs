namespace Teleopti.Ccc.AgentPortal.Main
{
    partial class ScoreCardControl
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
            this.toolStripScoreCards = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // toolStripScoreCards
            // 
            this.toolStripScoreCards.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Bottom;
            this.toolStripScoreCards.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripScoreCards.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripScoreCards.Image = null;
            this.toolStripScoreCards.Location = new System.Drawing.Point(0, 0);
            this.toolStripScoreCards.Name = "toolStripScoreCards";
            this.toolStripScoreCards.ShowCaption = false;
            this.toolStripScoreCards.ShowLauncher = false;
            this.toolStripScoreCards.Size = new System.Drawing.Size(187, 25);
            this.toolStripScoreCards.TabIndex = 0;
            this.toolStripScoreCards.Text = "xxScorecard ";
            this.toolStripScoreCards.DoubleClick += new System.EventHandler(this.toolStripScoreCards_DoubleClick);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 25);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(187, 113);
            this.webBrowser1.TabIndex = 1;
            // 
            // ScoreCardControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.toolStripScoreCards);
            this.Name = "ScoreCardControl";
            this.Size = new System.Drawing.Size(187, 138);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripScoreCards;
        private System.Windows.Forms.WebBrowser webBrowser1;



    }
}
