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
                if(kpiSettings1!=null) kpiSettings1.Dispose(); //rk: no idea why this is needed, but it does
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvApply = new Syncfusion.Windows.Forms.ButtonAdv();
            this.panel1 = new System.Windows.Forms.Panel();
            this.treeViewOptions = new System.Windows.Forms.TreeView();
            this.PanelContent = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.kpiSettings1 = new Teleopti.Ccc.Win.Common.Configuration.KpiSettings();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PanelContent)).BeginInit();
            this.PanelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.CausesValidation = false;
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(1, 670);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(998, 30);
            this.panel2.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.CausesValidation = false;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvOK, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvApply, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(998, 30);
            this.tableLayoutPanel1.TabIndex = 2;
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
            this.buttonAdvOK.Location = new System.Drawing.Point(758, 3);
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
            this.buttonAdvCancel.Location = new System.Drawing.Point(920, 3);
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
            this.buttonAdvApply.Location = new System.Drawing.Point(839, 3);
            this.buttonAdvApply.Name = "buttonAdvApply";
            this.buttonAdvApply.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvApply.TabIndex = 2;
            this.buttonAdvApply.Text = "xxApply";
            this.buttonAdvApply.UseVisualStyle = true;
            this.buttonAdvApply.Click += new System.EventHandler(this.ButtonApplyClick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.treeViewOptions);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(1, 32);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(185, 638);
            this.panel1.TabIndex = 4;
            // 
            // treeViewOptions
            // 
            this.treeViewOptions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewOptions.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeViewOptions.HideSelection = false;
            this.treeViewOptions.Indent = 15;
            this.treeViewOptions.ItemHeight = 30;
            this.treeViewOptions.LineColor = System.Drawing.Color.SkyBlue;
            this.treeViewOptions.Location = new System.Drawing.Point(0, 0);
            this.treeViewOptions.Margin = new System.Windows.Forms.Padding(10, 10, 3, 3);
            this.treeViewOptions.Name = "treeViewOptions";
            this.treeViewOptions.ShowLines = false;
            this.treeViewOptions.Size = new System.Drawing.Size(183, 636);
            this.treeViewOptions.TabIndex = 0;
            this.treeViewOptions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewOptions_AfterSelect);
            // 
            // PanelContent
            // 
            this.PanelContent.BackColor = System.Drawing.Color.Transparent;
            this.PanelContent.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.PanelContent.BorderColor = System.Drawing.Color.Gray;
            this.PanelContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PanelContent.Controls.Add(this.kpiSettings1);
            this.PanelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelContent.Location = new System.Drawing.Point(186, 32);
            this.PanelContent.Name = "PanelContent";
            this.PanelContent.Size = new System.Drawing.Size(813, 638);
            this.PanelContent.TabIndex = 5;
            // 
            // kpiSettings1
            // 
            this.kpiSettings1.BackColor = System.Drawing.SystemColors.Window;
            this.kpiSettings1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.kpiSettings1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiSettings1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.kpiSettings1.Location = new System.Drawing.Point(0, 0);
            this.kpiSettings1.Name = "kpiSettings1";
            this.kpiSettings1.Size = new System.Drawing.Size(813, 638);
            this.kpiSettings1.TabIndex = 0;
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
            this.Controls.Add(this.ribbonControlAdv1);
            this.Controls.Add(this.PanelContent);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(200, 200);
            this.Name = "SettingsScreen";
            this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxOptions";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PanelContent)).EndInit();
            this.PanelContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvApply;
        private Syncfusion.Windows.Forms.Tools.GradientPanel PanelContent;
        private KpiSettings kpiSettings1;
        private System.Windows.Forms.TreeView treeViewOptions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;

        
    }
}