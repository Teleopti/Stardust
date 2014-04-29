using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Budgeting
{
    partial class BudgetGroupGroupNavigatorView
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
            if (disposing){
                if(components != null)
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BudgetGroupGroupNavigatorView));
			this.contextMenuStripBudgetGroup = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemBudgetGroupNewBudgetGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.imageListBudgetGroup = new System.Windows.Forms.ImageList(this.components);
			this.contextMenuBudgetGroup = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemNewBudgetGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemOpenBudgetGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemProperty = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripManagePlanningGroupFromSkill = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemPropertiesFromSkill = new System.Windows.Forms.ToolStripMenuItem();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.treeViewAdvBudgetGroups = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			this.toolStripBudgetGroups = new System.Windows.Forms.ToolStrip();
			this.toolStripLabelSkillActions = new System.Windows.Forms.ToolStripLabel();
			this.toolStripButtonNewBudgetGroup = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonOpenBudgetGroup = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonBudgetGroupProperties = new System.Windows.Forms.ToolStripButton();
			this.toolStripRoot = new System.Windows.Forms.ToolStrip();
			this.toolStripLabelNewPlanningGroup = new System.Windows.Forms.ToolStripLabel();
			this.toolStripButtonNewBudgetGroup2 = new System.Windows.Forms.ToolStripButton();
			this.contextMenuStripBudgetGroup.SuspendLayout();
			this.contextMenuBudgetGroup.SuspendLayout();
			this.contextMenuStripManagePlanningGroupFromSkill.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewAdvBudgetGroups)).BeginInit();
			this.toolStripBudgetGroups.SuspendLayout();
			this.toolStripRoot.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenuStripBudgetGroup
			// 
			this.contextMenuStripBudgetGroup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemBudgetGroupNewBudgetGroup});
			this.contextMenuStripBudgetGroup.Name = "contextMenuStripForecasts";
			this.contextMenuStripBudgetGroup.Size = new System.Drawing.Size(234, 26);
			// 
			// toolStripMenuItemBudgetGroupNewBudgetGroup
			// 
			this.toolStripMenuItemBudgetGroupNewBudgetGroup.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripMenuItemBudgetGroupNewBudgetGroup.Name = "toolStripMenuItemBudgetGroupNewBudgetGroup";
			this.toolStripMenuItemBudgetGroupNewBudgetGroup.Size = new System.Drawing.Size(233, 22);
			this.toolStripMenuItemBudgetGroupNewBudgetGroup.Text = "xxNewBudgetGroupThreeDots";
			this.toolStripMenuItemBudgetGroupNewBudgetGroup.Click += new System.EventHandler(this.toolStripButtonNewBudgetGroup_Click);
			// 
			// imageListBudgetGroup
			// 
			this.imageListBudgetGroup.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListBudgetGroup.ImageStream")));
			this.imageListBudgetGroup.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListBudgetGroup.Images.SetKeyName(0, "ccc_SkillGeneral.png");
			this.imageListBudgetGroup.Images.SetKeyName(1, "ccc_result_view_16x16.png");
			this.imageListBudgetGroup.Images.SetKeyName(2, "ccc_Site_16x16.png");
			// 
			// contextMenuBudgetGroup
			// 
			this.contextMenuBudgetGroup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemNewBudgetGroup,
            this.toolStripSeparator5,
            this.toolStripMenuItemOpenBudgetGroup,
            this.toolStripSeparator4,
            this.toolStripMenuItem6,
            this.toolStripSeparator3,
            this.toolStripMenuItemProperty});
			this.contextMenuBudgetGroup.Name = "contextMenuStrip1";
			this.contextMenuBudgetGroup.Size = new System.Drawing.Size(239, 126);
			// 
			// toolStripMenuItemNewBudgetGroup
			// 
			this.toolStripMenuItemNewBudgetGroup.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripMenuItemNewBudgetGroup.Name = "toolStripMenuItemNewBudgetGroup";
			this.toolStripMenuItemNewBudgetGroup.Padding = new System.Windows.Forms.Padding(0);
			this.toolStripMenuItemNewBudgetGroup.Size = new System.Drawing.Size(238, 20);
			this.toolStripMenuItemNewBudgetGroup.Text = "xxNewBudgetGroupThreeDots";
			this.toolStripMenuItemNewBudgetGroup.Click += new System.EventHandler(this.toolStripMenuItemNewBudgetGroup_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(235, 6);
			// 
			// toolStripMenuItemOpenBudgetGroup
			// 
			this.toolStripMenuItemOpenBudgetGroup.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Open_small;
			this.toolStripMenuItemOpenBudgetGroup.Name = "toolStripMenuItemOpenBudgetGroup";
			this.toolStripMenuItemOpenBudgetGroup.Padding = new System.Windows.Forms.Padding(0);
			this.toolStripMenuItemOpenBudgetGroup.Size = new System.Drawing.Size(238, 20);
			this.toolStripMenuItemOpenBudgetGroup.Text = "xxOpenBudgetGroupThreeDots";
			this.toolStripMenuItemOpenBudgetGroup.Click += new System.EventHandler(this.toolStripButtonOpenBudgetGroup_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(235, 6);
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.toolStripMenuItem6.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItem6.Size = new System.Drawing.Size(238, 22);
			this.toolStripMenuItem6.Text = "xxDelete";
			this.toolStripMenuItem6.Click += new System.EventHandler(this.toolStripButtonDelete_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(235, 6);
			// 
			// toolStripMenuItemProperty
			// 
			this.toolStripMenuItemProperty.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Properties_32x32;
			this.toolStripMenuItemProperty.Name = "toolStripMenuItemProperty";
			this.toolStripMenuItemProperty.Padding = new System.Windows.Forms.Padding(0);
			this.toolStripMenuItemProperty.Size = new System.Drawing.Size(238, 20);
			this.toolStripMenuItemProperty.Text = "xxPropertiesThreeDots";
			this.toolStripMenuItemProperty.Click += new System.EventHandler(this.toolStripButtonBudgetGroupProperties_Click);
			// 
			// contextMenuStripManagePlanningGroupFromSkill
			// 
			this.contextMenuStripManagePlanningGroupFromSkill.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemPropertiesFromSkill});
			this.contextMenuStripManagePlanningGroupFromSkill.Name = "contextMenuStrip1";
			this.contextMenuStripManagePlanningGroupFromSkill.Size = new System.Drawing.Size(192, 26);
			// 
			// toolStripMenuItemPropertiesFromSkill
			// 
			this.toolStripMenuItemPropertiesFromSkill.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Properties_32x32;
			this.toolStripMenuItemPropertiesFromSkill.Name = "toolStripMenuItemPropertiesFromSkill";
			this.toolStripMenuItemPropertiesFromSkill.Size = new System.Drawing.Size(191, 22);
			this.toolStripMenuItemPropertiesFromSkill.Text = "xxPropertiesThreeDots";
			// 
			// splitContainer1
			// 
			this.splitContainer1.BackColor = System.Drawing.Color.Silver;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = new System.Drawing.Point(10, 10);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeViewAdvBudgetGroups);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.AutoScroll = true;
			this.splitContainer1.Panel2.BackColor = System.Drawing.Color.White;
			this.splitContainer1.Panel2.Controls.Add(this.toolStripBudgetGroups);
			this.splitContainer1.Panel2.Controls.Add(this.toolStripRoot);
			this.splitContainer1.Panel2.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
			this.splitContainer1.Size = new System.Drawing.Size(198, 536);
			this.splitContainer1.SplitterDistance = 413;
			this.splitContainer1.SplitterWidth = 2;
			this.splitContainer1.TabIndex = 5;
			// 
			// treeViewAdvBudgetGroups
			// 
			this.treeViewAdvBudgetGroups.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeViewAdvBudgetGroups.ContextMenuStrip = this.contextMenuStripBudgetGroup;
			this.treeViewAdvBudgetGroups.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.treeViewAdvBudgetGroups.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdvBudgetGroups.HelpTextControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewAdvBudgetGroups.HelpTextControl.Name = "helpText";
			this.treeViewAdvBudgetGroups.HelpTextControl.Size = new System.Drawing.Size(49, 15);
			this.treeViewAdvBudgetGroups.HelpTextControl.TabIndex = 0;
			this.treeViewAdvBudgetGroups.HelpTextControl.Text = "help text";
			this.treeViewAdvBudgetGroups.LeftImageList = this.imageListBudgetGroup;
			this.treeViewAdvBudgetGroups.Location = new System.Drawing.Point(0, 0);
			this.treeViewAdvBudgetGroups.Name = "treeViewAdvBudgetGroups";
			this.treeViewAdvBudgetGroups.ShowFocusRect = true;
			this.treeViewAdvBudgetGroups.Size = new System.Drawing.Size(198, 413);
			this.treeViewAdvBudgetGroups.TabIndex = 0;
			this.treeViewAdvBudgetGroups.Text = "treeViewAdv1";
			this.treeViewAdvBudgetGroups.ThemesEnabled = true;
			// 
			// 
			// 
			this.treeViewAdvBudgetGroups.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
			this.treeViewAdvBudgetGroups.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdvBudgetGroups.ToolTipControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewAdvBudgetGroups.ToolTipControl.Name = "toolTip";
			this.treeViewAdvBudgetGroups.ToolTipControl.Size = new System.Drawing.Size(41, 15);
			this.treeViewAdvBudgetGroups.ToolTipControl.TabIndex = 1;
			this.treeViewAdvBudgetGroups.ToolTipControl.Text = "toolTip";
			this.treeViewAdvBudgetGroups.AfterSelect += new System.EventHandler(this.TreeViewAdvBudgetGroupsAfterSelect);
			// 
			// toolStripBudgetGroups
			// 
			this.toolStripBudgetGroups.BackColor = System.Drawing.Color.Transparent;
			this.toolStripBudgetGroups.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripBudgetGroups.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelSkillActions,
            this.toolStripButtonNewBudgetGroup,
            this.toolStripButtonOpenBudgetGroup,
            this.toolStripSeparator2,
            this.toolStripButtonDelete,
            this.toolStripSeparator1,
            this.toolStripButtonBudgetGroupProperties});
			this.toolStripBudgetGroups.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripBudgetGroups.Location = new System.Drawing.Point(0, 0);
			this.toolStripBudgetGroups.Name = "toolStripBudgetGroups";
			this.toolStripBudgetGroups.Padding = new System.Windows.Forms.Padding(1);
			this.toolStripBudgetGroups.Size = new System.Drawing.Size(181, 181);
			this.toolStripBudgetGroups.TabIndex = 5;
			this.toolStripBudgetGroups.Text = "xxActions";
			this.toolStripBudgetGroups.Visible = false;
			// 
			// toolStripLabelSkillActions
			// 
			this.toolStripLabelSkillActions.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.toolStripLabelSkillActions.Name = "toolStripLabelSkillActions";
			this.toolStripLabelSkillActions.Size = new System.Drawing.Size(178, 19);
			this.toolStripLabelSkillActions.Text = "xxActions";
			// 
			// toolStripButtonNewBudgetGroup
			// 
			this.toolStripButtonNewBudgetGroup.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripButtonNewBudgetGroup.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonNewBudgetGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonNewBudgetGroup.Name = "toolStripButtonNewBudgetGroup";
			this.toolStripButtonNewBudgetGroup.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonNewBudgetGroup.Size = new System.Drawing.Size(178, 28);
			this.toolStripButtonNewBudgetGroup.Text = "xxNewBudgetGroupThreeDots";
			this.toolStripButtonNewBudgetGroup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonNewBudgetGroup.Click += new System.EventHandler(this.toolStripButtonNewBudgetGroup_Click);
			// 
			// toolStripButtonOpenBudgetGroup
			// 
			this.toolStripButtonOpenBudgetGroup.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Open_small;
			this.toolStripButtonOpenBudgetGroup.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonOpenBudgetGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonOpenBudgetGroup.Name = "toolStripButtonOpenBudgetGroup";
			this.toolStripButtonOpenBudgetGroup.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonOpenBudgetGroup.Size = new System.Drawing.Size(178, 28);
			this.toolStripButtonOpenBudgetGroup.Text = "xxOpenBudgetGroupThreeDots";
			this.toolStripButtonOpenBudgetGroup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonOpenBudgetGroup.Click += new System.EventHandler(this.toolStripButtonOpenBudgetGroup_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(178, 6);
			// 
			// toolStripButtonDelete
			// 
			this.toolStripButtonDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripButtonDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDelete.Name = "toolStripButtonDelete";
			this.toolStripButtonDelete.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonDelete.Size = new System.Drawing.Size(178, 28);
			this.toolStripButtonDelete.Text = "xxDelete";
			this.toolStripButtonDelete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonDelete.Click += new System.EventHandler(this.toolStripButtonDelete_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(178, 6);
			// 
			// toolStripButtonBudgetGroupProperties
			// 
			this.toolStripButtonBudgetGroupProperties.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Properties_32x32;
			this.toolStripButtonBudgetGroupProperties.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonBudgetGroupProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonBudgetGroupProperties.Name = "toolStripButtonBudgetGroupProperties";
			this.toolStripButtonBudgetGroupProperties.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonBudgetGroupProperties.Size = new System.Drawing.Size(178, 28);
			this.toolStripButtonBudgetGroupProperties.Text = "xxPropertiesThreeDots";
			this.toolStripButtonBudgetGroupProperties.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonBudgetGroupProperties.Click += new System.EventHandler(this.toolStripButtonBudgetGroupProperties_Click);
			// 
			// toolStripRoot
			// 
			this.toolStripRoot.BackColor = System.Drawing.Color.Transparent;
			this.toolStripRoot.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripRoot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelNewPlanningGroup,
            this.toolStripButtonNewBudgetGroup2});
			this.toolStripRoot.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripRoot.Location = new System.Drawing.Point(0, 0);
			this.toolStripRoot.Name = "toolStripRoot";
			this.toolStripRoot.Padding = new System.Windows.Forms.Padding(1);
			this.toolStripRoot.Size = new System.Drawing.Size(198, 68);
			this.toolStripRoot.TabIndex = 5;
			this.toolStripRoot.Text = "xxActions";
			this.toolStripRoot.Visible = false;
			// 
			// toolStripLabelNewPlanningGroup
			// 
			this.toolStripLabelNewPlanningGroup.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.toolStripLabelNewPlanningGroup.Name = "toolStripLabelNewPlanningGroup";
			this.toolStripLabelNewPlanningGroup.Size = new System.Drawing.Size(195, 19);
			this.toolStripLabelNewPlanningGroup.Text = "xxActions";
			// 
			// toolStripButtonNewBudgetGroup2
			// 
			this.toolStripButtonNewBudgetGroup2.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripButtonNewBudgetGroup2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonNewBudgetGroup2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonNewBudgetGroup2.Name = "toolStripButtonNewBudgetGroup2";
			this.toolStripButtonNewBudgetGroup2.Size = new System.Drawing.Size(195, 20);
			this.toolStripButtonNewBudgetGroup2.Text = "xxNewBudgetGroupThreeDots";
			this.toolStripButtonNewBudgetGroup2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonNewBudgetGroup2.Click += new System.EventHandler(this.toolStripButtonNewBudgetGroup_Click);
			// 
			// BudgetGroupGroupNavigatorView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "BudgetGroupGroupNavigatorView";
			this.Size = new System.Drawing.Size(218, 556);
			this.Load += new System.EventHandler(this.BudgetGroupGroupNavigatorView_Load);
			this.contextMenuStripBudgetGroup.ResumeLayout(false);
			this.contextMenuBudgetGroup.ResumeLayout(false);
			this.contextMenuStripManagePlanningGroupFromSkill.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.treeViewAdvBudgetGroups)).EndInit();
			this.toolStripBudgetGroups.ResumeLayout(false);
			this.toolStripBudgetGroups.PerformLayout();
			this.toolStripRoot.ResumeLayout(false);
			this.toolStripRoot.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewAdvBudgetGroups;
        private System.Windows.Forms.ToolStrip toolStripBudgetGroups;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSkillActions;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStrip toolStripRoot;
        private System.Windows.Forms.ToolStripLabel toolStripLabelNewPlanningGroup;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripBudgetGroup;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBudgetGroupNewBudgetGroup;
        private System.Windows.Forms.ImageList imageListBudgetGroup;
        private System.Windows.Forms.ContextMenuStrip contextMenuBudgetGroup;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenBudgetGroup;
		  private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemProperty;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripManagePlanningGroupFromSkill;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPropertiesFromSkill;
        private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenBudgetGroup;
        private System.Windows.Forms.ToolStripButton toolStripButtonBudgetGroupProperties;
        private System.Windows.Forms.ToolStripButton toolStripButtonNewBudgetGroup;
        private System.Windows.Forms.ToolStripButton toolStripButtonNewBudgetGroup2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewBudgetGroup;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
    }
}
