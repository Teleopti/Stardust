﻿namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    partial class EditOutlier
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditOutlier));
			this.textBoxTemplateName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.labelTemplateName = new System.Windows.Forms.Label();
			this.splitContainerAdvContent = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.listBoxDateProviders = new System.Windows.Forms.ListBox();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.textBoxTemplateName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvContent)).BeginInit();
			this.splitContainerAdvContent.Panel1.SuspendLayout();
			this.splitContainerAdvContent.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.gradientPanel1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxTemplateName
			// 
			this.textBoxTemplateName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxTemplateName.BackColor = System.Drawing.Color.White;
			this.textBoxTemplateName.BeforeTouchSize = new System.Drawing.Size(219, 22);
			this.textBoxTemplateName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxTemplateName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxTemplateName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxTemplateName.Location = new System.Drawing.Point(93, 4);
			this.textBoxTemplateName.MaxLength = 50;
			this.textBoxTemplateName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxTemplateName.Name = "textBoxTemplateName";
			this.textBoxTemplateName.Size = new System.Drawing.Size(219, 22);
			this.textBoxTemplateName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxTemplateName.TabIndex = 1;
			this.textBoxTemplateName.TextChanged += new System.EventHandler(this.textBoxTemplateName_TextChanged);
			// 
			// labelTemplateName
			// 
			this.labelTemplateName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTemplateName.AutoSize = true;
			this.labelTemplateName.Location = new System.Drawing.Point(10, 8);
			this.labelTemplateName.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
			this.labelTemplateName.Name = "labelTemplateName";
			this.labelTemplateName.Size = new System.Drawing.Size(77, 13);
			this.labelTemplateName.TabIndex = 0;
			this.labelTemplateName.Text = "xxNameColon";
			// 
			// splitContainerAdvContent
			// 
			this.splitContainerAdvContent.BeforeTouchSize = 1;
			this.tableLayoutPanel1.SetColumnSpan(this.splitContainerAdvContent, 3);
			this.splitContainerAdvContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdvContent.IsSplitterFixed = true;
			this.splitContainerAdvContent.Location = new System.Drawing.Point(3, 33);
			this.splitContainerAdvContent.Name = "splitContainerAdvContent";
			// 
			// splitContainerAdvContent.Panel1
			// 
			this.splitContainerAdvContent.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
			this.splitContainerAdvContent.Panel1.Controls.Add(this.listBoxDateProviders);
			this.splitContainerAdvContent.Size = new System.Drawing.Size(598, 245);
			this.splitContainerAdvContent.SplitterDistance = 152;
			this.splitContainerAdvContent.SplitterWidth = 1;
			this.splitContainerAdvContent.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
			this.splitContainerAdvContent.TabIndex = 0;
			this.splitContainerAdvContent.Text = "yySplitContainerAdv1";
			// 
			// listBoxDateProviders
			// 
			this.listBoxDateProviders.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listBoxDateProviders.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxDateProviders.Enabled = false;
			this.listBoxDateProviders.FormattingEnabled = true;
			this.listBoxDateProviders.Location = new System.Drawing.Point(0, 0);
			this.listBoxDateProviders.Name = "listBoxDateProviders";
			this.listBoxDateProviders.Size = new System.Drawing.Size(152, 245);
			this.listBoxDateProviders.TabIndex = 0;
			this.listBoxDateProviders.SelectedIndexChanged += new System.EventHandler(this.listBoxDateProviders_SelectedIndexChanged);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(82, 24);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(512, 286);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(82, 24);
			this.buttonAdvCancel.TabIndex = 1;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
			// 
			// buttonAdvOK
			// 
			this.buttonAdvOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOK.BeforeTouchSize = new System.Drawing.Size(82, 24);
			this.buttonAdvOK.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOK.IsBackStageButton = false;
			this.buttonAdvOK.Location = new System.Drawing.Point(412, 286);
			this.buttonAdvOK.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.buttonAdvOK.Name = "buttonAdvOK";
			this.buttonAdvOK.Size = new System.Drawing.Size(82, 24);
			this.buttonAdvOK.TabIndex = 0;
			this.buttonAdvOK.Text = "xxOk";
			this.buttonAdvOK.UseVisualStyle = true;
			this.buttonAdvOK.Click += new System.EventHandler(this.buttonAdvOK_Click);
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 1);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MenuButtonWidth = 56;
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			this.ribbonControlAdv1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(608, 33);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "StartMenu";
			this.ribbonControlAdv1.TabIndex = 9;
			this.ribbonControlAdv1.Text = "xxOutlier";
			this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdv1.TitleColor = System.Drawing.Color.Black;
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel1.Controls.Add(this.tableLayoutPanel1);
			this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel1.Location = new System.Drawing.Point(1, 33);
			this.gradientPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Size = new System.Drawing.Size(604, 316);
			this.gradientPanel1.TabIndex = 10;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.Controls.Add(this.splitContainerAdvContent, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelTemplateName, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxTemplateName, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvOK, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 2, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(604, 316);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// EditOutlier
			// 
			this.AcceptButton = this.buttonAdvOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Borders = new System.Windows.Forms.Padding(0);
			this.CancelButton = this.buttonAdvCancel;
			this.ClientSize = new System.Drawing.Size(606, 349);
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.gradientPanel1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(157, 38);
			this.Name = "EditOutlier";
			this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxOutlier";
			this.Load += new System.EventHandler(this.EditOutlier_Load);
			((System.ComponentModel.ISupportInitialize)(this.textBoxTemplateName)).EndInit();
			this.splitContainerAdvContent.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvContent)).EndInit();
			this.splitContainerAdvContent.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.gradientPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt  textBoxTemplateName;
        private System.Windows.Forms.Label labelTemplateName;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvContent;
        private System.Windows.Forms.ListBox listBoxDateProviders;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

    }
}