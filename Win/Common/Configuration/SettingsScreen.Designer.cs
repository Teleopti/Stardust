using System;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class SettingsScreen
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
                if (_core != null) _core.Dispose();
                if (_timer != null) _timer.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsScreen));
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.treeViewOptions = new System.Windows.Forms.TreeView();
            this.PanelContent = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvApply = new Syncfusion.Windows.Forms.ButtonAdv();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PanelContent)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.AutoSize = true;
            this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControlAdv1.HideMenuButtonToolTip = false;
            this.ribbonControlAdv1.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 1);
            this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
            this.ribbonControlAdv1.MenuButtonEnabled = true;
            this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Segoe UI", 8.25F);
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
            this.ribbonControlAdv1.ShowLauncher = false;
            this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
            this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(1002, 32);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "StartMenu";
            this.ribbonControlAdv1.TabIndex = 1;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
            this.ribbonControlAdv1.TitleColor = System.Drawing.Color.Black;
            this.ribbonControlAdv1.TitleFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.CausesValidation = false;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.splitContainer, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvOK, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvApply, 2, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 31);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(990, 665);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // splitContainer
            // 
            this.splitContainer.BeforeTouchSize = 7;
            this.tableLayoutPanel1.SetColumnSpan(this.splitContainer, 4);
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.HotExpandLine = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(99)))));
            this.splitContainer.Location = new System.Drawing.Point(3, 3);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
            this.splitContainer.Panel1.Controls.Add(this.treeViewOptions);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
            this.splitContainer.Panel2.Controls.Add(this.PanelContent);
            this.splitContainer.Size = new System.Drawing.Size(984, 609);
            this.splitContainer.SplitterDistance = 198;
            this.splitContainer.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Mozilla;
            this.splitContainer.TabIndex = 7;
            // 
            // treeViewOptions
            // 
            this.treeViewOptions.BackColor = System.Drawing.Color.White;
            this.treeViewOptions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewOptions.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeViewOptions.FullRowSelect = true;
            this.treeViewOptions.HideSelection = false;
            this.treeViewOptions.Indent = 15;
            this.treeViewOptions.ItemHeight = 30;
            this.treeViewOptions.LineColor = System.Drawing.Color.SkyBlue;
            this.treeViewOptions.Location = new System.Drawing.Point(0, 0);
            this.treeViewOptions.Margin = new System.Windows.Forms.Padding(10, 10, 3, 3);
            this.treeViewOptions.Name = "treeViewOptions";
            this.treeViewOptions.ShowLines = false;
            this.treeViewOptions.Size = new System.Drawing.Size(198, 609);
            this.treeViewOptions.TabIndex = 1;
            // 
            // PanelContent
            // 
            this.PanelContent.BackColor = System.Drawing.Color.MistyRose;
            this.PanelContent.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.PanelContent.BorderColor = System.Drawing.Color.Gray;
            this.PanelContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PanelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelContent.Location = new System.Drawing.Point(0, 0);
            this.PanelContent.Name = "PanelContent";
            this.PanelContent.Size = new System.Drawing.Size(779, 609);
            this.PanelContent.TabIndex = 6;
            // 
            // buttonAdvOK
            // 
            this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.buttonAdvOK.BeforeTouchSize = new System.Drawing.Size(75, 23);
            this.buttonAdvOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonAdvOK.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonAdvOK.ForeColor = System.Drawing.Color.White;
            this.buttonAdvOK.IsBackStageButton = false;
            this.buttonAdvOK.Location = new System.Drawing.Point(715, 625);
            this.buttonAdvOK.Margin = new System.Windows.Forms.Padding(10);
            this.buttonAdvOK.Name = "buttonAdvOK";
            this.buttonAdvOK.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.buttonAdvOK.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvOK.TabIndex = 0;
            this.buttonAdvOK.Text = "xxOk";
            this.buttonAdvOK.UseVisualStyle = true;
            this.buttonAdvOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(75, 23);
            this.buttonAdvCancel.CausesValidation = false;
            this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
            this.buttonAdvCancel.IsBackStageButton = false;
            this.buttonAdvCancel.Location = new System.Drawing.Point(905, 625);
            this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(10);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvCancel.TabIndex = 1;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            this.buttonAdvCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // buttonAdvApply
            // 
            this.buttonAdvApply.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.buttonAdvApply.BeforeTouchSize = new System.Drawing.Size(75, 23);
            this.buttonAdvApply.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonAdvApply.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonAdvApply.ForeColor = System.Drawing.Color.White;
            this.buttonAdvApply.IsBackStageButton = false;
            this.buttonAdvApply.Location = new System.Drawing.Point(810, 625);
            this.buttonAdvApply.Margin = new System.Windows.Forms.Padding(10);
            this.buttonAdvApply.Name = "buttonAdvApply";
            this.buttonAdvApply.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvApply.TabIndex = 2;
            this.buttonAdvApply.Text = "xxApply";
            this.buttonAdvApply.UseVisualStyle = true;
            this.buttonAdvApply.Click += new System.EventHandler(this.ButtonApplyClick);
            // 
            // SettingsScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Borders = new System.Windows.Forms.Padding(0);
            this.CancelButton = this.buttonAdvCancel;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.ribbonControlAdv1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(200, 200);
            this.Name = "SettingsScreen";
            this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxOptions";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.SizeChanged += new System.EventHandler(this.settingsScreenSizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PanelContent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainer;
        private System.Windows.Forms.TreeView treeViewOptions;
        private Syncfusion.Windows.Forms.Tools.GradientPanel PanelContent;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvApply;
        
    }
}