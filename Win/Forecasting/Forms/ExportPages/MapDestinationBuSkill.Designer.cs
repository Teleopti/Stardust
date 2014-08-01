namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    partial class MapDestinationBuSkill
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapDestinationBuSkill));
			this.listViewSkills = new System.Windows.Forms.ListView();
			this.columnHeaderExport = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderSkill = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.textBoxExFilter = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExFilter)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// listViewSkills
			// 
			this.listViewSkills.Activation = System.Windows.Forms.ItemActivation.TwoClick;
			this.listViewSkills.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listViewSkills.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderExport,
            this.columnHeaderSkill});
			this.tableLayoutPanel1.SetColumnSpan(this.listViewSkills, 2);
			this.listViewSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewSkills.FullRowSelect = true;
			this.listViewSkills.Location = new System.Drawing.Point(5, 30);
			this.listViewSkills.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.listViewSkills.MultiSelect = false;
			this.listViewSkills.Name = "listViewSkills";
			this.listViewSkills.ShowGroups = false;
			this.listViewSkills.Size = new System.Drawing.Size(315, 285);
			this.listViewSkills.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewSkills.TabIndex = 6;
			this.listViewSkills.UseCompatibleStateImageBehavior = false;
			this.listViewSkills.View = System.Windows.Forms.View.Details;
			this.listViewSkills.DoubleClick += new System.EventHandler(this.listViewSkills_DoubleClick);
			// 
			// columnHeaderExport
			// 
			this.columnHeaderExport.DisplayIndex = 1;
			this.columnHeaderExport.Text = "xxBusinessUnit";
			this.columnHeaderExport.Width = 139;
			// 
			// columnHeaderSkill
			// 
			this.columnHeaderSkill.DisplayIndex = 0;
			this.columnHeaderSkill.Text = "xxSkill";
			this.columnHeaderSkill.Width = 146;
			// 
			// textBoxExFilter
			// 
			this.textBoxExFilter.BackColor = System.Drawing.Color.White;
			this.textBoxExFilter.BeforeTouchSize = new System.Drawing.Size(233, 22);
			this.textBoxExFilter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxExFilter.Location = new System.Drawing.Point(83, 3);
			this.textBoxExFilter.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExFilter.Name = "textBoxExFilter";
			this.textBoxExFilter.Size = new System.Drawing.Size(233, 22);
			this.textBoxExFilter.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExFilter.TabIndex = 0;
			this.textBoxExFilter.TextChanged += new System.EventHandler(this.textBoxExFilter_TextChanged);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 2);
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.textBoxExFilter, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(319, 24);
			this.tableLayoutPanel2.TabIndex = 7;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "xxFilterColon";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.listViewSkills, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvOk, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(325, 347);
			this.tableLayoutPanel1.TabIndex = 8;
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(247, 319);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvCancel.TabIndex = 8;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOk.IsBackStageButton = false;
			this.buttonAdvOk.Location = new System.Drawing.Point(166, 319);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvOk.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvOk.TabIndex = 9;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOk_Click);
			// 
			// MapDestinationBuSkill
			// 
			this.AcceptButton = this.buttonAdvOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonAdvCancel;
			this.ClientSize = new System.Drawing.Size(325, 347);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MapDestinationBuSkill";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxMapDestination";
			this.Load += new System.EventHandler(this.MapDestinationBuSkill_Load);
			((System.ComponentModel.ISupportInitialize)(this.textBoxExFilter)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewSkills;
        private System.Windows.Forms.ColumnHeader columnHeaderExport;
        private System.Windows.Forms.ColumnHeader columnHeaderSkill;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExFilter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
    }
}