﻿namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	partial class ModulePanelItem
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panelImage = new System.Windows.Forms.Panel();
			this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
			this.labelModuleText = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.panelImage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 43F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.panelImage, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelModuleText, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(160, 41);
			this.tableLayoutPanel1.TabIndex = 0;
			this.tableLayoutPanel1.Click += new System.EventHandler(this.onClick);
			this.tableLayoutPanel1.MouseEnter += new System.EventHandler(this.onMouseEnter);
			this.tableLayoutPanel1.MouseLeave += new System.EventHandler(this.onMouseLeave);
			// 
			// panelImage
			// 
			this.panelImage.BackgroundImage = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.help_32;
			this.panelImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.panelImage.Controls.Add(this.pictureBoxIcon);
			this.panelImage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelImage.Location = new System.Drawing.Point(0, 0);
			this.panelImage.Margin = new System.Windows.Forms.Padding(0);
			this.panelImage.Name = "panelImage";
			this.panelImage.Size = new System.Drawing.Size(43, 41);
			this.panelImage.TabIndex = 0;
			this.panelImage.Click += new System.EventHandler(this.onClick);
			this.panelImage.MouseEnter += new System.EventHandler(this.onMouseEnter);
			this.panelImage.MouseLeave += new System.EventHandler(this.onMouseLeave);
			// 
			// pictureBoxIcon
			// 
			this.pictureBoxIcon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBoxIcon.Location = new System.Drawing.Point(0, 0);
			this.pictureBoxIcon.Margin = new System.Windows.Forms.Padding(0);
			this.pictureBoxIcon.Name = "pictureBoxIcon";
			this.pictureBoxIcon.Size = new System.Drawing.Size(43, 41);
			this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBoxIcon.TabIndex = 1;
			this.pictureBoxIcon.TabStop = false;
			this.pictureBoxIcon.Click += new System.EventHandler(this.onClick);
			this.pictureBoxIcon.MouseEnter += new System.EventHandler(this.onMouseEnter);
			this.pictureBoxIcon.MouseLeave += new System.EventHandler(this.onMouseLeave);
			// 
			// labelModuleText
			// 
			this.labelModuleText.AutoSize = true;
			this.labelModuleText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelModuleText.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelModuleText.Location = new System.Drawing.Point(43, 0);
			this.labelModuleText.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
			this.labelModuleText.Name = "labelModuleText";
			this.labelModuleText.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
			this.labelModuleText.Size = new System.Drawing.Size(113, 41);
			this.labelModuleText.TabIndex = 1;
			this.labelModuleText.Text = "ItemText";
			this.labelModuleText.Click += new System.EventHandler(this.onClick);
			this.labelModuleText.MouseEnter += new System.EventHandler(this.onMouseEnter);
			this.labelModuleText.MouseLeave += new System.EventHandler(this.onMouseLeave);
			// 
			// toolTip1
			// 
			this.toolTip1.Active = false;
			// 
			// ModulePanelItem
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Name = "ModulePanelItem";
			this.Size = new System.Drawing.Size(160, 41);
			this.Click += new System.EventHandler(this.onClick);
			this.MouseEnter += new System.EventHandler(this.onMouseEnter);
			this.MouseLeave += new System.EventHandler(this.onMouseLeave);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panelImage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel panelImage;
		private System.Windows.Forms.Label labelModuleText;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.PictureBox pictureBoxIcon;
	}
}
