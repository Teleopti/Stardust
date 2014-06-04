namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class StateGroupControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
			this.labelHeader = new System.Windows.Forms.Label();
			this.treeViewAdv1 = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonDelete = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonNew = new System.Windows.Forms.Button();
			this.labelSubHeader1 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
			this.gradientPanelHeader.SuspendLayout();
			this.tableLayoutPanelHeader.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewAdv1)).BeginInit();
			this.tableLayoutPanelSubHeader1.SuspendLayout();
			this.tableLayoutPanelBody.SuspendLayout();
			this.SuspendLayout();
			// 
			// gradientPanelHeader
			// 
			this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.gradientPanelHeader.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.gradientPanelHeader.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.White);
			this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
			this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
			this.gradientPanelHeader.Name = "gradientPanelHeader";
			this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(10);
			this.gradientPanelHeader.Size = new System.Drawing.Size(550, 55);
			this.gradientPanelHeader.TabIndex = 57;
			// 
			// tableLayoutPanelHeader
			// 
			this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelHeader.ColumnCount = 1;
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 530F));
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
			this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelHeader.Location = new System.Drawing.Point(10, 10);
			this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
			this.tableLayoutPanelHeader.RowCount = 1;
			this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Size = new System.Drawing.Size(530, 35);
			this.tableLayoutPanelHeader.TabIndex = 0;
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHeader.AutoSize = true;
			this.labelHeader.Font = new System.Drawing.Font("Tahoma", 11.25F);
			this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelHeader.Location = new System.Drawing.Point(3, 8);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.labelHeader.Size = new System.Drawing.Size(227, 18);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "xxManageStateGroupsAndStates";
			// 
			// treeViewAdv1
			// 
			this.treeViewAdv1.BeforeTouchSize = new System.Drawing.Size(544, 353);
			this.treeViewAdv1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeViewAdv1.CanSelectDisabledNode = false;
			this.treeViewAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.treeViewAdv1.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdv1.HelpTextControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewAdv1.HelpTextControl.Name = "helpText";
			this.treeViewAdv1.HelpTextControl.Size = new System.Drawing.Size(49, 15);
			this.treeViewAdv1.HelpTextControl.TabIndex = 0;
			this.treeViewAdv1.HelpTextControl.Text = "help text";
			this.treeViewAdv1.Location = new System.Drawing.Point(3, 36);
			this.treeViewAdv1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.treeViewAdv1.Name = "treeViewAdv1";
			this.treeViewAdv1.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectSameLevel;
			this.treeViewAdv1.ShowFocusRect = true;
			this.treeViewAdv1.Size = new System.Drawing.Size(544, 353);
			this.treeViewAdv1.Style = Syncfusion.Windows.Forms.Tools.TreeStyle.Default;
			this.treeViewAdv1.TabIndex = 65;
			this.treeViewAdv1.Text = "treeViewAdv1";
			// 
			// 
			// 
			this.treeViewAdv1.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
			this.treeViewAdv1.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdv1.ToolTipControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewAdv1.ToolTipControl.Name = "toolTip";
			this.treeViewAdv1.ToolTipControl.Size = new System.Drawing.Size(41, 15);
			this.treeViewAdv1.ToolTipControl.TabIndex = 1;
			this.treeViewAdv1.ToolTipControl.Text = "toolTip";
			this.treeViewAdv1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewAdv1_ItemDrag);
			this.treeViewAdv1.NodeEditorValidateString += new Syncfusion.Windows.Forms.Tools.TreeNodeAdvCancelableEditEventHandler(this.treeViewAdv1_NodeEditorValidateString);
			this.treeViewAdv1.NodeEditorValidating += new Syncfusion.Windows.Forms.Tools.TreeNodeAdvCancelableEditEventHandler(this.treeViewAdv1_NodeEditorValidating);
			this.treeViewAdv1.NodeEditorValidated += new Syncfusion.Windows.Forms.Tools.TreeNodeAdvEditEventHandler(this.treeViewAdv1_NodeEditorValidated);
			this.treeViewAdv1.EditCancelled += new Syncfusion.Windows.Forms.Tools.TreeNodeAdvEditEventHandler(this.treeViewAdv1_EditCancelled);
			this.treeViewAdv1.BeforeEdit += new Syncfusion.Windows.Forms.Tools.TreeViewAdvBeforeEditEventHandler(this.treeViewAdv1_BeforeEdit);
			this.treeViewAdv1.BeforeCheck += new Syncfusion.Windows.Forms.Tools.TreeViewAdvBeforeCheckEventHandler(this.treeViewAdv1BeforeCheck);
			this.treeViewAdv1.AfterCheck += new Syncfusion.Windows.Forms.Tools.TreeNodeAdvEventHandler(this.treeViewAdv1_AfterCheck);
			this.treeViewAdv1.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeViewAdv1DragDrop);
			this.treeViewAdv1.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeViewAdv1DragOver);
			this.treeViewAdv1.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.treeViewAdv1_QueryContinueDrag);
			this.treeViewAdv1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeViewAdv1MouseDown);
			// 
			// tableLayoutPanelSubHeader1
			// 
			this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader1.ColumnCount = 3;
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonDelete, 2, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonNew, 1, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
			this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
			this.tableLayoutPanelSubHeader1.RowCount = 1;
			this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(544, 27);
			this.tableLayoutPanelSubHeader1.TabIndex = 66;
			// 
			// buttonDelete
			// 
			this.buttonDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonDelete.BeforeTouchSize = new System.Drawing.Size(24, 24);
			this.buttonDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_delete_32x32;
			this.buttonDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.buttonDelete.IsBackStageButton = false;
			this.buttonDelete.Location = new System.Drawing.Point(520, 1);
			this.buttonDelete.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.Size = new System.Drawing.Size(24, 24);
			this.buttonDelete.TabIndex = 7;
			this.buttonDelete.TabStop = false;
			// 
			// buttonNew
			// 
			this.buttonNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonNew.BackColor = System.Drawing.Color.White;
			this.buttonNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
			this.buttonNew.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.buttonNew.Location = new System.Drawing.Point(493, 1);
			this.buttonNew.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
			this.buttonNew.Name = "buttonNew";
			this.buttonNew.Size = new System.Drawing.Size(24, 24);
			this.buttonNew.TabIndex = 6;
			this.buttonNew.UseVisualStyleBackColor = false;
			// 
			// labelSubHeader1
			// 
			this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSubHeader1.AutoSize = true;
			this.labelSubHeader1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelSubHeader1.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelSubHeader1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelSubHeader1.Location = new System.Drawing.Point(3, 7);
			this.labelSubHeader1.Name = "labelSubHeader1";
			this.labelSubHeader1.Size = new System.Drawing.Size(92, 13);
			this.labelSubHeader1.TabIndex = 0;
			this.labelSubHeader1.Text = "xxStateGroups";
			this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanelBody
			// 
			this.tableLayoutPanelBody.ColumnCount = 1;
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Controls.Add(this.treeViewAdv1, 0, 1);
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
			this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 55);
			this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
			this.tableLayoutPanelBody.RowCount = 2;
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Size = new System.Drawing.Size(550, 392);
			this.tableLayoutPanelBody.TabIndex = 67;
			// 
			// StateGroupControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanelBody);
			this.Controls.Add(this.gradientPanelHeader);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "StateGroupControl";
			this.Size = new System.Drawing.Size(550, 447);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
			this.gradientPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewAdv1)).EndInit();
			this.tableLayoutPanelSubHeader1.ResumeLayout(false);
			this.tableLayoutPanelSubHeader1.PerformLayout();
			this.tableLayoutPanelBody.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewAdv1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonDelete;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Label labelSubHeader1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
    }
}
