namespace Teleopti.Ccc.Win.Common.PropertyPageAndWizard
{
    partial class PropertiesPages
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Wizard");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertiesPages));
            this.treeViewPages = new System.Windows.Forms.TreeView();
            this.splitContainerPages = new System.Windows.Forms.SplitContainer();
            this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.splitContainerHorizontal = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanelButtonsRtl = new System.Windows.Forms.TableLayoutPanel();
            this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.splitContainerPages.Panel1.SuspendLayout();
            this.splitContainerPages.SuspendLayout();
            this.splitContainerHorizontal.Panel1.SuspendLayout();
            this.splitContainerHorizontal.Panel2.SuspendLayout();
            this.splitContainerHorizontal.SuspendLayout();
            this.tableLayoutPanelButtonsRtl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.SuspendLayout();
            // 
            // treeViewPages
            // 
            this.treeViewPages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewPages.FullRowSelect = true;
            this.treeViewPages.Location = new System.Drawing.Point(10, 10);
            this.treeViewPages.Name = "treeViewPages";
            treeNode1.Name = "Node0";
            treeNode1.Text = "Wizard";
            this.treeViewPages.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewPages.ShowPlusMinus = false;
            this.treeViewPages.ShowRootLines = false;
            this.treeViewPages.Size = new System.Drawing.Size(128, 354);
            this.treeViewPages.TabIndex = 0;
            this.treeViewPages.TabStop = false;
            this.treeViewPages.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewPages_AfterSelect);
            // 
            // splitContainerPages
            // 
            this.splitContainerPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerPages.IsSplitterFixed = true;
            this.splitContainerPages.Location = new System.Drawing.Point(0, 0);
            this.splitContainerPages.Name = "splitContainerPages";
            // 
            // splitContainerPages.Panel1
            // 
            this.splitContainerPages.Panel1.Controls.Add(this.treeViewPages);
            this.splitContainerPages.Panel1.Padding = new System.Windows.Forms.Padding(10);
            this.splitContainerPages.Panel1MinSize = 120;
            this.splitContainerPages.Size = new System.Drawing.Size(538, 374);
            this.splitContainerPages.SplitterDistance = 148;
            this.splitContainerPages.SplitterWidth = 1;
            this.splitContainerPages.TabIndex = 6;
            this.splitContainerPages.TabStop = false;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonOK.Location = new System.Drawing.Point(353, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(85, 19);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "xxOk";
            this.buttonOK.UseVisualStyle = true;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // splitContainerHorizontal
            // 
            this.splitContainerHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerHorizontal.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerHorizontal.IsSplitterFixed = true;
            this.splitContainerHorizontal.Location = new System.Drawing.Point(6, 34);
            this.splitContainerHorizontal.Name = "splitContainerHorizontal";
            this.splitContainerHorizontal.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerHorizontal.Panel1
            // 
            this.splitContainerHorizontal.Panel1.Controls.Add(this.splitContainerPages);
            // 
            // splitContainerHorizontal.Panel2
            // 
            this.splitContainerHorizontal.Panel2.Controls.Add(this.tableLayoutPanelButtonsRtl);
            this.splitContainerHorizontal.Size = new System.Drawing.Size(538, 400);
            this.splitContainerHorizontal.SplitterDistance = 374;
            this.splitContainerHorizontal.SplitterWidth = 1;
            this.splitContainerHorizontal.TabIndex = 8;
            this.splitContainerHorizontal.TabStop = false;
            // 
            // tableLayoutPanelButtonsRtl
            // 
            this.tableLayoutPanelButtonsRtl.ColumnCount = 2;
            this.tableLayoutPanelButtonsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 82F));
            this.tableLayoutPanelButtonsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tableLayoutPanelButtonsRtl.Controls.Add(this.buttonOK, 0, 0);
            this.tableLayoutPanelButtonsRtl.Controls.Add(this.buttonCancel, 1, 0);
            this.tableLayoutPanelButtonsRtl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelButtonsRtl.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelButtonsRtl.Name = "tableLayoutPanelButtonsRtl";
            this.tableLayoutPanelButtonsRtl.RowCount = 1;
            this.tableLayoutPanelButtonsRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelButtonsRtl.Size = new System.Drawing.Size(538, 25);
            this.tableLayoutPanelButtonsRtl.TabIndex = 4;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(450, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(85, 19);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "xxCancel";
            this.buttonCancel.UseVisualStyle = true;
            this.buttonCancel.UseVisualStyleBackColor = true;
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
            this.ribbonControlAdv1.Size = new System.Drawing.Size(548, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "StartMenu";
            this.ribbonControlAdv1.TabIndex = 9;
            this.ribbonControlAdv1.Text = "yyribbonControlAdv1";
            // 
            // PropertiesPages
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 440);
            this.Controls.Add(this.ribbonControlAdv1);
            this.Controls.Add(this.splitContainerHorizontal);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(550, 440);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(550, 440);
            this.Name = "PropertiesPages";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xxProperties";
            this.Load += new System.EventHandler(this.PropertiesPages_Load);
            this.splitContainerPages.Panel1.ResumeLayout(false);
            this.splitContainerPages.ResumeLayout(false);
            this.splitContainerHorizontal.Panel1.ResumeLayout(false);
            this.splitContainerHorizontal.Panel2.ResumeLayout(false);
            this.splitContainerHorizontal.ResumeLayout(false);
            this.tableLayoutPanelButtonsRtl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewPages;
        private System.Windows.Forms.SplitContainer splitContainerPages;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
        private System.Windows.Forms.SplitContainer splitContainerHorizontal;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtonsRtl;

    }
}
