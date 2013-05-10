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
			this.button1 = new System.Windows.Forms.Button();
			this.searchPersonView1 = new Teleopti.Ccc.Win.SearchPersonView();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(268, 458);
			this.button1.Margin = new System.Windows.Forms.Padding(10);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "xxOK";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// searchPersonView1
			// 
			this.searchPersonView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.searchPersonView1.Location = new System.Drawing.Point(0, 0);
			this.searchPersonView1.Margin = new System.Windows.Forms.Padding(0);
			this.searchPersonView1.Name = "searchPersonView1";
			this.searchPersonView1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
			this.searchPersonView1.Size = new System.Drawing.Size(353, 446);
			this.searchPersonView1.TabIndex = 0;
			this.searchPersonView1.ItemDoubleClick += new System.EventHandler<System.EventArgs>(this.searchPersonView1_ItemDoubleClick);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.searchPersonView1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.button1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(353, 491);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// SearchPerson
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(353, 491);
			this.Controls.Add(this.tableLayoutPanel1);
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(140, 46);
			this.Name = "SearchPerson";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "xxFind";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SearchPerson_FormClosing);
			this.Load += new System.EventHandler(this.SearchPerson_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private SearchPersonView searchPersonView1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}