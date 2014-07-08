namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class TwoListSelector
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
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                ReleaseManagedResources();
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonAdvDeselectAll = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvDeselectOne = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvSelectOne = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvSelectAll = new Syncfusion.Windows.Forms.ButtonAdv();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.listBox2 = new System.Windows.Forms.ListBox();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.availableLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.selectedLabel = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.listBox2, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.listBox1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.availableLabel, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.selectedLabel, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(413, 344);
			this.tableLayoutPanel1.TabIndex = 9;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.label3, 0, 2);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(171, 25);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(70, 316);
			this.tableLayoutPanel2.TabIndex = 13;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonAdvDeselectAll);
			this.panel1.Controls.Add(this.buttonAdvDeselectOne);
			this.panel1.Controls.Add(this.buttonAdvSelectOne);
			this.panel1.Controls.Add(this.buttonAdvSelectAll);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 81);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(64, 114);
			this.panel1.TabIndex = 11;
			// 
			// buttonAdvDeselectAll
			// 
			this.buttonAdvDeselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvDeselectAll.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvDeselectAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvDeselectAll.BeforeTouchSize = new System.Drawing.Size(57, 23);
			this.buttonAdvDeselectAll.ForeColor = System.Drawing.Color.White;
			this.buttonAdvDeselectAll.IsBackStageButton = false;
			this.buttonAdvDeselectAll.Location = new System.Drawing.Point(3, 90);
			this.buttonAdvDeselectAll.Name = "buttonAdvDeselectAll";
			this.buttonAdvDeselectAll.Size = new System.Drawing.Size(57, 23);
			this.buttonAdvDeselectAll.TabIndex = 12;
			this.buttonAdvDeselectAll.Text = "<<";
			this.buttonAdvDeselectAll.UseVisualStyle = true;
			this.buttonAdvDeselectAll.Click += new System.EventHandler(this.buttonAdvDeselectAll_Click);
			// 
			// buttonAdvDeselectOne
			// 
			this.buttonAdvDeselectOne.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvDeselectOne.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvDeselectOne.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvDeselectOne.BeforeTouchSize = new System.Drawing.Size(57, 23);
			this.buttonAdvDeselectOne.ForeColor = System.Drawing.Color.White;
			this.buttonAdvDeselectOne.IsBackStageButton = false;
			this.buttonAdvDeselectOne.Location = new System.Drawing.Point(3, 61);
			this.buttonAdvDeselectOne.Name = "buttonAdvDeselectOne";
			this.buttonAdvDeselectOne.Size = new System.Drawing.Size(57, 23);
			this.buttonAdvDeselectOne.TabIndex = 11;
			this.buttonAdvDeselectOne.Text = "<";
			this.buttonAdvDeselectOne.UseVisualStyle = true;
			this.buttonAdvDeselectOne.Click += new System.EventHandler(this.buttonAdvDeselectOne_Click);
			// 
			// buttonAdvSelectOne
			// 
			this.buttonAdvSelectOne.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvSelectOne.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvSelectOne.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvSelectOne.BeforeTouchSize = new System.Drawing.Size(57, 23);
			this.buttonAdvSelectOne.ForeColor = System.Drawing.Color.White;
			this.buttonAdvSelectOne.IsBackStageButton = false;
			this.buttonAdvSelectOne.Location = new System.Drawing.Point(3, 32);
			this.buttonAdvSelectOne.Name = "buttonAdvSelectOne";
			this.buttonAdvSelectOne.Size = new System.Drawing.Size(57, 23);
			this.buttonAdvSelectOne.TabIndex = 10;
			this.buttonAdvSelectOne.Text = ">";
			this.buttonAdvSelectOne.UseVisualStyle = true;
			this.buttonAdvSelectOne.Click += new System.EventHandler(this.buttonAdvSelectOne_Click);
			// 
			// buttonAdvSelectAll
			// 
			this.buttonAdvSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvSelectAll.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvSelectAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvSelectAll.BeforeTouchSize = new System.Drawing.Size(57, 23);
			this.buttonAdvSelectAll.ForeColor = System.Drawing.Color.White;
			this.buttonAdvSelectAll.IsBackStageButton = false;
			this.buttonAdvSelectAll.Location = new System.Drawing.Point(3, 3);
			this.buttonAdvSelectAll.Name = "buttonAdvSelectAll";
			this.buttonAdvSelectAll.Size = new System.Drawing.Size(57, 23);
			this.buttonAdvSelectAll.TabIndex = 9;
			this.buttonAdvSelectAll.Text = ">>";
			this.buttonAdvSelectAll.UseVisualStyle = true;
			this.buttonAdvSelectAll.Click += new System.EventHandler(this.buttonAdvSelectAll_Click);
			// 
			// label2
			// 
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 78);
			this.label2.TabIndex = 12;
			// 
			// label3
			// 
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 198);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 118);
			this.label3.TabIndex = 13;
			// 
			// listBox2
			// 
			this.listBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox2.FormattingEnabled = true;
			this.listBox2.Location = new System.Drawing.Point(247, 25);
			this.listBox2.Name = "listBox2";
			this.listBox2.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listBox2.Size = new System.Drawing.Size(163, 316);
			this.listBox2.Sorted = true;
			this.listBox2.TabIndex = 12;
			this.listBox2.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox2_DrawItem);
			this.listBox2.DoubleClick += new System.EventHandler(this.listBox2_DoubleClick);
			// 
			// listBox1
			// 
			this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox1.FormattingEnabled = true;
			this.listBox1.Location = new System.Drawing.Point(3, 25);
			this.listBox1.Name = "listBox1";
			this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listBox1.Size = new System.Drawing.Size(162, 316);
			this.listBox1.Sorted = true;
			this.listBox1.TabIndex = 1;
			this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
			// 
			// availableLabel
			// 
			this.availableLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.availableLabel.Location = new System.Drawing.Point(3, 0);
			this.availableLabel.Name = "availableLabel";
			this.availableLabel.Size = new System.Drawing.Size(162, 22);
			this.availableLabel.TabIndex = 0;
			this.availableLabel.Text = "gradientLabel1";
			this.availableLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(171, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(70, 22);
			this.label1.TabIndex = 13;
			// 
			// selectedLabel
			// 
			this.selectedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.selectedLabel.Location = new System.Drawing.Point(247, 0);
			this.selectedLabel.Name = "selectedLabel";
			this.selectedLabel.Size = new System.Drawing.Size(163, 22);
			this.selectedLabel.TabIndex = 14;
			this.selectedLabel.Text = "gradientLabel2";
			this.selectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TwoListSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "TwoListSelector";
			this.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Size = new System.Drawing.Size(413, 344);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Panel panel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDeselectAll;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDeselectOne;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSelectOne;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSelectAll;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label availableLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label selectedLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;

    }
}