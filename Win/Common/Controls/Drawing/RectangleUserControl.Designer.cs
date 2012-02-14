namespace Teleopti.Ccc.Win.Common.Controls.Drawing
{
    partial class RectangleControl
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
            LineDisplayInfo lineDisplayInfo1 = new LineDisplayInfo();
            LineDisplayInfo lineDisplayInfo2 = new LineDisplayInfo();
            LineDisplayInfo lineDisplayInfo3 = new LineDisplayInfo();
            LineDisplayInfo lineDisplayInfo4 = new LineDisplayInfo();
            this.topLine = new LineControl();
            this.bottomLine = new LineControl();
            this.leftLine = new LineControl();
            this.rightLine = new LineControl();
            this.SuspendLayout();
            // 
            // topLine
            // 
            this.topLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.topLine.BackColor = System.Drawing.Color.Transparent;
            lineDisplayInfo1.LeftArrow = false;
            lineDisplayInfo1.LineColor = System.Drawing.SystemColors.ControlText;
            lineDisplayInfo1.Padding = new System.Windows.Forms.Padding(5);
            lineDisplayInfo1.RightArrow = false;
            this.topLine.LineDisplayInfo = lineDisplayInfo1;
            this.topLine.Location = new System.Drawing.Point(0, 0);
            this.topLine.Margin = new System.Windows.Forms.Padding(0);
            this.topLine.MinimumSize = new System.Drawing.Size(10, 10);
            this.topLine.Name = "topLine";
            this.topLine.Padding = new System.Windows.Forms.Padding(5);
            this.topLine.Size = new System.Drawing.Size(50, 10);
            this.topLine.TabIndex = 0;
            // 
            // bottomLine
            // 
            this.bottomLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomLine.BackColor = System.Drawing.Color.Transparent;
            lineDisplayInfo2.LeftArrow = false;
            lineDisplayInfo2.LineColor = System.Drawing.SystemColors.ControlText;
            lineDisplayInfo2.Padding = new System.Windows.Forms.Padding(5);
            lineDisplayInfo2.RightArrow = false;
            this.bottomLine.LineDisplayInfo = lineDisplayInfo2;
            this.bottomLine.Location = new System.Drawing.Point(0, 40);
            this.bottomLine.Margin = new System.Windows.Forms.Padding(0);
            this.bottomLine.MinimumSize = new System.Drawing.Size(10, 10);
            this.bottomLine.Name = "bottomLine";
            this.bottomLine.Padding = new System.Windows.Forms.Padding(5);
            this.bottomLine.Size = new System.Drawing.Size(50, 10);
            this.bottomLine.TabIndex = 1;
            // 
            // leftLine
            // 
            this.leftLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.leftLine.BackColor = System.Drawing.Color.Transparent;
            lineDisplayInfo3.LeftArrow = false;
            lineDisplayInfo3.LineColor = System.Drawing.SystemColors.ControlText;
            lineDisplayInfo3.Padding = new System.Windows.Forms.Padding(5);
            lineDisplayInfo3.RightArrow = false;
            this.leftLine.LineDisplayInfo = lineDisplayInfo3;
            this.leftLine.Location = new System.Drawing.Point(0, 0);
            this.leftLine.Margin = new System.Windows.Forms.Padding(0);
            this.leftLine.MinimumSize = new System.Drawing.Size(10, 10);
            this.leftLine.Name = "leftLine";
            this.leftLine.Padding = new System.Windows.Forms.Padding(5);
            this.leftLine.Size = new System.Drawing.Size(10, 50);
            this.leftLine.TabIndex = 2;
            // 
            // rightLine
            // 
            this.rightLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rightLine.BackColor = System.Drawing.Color.Transparent;
            lineDisplayInfo4.LeftArrow = false;
            lineDisplayInfo4.LineColor = System.Drawing.SystemColors.ControlText;
            lineDisplayInfo4.Padding = new System.Windows.Forms.Padding(5);
            lineDisplayInfo4.RightArrow = false;
            this.rightLine.LineDisplayInfo = lineDisplayInfo4;
            this.rightLine.Location = new System.Drawing.Point(40, 0);
            this.rightLine.Margin = new System.Windows.Forms.Padding(0);
            this.rightLine.MinimumSize = new System.Drawing.Size(10, 10);
            this.rightLine.Name = "rightLine";
            this.rightLine.Padding = new System.Windows.Forms.Padding(5);
            this.rightLine.Size = new System.Drawing.Size(10, 50);
            this.rightLine.TabIndex = 3;
            // 
            // RectangleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.rightLine);
            this.Controls.Add(this.leftLine);
            this.Controls.Add(this.bottomLine);
            this.Controls.Add(this.topLine);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "RectangleControl";
            this.Size = new System.Drawing.Size(50, 50);
            this.ResumeLayout(false);

        }

        #endregion

        private LineControl topLine;
        private LineControl bottomLine;
        private LineControl leftLine;
        private LineControl rightLine;
    }
}
