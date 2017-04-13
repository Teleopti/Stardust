﻿namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday.Reforecast
{
    partial class SelectWorkload
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
			this.listViewWorkloads = new System.Windows.Forms.ListView();
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
			this.textBoxExFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxExFilter.Location = new System.Drawing.Point(84, 3);
			this.textBoxExFilter.Name = "textBoxExFilter";
			this.textBoxExFilter.Size = new System.Drawing.Size(323, 23);
			this.textBoxExFilter.TabIndex = 0;
			this.textBoxExFilter.TextChanged += new System.EventHandler(this.textBoxExFilter_TextChanged);
			// 
			// listViewWorkloads
			// 
			this.listViewWorkloads.Activation = System.Windows.Forms.ItemActivation.TwoClick;
			this.listViewWorkloads.CheckBoxes = true;
			this.listViewWorkloads.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewWorkloads.FullRowSelect = true;
			this.listViewWorkloads.Location = new System.Drawing.Point(6, 34);
			this.listViewWorkloads.Margin = new System.Windows.Forms.Padding(6, 0, 6, 6);
			this.listViewWorkloads.MultiSelect = false;
			this.listViewWorkloads.Name = "listViewWorkloads";
			this.listViewWorkloads.ShowGroups = false;
			this.listViewWorkloads.Size = new System.Drawing.Size(404, 253);
			this.listViewWorkloads.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewWorkloads.TabIndex = 6;
			this.listViewWorkloads.TileSize = new System.Drawing.Size(100, 100);
			this.listViewWorkloads.UseCompatibleStateImageBehavior = false;
			this.listViewWorkloads.View = System.Windows.Forms.View.Details;
			this.listViewWorkloads.ColumnClick += ListViewWorkloadsOnColumnClick;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.listViewWorkloads, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(416, 288);
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
			this.tableLayoutPanel2.Size = new System.Drawing.Size(410, 28);
			this.tableLayoutPanel2.TabIndex = 7;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 15);
			this.label1.TabIndex = 4;
			this.label1.Text = "xxFilterColon";
			// 
			// SelectWorkload
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "SelectWorkload";
			this.Size = new System.Drawing.Size(416, 288);
			((System.ComponentModel.ISupportInitialize)(this.textBoxExFilter)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

        }
        
        #endregion

        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExFilter;
        private System.Windows.Forms.ListView listViewWorkloads;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;




    }
}
