using System;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	partial class SettingsScreen
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
			if (disposing)
			{
				if (components != null) components.Dispose();
				if (_core != null) _core.Dispose();
				if (_timer != null) _timer.Dispose();
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.splitContainer = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.treeViewOptions = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			this.PanelContent = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvApply = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewOptions)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PanelContent)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.CausesValidation = false;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.Controls.Add(this.splitContainer, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvOK, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 3, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvApply, 2, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1162, 803);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// splitContainer
			// 
			this.splitContainer.BackColor = System.Drawing.Color.White;
			this.splitContainer.BeforeTouchSize = 5;
			this.tableLayoutPanel1.SetColumnSpan(this.splitContainer, 4);
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = new System.Drawing.Point(3, 3);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.BackColor = System.Drawing.Color.White;
			this.splitContainer.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainer.Panel1.Controls.Add(this.treeViewOptions);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainer.Panel2.Controls.Add(this.PanelContent);
			this.splitContainer.Size = new System.Drawing.Size(1156, 739);
			this.splitContainer.SplitterDistance = 232;
			this.splitContainer.SplitterWidth = 5;
			this.splitContainer.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainer.TabIndex = 7;
			// 
			// treeViewOptions
			// 
			this.treeViewOptions.BackColor = System.Drawing.Color.White;
			this.treeViewOptions.BeforeTouchSize = new System.Drawing.Size(312, 368);
			this.treeViewOptions.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.treeViewOptions.BorderColor = System.Drawing.Color.Transparent;
			this.treeViewOptions.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.treeViewOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewOptions.CanSelectDisabledNode = false;
			this.treeViewOptions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewOptions.EnableTouchMode = true;
			this.treeViewOptions.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// 
			// 
			this.treeViewOptions.HelpTextControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewOptions.HelpTextControl.Name = "helpText";
			this.treeViewOptions.HelpTextControl.TabIndex = 0;
			this.treeViewOptions.HideSelection = false;
			this.treeViewOptions.Indent = 15;
			this.treeViewOptions.ItemHeight = 32;
			this.treeViewOptions.LineColor = System.Drawing.Color.SkyBlue;
			this.treeViewOptions.Location = new System.Drawing.Point(0, 0);
			this.treeViewOptions.Margin = new System.Windows.Forms.Padding(12, 12, 3, 3);
			this.treeViewOptions.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.treeViewOptions.MetroScrollBars = true;
			this.treeViewOptions.Name = "treeViewOptions";
			this.treeViewOptions.SelectedNodeBackground = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255))))));
			this.treeViewOptions.ShowFocusRect = false;
			this.treeViewOptions.ShowLines = false;
			this.treeViewOptions.Size = new System.Drawing.Size(232, 739);
			this.treeViewOptions.Style = Syncfusion.Windows.Forms.Tools.TreeStyle.Metro;
			this.treeViewOptions.TabIndex = 1;
			this.treeViewOptions.ThemesEnabled = false;
			// 
			// 
			// 
			this.treeViewOptions.ToolTipControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewOptions.ToolTipControl.Name = "toolTip";
			this.treeViewOptions.ToolTipControl.TabIndex = 1;
			// 
			// PanelContent
			// 
			this.PanelContent.BackColor = System.Drawing.Color.MistyRose;
			this.PanelContent.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.PanelContent.BorderColor = System.Drawing.Color.Gray;
			this.PanelContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PanelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PanelContent.Location = new System.Drawing.Point(0, 0);
			this.PanelContent.Name = "PanelContent";
			this.PanelContent.Size = new System.Drawing.Size(919, 739);
			this.PanelContent.TabIndex = 6;
			// 
			// buttonAdvOK
			// 
			this.buttonAdvOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOK.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvOK.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.buttonAdvOK.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOK.IsBackStageButton = false;
			this.buttonAdvOK.Location = new System.Drawing.Point(825, 766);
			this.buttonAdvOK.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvOK.Name = "buttonAdvOK";
			this.buttonAdvOK.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.buttonAdvOK.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOK.TabIndex = 0;
			this.buttonAdvOK.Text = "xxOk";
			this.buttonAdvOK.UseVisualStyle = true;
			this.buttonAdvOK.Click += new System.EventHandler(this.buttonOkClick);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.CausesValidation = false;
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(1065, 766);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 1;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonCancelClick);
			// 
			// buttonAdvApply
			// 
			this.buttonAdvApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvApply.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvApply.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvApply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvApply.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.buttonAdvApply.ForeColor = System.Drawing.Color.White;
			this.buttonAdvApply.IsBackStageButton = false;
			this.buttonAdvApply.Location = new System.Drawing.Point(945, 766);
			this.buttonAdvApply.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvApply.Name = "buttonAdvApply";
			this.buttonAdvApply.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvApply.TabIndex = 2;
			this.buttonAdvApply.Text = "xxApply";
			this.buttonAdvApply.UseVisualStyle = true;
			this.buttonAdvApply.Click += new System.EventHandler(this.buttonApplyClick);
			// 
			// SettingsScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.buttonAdvCancel;
			this.ClientSize = new System.Drawing.Size(1166, 807);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.MinimumSize = new System.Drawing.Size(241, 225);
			this.Name = "SettingsScreen";
			this.Padding = new System.Windows.Forms.Padding(2);
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxOptions";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.tableLayoutPanel1.ResumeLayout(false);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.treeViewOptions)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PanelContent)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainer;
		private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewOptions;
		private Syncfusion.Windows.Forms.Tools.GradientPanel PanelContent;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvApply;
		
	}
}