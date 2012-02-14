namespace Teleopti.Ccc.Win.Grouping
{
    partial class GroupPagePreviewScreen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_treeViewDragHighlightTracker")]
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GroupPagePreviewScreen));
            Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo treeNodeAdvStyleInfo1 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo();
            this.contextMenuStripTreeActions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemCut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemNewGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemChangeNameOnGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemCollapseAllNodes = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExpandOnlyRootNodes = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExpandAllNodes = new System.Windows.Forms.ToolStripMenuItem();
            this.imageListTreeIcons = new System.Windows.Forms.ImageList(this.components);
            this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.splitContainerAdvMain = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.findPersonsView1 = new Teleopti.Ccc.Win.Grouping.FindPersonsView();
            this.tableLayoutPanelGroups = new System.Windows.Forms.TableLayoutPanel();
            this.treeViewAdvPreviewTree = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelControlButtons = new System.Windows.Forms.TableLayoutPanel();
            this.contextMenuStripTreeActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
            this.gradientPanelHeader.SuspendLayout();
            this.tableLayoutPanelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvMain)).BeginInit();
            this.splitContainerAdvMain.Panel1.SuspendLayout();
            this.splitContainerAdvMain.Panel2.SuspendLayout();
            this.splitContainerAdvMain.SuspendLayout();
            this.tableLayoutPanelGroups.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeViewAdvPreviewTree)).BeginInit();
            this.tableLayoutPanelMain.SuspendLayout();
            this.tableLayoutPanelControlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStripTreeActions
            // 
            this.contextMenuStripTreeActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCut,
            this.toolStripMenuItemPaste,
            this.toolStripSeparator1,
            this.toolStripMenuItemNewGroup,
            this.toolStripMenuItemChangeNameOnGroup,
            this.toolStripSeparator2,
            this.toolStripMenuItemDelete,
            this.toolStripSeparator3,
            this.toolStripMenuItemCollapseAllNodes,
            this.toolStripMenuItemExpandOnlyRootNodes,
            this.toolStripMenuItemExpandAllNodes});
            this.contextMenuStripTreeActions.Name = "contextMenuStripTreeActions";
            this.contextMenuStripTreeActions.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.contextMenuStripTreeActions.Size = new System.Drawing.Size(205, 198);
            this.contextMenuStripTreeActions.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripTreeActions_Opening);
            // 
            // toolStripMenuItemCut
            // 
            this.toolStripMenuItemCut.Name = "toolStripMenuItemCut";
            this.toolStripMenuItemCut.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemCut.Text = "xxCut";
            this.toolStripMenuItemCut.Click += new System.EventHandler(this.toolStripMenuItemCut_Click);
            // 
            // toolStripMenuItemPaste
            // 
            this.toolStripMenuItemPaste.Name = "toolStripMenuItemPaste";
            this.toolStripMenuItemPaste.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemPaste.Text = "xxPaste";
            this.toolStripMenuItemPaste.Click += new System.EventHandler(this.toolStripMenuItemPaste_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(201, 6);
            // 
            // toolStripMenuItemNewGroup
            // 
            this.toolStripMenuItemNewGroup.Name = "toolStripMenuItemNewGroup";
            this.toolStripMenuItemNewGroup.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemNewGroup.Text = "xxNewGroup";
            this.toolStripMenuItemNewGroup.Click += new System.EventHandler(this.toolStripMenuItemNewGroup_Click);
            // 
            // toolStripMenuItemChangeNameOnGroup
            // 
            this.toolStripMenuItemChangeNameOnGroup.Name = "toolStripMenuItemChangeNameOnGroup";
            this.toolStripMenuItemChangeNameOnGroup.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemChangeNameOnGroup.Text = "xxChangeNameonGroup";
            this.toolStripMenuItemChangeNameOnGroup.Click += new System.EventHandler(this.toolStripMenuItemChangeNameOnGroup_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(201, 6);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemDelete.Text = "xxDelete";
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(201, 6);
            // 
            // toolStripMenuItemCollapseAllNodes
            // 
            this.toolStripMenuItemCollapseAllNodes.Name = "toolStripMenuItemCollapseAllNodes";
            this.toolStripMenuItemCollapseAllNodes.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemCollapseAllNodes.Text = "xxCollapseAllNodes";
            this.toolStripMenuItemCollapseAllNodes.Click += new System.EventHandler(this.toolStripMenuItemCollapseAllNodes_Click);
            // 
            // toolStripMenuItemExpandOnlyRootNodes
            // 
            this.toolStripMenuItemExpandOnlyRootNodes.Name = "toolStripMenuItemExpandOnlyRootNodes";
            this.toolStripMenuItemExpandOnlyRootNodes.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemExpandOnlyRootNodes.Text = "xxExpandRoot";
            this.toolStripMenuItemExpandOnlyRootNodes.Click += new System.EventHandler(this.toolStripMenuItemExpandOnlyRootNodes_Click);
            // 
            // toolStripMenuItemExpandAllNodes
            // 
            this.toolStripMenuItemExpandAllNodes.Name = "toolStripMenuItemExpandAllNodes";
            this.toolStripMenuItemExpandAllNodes.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemExpandAllNodes.Text = "xxExpandAllNodes";
            this.toolStripMenuItemExpandAllNodes.Click += new System.EventHandler(this.toolStripMenuItemExpandAllNodes_Click);
            // 
            // imageListTreeIcons
            // 
            this.imageListTreeIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTreeIcons.ImageStream")));
            this.imageListTreeIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListTreeIcons.Images.SetKeyName(0, "ccc_tree_Agent.png");
            this.imageListTreeIcons.Images.SetKeyName(1, "ccc_tree_Site.png");
            this.imageListTreeIcons.Images.SetKeyName(2, "ccc_Tree_Team.png");
            this.imageListTreeIcons.Images.SetKeyName(3, "ccc_tree_Agent.png");
            this.imageListTreeIcons.Images.SetKeyName(4, "ccc_tree_User.png");
            // 
            // gradientPanelHeader
            // 
            this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gradientPanelHeader.BackColor = System.Drawing.Color.Transparent;
            this.gradientPanelHeader.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
            this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanelHeader.Location = new System.Drawing.Point(3, 3);
            this.gradientPanelHeader.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.gradientPanelHeader.Name = "gradientPanelHeader";
            this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(10);
            this.gradientPanelHeader.Size = new System.Drawing.Size(629, 50);
            this.gradientPanelHeader.TabIndex = 57;
            // 
            // tableLayoutPanelHeader
            // 
            this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelHeader.ColumnCount = 1;
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 997F));
            this.tableLayoutPanelHeader.Controls.Add(this.labelTitle, 1, 0);
            this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelHeader.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
            this.tableLayoutPanelHeader.RowCount = 1;
            this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanelHeader.Size = new System.Drawing.Size(609, 30);
            this.tableLayoutPanelHeader.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.labelTitle.ForeColor = System.Drawing.Color.MidnightBlue;
            this.labelTitle.Location = new System.Drawing.Point(3, 9);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.labelTitle.Size = new System.Drawing.Size(188, 18);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "xxPreviewOrEditGroupPage";
            // 
            // splitContainerAdvMain
            // 
            this.splitContainerAdvMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerAdvMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerAdvMain.Location = new System.Drawing.Point(3, 56);
            this.splitContainerAdvMain.Name = "splitContainerAdvMain";
            // 
            // splitContainerAdvMain.Panel1
            // 
            this.splitContainerAdvMain.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdvMain.Panel1.Controls.Add(this.findPersonsView1);
            // 
            // splitContainerAdvMain.Panel2
            // 
            this.splitContainerAdvMain.Panel2.BackColor = System.Drawing.Color.White;
            this.splitContainerAdvMain.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdvMain.Panel2.Controls.Add(this.tableLayoutPanelGroups);
            this.splitContainerAdvMain.Size = new System.Drawing.Size(629, 608);
            this.splitContainerAdvMain.SplitterDistance = 218;
            this.splitContainerAdvMain.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.splitContainerAdvMain.TabIndex = 58;
            this.splitContainerAdvMain.Text = "splitContainerAdv1";
            this.splitContainerAdvMain.ThemesEnabled = true;
            // 
            // findPersonsView1
            // 
            this.findPersonsView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.findPersonsView1.FindText = "";
            this.findPersonsView1.FromDate = new System.DateTime(2011, 6, 15, 13, 21, 32, 852);
            this.findPersonsView1.Location = new System.Drawing.Point(0, 0);
            this.findPersonsView1.Name = "findPersonsView1";
            this.findPersonsView1.Size = new System.Drawing.Size(216, 606);
            this.findPersonsView1.TabIndex = 0;
            this.findPersonsView1.ToDate = new System.DateTime(2011, 6, 15, 13, 21, 32, 852);
            // 
            // tableLayoutPanelGroups
            // 
            this.tableLayoutPanelGroups.ColumnCount = 1;
            this.tableLayoutPanelGroups.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelGroups.Controls.Add(this.treeViewAdvPreviewTree, 0, 0);
            this.tableLayoutPanelGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelGroups.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelGroups.Name = "tableLayoutPanelGroups";
            this.tableLayoutPanelGroups.RowCount = 1;
            this.tableLayoutPanelGroups.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 304F));
            this.tableLayoutPanelGroups.Size = new System.Drawing.Size(402, 606);
            this.tableLayoutPanelGroups.TabIndex = 1;
            // 
            // treeViewAdvPreviewTree
            // 
            this.treeViewAdvPreviewTree.AllowDrop = true;
            this.treeViewAdvPreviewTree.BackColor = System.Drawing.Color.White;
            treeNodeAdvStyleInfo1.EnsureDefaultOptionedChild = true;
            this.treeViewAdvPreviewTree.BaseStylePairs.AddRange(new Syncfusion.Windows.Forms.Tools.StyleNamePair[] {
            new Syncfusion.Windows.Forms.Tools.StyleNamePair("Standard", treeNodeAdvStyleInfo1)});
            this.treeViewAdvPreviewTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewAdvPreviewTree.ContextMenuStrip = this.contextMenuStripTreeActions;
            this.treeViewAdvPreviewTree.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // 
            // 
            this.treeViewAdvPreviewTree.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewAdvPreviewTree.HelpTextControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewAdvPreviewTree.HelpTextControl.Name = "helpText";
            this.treeViewAdvPreviewTree.HelpTextControl.Size = new System.Drawing.Size(49, 15);
            this.treeViewAdvPreviewTree.HelpTextControl.TabIndex = 0;
            this.treeViewAdvPreviewTree.HelpTextControl.Text = "help text";
            this.treeViewAdvPreviewTree.LeftImageList = this.imageListTreeIcons;
            this.treeViewAdvPreviewTree.Location = new System.Drawing.Point(8, 8);
            this.treeViewAdvPreviewTree.Margin = new System.Windows.Forms.Padding(8, 8, 0, 0);
            this.treeViewAdvPreviewTree.Name = "treeViewAdvPreviewTree";
            this.treeViewAdvPreviewTree.Office2007ScrollBars = true;
            this.treeViewAdvPreviewTree.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectSameLevel;
            this.treeViewAdvPreviewTree.Size = new System.Drawing.Size(394, 598);
            this.treeViewAdvPreviewTree.TabIndex = 1;
            // 
            // 
            // 
            this.treeViewAdvPreviewTree.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
            this.treeViewAdvPreviewTree.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewAdvPreviewTree.ToolTipControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewAdvPreviewTree.ToolTipControl.Name = "toolTip";
            this.treeViewAdvPreviewTree.ToolTipControl.Size = new System.Drawing.Size(41, 15);
            this.treeViewAdvPreviewTree.ToolTipControl.TabIndex = 1;
            this.treeViewAdvPreviewTree.ToolTipControl.Text = "toolTip";
            this.treeViewAdvPreviewTree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewAdvPreviewTree_ItemDrag);
            this.treeViewAdvPreviewTree.NodeEditorValidated += new Syncfusion.Windows.Forms.Tools.TreeNodeAdvEditEventHandler(this.treeViewAdvPreviewTree_NodeEditorValidated);
            this.treeViewAdvPreviewTree.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewAdvPreviewTree_DragDrop);
            this.treeViewAdvPreviewTree.DragOver += new System.Windows.Forms.DragEventHandler(this.treeViewAdvPreviewTree_DragOver);
            this.treeViewAdvPreviewTree.DragLeave += new System.EventHandler(this.treeViewAdvPreviewTree_DragLeave);
            this.treeViewAdvPreviewTree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeViewAdvPreviewTreeKeyDown);
            // 
            // buttonAdvOk
            // 
            this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvOk.Location = new System.Drawing.Point(464, 3);
            this.buttonAdvOk.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
            this.buttonAdvOk.Name = "buttonAdvOk";
            this.buttonAdvOk.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvOk.TabIndex = 59;
            this.buttonAdvOk.Text = "xxOk";
            this.buttonAdvOk.UseVisualStyle = true;
            this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOk_Click);
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvCancel.Location = new System.Drawing.Point(548, 3);
            this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvCancel.TabIndex = 60;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.splitContainerAdvMain, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.gradientPanelHeader, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelControlButtons, 0, 2);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 3;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(635, 706);
            this.tableLayoutPanelMain.TabIndex = 58;
            // 
            // tableLayoutPanelControlButtons
            // 
            this.tableLayoutPanelControlButtons.ColumnCount = 3;
            this.tableLayoutPanelControlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelControlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelControlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelControlButtons.Controls.Add(this.buttonAdvCancel, 2, 0);
            this.tableLayoutPanelControlButtons.Controls.Add(this.buttonAdvOk, 1, 0);
            this.tableLayoutPanelControlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelControlButtons.Location = new System.Drawing.Point(3, 670);
            this.tableLayoutPanelControlButtons.Name = "tableLayoutPanelControlButtons";
            this.tableLayoutPanelControlButtons.RowCount = 1;
            this.tableLayoutPanelControlButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelControlButtons.Size = new System.Drawing.Size(629, 33);
            this.tableLayoutPanelControlButtons.TabIndex = 59;
            // 
            // GroupPagePreviewScreen
            // 
            this.AcceptButton = this.buttonAdvOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAdvCancel;
            this.ClientSize = new System.Drawing.Size(635, 706);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GroupPagePreviewScreen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xxGroupPagePreviewScreen";
            this.Load += new System.EventHandler(this.GroupPagePreviewScreen_Load);
            this.contextMenuStripTreeActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
            this.gradientPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.PerformLayout();
            this.splitContainerAdvMain.Panel1.ResumeLayout(false);
            this.splitContainerAdvMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvMain)).EndInit();
            this.splitContainerAdvMain.ResumeLayout(false);
            this.tableLayoutPanelGroups.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeViewAdvPreviewTree)).EndInit();
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelControlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStripTreeActions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCut;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPaste;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewGroup;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemChangeNameOnGroup;
        private System.Windows.Forms.ImageList imageListTreeIcons;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCollapseAllNodes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExpandOnlyRootNodes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExpandAllNodes;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelTitle;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelGroups;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelControlButtons;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewAdvPreviewTree;
        private FindPersonsView findPersonsView1;
    }
}
