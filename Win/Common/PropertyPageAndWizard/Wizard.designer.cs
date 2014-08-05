namespace Teleopti.Ccc.Win.Common.PropertyPageAndWizard
{
    partial class Wizard
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
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Wizard");
			this.buttonFinish = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonNext = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonBack = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.splitContainerPages = new System.Windows.Forms.SplitContainer();
			this.treeViewPages = new System.Windows.Forms.TreeView();
			this.splitContainerVertical = new System.Windows.Forms.SplitContainer();
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.labelHeading = new System.Windows.Forms.Label();
			this.splitContainerHorizontal = new System.Windows.Forms.SplitContainer();
			this.tableLayoutPanelButtonsRtl = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerPages)).BeginInit();
			this.splitContainerPages.Panel1.SuspendLayout();
			this.splitContainerPages.Panel2.SuspendLayout();
			this.splitContainerPages.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerVertical)).BeginInit();
			this.splitContainerVertical.Panel1.SuspendLayout();
			this.splitContainerVertical.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.gradientPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHorizontal)).BeginInit();
			this.splitContainerHorizontal.Panel1.SuspendLayout();
			this.splitContainerHorizontal.Panel2.SuspendLayout();
			this.splitContainerHorizontal.SuspendLayout();
			this.tableLayoutPanelButtonsRtl.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonFinish
			// 
			this.buttonFinish.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonFinish.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonFinish.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonFinish.BeforeTouchSize = new System.Drawing.Size(99, 21);
			this.buttonFinish.ForeColor = System.Drawing.Color.White;
			this.buttonFinish.IsBackStageButton = false;
			this.buttonFinish.Location = new System.Drawing.Point(408, 3);
			this.buttonFinish.Name = "buttonFinish";
			this.buttonFinish.Size = new System.Drawing.Size(99, 21);
			this.buttonFinish.TabIndex = 7;
			this.buttonFinish.Text = "xxFinishAmpersand";
			this.buttonFinish.UseVisualStyle = true;
			this.buttonFinish.UseVisualStyleBackColor = true;
			this.buttonFinish.Click += new System.EventHandler(this.buttonFinish_Click);
			// 
			// buttonNext
			// 
			this.buttonNext.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonNext.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonNext.BeforeTouchSize = new System.Drawing.Size(99, 21);
			this.buttonNext.ForeColor = System.Drawing.Color.White;
			this.buttonNext.IsBackStageButton = false;
			this.buttonNext.Location = new System.Drawing.Point(291, 3);
			this.buttonNext.Name = "buttonNext";
			this.buttonNext.Size = new System.Drawing.Size(99, 21);
			this.buttonNext.TabIndex = 6;
			this.buttonNext.Text = "xxNextAmpersandArrow";
			this.buttonNext.UseVisualStyle = true;
			this.buttonNext.UseVisualStyleBackColor = true;
			this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
			// 
			// buttonBack
			// 
			this.buttonBack.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonBack.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonBack.BeforeTouchSize = new System.Drawing.Size(99, 21);
			this.buttonBack.ForeColor = System.Drawing.Color.White;
			this.buttonBack.IsBackStageButton = false;
			this.buttonBack.Location = new System.Drawing.Point(174, 3);
			this.buttonBack.Name = "buttonBack";
			this.buttonBack.Size = new System.Drawing.Size(99, 21);
			this.buttonBack.TabIndex = 5;
			this.buttonBack.Text = "xxBackAmpersandArrow";
			this.buttonBack.UseVisualStyle = true;
			this.buttonBack.UseVisualStyleBackColor = true;
			this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.BeforeTouchSize = new System.Drawing.Size(99, 21);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.IsBackStageButton = false;
			this.buttonCancel.Location = new System.Drawing.Point(525, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(99, 21);
			this.buttonCancel.TabIndex = 8;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// splitContainerPages
			// 
			this.splitContainerPages.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.splitContainerPages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerPages.Location = new System.Drawing.Point(0, 0);
			this.splitContainerPages.Name = "splitContainerPages";
			// 
			// splitContainerPages.Panel1
			// 
			this.splitContainerPages.Panel1.Controls.Add(this.treeViewPages);
			this.splitContainerPages.Panel1.Padding = new System.Windows.Forms.Padding(12);
			this.splitContainerPages.Panel1MinSize = 120;
			// 
			// splitContainerPages.Panel2
			// 
			this.splitContainerPages.Panel2.Controls.Add(this.splitContainerVertical);
			this.splitContainerPages.Size = new System.Drawing.Size(627, 449);
			this.splitContainerPages.SplitterDistance = 170;
			this.splitContainerPages.SplitterWidth = 1;
			this.splitContainerPages.TabIndex = 6;
			this.splitContainerPages.TabStop = false;
			this.splitContainerPages.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainerPages_Paint);
			this.splitContainerPages.DoubleClick += new System.EventHandler(this.splitContainerPages_DoubleClick);
			// 
			// treeViewPages
			// 
			this.treeViewPages.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeViewPages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewPages.FullRowSelect = true;
			this.treeViewPages.Location = new System.Drawing.Point(12, 12);
			this.treeViewPages.Name = "treeViewPages";
			treeNode1.Name = "Node0";
			treeNode1.Text = "Wizard";
			this.treeViewPages.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
			this.treeViewPages.ShowPlusMinus = false;
			this.treeViewPages.ShowRootLines = false;
			this.treeViewPages.Size = new System.Drawing.Size(146, 425);
			this.treeViewPages.TabIndex = 0;
			this.treeViewPages.TabStop = false;
			this.treeViewPages.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewPages_BeforeCollapse);
			this.treeViewPages.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewPages_BeforeSelect);
			this.treeViewPages.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewPages_AfterSelect);
			// 
			// splitContainerVertical
			// 
			this.splitContainerVertical.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerVertical.IsSplitterFixed = true;
			this.splitContainerVertical.Location = new System.Drawing.Point(0, 0);
			this.splitContainerVertical.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainerVertical.Name = "splitContainerVertical";
			this.splitContainerVertical.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerVertical.Panel1
			// 
			this.splitContainerVertical.Panel1.Controls.Add(this.gradientPanel1);
			this.splitContainerVertical.Panel1MinSize = 40;
			this.splitContainerVertical.Size = new System.Drawing.Size(456, 449);
			this.splitContainerVertical.SplitterDistance = 41;
			this.splitContainerVertical.SplitterWidth = 1;
			this.splitContainerVertical.TabIndex = 0;
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.None, System.Drawing.Color.White, System.Drawing.Color.White);
			this.gradientPanel1.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.gradientPanel1.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel1.Controls.Add(this.labelHeading);
			this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Size = new System.Drawing.Size(456, 41);
			this.gradientPanel1.TabIndex = 0;
			// 
			// labelHeading
			// 
			this.labelHeading.AutoSize = true;
			this.labelHeading.BackColor = System.Drawing.Color.Transparent;
			this.labelHeading.Font = new System.Drawing.Font("Segoe UI", 14F);
			this.labelHeading.Location = new System.Drawing.Point(3, 5);
			this.labelHeading.Name = "labelHeading";
			this.labelHeading.Size = new System.Drawing.Size(101, 25);
			this.labelHeading.TabIndex = 0;
			this.labelHeading.Text = "xxHeading";
			// 
			// splitContainerHorizontal
			// 
			this.splitContainerHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerHorizontal.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainerHorizontal.IsSplitterFixed = true;
			this.splitContainerHorizontal.Location = new System.Drawing.Point(0, 0);
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
			this.splitContainerHorizontal.Size = new System.Drawing.Size(627, 477);
			this.splitContainerHorizontal.SplitterDistance = 449;
			this.splitContainerHorizontal.SplitterWidth = 1;
			this.splitContainerHorizontal.TabIndex = 7;
			this.splitContainerHorizontal.TabStop = false;
			// 
			// tableLayoutPanelButtonsRtl
			// 
			this.tableLayoutPanelButtonsRtl.ColumnCount = 4;
			this.tableLayoutPanelButtonsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelButtonsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
			this.tableLayoutPanelButtonsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
			this.tableLayoutPanelButtonsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
			this.tableLayoutPanelButtonsRtl.Controls.Add(this.buttonBack, 0, 0);
			this.tableLayoutPanelButtonsRtl.Controls.Add(this.buttonCancel, 3, 0);
			this.tableLayoutPanelButtonsRtl.Controls.Add(this.buttonFinish, 2, 0);
			this.tableLayoutPanelButtonsRtl.Controls.Add(this.buttonNext, 1, 0);
			this.tableLayoutPanelButtonsRtl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelButtonsRtl.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelButtonsRtl.Name = "tableLayoutPanelButtonsRtl";
			this.tableLayoutPanelButtonsRtl.RowCount = 1;
			this.tableLayoutPanelButtonsRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelButtonsRtl.Size = new System.Drawing.Size(627, 27);
			this.tableLayoutPanelButtonsRtl.TabIndex = 9;
			// 
			// Wizard
			// 
			this.AcceptButton = this.buttonNext;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(627, 477);
			this.Controls.Add(this.splitContainerHorizontal);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(639, 513);
			this.Name = "Wizard";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxWizard";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Wizard_FormClosed);
			this.Load += new System.EventHandler(this.PropertyPageWizard_Load);
			this.Shown += new System.EventHandler(this.Wizard_Shown);
			this.splitContainerPages.Panel1.ResumeLayout(false);
			this.splitContainerPages.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerPages)).EndInit();
			this.splitContainerPages.ResumeLayout(false);
			this.splitContainerVertical.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerVertical)).EndInit();
			this.splitContainerVertical.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.gradientPanel1.ResumeLayout(false);
			this.gradientPanel1.PerformLayout();
			this.splitContainerHorizontal.Panel1.ResumeLayout(false);
			this.splitContainerHorizontal.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHorizontal)).EndInit();
			this.splitContainerHorizontal.ResumeLayout(false);
			this.tableLayoutPanelButtonsRtl.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.ButtonAdv buttonFinish;
        private Syncfusion.Windows.Forms.ButtonAdv buttonNext;
        private Syncfusion.Windows.Forms.ButtonAdv buttonBack;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private System.Windows.Forms.SplitContainer splitContainerPages;
        private System.Windows.Forms.SplitContainer splitContainerHorizontal;
        private System.Windows.Forms.TreeView treeViewPages;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtonsRtl;
        private System.Windows.Forms.SplitContainer splitContainerVertical;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
        private System.Windows.Forms.Label labelHeading;
    }
}
