namespace Teleopti.Ccc.Win.Scheduling
{
    partial class RequestReplyDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.buttonReply = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.textBoxReply = new System.Windows.Forms.TextBox();
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.panelTextBoxHolder = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.panelTextBoxHolder.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(298, 44);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "";
            this.ribbonControlAdv1.TabIndex = 4;
            // 
            // buttonReply
            // 
            this.buttonReply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReply.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonReply.Location = new System.Drawing.Point(126, 259);
            this.buttonReply.Name = "buttonReply";
            this.buttonReply.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonReply.Size = new System.Drawing.Size(75, 23);
            this.buttonReply.TabIndex = 2;
            this.buttonReply.Text = "xxReply";
            this.buttonReply.UseVisualStyle = true;
            this.buttonReply.UseVisualStyleBackColor = true;
            this.buttonReply.Click += new System.EventHandler(this.buttonReply_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(207, 259);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "xxCancel";
            this.buttonCancel.UseVisualStyle = true;
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textBoxReply
            // 
            this.textBoxReply.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxReply.Location = new System.Drawing.Point(0, 88);
            this.textBoxReply.Multiline = true;
            this.textBoxReply.Name = "textBoxReply";
            this.textBoxReply.Size = new System.Drawing.Size(263, 82);
            this.textBoxReply.TabIndex = 1;
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMessage.Location = new System.Drawing.Point(1, 0);
            this.textBoxMessage.Multiline = true;
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.ReadOnly = true;
            this.textBoxMessage.Size = new System.Drawing.Size(263, 82);
            this.textBoxMessage.TabIndex = 0;
            this.textBoxMessage.TabStop = false;
            // 
            // panelTextBoxHolder
            // 
            this.panelTextBoxHolder.Controls.Add(this.textBoxMessage);
            this.panelTextBoxHolder.Controls.Add(this.textBoxReply);
            this.panelTextBoxHolder.Location = new System.Drawing.Point(18, 57);
            this.panelTextBoxHolder.Name = "panelTextBoxHolder";
            this.panelTextBoxHolder.Size = new System.Drawing.Size(264, 177);
            this.panelTextBoxHolder.TabIndex = 0;
            // 
            // RequestReplyDialog
            // 
            this.AcceptButton = this.buttonReply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(300, 300);
            this.Controls.Add(this.panelTextBoxHolder);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonReply);
            this.Controls.Add(this.ribbonControlAdv1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RequestReplyDialog";
            this.Text = "xxEnterReply";
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.panelTextBoxHolder.ResumeLayout(false);
            this.panelTextBoxHolder.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonReply;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private System.Windows.Forms.TextBox textBoxReply;
        private System.Windows.Forms.Panel panelTextBoxHolder;
        private System.Windows.Forms.TextBox textBoxMessage;
    }
}