namespace Teleopti.Ccc.AgentPortal.Settings
{
    partial class ChangePasswordControl
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
            this.autoLabelUserName = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.autoLabelConfimPassword = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.textBoxExtConfirmPassword = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
            this.gradientLabelTitle = new Syncfusion.Windows.Forms.Tools.GradientLabel();
            this.tableLayoutPanelDetail = new System.Windows.Forms.TableLayoutPanel();
            this.autoLabelOldPassword = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.textBoxExtOldPassword = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.textBoxExtNewPassword = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.autoLabelNewPassword = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtConfirmPassword)).BeginInit();
            this.tableLayoutPanelMain.SuspendLayout();
            this.tableLayoutPanelHeader.SuspendLayout();
            this.tableLayoutPanelDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtOldPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtNewPassword)).BeginInit();
            this.SuspendLayout();
            // 
            // autoLabelUserName
            // 
            this.autoLabelUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.autoLabelUserName.AutoSize = false;
            this.autoLabelUserName.BackColor = System.Drawing.Color.LightSteelBlue;
            this.autoLabelUserName.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.autoLabelUserName.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autoLabelUserName.ForeColor = System.Drawing.Color.GhostWhite;
            this.autoLabelUserName.Location = new System.Drawing.Point(0, 90);
            this.autoLabelUserName.Margin = new System.Windows.Forms.Padding(0);
            this.autoLabelUserName.Name = "autoLabelUserName";
            this.autoLabelUserName.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.autoLabelUserName.Size = new System.Drawing.Size(466, 19);
            this.autoLabelUserName.TabIndex = 16;
            this.autoLabelUserName.Text = "xxUserCredentialsFor";
            this.autoLabelUserName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // autoLabelConfimPassword
            // 
            this.autoLabelConfimPassword.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.autoLabelConfimPassword.AutoSize = false;
            this.autoLabelConfimPassword.Location = new System.Drawing.Point(11, 81);
            this.autoLabelConfimPassword.Name = "autoLabelConfimPassword";
            this.autoLabelConfimPassword.Size = new System.Drawing.Size(136, 21);
            this.autoLabelConfimPassword.TabIndex = 17;
            this.autoLabelConfimPassword.Text = "xxConfirmNewPassword";
            this.autoLabelConfimPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxExtConfirmPassword
            // 
            this.textBoxExtConfirmPassword.Location = new System.Drawing.Point(153, 81);
            this.textBoxExtConfirmPassword.MaxLength = 50;
            this.textBoxExtConfirmPassword.Name = "textBoxExtConfirmPassword";
            this.textBoxExtConfirmPassword.OverflowIndicatorToolTipText = null;
            this.textBoxExtConfirmPassword.PasswordChar = '*';
            this.textBoxExtConfirmPassword.Size = new System.Drawing.Size(162, 21);
            this.textBoxExtConfirmPassword.TabIndex = 2;
            this.textBoxExtConfirmPassword.WordWrap = false;
            this.textBoxExtConfirmPassword.Leave += new System.EventHandler(this.textBoxExt_Leave);
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelHeader, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelDetail, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.autoLabelUserName, 0, 1);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 3;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 339F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(466, 251);
            this.tableLayoutPanelMain.TabIndex = 25;
            // 
            // tableLayoutPanelHeader
            // 
            this.tableLayoutPanelHeader.ColumnCount = 1;
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHeader.Controls.Add(this.gradientLabelTitle, 0, 0);
            this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelHeader.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
            this.tableLayoutPanelHeader.RowCount = 1;
            this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanelHeader.Size = new System.Drawing.Size(466, 90);
            this.tableLayoutPanelHeader.TabIndex = 17;
            // 
            // gradientLabelTitle
            // 
            this.gradientLabelTitle.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.White);
            this.gradientLabelTitle.BorderSides = System.Windows.Forms.Border3DSide.Top;
            this.gradientLabelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientLabelTitle.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gradientLabelTitle.ForeColor = System.Drawing.Color.MidnightBlue;
            this.gradientLabelTitle.Location = new System.Drawing.Point(0, 0);
            this.gradientLabelTitle.Margin = new System.Windows.Forms.Padding(0);
            this.gradientLabelTitle.Name = "gradientLabelTitle";
            this.gradientLabelTitle.Padding = new System.Windows.Forms.Padding(75, 15, 0, 0);
            this.gradientLabelTitle.Size = new System.Drawing.Size(466, 90);
            this.gradientLabelTitle.TabIndex = 24;
            this.gradientLabelTitle.Text = "xxChangeYourPassword";
            this.gradientLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.gradientLabelTitle.UseMnemonic = false;
            // 
            // tableLayoutPanelDetail
            // 
            this.tableLayoutPanelDetail.ColumnCount = 2;
            this.tableLayoutPanelDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanelDetail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelDetail.Controls.Add(this.autoLabelOldPassword, 0, 0);
            this.tableLayoutPanelDetail.Controls.Add(this.textBoxExtOldPassword, 1, 0);
            this.tableLayoutPanelDetail.Controls.Add(this.textBoxExtNewPassword, 1, 1);
            this.tableLayoutPanelDetail.Controls.Add(this.autoLabelConfimPassword, 0, 2);
            this.tableLayoutPanelDetail.Controls.Add(this.autoLabelNewPassword, 0, 1);
            this.tableLayoutPanelDetail.Controls.Add(this.textBoxExtConfirmPassword, 1, 2);
            this.tableLayoutPanelDetail.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelDetail.Location = new System.Drawing.Point(0, 111);
            this.tableLayoutPanelDetail.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelDetail.Name = "tableLayoutPanelDetail";
            this.tableLayoutPanelDetail.Padding = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.tableLayoutPanelDetail.RowCount = 3;
            this.tableLayoutPanelDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanelDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelDetail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelDetail.Size = new System.Drawing.Size(466, 105);
            this.tableLayoutPanelDetail.TabIndex = 18;
            // 
            // autoLabelOldPassword
            // 
            this.autoLabelOldPassword.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.autoLabelOldPassword.AutoSize = false;
            this.autoLabelOldPassword.Location = new System.Drawing.Point(11, 25);
            this.autoLabelOldPassword.Name = "autoLabelOldPassword";
            this.autoLabelOldPassword.Size = new System.Drawing.Size(136, 21);
            this.autoLabelOldPassword.TabIndex = 19;
            this.autoLabelOldPassword.Text = "xxCurrentPassword";
            this.autoLabelOldPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxExtOldPassword
            // 
            this.textBoxExtOldPassword.Location = new System.Drawing.Point(153, 23);
            this.textBoxExtOldPassword.MaxLength = 50;
            this.textBoxExtOldPassword.Name = "textBoxExtOldPassword";
            this.textBoxExtOldPassword.OverflowIndicatorToolTipText = null;
            this.textBoxExtOldPassword.PasswordChar = '*';
            this.textBoxExtOldPassword.Size = new System.Drawing.Size(162, 21);
            this.textBoxExtOldPassword.TabIndex = 0;
            this.textBoxExtOldPassword.WordWrap = false;
            this.textBoxExtOldPassword.Leave += new System.EventHandler(this.textBoxExt_Leave);
            // 
            // textBoxExtNewPassword
            // 
            this.textBoxExtNewPassword.Location = new System.Drawing.Point(153, 54);
            this.textBoxExtNewPassword.MaxLength = 50;
            this.textBoxExtNewPassword.Name = "textBoxExtNewPassword";
            this.textBoxExtNewPassword.OverflowIndicatorToolTipText = null;
            this.textBoxExtNewPassword.PasswordChar = '*';
            this.textBoxExtNewPassword.Size = new System.Drawing.Size(162, 21);
            this.textBoxExtNewPassword.TabIndex = 1;
            this.textBoxExtNewPassword.WordWrap = false;
            this.textBoxExtNewPassword.Leave += new System.EventHandler(this.textBoxExt_Leave);
            // 
            // autoLabelNewPassword
            // 
            this.autoLabelNewPassword.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.autoLabelNewPassword.AutoSize = false;
            this.autoLabelNewPassword.Location = new System.Drawing.Point(26, 54);
            this.autoLabelNewPassword.Name = "autoLabelNewPassword";
            this.autoLabelNewPassword.Size = new System.Drawing.Size(121, 21);
            this.autoLabelNewPassword.TabIndex = 17;
            this.autoLabelNewPassword.Text = "xxNewPassword";
            this.autoLabelNewPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ChangePasswordControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ChangePasswordControl";
            this.Size = new System.Drawing.Size(466, 251);
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtConfirmPassword)).EndInit();
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelDetail.ResumeLayout(false);
            this.tableLayoutPanelDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtOldPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtNewPassword)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelUserName;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelConfimPassword;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtConfirmPassword;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private Syncfusion.Windows.Forms.Tools.GradientLabel gradientLabelTitle;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDetail;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtNewPassword;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelOldPassword;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtOldPassword;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelNewPassword;
    }
}
