namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    partial class SelectWorkload
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
			this.textBoxExFilter = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.treeViewSkills = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExFilter)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewSkills)).BeginInit();
			this.SuspendLayout();
			// 
			// textBoxExFilter
			// 
			this.textBoxExFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxExFilter.Location = new System.Drawing.Point(75, 3);
			this.textBoxExFilter.Name = "textBoxExFilter";
			this.textBoxExFilter.Size = new System.Drawing.Size(273, 20);
			this.textBoxExFilter.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.treeViewSkills, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(357, 250);
			this.tableLayoutPanel1.TabIndex = 8;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.textBoxExFilter, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(351, 24);
			this.tableLayoutPanel2.TabIndex = 7;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "xxFilterColon";
			// 
			// treeViewSkills
			// 
			this.treeViewSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.treeViewSkills.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewSkills.HelpTextControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewSkills.HelpTextControl.Name = "helpText";
			this.treeViewSkills.HelpTextControl.Size = new System.Drawing.Size(49, 15);
			this.treeViewSkills.HelpTextControl.TabIndex = 0;
			this.treeViewSkills.HelpTextControl.Text = "help text";
			this.treeViewSkills.InteractiveCheckBoxes = true;
			this.treeViewSkills.Location = new System.Drawing.Point(3, 33);
			this.treeViewSkills.Name = "treeViewSkills";
			this.treeViewSkills.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectSameLevel;
			this.treeViewSkills.ShowCheckBoxes = true;
			this.treeViewSkills.ShowFocusRect = true;
			this.treeViewSkills.Size = new System.Drawing.Size(351, 214);
			this.treeViewSkills.TabIndex = 8;
			this.treeViewSkills.Text = "treeViewAdv1";
			// 
			// 
			// 
			this.treeViewSkills.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
			this.treeViewSkills.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewSkills.ToolTipControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewSkills.ToolTipControl.Name = "toolTip";
			this.treeViewSkills.ToolTipControl.Size = new System.Drawing.Size(41, 15);
			this.treeViewSkills.ToolTipControl.TabIndex = 1;
			this.treeViewSkills.ToolTipControl.Text = "toolTip";
			// 
			// SelectWorkload
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SelectWorkload";
			this.Size = new System.Drawing.Size(357, 250);
			((System.ComponentModel.ISupportInitialize)(this.textBoxExFilter)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewSkills)).EndInit();
			this.ResumeLayout(false);

        }
        
        #endregion

		private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExFilter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
		private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewSkills;




    }
}
