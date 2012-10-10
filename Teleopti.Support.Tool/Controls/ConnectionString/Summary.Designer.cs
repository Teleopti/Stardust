namespace Teleopti.Support.Tool.Controls.ConnectionString
{
    partial class Summary
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
            this.leftFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Tsettings = new System.Windows.Forms.TableLayoutPanel();
            this.resultImage = new System.Windows.Forms.PictureBox();
            this.lResult = new System.Windows.Forms.Label();
            this.log = new System.Windows.Forms.LinkLabel();
            this.labelVerifySettings = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.labelStepsToExecute = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            ((System.ComponentModel.ISupportInitialize)(this.resultImage)).BeginInit();
            this.SuspendLayout();
            // 
            // leftFlowPanel
            // 
            this.leftFlowPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.leftFlowPanel.BackColor = System.Drawing.SystemColors.Window;
            this.leftFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.leftFlowPanel.Location = new System.Drawing.Point(322, 46);
            this.leftFlowPanel.Margin = new System.Windows.Forms.Padding(5);
            this.leftFlowPanel.Name = "leftFlowPanel";
            this.leftFlowPanel.Size = new System.Drawing.Size(376, 167);
            this.leftFlowPanel.TabIndex = 2;
            // 
            // Tsettings
            // 
            this.Tsettings.AutoSize = true;
            this.Tsettings.BackColor = System.Drawing.SystemColors.Window;
            this.Tsettings.ColumnCount = 2;
            this.Tsettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.Tsettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.Tsettings.Location = new System.Drawing.Point(14, 54);
            this.Tsettings.Margin = new System.Windows.Forms.Padding(1);
            this.Tsettings.MaximumSize = new System.Drawing.Size(292, 0);
            this.Tsettings.Name = "Tsettings";
            this.Tsettings.RowCount = 1;
            this.Tsettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.Tsettings.Size = new System.Drawing.Size(292, 0);
            this.Tsettings.TabIndex = 3;
            // 
            // resultImage
            // 
            this.resultImage.ErrorImage = global::Teleopti.Support.Tool.Properties.Resources.accept1;
            this.resultImage.Location = new System.Drawing.Point(47, 266);
            this.resultImage.Name = "resultImage";
            this.resultImage.Size = new System.Drawing.Size(50, 50);
            this.resultImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.resultImage.TabIndex = 5;
            this.resultImage.TabStop = false;
            // 
            // lResult
            // 
            this.lResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lResult.AutoEllipsis = true;
            this.lResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lResult.Location = new System.Drawing.Point(142, 223);
            this.lResult.Margin = new System.Windows.Forms.Padding(5);
            this.lResult.Name = "lResult";
            this.lResult.Size = new System.Drawing.Size(556, 75);
            this.lResult.TabIndex = 52;
            // 
            // log
            // 
            this.log.AutoSize = true;
            this.log.Location = new System.Drawing.Point(142, 303);
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(96, 13);
            this.log.TabIndex = 53;
            this.log.TabStop = true;
            this.log.Text = "View executing log";
            this.log.Visible = false;
            // 
            // labelVerifySettings
            // 
            this.labelVerifySettings.AutoSize = true;
            this.labelVerifySettings.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVerifySettings.Location = new System.Drawing.Point(10, 10);
            this.labelVerifySettings.Margin = new System.Windows.Forms.Padding(10);
            this.labelVerifySettings.Name = "labelVerifySettings";
            this.labelVerifySettings.Size = new System.Drawing.Size(111, 21);
            this.labelVerifySettings.TabIndex = 54;
            this.labelVerifySettings.Text = "Verify Settings";
            this.labelVerifySettings.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // labelStepsToExecute
            // 
            this.labelStepsToExecute.AutoSize = true;
            this.labelStepsToExecute.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStepsToExecute.Location = new System.Drawing.Point(318, 10);
            this.labelStepsToExecute.Margin = new System.Windows.Forms.Padding(10);
            this.labelStepsToExecute.Name = "labelStepsToExecute";
            this.labelStepsToExecute.Size = new System.Drawing.Size(122, 21);
            this.labelStepsToExecute.TabIndex = 55;
            this.labelStepsToExecute.Text = "Steps to execute";
            this.labelStepsToExecute.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // Summary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.labelStepsToExecute);
            this.Controls.Add(this.labelVerifySettings);
            this.Controls.Add(this.log);
            this.Controls.Add(this.lResult);
            this.Controls.Add(this.resultImage);
            this.Controls.Add(this.Tsettings);
            this.Controls.Add(this.leftFlowPanel);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "Summary";
            this.Size = new System.Drawing.Size(709, 363);
            ((System.ComponentModel.ISupportInitialize)(this.resultImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel leftFlowPanel;
        private System.Windows.Forms.TableLayoutPanel Tsettings;
      
        private System.Windows.Forms.PictureBox resultImage;
        private System.Windows.Forms.Label lResult;
        private System.Windows.Forms.LinkLabel log;
        private General.SmoothLabel labelVerifySettings;
        private General.SmoothLabel labelStepsToExecute;
    }
}
