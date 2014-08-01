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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.treeViewSkills = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewSkills)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.treeViewSkills, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 288F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(416, 288);
			this.tableLayoutPanel1.TabIndex = 8;
			// 
			// treeViewSkills
			// 
			this.treeViewSkills.BackColor = System.Drawing.Color.White;
			this.treeViewSkills.BeforeTouchSize = new System.Drawing.Size(410, 282);
			this.treeViewSkills.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.treeViewSkills.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeViewSkills.CanSelectDisabledNode = false;
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
			this.treeViewSkills.Location = new System.Drawing.Point(3, 3);
			this.treeViewSkills.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.treeViewSkills.Name = "treeViewSkills";
			this.treeViewSkills.SelectedNodeBackground = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220))))));
			this.treeViewSkills.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectSameLevel;
			this.treeViewSkills.ShowCheckBoxes = true;
			this.treeViewSkills.ShowFocusRect = false;
			this.treeViewSkills.Size = new System.Drawing.Size(410, 282);
			this.treeViewSkills.Style = Syncfusion.Windows.Forms.Tools.TreeStyle.Metro;
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
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "SelectWorkload";
			this.Size = new System.Drawing.Size(416, 288);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.treeViewSkills)).EndInit();
			this.ResumeLayout(false);

        }
        
        #endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewSkills;




    }
}
