namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	partial class EditOutlier
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
			this.textBoxTemplateName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.labelTemplateName = new System.Windows.Forms.Label();
			this.splitContainerAdvContent = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.listBoxDateProviders = new System.Windows.Forms.ListBox();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.textBoxTemplateName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvContent)).BeginInit();
			this.splitContainerAdvContent.Panel1.SuspendLayout();
			this.splitContainerAdvContent.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.gradientPanel1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxTemplateName
			// 
			this.textBoxTemplateName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxTemplateName.BackColor = System.Drawing.Color.White;
			this.textBoxTemplateName.BeforeTouchSize = new System.Drawing.Size(255, 23);
			this.textBoxTemplateName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxTemplateName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxTemplateName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxTemplateName.Location = new System.Drawing.Point(99, 6);
			this.textBoxTemplateName.MaxLength = 50;
			this.textBoxTemplateName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxTemplateName.Name = "textBoxTemplateName";
			this.textBoxTemplateName.Size = new System.Drawing.Size(255, 23);
			this.textBoxTemplateName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxTemplateName.TabIndex = 1;
			this.textBoxTemplateName.TextChanged += new System.EventHandler(this.textBoxTemplateNameTextChanged);
			// 
			// labelTemplateName
			// 
			this.labelTemplateName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTemplateName.AutoSize = true;
			this.labelTemplateName.Location = new System.Drawing.Point(12, 10);
			this.labelTemplateName.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
			this.labelTemplateName.Name = "labelTemplateName";
			this.labelTemplateName.Size = new System.Drawing.Size(81, 15);
			this.labelTemplateName.TabIndex = 0;
			this.labelTemplateName.Text = "xxNameColon";
			// 
			// splitContainerAdvContent
			// 
			this.splitContainerAdvContent.BeforeTouchSize = 1;
			this.tableLayoutPanel1.SetColumnSpan(this.splitContainerAdvContent, 3);
			this.splitContainerAdvContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdvContent.IsSplitterFixed = true;
			this.splitContainerAdvContent.Location = new System.Drawing.Point(3, 38);
			this.splitContainerAdvContent.Name = "splitContainerAdvContent";
			// 
			// splitContainerAdvContent.Panel1
			// 
			this.splitContainerAdvContent.Panel1.Controls.Add(this.listBoxDateProviders);
			this.splitContainerAdvContent.Size = new System.Drawing.Size(698, 307);
			this.splitContainerAdvContent.SplitterDistance = 177;
			this.splitContainerAdvContent.SplitterWidth = 1;
			this.splitContainerAdvContent.TabIndex = 0;
			this.splitContainerAdvContent.Text = "yySplitContainerAdv1";
			// 
			// listBoxDateProviders
			// 
			this.listBoxDateProviders.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listBoxDateProviders.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxDateProviders.Enabled = false;
			this.listBoxDateProviders.FormattingEnabled = true;
			this.listBoxDateProviders.ItemHeight = 15;
			this.listBoxDateProviders.Location = new System.Drawing.Point(0, 0);
			this.listBoxDateProviders.Name = "listBoxDateProviders";
			this.listBoxDateProviders.Size = new System.Drawing.Size(177, 307);
			this.listBoxDateProviders.TabIndex = 0;
			this.listBoxDateProviders.SelectedIndexChanged += new System.EventHandler(this.listBoxDateProvidersSelectedIndexChanged);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(607, 361);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 1;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// buttonAdvOK
			// 
			this.buttonAdvOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOK.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOK.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOK.IsBackStageButton = false;
			this.buttonAdvOK.Location = new System.Drawing.Point(487, 361);
			this.buttonAdvOK.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvOK.Name = "buttonAdvOK";
			this.buttonAdvOK.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOK.TabIndex = 0;
			this.buttonAdvOK.Text = "xxOk";
			this.buttonAdvOK.UseVisualStyle = true;
			this.buttonAdvOK.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel1.Controls.Add(this.tableLayoutPanel1);
			this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel1.Location = new System.Drawing.Point(1, 0);
			this.gradientPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Size = new System.Drawing.Size(704, 398);
			this.gradientPanel1.TabIndex = 10;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.Controls.Add(this.splitContainerAdvContent, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelTemplateName, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxTemplateName, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvOK, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 2, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(704, 398);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// EditOutlier
			// 
			this.AcceptButton = this.buttonAdvOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(706, 398);
			this.Controls.Add(this.gradientPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(180, 39);
			this.Name = "EditOutlier";
			this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxOutlier";
			this.Load += new System.EventHandler(this.editOutlierLoad);
			((System.ComponentModel.ISupportInitialize)(this.textBoxTemplateName)).EndInit();
			this.splitContainerAdvContent.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvContent)).EndInit();
			this.splitContainerAdvContent.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.gradientPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.Tools.TextBoxExt  textBoxTemplateName;
		private System.Windows.Forms.Label labelTemplateName;
		private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvContent;
		private System.Windows.Forms.ListBox listBoxDateProviders;
		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

	}
}