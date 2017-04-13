namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	partial class ExportControl
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
			this.labelSaveTo = new System.Windows.Forms.Label();
			this.labelInformation = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.labelSaveTo, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelInformation, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47.88733F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 52.11267F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(520, 60);
			this.tableLayoutPanel1.TabIndex = 0;
			this.tableLayoutPanel1.Click += new System.EventHandler(this.everyMouseClick);
			this.tableLayoutPanel1.Leave += new System.EventHandler(this.everyMouseLeave);
			this.tableLayoutPanel1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.everyMouseClick);
			this.tableLayoutPanel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.everyMouseDown);
			this.tableLayoutPanel1.MouseEnter += new System.EventHandler(this.everyMouseEnter);
			this.tableLayoutPanel1.MouseLeave += new System.EventHandler(this.everyMouseLeave);
			this.tableLayoutPanel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.everyMouseUp);
			// 
			// labelSaveTo
			// 
			this.labelSaveTo.CausesValidation = false;
			this.labelSaveTo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelSaveTo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSaveTo.Location = new System.Drawing.Point(53, 4);
			this.labelSaveTo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
			this.labelSaveTo.Name = "labelSaveTo";
			this.labelSaveTo.Size = new System.Drawing.Size(464, 24);
			this.labelSaveTo.TabIndex = 0;
			this.labelSaveTo.Text = "12345678901234567890123456789012345678901234567890abc";
			this.labelSaveTo.Click += new System.EventHandler(this.everyMouseClick);
			this.labelSaveTo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.everyMouseDown);
			this.labelSaveTo.MouseEnter += new System.EventHandler(this.everyMouseEnter);
			this.labelSaveTo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.everyMouseUp);
			// 
			// labelInformation
			// 
			this.labelInformation.AutoSize = true;
			this.labelInformation.CausesValidation = false;
			this.labelInformation.Location = new System.Drawing.Point(53, 31);
			this.labelInformation.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.labelInformation.Name = "labelInformation";
			this.labelInformation.Size = new System.Drawing.Size(207, 15);
			this.labelInformation.TabIndex = 1;
			this.labelInformation.Text = "xxSaveTheForecastToAnotherScenario";
			this.labelInformation.Click += new System.EventHandler(this.everyMouseClick);
			this.labelInformation.MouseDown += new System.Windows.Forms.MouseEventHandler(this.everyMouseDown);
			this.labelInformation.MouseEnter += new System.EventHandler(this.everyMouseEnter);
			this.labelInformation.MouseUp += new System.Windows.Forms.MouseEventHandler(this.everyMouseUp);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.pictureBox1.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Export2;
			this.pictureBox1.Location = new System.Drawing.Point(10, 14);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.tableLayoutPanel1.SetRowSpan(this.pictureBox1, 2);
			this.pictureBox1.Size = new System.Drawing.Size(35, 31);
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.everyMouseClick);
			this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.everyMouseDown);
			this.pictureBox1.MouseEnter += new System.EventHandler(this.everyMouseEnter);
			this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.everyMouseUp);
			// 
			// ExportControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.Name = "ExportControl";
			this.Size = new System.Drawing.Size(520, 60);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label labelSaveTo;
		private System.Windows.Forms.Label labelInformation;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}
