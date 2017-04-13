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
			this.splitContainerPages = new System.Windows.Forms.SplitContainer();
			this.treeViewPages = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			this.tableLayoutPanelButtonsRtl = new System.Windows.Forms.TableLayoutPanel();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanelContainer = new System.Windows.Forms.TableLayoutPanel();
			this.labelHeading = new System.Windows.Forms.Label();
			this.panelContainer = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerPages)).BeginInit();
			this.splitContainerPages.Panel1.SuspendLayout();
			this.splitContainerPages.Panel2.SuspendLayout();
			this.splitContainerPages.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewPages)).BeginInit();
			this.tableLayoutPanelButtonsRtl.SuspendLayout();
			this.tableLayoutPanelContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOK.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.IsBackStageButton = false;
			this.buttonOK.Location = new System.Drawing.Point(421, 7);
			this.buttonOK.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 27);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "xxOk";
			this.buttonOK.UseVisualStyle = true;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOkClick);
			// 
			// splitContainerPages
			// 
			this.splitContainerPages.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainerPages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerPages.IsSplitterFixed = true;
			this.splitContainerPages.Location = new System.Drawing.Point(0, 0);
			this.splitContainerPages.Name = "splitContainerPages";
			// 
			// splitContainerPages.Panel1
			// 
			this.splitContainerPages.Panel1.BackColor = System.Drawing.Color.White;
			this.splitContainerPages.Panel1.Controls.Add(this.treeViewPages);
			this.splitContainerPages.Panel1.Padding = new System.Windows.Forms.Padding(12);
			this.splitContainerPages.Panel1MinSize = 120;
			// 
			// splitContainerPages.Panel2
			// 
			this.splitContainerPages.Panel2.BackColor = System.Drawing.Color.White;
			this.splitContainerPages.Panel2.Controls.Add(this.tableLayoutPanelContainer);
			this.splitContainerPages.Size = new System.Drawing.Size(638, 411);
			this.splitContainerPages.SplitterDistance = 174;
			this.splitContainerPages.SplitterWidth = 3;
			this.splitContainerPages.TabIndex = 6;
			this.splitContainerPages.TabStop = false;
			this.splitContainerPages.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainerPages_SplitterMoved);
			// 
			// treeViewPages
			// 
			this.treeViewPages.BackColor = System.Drawing.Color.White;
			this.treeViewPages.BeforeTouchSize = new System.Drawing.Size(150, 387);
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
			this.treeViewPages.HideSelection = false;
			this.treeViewPages.ItemHeight = 25;
			this.treeViewPages.Location = new System.Drawing.Point(12, 12);
			this.treeViewPages.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.treeViewPages.Name = "treeViewPages";
			this.treeViewPages.SelectedNodeBackground = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255))))));
			this.treeViewPages.ShowFocusRect = false;
			this.treeViewPages.ShowPlusMinus = false;
			this.treeViewPages.ShowRootLines = false;
			this.treeViewPages.Size = new System.Drawing.Size(150, 387);
			this.treeViewPages.Style = Syncfusion.Windows.Forms.Tools.TreeStyle.Metro;
			this.treeViewPages.TabIndex = 0;
			this.treeViewPages.TabStop = false;
			// 
			// 
			// 
			this.treeViewPages.ToolTipControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewPages.ToolTipControl.Name = "toolTip";
			this.treeViewPages.ToolTipControl.TabIndex = 1;
			this.treeViewPages.AfterSelect += new System.EventHandler(this.treeViewPagesAfterSelect);
			// 
			// tableLayoutPanelButtonsRtl
			// 
			this.tableLayoutPanelButtonsRtl.ColumnCount = 2;
			this.tableLayoutPanelButtonsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelButtonsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelButtonsRtl.Controls.Add(this.buttonOK, 0, 0);
			this.tableLayoutPanelButtonsRtl.Controls.Add(this.buttonCancel, 1, 0);
			this.tableLayoutPanelButtonsRtl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tableLayoutPanelButtonsRtl.Location = new System.Drawing.Point(0, 411);
			this.tableLayoutPanelButtonsRtl.Name = "tableLayoutPanelButtonsRtl";
			this.tableLayoutPanelButtonsRtl.RowCount = 1;
			this.tableLayoutPanelButtonsRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelButtonsRtl.Size = new System.Drawing.Size(638, 49);
			this.tableLayoutPanelButtonsRtl.TabIndex = 4;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.IsBackStageButton = false;
			this.buttonCancel.Location = new System.Drawing.Point(541, 7);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanelContainer
			// 
			this.tableLayoutPanelContainer.ColumnCount = 1;
			this.tableLayoutPanelContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelContainer.Controls.Add(this.labelHeading, 0, 0);
			this.tableLayoutPanelContainer.Controls.Add(this.panelContainer, 0, 1);
			this.tableLayoutPanelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelContainer.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelContainer.Name = "tableLayoutPanelContainer";
			this.tableLayoutPanelContainer.RowCount = 2;
			this.tableLayoutPanelContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelContainer.Size = new System.Drawing.Size(461, 411);
			this.tableLayoutPanelContainer.TabIndex = 0;
			// 
			// labelHeading
			// 
			this.labelHeading.AutoSize = true;
			this.labelHeading.BackColor = System.Drawing.Color.Transparent;
			this.labelHeading.Font = new System.Drawing.Font("Segoe UI", 14F);
			this.labelHeading.Location = new System.Drawing.Point(3, 3);
			this.labelHeading.Margin = new System.Windows.Forms.Padding(3);
			this.labelHeading.Name = "labelHeading";
			this.labelHeading.Size = new System.Drawing.Size(101, 25);
			this.labelHeading.TabIndex = 1;
			this.labelHeading.Text = "xxHeading";
			// 
			// panelContainer
			// 
			this.panelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelContainer.Location = new System.Drawing.Point(3, 43);
			this.panelContainer.Name = "panelContainer";
			this.panelContainer.Size = new System.Drawing.Size(455, 365);
			this.panelContainer.TabIndex = 2;
			// 
			// PropertiesPages
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(638, 460);
			this.Controls.Add(this.splitContainerPages);
			this.Controls.Add(this.tableLayoutPanelButtonsRtl);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.MinimizeBox = false;
			this.Name = "PropertiesPages";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxProperties";
			this.Load += new System.EventHandler(this.propertiesPagesLoad);
			this.splitContainerPages.Panel1.ResumeLayout(false);
			this.splitContainerPages.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerPages)).EndInit();
			this.splitContainerPages.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.treeViewPages)).EndInit();
			this.tableLayoutPanelButtonsRtl.ResumeLayout(false);
			this.tableLayoutPanelContainer.ResumeLayout(false);
			this.tableLayoutPanelContainer.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

		private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewPages;
        private System.Windows.Forms.SplitContainer splitContainerPages;
		private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
		private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtonsRtl;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelContainer;
		private System.Windows.Forms.Label labelHeading;
		private System.Windows.Forms.Panel panelContainer;

    }
}
