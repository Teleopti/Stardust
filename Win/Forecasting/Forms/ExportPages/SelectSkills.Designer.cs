namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    partial class SelectSkills
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
			this.textBoxExFilter = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.listViewSkills = new System.Windows.Forms.ListView();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExFilter)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxExFilter
			// 
			this.textBoxExFilter.BackColor = System.Drawing.Color.White;
			this.textBoxExFilter.BeforeTouchSize = new System.Drawing.Size(273, 20);
			this.textBoxExFilter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxExFilter.Location = new System.Drawing.Point(75, 3);
			this.textBoxExFilter.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExFilter.Name = "textBoxExFilter";
			this.textBoxExFilter.Size = new System.Drawing.Size(273, 20);
			this.textBoxExFilter.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExFilter.TabIndex = 0;
			this.textBoxExFilter.TextChanged += new System.EventHandler(this.textBoxExFilter_TextChanged);
			// 
			// listViewSkills
			// 
			this.listViewSkills.Activation = System.Windows.Forms.ItemActivation.TwoClick;
			this.listViewSkills.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listViewSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewSkills.FullRowSelect = true;
			this.listViewSkills.Location = new System.Drawing.Point(5, 30);
			this.listViewSkills.Margin = new System.Windows.Forms.Padding(5, 0, 5, 5);
			this.listViewSkills.MultiSelect = false;
			this.listViewSkills.Name = "listViewSkills";
			this.listViewSkills.ShowGroups = false;
			this.listViewSkills.Size = new System.Drawing.Size(347, 220);
			this.listViewSkills.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewSkills.TabIndex = 6;
			this.listViewSkills.TileSize = new System.Drawing.Size(100, 100);
			this.listViewSkills.UseCompatibleStateImageBehavior = false;
			this.listViewSkills.View = System.Windows.Forms.View.Details;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.listViewSkills, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(357, 250);
			this.tableLayoutPanel1.TabIndex = 8;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.textBoxExFilter, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(351, 24);
			this.tableLayoutPanel2.TabIndex = 7;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "xxFilterColon";
			// 
			// SelectSkills
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SelectSkills";
			this.Size = new System.Drawing.Size(357, 250);
			((System.ComponentModel.ISupportInitialize)(this.textBoxExFilter)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExFilter;
        private System.Windows.Forms.ListView listViewSkills;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;




    }
}
