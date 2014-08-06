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
			this.components = new System.ComponentModel.Container();
			this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.splitContainerHorizontal = new System.Windows.Forms.SplitContainer();
			this.splitContainerPages = new System.Windows.Forms.SplitContainer();
			this.treeViewPages = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			this.tableLayoutPanelButtonsRtl = new System.Windows.Forms.TableLayoutPanel();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHorizontal)).BeginInit();
			this.splitContainerHorizontal.Panel1.SuspendLayout();
			this.splitContainerHorizontal.Panel2.SuspendLayout();
			this.splitContainerHorizontal.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerPages)).BeginInit();
			this.splitContainerPages.Panel1.SuspendLayout();
			this.splitContainerPages.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewPages)).BeginInit();
			this.tableLayoutPanelButtonsRtl.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOK.BeforeTouchSize = new System.Drawing.Size(99, 27);
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.IsBackStageButton = false;
			this.buttonOK.Location = new System.Drawing.Point(412, 7);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(99, 27);
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
			this.splitContainerHorizontal.Size = new System.Drawing.Size(627, 462);
			this.splitContainerHorizontal.SplitterDistance = 419;
			this.splitContainerHorizontal.SplitterWidth = 1;
			this.splitContainerHorizontal.TabIndex = 8;
			this.splitContainerHorizontal.TabStop = false;
			// 
			// splitContainerPages
			// 
			this.splitContainerPages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerPages.Location = new System.Drawing.Point(0, 0);
			this.splitContainerPages.Name = "splitContainerPages";
			// 
			// splitContainerPages.Panel1
			// 
			this.splitContainerPages.Panel1.Controls.Add(this.treeViewPages);
			this.splitContainerPages.Panel1.Padding = new System.Windows.Forms.Padding(12);
			this.splitContainerPages.Panel1MinSize = 120;
			this.splitContainerPages.Size = new System.Drawing.Size(627, 419);
			this.splitContainerPages.SplitterDistance = 171;
			this.splitContainerPages.SplitterWidth = 1;
			this.splitContainerPages.TabIndex = 6;
			this.splitContainerPages.TabStop = false;
			// 
			// treeViewPages
			// 
			this.treeViewPages.BackColor = System.Drawing.Color.White;
			this.treeViewPages.BeforeTouchSize = new System.Drawing.Size(147, 395);
			this.treeViewPages.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.treeViewPages.BorderColor = System.Drawing.Color.Transparent;
			this.treeViewPages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewPages.CanSelectDisabledNode = false;
			this.treeViewPages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewPages.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			// 
			// 
			// 
			this.treeViewPages.HelpTextControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewPages.HelpTextControl.Name = "helpText";
			this.treeViewPages.HelpTextControl.TabIndex = 0;
			this.treeViewPages.Location = new System.Drawing.Point(12, 12);
			this.treeViewPages.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.treeViewPages.Name = "treeViewPages";
			this.treeViewPages.SelectedNodeBackground = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220))))));
			this.treeViewPages.ShowFocusRect = false;
			this.treeViewPages.ShowPlusMinus = false;
			this.treeViewPages.ShowRootLines = false;
			this.treeViewPages.Size = new System.Drawing.Size(147, 395);
			this.treeViewPages.Style = Syncfusion.Windows.Forms.Tools.TreeStyle.Metro;
			this.treeViewPages.TabIndex = 0;
			this.treeViewPages.TabStop = false;
			// 
			// 
			// 
			this.treeViewPages.ToolTipControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewPages.ToolTipControl.Name = "toolTip";
			this.treeViewPages.ToolTipControl.TabIndex = 1;
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
			this.tableLayoutPanelButtonsRtl.Size = new System.Drawing.Size(627, 42);
			this.tableLayoutPanelButtonsRtl.TabIndex = 4;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.BeforeTouchSize = new System.Drawing.Size(99, 27);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.IsBackStageButton = false;
			this.buttonCancel.Location = new System.Drawing.Point(525, 7);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(99, 27);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// PropertiesPages
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(627, 462);
			this.Controls.Add(this.splitContainerHorizontal);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(639, 502);
			this.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(639, 502);
			this.Name = "PropertiesPages";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxProperties";
			this.Load += new System.EventHandler(this.propertiesPagesLoad);
			this.splitContainerHorizontal.Panel1.ResumeLayout(false);
			this.splitContainerHorizontal.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHorizontal)).EndInit();
			this.splitContainerHorizontal.ResumeLayout(false);
			this.splitContainerPages.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerPages)).EndInit();
			this.splitContainerPages.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.treeViewPages)).EndInit();
			this.tableLayoutPanelButtonsRtl.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

		private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewPages;
        private System.Windows.Forms.SplitContainer splitContainerPages;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
        private System.Windows.Forms.SplitContainer splitContainerHorizontal;
		private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtonsRtl;

    }
}
