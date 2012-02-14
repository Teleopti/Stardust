using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Payroll
{
    partial class DefinitionSetNavigationView
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
            this.tvDefinitionSets = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            ((System.ComponentModel.ISupportInitialize)(this.tvDefinitionSets)).BeginInit();
            this.SuspendLayout();
            // 
            // tvDefinitionSets
            // 
            this.tvDefinitionSets.AccelerateScrolling = Syncfusion.Windows.Forms.AccelerateScrollingBehavior.Immediate;
            treeNodeAdvStyleInfo1.EnsureDefaultOptionedChild = true;
            this.tvDefinitionSets.BaseStylePairs.AddRange(new Syncfusion.Windows.Forms.Tools.StyleNamePair[] {
            new Syncfusion.Windows.Forms.Tools.StyleNamePair("Standard", treeNodeAdvStyleInfo1)});
            this.tvDefinitionSets.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.tvDefinitionSets.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // 
            // 
            this.tvDefinitionSets.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tvDefinitionSets.HelpTextControl.Location = new System.Drawing.Point(0, 0);
            this.tvDefinitionSets.HelpTextControl.Name = "helpText";
            this.tvDefinitionSets.HelpTextControl.Size = new System.Drawing.Size(49, 15);
            this.tvDefinitionSets.HelpTextControl.TabIndex = 0;
            this.tvDefinitionSets.HelpTextControl.Text = "help text";
            this.tvDefinitionSets.HideSelection = false;
            this.tvDefinitionSets.LineStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            this.tvDefinitionSets.Location = new System.Drawing.Point(0, 0);
            this.tvDefinitionSets.Name = "tvDefinitionSets";
            this.tvDefinitionSets.Size = new System.Drawing.Size(175, 150);
            this.tvDefinitionSets.TabIndex = 0;
            // 
            // 
            // 
            this.tvDefinitionSets.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
            this.tvDefinitionSets.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tvDefinitionSets.ToolTipControl.Location = new System.Drawing.Point(0, 0);
            this.tvDefinitionSets.ToolTipControl.Name = "toolTip";
            this.tvDefinitionSets.ToolTipControl.Size = new System.Drawing.Size(41, 15);
            this.tvDefinitionSets.ToolTipControl.TabIndex = 1;
            this.tvDefinitionSets.ToolTipControl.Text = "toolTip";
            this.tvDefinitionSets.Click += new System.EventHandler(this.tvDefinitionSets_Click);
            // 
            // DefinitionSetNavigationView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvDefinitionSets);
            this.Name = "DefinitionSetNavigationView";
            ((System.ComponentModel.ISupportInitialize)(this.tvDefinitionSets)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TreeViewAdv tvDefinitionSets;
    }
}
