
namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
	partial class MultisiteChildSkills
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.listBoxChildSkills = new System.Windows.Forms.ListBox();
			this.buttonAdvRename = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvAdd = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvRemove = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvManageDayTemplates = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panel1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.tableLayoutPanel1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(417, 265);
			this.panel1.TabIndex = 6;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 145F));
			this.tableLayoutPanel1.Controls.Add(this.listBoxChildSkills, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvRename, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvAdd, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvRemove, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvManageDayTemplates, 3, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(417, 265);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// listBoxChildSkills
			// 
			this.listBoxChildSkills.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tableLayoutPanel1.SetColumnSpan(this.listBoxChildSkills, 4);
			this.listBoxChildSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxChildSkills.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.listBoxChildSkills.FormattingEnabled = true;
			this.listBoxChildSkills.ItemHeight = 17;
			this.listBoxChildSkills.Location = new System.Drawing.Point(3, 3);
			this.listBoxChildSkills.Name = "listBoxChildSkills";
			this.listBoxChildSkills.Size = new System.Drawing.Size(411, 209);
			this.listBoxChildSkills.TabIndex = 0;
			this.listBoxChildSkills.DoubleClick += new System.EventHandler(this.listBoxChildSkillsDoubleClick);
			// 
			// buttonAdvRename
			// 
			this.buttonAdvRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvRename.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvRename.AutoSize = true;
			this.buttonAdvRename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvRename.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvRename.ForeColor = System.Drawing.Color.White;
			this.buttonAdvRename.IsBackStageButton = false;
			this.buttonAdvRename.Location = new System.Drawing.Point(180, 228);
			this.buttonAdvRename.Margin = new System.Windows.Forms.Padding(0, 0, 5, 10);
			this.buttonAdvRename.Name = "buttonAdvRename";
			this.buttonAdvRename.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvRename.TabIndex = 3;
			this.buttonAdvRename.Text = "xxRename";
			this.buttonAdvRename.UseVisualStyle = true;
			this.buttonAdvRename.Click += new System.EventHandler(this.buttonAdvRenameClick);
			// 
			// buttonAdvAdd
			// 
			this.buttonAdvAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvAdd.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvAdd.AutoSize = true;
			this.buttonAdvAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvAdd.BeforeTouchSize = new System.Drawing.Size(72, 27);
			this.buttonAdvAdd.ForeColor = System.Drawing.Color.White;
			this.buttonAdvAdd.IsBackStageButton = false;
			this.buttonAdvAdd.Location = new System.Drawing.Point(5, 228);
			this.buttonAdvAdd.Margin = new System.Windows.Forms.Padding(0, 0, 5, 10);
			this.buttonAdvAdd.Name = "buttonAdvAdd";
			this.buttonAdvAdd.Size = new System.Drawing.Size(72, 27);
			this.buttonAdvAdd.TabIndex = 1;
			this.buttonAdvAdd.Text = "xxNew";
			this.buttonAdvAdd.UseVisualStyle = true;
			this.buttonAdvAdd.Click += new System.EventHandler(this.buttonAdvAddClick);
			// 
			// buttonAdvRemove
			// 
			this.buttonAdvRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvRemove.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvRemove.AutoSize = true;
			this.buttonAdvRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvRemove.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvRemove.ForeColor = System.Drawing.Color.White;
			this.buttonAdvRemove.IsBackStageButton = false;
			this.buttonAdvRemove.Location = new System.Drawing.Point(85, 228);
			this.buttonAdvRemove.Margin = new System.Windows.Forms.Padding(0, 0, 5, 10);
			this.buttonAdvRemove.Name = "buttonAdvRemove";
			this.buttonAdvRemove.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvRemove.TabIndex = 2;
			this.buttonAdvRemove.Text = "xxDelete";
			this.buttonAdvRemove.UseVisualStyle = true;
			this.buttonAdvRemove.Click += new System.EventHandler(this.buttonAdvRemoveClick);
			// 
			// buttonAdvManageDayTemplates
			// 
			this.buttonAdvManageDayTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvManageDayTemplates.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvManageDayTemplates.AutoSize = true;
			this.buttonAdvManageDayTemplates.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvManageDayTemplates.BeforeTouchSize = new System.Drawing.Size(136, 27);
			this.buttonAdvManageDayTemplates.ForeColor = System.Drawing.Color.White;
			this.buttonAdvManageDayTemplates.IsBackStageButton = false;
			this.buttonAdvManageDayTemplates.Location = new System.Drawing.Point(276, 228);
			this.buttonAdvManageDayTemplates.Margin = new System.Windows.Forms.Padding(0, 0, 5, 10);
			this.buttonAdvManageDayTemplates.Name = "buttonAdvManageDayTemplates";
			this.buttonAdvManageDayTemplates.Size = new System.Drawing.Size(136, 27);
			this.buttonAdvManageDayTemplates.TabIndex = 4;
			this.buttonAdvManageDayTemplates.Text = "xxTemplatesThreeDots";
			this.buttonAdvManageDayTemplates.UseVisualStyle = true;
			this.buttonAdvManageDayTemplates.Click += new System.EventHandler(this.buttonAdvManageDayTemplatesClick);
			// 
			// MultisiteChildSkills
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "MultisiteChildSkills";
			this.Size = new System.Drawing.Size(417, 265);
			this.panel1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAdd;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvManageDayTemplates;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRemove;
		private System.Windows.Forms.ListBox listBoxChildSkills;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRename;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel panel1;
	}
}