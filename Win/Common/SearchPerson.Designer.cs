namespace Teleopti.Ccc.Win.Common
{
	partial class SearchPerson
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.button1 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.searchPersonView1 = new Teleopti.Ccc.Win.SearchPersonView();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.button1.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.button1.ForeColor = System.Drawing.Color.White;
			this.button1.IsBackStageButton = false;
			this.button1.Location = new System.Drawing.Point(266, 455);
			this.button1.Margin = new System.Windows.Forms.Padding(3, 3, 12, 12);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(87, 27);
			this.button1.TabIndex = 1;
			this.button1.Text = "xxOK";
			this.button1.UseVisualStyle = true;
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1Click);
			// 
			// searchPersonView1
			// 
			this.searchPersonView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.searchPersonView1.Location = new System.Drawing.Point(0, 0);
			this.searchPersonView1.Margin = new System.Windows.Forms.Padding(0);
			this.searchPersonView1.Name = "searchPersonView1";
			this.searchPersonView1.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
			this.searchPersonView1.Size = new System.Drawing.Size(365, 442);
			this.searchPersonView1.TabIndex = 0;
			this.searchPersonView1.ItemDoubleClick += new System.EventHandler<System.EventArgs>(this.searchPersonView1ItemDoubleClick);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.searchPersonView1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.button1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(365, 494);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// SearchPerson
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(365, 494);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(161, 47);
			this.Name = "SearchPerson";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "xxFind";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.searchPersonFormClosing);
			this.Load += new System.EventHandler(this.searchPersonLoad);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private SearchPersonView searchPersonView1;
		private Syncfusion.Windows.Forms.ButtonAdv button1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}