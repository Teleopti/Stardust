namespace Teleopti.Ccc.Win.Scheduling
{
	partial class FilterMultiplePersons
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.gridListControl1 = new Syncfusion.Windows.Forms.Grid.GridListControl();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.gridListControlSelectedItems = new Syncfusion.Windows.Forms.Grid.GridListControl();
			this.panel3 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonAdd = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonOk = new Syncfusion.Windows.Forms.ButtonAdv();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridListControl1)).BeginInit();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridListControlSelectedItems)).BeginInit();
			this.panel3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
			this.splitContainer1.Panel1.Controls.Add(this.panel2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.gridListControlSelectedItems);
			this.splitContainer1.Panel2.Controls.Add(this.panel3);
			this.splitContainer1.Size = new System.Drawing.Size(528, 674);
			this.splitContainer1.SplitterDistance = 360;
			this.splitContainer1.SplitterWidth = 6;
			this.splitContainer1.TabIndex = 1;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.gridListControl1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.textBox1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 27);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(528, 333);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// gridListControl1
			// 
			this.gridListControl1.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			this.gridListControl1.BackColor = System.Drawing.SystemColors.Control;
			this.gridListControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.gridListControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridListControl1.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridListControl1.ItemHeight = 20;
			this.gridListControl1.Location = new System.Drawing.Point(3, 34);
			this.gridListControl1.MultiColumn = false;
			this.gridListControl1.Name = "gridListControl1";
			this.gridListControl1.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridListControl1.Properties.ForceImmediateRepaint = false;
			this.gridListControl1.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridListControl1.Properties.MarkColHeader = false;
			this.gridListControl1.Properties.MarkRowHeader = false;
			this.gridListControl1.SelectedIndex = -1;
			this.gridListControl1.Size = new System.Drawing.Size(528, 304);
			this.gridListControl1.TabIndex = 3;
			this.gridListControl1.ThemesEnabled = true;
			this.gridListControl1.TopIndex = 0;
			// 
			// textBox1
			// 
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.textBox1.Location = new System.Drawing.Point(3, 6);
			this.textBox1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(528, 21);
			this.textBox1.TabIndex = 1;
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1TextChanged);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.label1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(528, 27);
			this.panel2.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(52, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "xxSearch";
			// 
			// gridListControlSelectedItems
			// 
			this.gridListControlSelectedItems.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			this.gridListControlSelectedItems.BackColor = System.Drawing.SystemColors.Control;
			this.gridListControlSelectedItems.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.gridListControlSelectedItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridListControlSelectedItems.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridListControlSelectedItems.ItemHeight = 20;
			this.gridListControlSelectedItems.Location = new System.Drawing.Point(0, 21);
			this.gridListControlSelectedItems.MultiColumn = false;
			this.gridListControlSelectedItems.Name = "gridListControlSelectedItems";
			this.gridListControlSelectedItems.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridListControlSelectedItems.Properties.ForceImmediateRepaint = false;
			this.gridListControlSelectedItems.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridListControlSelectedItems.Properties.MarkColHeader = false;
			this.gridListControlSelectedItems.Properties.MarkRowHeader = false;
			this.gridListControlSelectedItems.SelectedIndex = -1;
			this.gridListControlSelectedItems.Size = new System.Drawing.Size(528, 287);
			this.gridListControlSelectedItems.TabIndex = 4;
			this.gridListControlSelectedItems.ThemesEnabled = true;
			this.gridListControlSelectedItems.TopIndex = 0;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.label2);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(528, 21);
			this.panel3.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(12, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(74, 15);
			this.label2.TabIndex = 1;
			this.label2.Text = "xxFilteredList";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonAdd);
			this.panel1.Controls.Add(this.buttonCancel);
			this.panel1.Controls.Add(this.buttonOk);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 674);
			this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(528, 41);
			this.panel1.TabIndex = 2;
			// 
			// buttonAdd
			// 
			this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdd.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdd.BeforeTouchSize = new System.Drawing.Size(95, 27);
			this.buttonAdd.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonAdd.ForeColor = System.Drawing.Color.White;
			this.buttonAdd.IsBackStageButton = false;
			this.buttonAdd.Location = new System.Drawing.Point(223, 9);
			this.buttonAdd.Margin = new System.Windows.Forms.Padding(4, 16, 4, 5);
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdd.Size = new System.Drawing.Size(95, 27);
			this.buttonAdd.TabIndex = 6;
			this.buttonAdd.Text = "xxAdd";
			this.buttonAdd.UseVisualStyle = true;
			this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.BeforeTouchSize = new System.Drawing.Size(95, 27);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.IsBackStageButton = false;
			this.buttonCancel.Location = new System.Drawing.Point(429, 9);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 16, 4, 5);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonCancel.Size = new System.Drawing.Size(95, 27);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			// 
			// buttonOk
			// 
			this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOk.BeforeTouchSize = new System.Drawing.Size(95, 27);
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonOk.ForeColor = System.Drawing.Color.White;
			this.buttonOk.IsBackStageButton = false;
			this.buttonOk.Location = new System.Drawing.Point(326, 9);
			this.buttonOk.Margin = new System.Windows.Forms.Padding(4, 16, 4, 5);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonOk.Size = new System.Drawing.Size(95, 27);
			this.buttonOk.TabIndex = 4;
			this.buttonOk.Text = "xxOK";
			this.buttonOk.UseVisualStyle = true;
			// 
			// FilterMultiplePersons
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(528, 715);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Segoe UI", 12F);
			this.HelpButton = false;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FilterMultiplePersons";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "xxFilterMultiplePerson";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridListControl1)).EndInit();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridListControlSelectedItems)).EndInit();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Panel panel1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonOk;
		private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.Grid.GridListControl gridListControl1;
		private System.Windows.Forms.TextBox textBox1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdd;
		private Syncfusion.Windows.Forms.Grid.GridListControl gridListControlSelectedItems;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label label2;
	}
}
