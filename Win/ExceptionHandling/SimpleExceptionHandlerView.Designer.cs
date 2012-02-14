
namespace Teleopti.Ccc.Win.ExceptionHandling
{
    partial class SimpleExceptionHandlerView
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
            this.buttonCopyErrorDetails = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.labelInformationText = new System.Windows.Forms.Label();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
            this.gradientPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCopyErrorDetails
            // 
            this.buttonCopyErrorDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopyErrorDetails.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonCopyErrorDetails.Location = new System.Drawing.Point(188, 117);
            this.buttonCopyErrorDetails.Name = "buttonCopyErrorDetails";
            this.buttonCopyErrorDetails.Size = new System.Drawing.Size(102, 23);
            this.buttonCopyErrorDetails.TabIndex = 0;
            this.buttonCopyErrorDetails.Text = "xxCopyDetails";
            this.buttonCopyErrorDetails.UseVisualStyle = true;
            this.buttonCopyErrorDetails.UseVisualStyleBackColor = true;
            this.buttonCopyErrorDetails.Click += new System.EventHandler(this.buttonCopyErrorDetails_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(296, 117);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(106, 23);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "xxOK";
            this.buttonOk.UseVisualStyle = true;
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // labelInformationText
            // 
            this.labelInformationText.BackColor = System.Drawing.Color.White;
            this.labelInformationText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelInformationText.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelInformationText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInformationText.Location = new System.Drawing.Point(0, 0);
            this.labelInformationText.Name = "labelInformationText";
            this.labelInformationText.Padding = new System.Windows.Forms.Padding(5);
            this.labelInformationText.Size = new System.Drawing.Size(405, 109);
            this.labelInformationText.TabIndex = 4;
            this.labelInformationText.Text = "xxInformationText";
            this.labelInformationText.UseMnemonic = false;
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            this.ribbonControlAdv1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Blue;
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.AllowMerge = false;
            this.ribbonControlAdv1.OfficeMenu.AutoClose = false;
            this.ribbonControlAdv1.OfficeMenu.AutoSize = false;
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.ShowContextMenu = false;
            this.ribbonControlAdv1.ShowLauncher = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(415, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlAdv1.TabIndex = 5;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // gradientPanel1
            // 
            this.gradientPanel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanel1.Controls.Add(this.buttonOk);
            this.gradientPanel1.Controls.Add(this.buttonCopyErrorDetails);
            this.gradientPanel1.Controls.Add(this.labelInformationText);
            this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanel1.Location = new System.Drawing.Point(6, 34);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Size = new System.Drawing.Size(405, 143);
            this.gradientPanel1.TabIndex = 6;
            // 
            // SimpleExceptionHandlerView
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(417, 183);
            this.ControlBox = false;
            this.Controls.Add(this.gradientPanel1);
            this.Controls.Add(this.ribbonControlAdv1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "SimpleExceptionHandlerView";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxTeleoptiCCC";
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
            this.gradientPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.ButtonAdv buttonCopyErrorDetails;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOk;
        private System.Windows.Forms.Label labelInformationText;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
    }
}