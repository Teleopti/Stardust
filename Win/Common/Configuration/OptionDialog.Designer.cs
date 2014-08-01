namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class OptionDialog
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
            if (disposing)
            {
                if (components != null) components.Dispose();
                if (Core != null) Core.Dispose();
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionDialog));
			this.tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.ribbonControlAdvForm = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelBottom.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvForm)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanelBottom
			// 
			this.tableLayoutPanelBottom.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelBottom.ColumnCount = 3;
			this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelBottom.Controls.Add(this.buttonAdvOK, 1, 0);
			this.tableLayoutPanelBottom.Controls.Add(this.buttonAdvCancel, 2, 0);
			this.tableLayoutPanelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tableLayoutPanelBottom.Location = new System.Drawing.Point(0, 178);
			this.tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
			this.tableLayoutPanelBottom.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.tableLayoutPanelBottom.RowCount = 1;
			this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBottom.Size = new System.Drawing.Size(411, 50);
			this.tableLayoutPanelBottom.TabIndex = 4;
			// 
			// buttonAdvOK
			// 
			this.buttonAdvOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOK.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOK.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOK.IsBackStageButton = false;
			this.buttonAdvOK.Location = new System.Drawing.Point(195, 9);
			this.buttonAdvOK.Name = "buttonAdvOK";
			this.buttonAdvOK.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.buttonAdvOK.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOK.TabIndex = 0;
			this.buttonAdvOK.Text = "xxOk";
			this.buttonAdvOK.UseVisualStyle = true;
			this.buttonAdvOK.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.CausesValidation = false;
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(315, 9);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 1;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			// 
			// ribbonControlAdvForm
			// 
			this.ribbonControlAdvForm.CaptionFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvForm.HideMenuButtonToolTip = false;
			this.ribbonControlAdvForm.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdvForm.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdvForm.MenuButtonEnabled = true;
			this.ribbonControlAdvForm.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvForm.MenuButtonText = "";
			this.ribbonControlAdvForm.MenuButtonVisible = false;
			this.ribbonControlAdvForm.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdvForm.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdvForm.MinimumSize = new System.Drawing.Size(0, 31);
			this.ribbonControlAdvForm.Name = "ribbonControlAdvForm";
			this.ribbonControlAdvForm.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			// 
			// ribbonControlAdvForm.OfficeMenu
			// 
			this.ribbonControlAdvForm.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdvForm.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdvForm.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdvForm.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdvForm.QuickPanelVisible = false;
			this.ribbonControlAdvForm.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdvForm.SelectedTab = null;
			this.ribbonControlAdvForm.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdvForm.ShowContextMenu = false;
			this.ribbonControlAdvForm.ShowLauncher = false;
			this.ribbonControlAdvForm.ShowRibbonDisplayOptionButton = true;
			this.ribbonControlAdvForm.Size = new System.Drawing.Size(409, 38);
			this.ribbonControlAdvForm.SystemText.QuickAccessDialogDropDownName = "xxStartMenu";
			this.ribbonControlAdvForm.TabIndex = 5;
			this.ribbonControlAdvForm.Text = "ribbonControlAdv1";
			this.ribbonControlAdvForm.TitleFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Size = new System.Drawing.Size(411, 228);
			this.gradientPanel1.TabIndex = 6;
			// 
			// OptionDialog
			// 
			this.AcceptButton = this.buttonAdvOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonAdvCancel;
			this.ClientSize = new System.Drawing.Size(411, 228);
			this.Controls.Add(this.tableLayoutPanelBottom);
			this.Controls.Add(this.gradientPanel1);
			this.Controls.Add(this.ribbonControlAdvForm);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OptionDialog";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxOptions";
			this.tableLayoutPanelBottom.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvForm)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottom;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdvForm;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;

    }
}