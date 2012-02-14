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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditOutlier));
            this.textBoxTemplateName = new System.Windows.Forms.TextBox();
            this.labelTemplateName = new System.Windows.Forms.Label();
            this.splitContainerAdvContent = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.listBoxDateProviders = new System.Windows.Forms.ListBox();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
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
            this.textBoxTemplateName.Location = new System.Drawing.Point(88, 5);
            this.textBoxTemplateName.MaxLength = 50;
            this.textBoxTemplateName.Name = "textBoxTemplateName";
            this.textBoxTemplateName.Size = new System.Drawing.Size(219, 20);
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
            this.labelTemplateName.Size = new System.Drawing.Size(72, 13);
            this.labelTemplateName.TabIndex = 0;
            this.labelTemplateName.Text = "xxNameColon";
            // 
            // splitContainerAdvContent
            // 
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
            // 
            // splitContainerAdvContent.Panel2
            // 
            this.splitContainerAdvContent.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdvContent.Size = new System.Drawing.Size(588, 238);
            this.splitContainerAdvContent.SplitterDistance = 150;
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
            this.listBoxDateProviders.Size = new System.Drawing.Size(150, 238);
            this.listBoxDateProviders.TabIndex = 0;
            this.listBoxDateProviders.SelectedIndexChanged += new System.EventHandler(this.listBoxDateProviders_SelectedIndexChanged);
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvCancel.Location = new System.Drawing.Point(502, 279);
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
            this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvOK.Location = new System.Drawing.Point(402, 279);
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
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(604, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "StartMenu";
            this.ribbonControlAdv1.TabIndex = 9;
            this.ribbonControlAdv1.Text = "xxOutlier";
            // 
            // gradientPanel1
            // 
            this.gradientPanel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanel1.Controls.Add(this.tableLayoutPanel1);
            this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanel1.Location = new System.Drawing.Point(6, 34);
            this.gradientPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Size = new System.Drawing.Size(594, 309);
            this.gradientPanel1.TabIndex = 10;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(594, 309);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // EditOutlier
            // 
            this.AcceptButton = this.buttonAdvOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAdvCancel;
            this.ClientSize = new System.Drawing.Size(606, 349);
            this.Controls.Add(this.gradientPanel1);
            this.Controls.Add(this.ribbonControlAdv1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditOutlier";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xxOutlier";
            this.Load += new System.EventHandler(this.EditOutlier_Load);
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
        private System.Windows.Forms.TextBox textBoxTemplateName;
        private System.Windows.Forms.Label labelTemplateName;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvContent;
        private System.Windows.Forms.ListBox listBoxDateProviders;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

    }
}