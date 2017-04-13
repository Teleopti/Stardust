namespace Teleopti.Ccc.Win.Budgeting
{
	partial class PromptDoubleBox
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
			this.tableLayoutPanelFields = new System.Windows.Forms.TableLayoutPanel();
			this.labelName = new System.Windows.Forms.Label();
			this.numericTextBox1 = new Teleopti.Ccc.Win.Common.Controls.NumericTextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvSave = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanelFields.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanelFields
			// 
			this.tableLayoutPanelFields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanelFields.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelFields.ColumnCount = 2;
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelFields.Controls.Add(this.labelName, 0, 0);
			this.tableLayoutPanelFields.Controls.Add(this.numericTextBox1, 1, 0);
			this.tableLayoutPanelFields.Controls.Add(this.tableLayoutPanel1, 0, 1);
			this.tableLayoutPanelFields.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelFields.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelFields.Name = "tableLayoutPanelFields";
			this.tableLayoutPanelFields.RowCount = 2;
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 104F));
			this.tableLayoutPanelFields.Size = new System.Drawing.Size(393, 161);
			this.tableLayoutPanelFields.TabIndex = 1;
			// 
			// labelName
			// 
			this.labelName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelName.AutoSize = true;
			this.labelName.Location = new System.Drawing.Point(60, 20);
			this.labelName.Margin = new System.Windows.Forms.Padding(12, 20, 0, 0);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(136, 15);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "xxEnterParameter0Colon";
			// 
			// numericTextBox1
			// 
			this.numericTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.numericTextBox1.CurrentCulture = new System.Globalization.CultureInfo("sv-SE");
			this.numericTextBox1.DoubleValue = 0D;
			this.numericTextBox1.Location = new System.Drawing.Point(199, 17);
			this.numericTextBox1.MaxValue = 1.7976931348623157E+308D;
			this.numericTextBox1.MinValue = -1.7976931348623157E+308D;
			this.numericTextBox1.Name = "numericTextBox1";
			this.numericTextBox1.Size = new System.Drawing.Size(112, 23);
			this.numericTextBox1.TabIndex = 1;
			this.numericTextBox1.Text = "0";
			this.numericTextBox1.TextChanged += new System.EventHandler(this.numericTextBox1TextChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanelFields.SetColumnSpan(this.tableLayoutPanel1, 2);
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvSave, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 57);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(393, 104);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// buttonAdvSave
			// 
			this.buttonAdvSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvSave.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvSave.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvSave.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvSave.ForeColor = System.Drawing.Color.White;
			this.buttonAdvSave.IsBackStageButton = false;
			this.buttonAdvSave.Location = new System.Drawing.Point(176, 67);
			this.buttonAdvSave.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvSave.Name = "buttonAdvSave";
			this.buttonAdvSave.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvSave.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvSave.TabIndex = 2;
			this.buttonAdvSave.Text = "xxOk";
			this.buttonAdvSave.UseVisualStyle = true;
			this.buttonAdvSave.UseVisualStyleBackColor = false;
			this.buttonAdvSave.Click += new System.EventHandler(this.buttonAdvSaveClick);
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
			this.buttonAdvCancel.Location = new System.Drawing.Point(296, 67);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 3;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// PromptDoubleBox
			// 
			this.AcceptButton = this.buttonAdvSave;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(393, 161);
			this.Controls.Add(this.tableLayoutPanelFields);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PromptDoubleBox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxNameType";
			this.TopMost = true;
			this.tableLayoutPanelFields.ResumeLayout(false);
			this.tableLayoutPanelFields.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSave;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFields;
		private Common.Controls.NumericTextBox numericTextBox1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}