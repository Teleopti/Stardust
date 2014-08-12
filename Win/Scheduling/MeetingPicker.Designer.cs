namespace Teleopti.Ccc.Win.Scheduling
{
	partial class MeetingPicker
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
			this.comboBoxMeetings = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.buttonAdv1 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdv2 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelPerson = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxMeetings)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// comboBoxMeetings
			// 
			this.comboBoxMeetings.BackColor = System.Drawing.Color.White;
			this.comboBoxMeetings.BeforeTouchSize = new System.Drawing.Size(404, 23);
			this.comboBoxMeetings.Cursor = System.Windows.Forms.Cursors.Default;
			this.comboBoxMeetings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMeetings.Location = new System.Drawing.Point(50, 93);
			this.comboBoxMeetings.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.comboBoxMeetings.Name = "comboBoxMeetings";
			this.comboBoxMeetings.Size = new System.Drawing.Size(404, 23);
			this.comboBoxMeetings.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxMeetings.TabIndex = 2;
			// 
			// buttonAdv1
			// 
			this.buttonAdv1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdv1.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdv1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdv1.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdv1.ForeColor = System.Drawing.Color.White;
			this.buttonAdv1.IsBackStageButton = false;
			this.buttonAdv1.Location = new System.Drawing.Point(271, 10);
			this.buttonAdv1.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdv1.Name = "buttonAdv1";
			this.buttonAdv1.Size = new System.Drawing.Size(87, 27);
			this.buttonAdv1.TabIndex = 4;
			this.buttonAdv1.Text = "xxOk";
			this.buttonAdv1.UseVisualStyle = true;
			this.buttonAdv1.Click += new System.EventHandler(this.buttonOkClick);
			// 
			// buttonAdv2
			// 
			this.buttonAdv2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdv2.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdv2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdv2.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdv2.ForeColor = System.Drawing.Color.White;
			this.buttonAdv2.IsBackStageButton = false;
			this.buttonAdv2.Location = new System.Drawing.Point(391, 10);
			this.buttonAdv2.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdv2.Name = "buttonAdv2";
			this.buttonAdv2.Size = new System.Drawing.Size(87, 27);
			this.buttonAdv2.TabIndex = 5;
			this.buttonAdv2.Text = "xxCancel";
			this.buttonAdv2.UseVisualStyle = true;
			this.buttonAdv2.Click += new System.EventHandler(this.buttonCancelClick);
			// 
			// labelPerson
			// 
			this.labelPerson.AutoSize = true;
			this.labelPerson.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.labelPerson.Location = new System.Drawing.Point(47, 62);
			this.labelPerson.Margin = new System.Windows.Forms.Padding(0);
			this.labelPerson.Name = "labelPerson";
			this.labelPerson.Size = new System.Drawing.Size(63, 17);
			this.labelPerson.TabIndex = 6;
			this.labelPerson.Text = "personen";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.Controls.Add(this.buttonAdv2, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdv1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 145);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(488, 47);
			this.tableLayoutPanel1.TabIndex = 7;
			// 
			// MeetingPicker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(488, 192);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.labelPerson);
			this.Controls.Add(this.comboBoxMeetings);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(212, 39);
			this.Name = "MeetingPicker";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxMeetingPicker";
			((System.ComponentModel.ISupportInitialize)(this.comboBoxMeetings)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxMeetings;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdv1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdv2;
		private System.Windows.Forms.Label labelPerson;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}