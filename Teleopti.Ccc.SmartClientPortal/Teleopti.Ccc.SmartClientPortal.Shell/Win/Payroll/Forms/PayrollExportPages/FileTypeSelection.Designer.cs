namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.Forms.PayrollExportPages
{
    partial class FileTypeSelection
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
            Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo treeNodeAdvStyleInfo1 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo();
            Syncfusion.Windows.Forms.Tools.TreeNodeAdv treeNodeAdv1 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdv();
            Syncfusion.Windows.Forms.Tools.TreeNodeAdv treeNodeAdv2 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdv();
            Syncfusion.Windows.Forms.Tools.TreeNodeAdv treeNodeAdv3 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdv();
            this.treeViewAdvExportType = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            ((System.ComponentModel.ISupportInitialize) (this.treeViewAdvExportType)).BeginInit();
            this.SuspendLayout();
            // 
            // treeViewAdvExportType
            // 
            treeNodeAdvStyleInfo1.EnsureDefaultOptionedChild = true;
            this.treeViewAdvExportType.BaseStylePairs.AddRange(new Syncfusion.Windows.Forms.Tools.StyleNamePair[] {
            new Syncfusion.Windows.Forms.Tools.StyleNamePair("Standard", treeNodeAdvStyleInfo1)});
            this.treeViewAdvExportType.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.treeViewAdvExportType.BorderColor = System.Drawing.Color.Transparent;
            this.treeViewAdvExportType.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewAdvExportType.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // 
            // 
            this.treeViewAdvExportType.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewAdvExportType.HelpTextControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewAdvExportType.HelpTextControl.Name = "helpText";
            this.treeViewAdvExportType.HelpTextControl.Size = new System.Drawing.Size(49, 15);
            this.treeViewAdvExportType.HelpTextControl.TabIndex = 0;
            this.treeViewAdvExportType.HelpTextControl.Text = "help text";
            this.treeViewAdvExportType.LineColor = System.Drawing.Color.Transparent;
            this.treeViewAdvExportType.Location = new System.Drawing.Point(10, 10);
            this.treeViewAdvExportType.Name = "treeViewAdvExportType";
            treeNodeAdv1.ChildStyle.EnsureDefaultOptionedChild = true;
            treeNodeAdv1.EnsureDefaultOptionedChild = true;
            treeNodeAdv1.Height = 30;
            treeNodeAdv1.Text = "Export Type 1";
            treeNodeAdv2.ChildStyle.EnsureDefaultOptionedChild = true;
            treeNodeAdv2.EnsureDefaultOptionedChild = true;
            treeNodeAdv2.Height = 30;
            treeNodeAdv2.Text = "Export Type 2";
            treeNodeAdv3.ChildStyle.EnsureDefaultOptionedChild = true;
            treeNodeAdv3.EnsureDefaultOptionedChild = true;
            treeNodeAdv3.Height = 30;
            treeNodeAdv3.Text = "Export Type 3";
            this.treeViewAdvExportType.Nodes.AddRange(new Syncfusion.Windows.Forms.Tools.TreeNodeAdv[] {
            treeNodeAdv1,
            treeNodeAdv2,
            treeNodeAdv3});
            this.treeViewAdvExportType.SelectedNodeBackground = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.HighlightText);
            this.treeViewAdvExportType.SelectedNodeForeColor = System.Drawing.SystemColors.ControlText;
            this.treeViewAdvExportType.SelectOnCollapse = false;
            this.treeViewAdvExportType.ShouldSelectNodeOnEnter = false;
            this.treeViewAdvExportType.ShowCheckBoxes = false;
            this.treeViewAdvExportType.ShowDragNodeCue = false;
            this.treeViewAdvExportType.ShowLines = false;
            this.treeViewAdvExportType.ShowOptionButtons = true;
            this.treeViewAdvExportType.ShowPlusMinus = false;
            this.treeViewAdvExportType.ShowRootLines = false;
            this.treeViewAdvExportType.Size = new System.Drawing.Size(280, 180);
            this.treeViewAdvExportType.TabIndex = 2;
            this.treeViewAdvExportType.Text = "treeViewAdv1";
            this.treeViewAdvExportType.ThemesEnabled = true;
            // 
            // 
            // 
            this.treeViewAdvExportType.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
            this.treeViewAdvExportType.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewAdvExportType.ToolTipControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewAdvExportType.ToolTipControl.Name = "toolTip";
            this.treeViewAdvExportType.ToolTipControl.Size = new System.Drawing.Size(41, 15);
            this.treeViewAdvExportType.ToolTipControl.TabIndex = 1;
            this.treeViewAdvExportType.ToolTipControl.Text = "toolTip";
            // 
            // FileTypeSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.treeViewAdvExportType);
            this.Name = "FileTypeSelection";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(300, 200);
            ((System.ComponentModel.ISupportInitialize) (this.treeViewAdvExportType)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewAdvExportType;

    }
}
