namespace Teleopti.Ccc.Win.Shifts
{
    partial class PromptDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PromptDialog));
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.treeViewAdvLoV = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			this.btnOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewAdvLoV)).BeginInit();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ccc_ShiftRuleSet.png");
			this.imageList1.Images.SetKeyName(1, "ccc_ShiftBag.png");
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.treeViewAdvLoV, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnOk, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(587, 693);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// treeViewAdvLoV
			// 
			this.treeViewAdvLoV.AccelerateScrolling = Syncfusion.Windows.Forms.AccelerateScrollingBehavior.Immediate;
			this.treeViewAdvLoV.BackColor = System.Drawing.Color.White;
			this.treeViewAdvLoV.BeforeTouchSize = new System.Drawing.Size(581, 654);
			this.treeViewAdvLoV.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.treeViewAdvLoV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdvLoV.CanSelectDisabledNode = false;
			this.treeViewAdvLoV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewAdvLoV.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			// 
			// 
			// 
			this.treeViewAdvLoV.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdvLoV.HelpTextControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewAdvLoV.HelpTextControl.Name = "helpText";
			this.treeViewAdvLoV.HelpTextControl.Size = new System.Drawing.Size(62, 15);
			this.treeViewAdvLoV.HelpTextControl.TabIndex = 0;
			this.treeViewAdvLoV.HelpTextControl.Text = "xxHelpText";
			this.treeViewAdvLoV.LineStyle = System.Drawing.Drawing2D.DashStyle.Solid;
			this.treeViewAdvLoV.LoadOnDemand = true;
			this.treeViewAdvLoV.Location = new System.Drawing.Point(3, 3);
			this.treeViewAdvLoV.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.treeViewAdvLoV.Name = "treeViewAdvLoV";
			this.treeViewAdvLoV.OwnerDrawNodes = true;
			this.treeViewAdvLoV.SelectedNodeBackground = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220))))));
			this.treeViewAdvLoV.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectSameLevel;
			this.treeViewAdvLoV.ShowCheckBoxes = true;
			this.treeViewAdvLoV.ShowFocusRect = false;
			this.treeViewAdvLoV.ShowPlusMinus = false;
			this.treeViewAdvLoV.Size = new System.Drawing.Size(581, 654);
			this.treeViewAdvLoV.SortWithChildNodes = true;
			this.treeViewAdvLoV.Style = Syncfusion.Windows.Forms.Tools.TreeStyle.Metro;
			this.treeViewAdvLoV.TabIndex = 33;
			this.treeViewAdvLoV.Text = "yytreeViewAdv1";
			this.treeViewAdvLoV.ThemesEnabled = true;
			// 
			// 
			// 
			this.treeViewAdvLoV.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
			this.treeViewAdvLoV.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewAdvLoV.ToolTipControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewAdvLoV.ToolTipControl.Name = "toolTip";
			this.treeViewAdvLoV.ToolTipControl.Size = new System.Drawing.Size(51, 15);
			this.treeViewAdvLoV.ToolTipControl.TabIndex = 1;
			this.treeViewAdvLoV.ToolTipControl.Text = "yytoolTip";
			// 
			// btnOk
			// 
			this.btnOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnOk.ForeColor = System.Drawing.Color.White;
			this.btnOk.IsBackStageButton = false;
			this.btnOk.Location = new System.Drawing.Point(497, 663);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(87, 27);
			this.btnOk.TabIndex = 34;
			this.btnOk.Text = "xxOk";
			this.btnOk.UseVisualStyle = true;
			this.btnOk.Click += new System.EventHandler(this.btnOkClick);
			// 
			// PromptDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(587, 693);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PromptDialog";
			this.ShowIcon = false;
			this.Text = "Caption";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.promptDialogFormClosed);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.treeViewAdvLoV)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewAdvLoV;
        private Syncfusion.Windows.Forms.ButtonAdv btnOk;
    }
}
