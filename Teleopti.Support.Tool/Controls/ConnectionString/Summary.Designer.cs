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
            this.LVerifySettings = new System.Windows.Forms.Label();
            this.leftFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Tsettings = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
          
            this.resultImage = new System.Windows.Forms.PictureBox();
            this.lResult = new System.Windows.Forms.Label();
            this.log = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.resultImage)).BeginInit();
            this.SuspendLayout();
            // 
            // LVerifySettings
            // 
            this.LVerifySettings.AutoSize = true;
            this.LVerifySettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LVerifySettings.Location = new System.Drawing.Point(319, 35);
            this.LVerifySettings.Name = "LVerifySettings";
            this.LVerifySettings.Size = new System.Drawing.Size(192, 16);
            this.LVerifySettings.TabIndex = 1;
            this.LVerifySettings.Text = "Steps that will be executed";
            // 
            // leftFlowPanel
            // 
            this.leftFlowPanel.BackColor = System.Drawing.SystemColors.Window;
            this.leftFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.leftFlowPanel.Location = new System.Drawing.Point(322, 54);
            this.leftFlowPanel.Name = "leftFlowPanel";
            this.leftFlowPanel.Size = new System.Drawing.Size(384, 167);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Verify settings ";
            // 
            // resultImage
            // 
            this.resultImage.ErrorImage = global::Teleopti.Support.Tool.Properties.Resources.accept1;
            this.resultImage.Location = new System.Drawing.Point(14, 279);
            this.resultImage.Name = "resultImage";
            this.resultImage.Size = new System.Drawing.Size(50, 50);
            this.resultImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.resultImage.TabIndex = 5;
            this.resultImage.TabStop = false;
            // 
            // lResult
            // 
            this.lResult.AutoEllipsis = true;
            this.lResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lResult.Location = new System.Drawing.Point(113, 279);
            this.lResult.Name = "lResult";
            this.lResult.Size = new System.Drawing.Size(556, 84);
            this.lResult.TabIndex = 52;
            // 
            // log
            // 
            this.log.AutoSize = true;
            this.log.Location = new System.Drawing.Point(134, 365);
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(96, 13);
            this.log.TabIndex = 53;
            this.log.TabStop = true;
            this.log.Text = "View executing log";
            this.log.Visible = false;
          
            // 
            // ConnSummary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.log);
            this.Controls.Add(this.lResult);
            this.Controls.Add(this.resultImage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Tsettings);
            this.Controls.Add(this.leftFlowPanel);
            this.Controls.Add(this.LVerifySettings);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "ConnSummary";
            this.Size = new System.Drawing.Size(797, 450);
            ((System.ComponentModel.ISupportInitialize)(this.resultImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LVerifySettings;
        private System.Windows.Forms.FlowLayoutPanel leftFlowPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel Tsettings;
      
        private System.Windows.Forms.PictureBox resultImage;
        private System.Windows.Forms.Label lResult;
        private System.Windows.Forms.LinkLabel log;
    }
}
