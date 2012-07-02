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
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(353, 525);
			this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(100, 28);
			this.button1.TabIndex = 1;
			this.button1.Text = "xxOK";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// searchPersonView1
			// 
			this.searchPersonView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.searchPersonView1.Location = new System.Drawing.Point(5, 5);
			this.searchPersonView1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.searchPersonView1.Name = "searchPersonView1";
			this.searchPersonView1.Size = new System.Drawing.Size(419, 489);
			this.searchPersonView1.TabIndex = 0;
			this.searchPersonView1.ItemDoubleClick += new System.EventHandler<System.EventArgs>(this.searchPersonView1_ItemDoubleClick);
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.AutoSize = true;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MinimumSize = new System.Drawing.Size(0, 41);
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.ShowContextMenu = false;
			this.ribbonControlAdv1.ShowLauncher = false;
			this.ribbonControlAdv1.ShowMinimizeButton = false;
			this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(469, 41);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdv1.TabIndex = 2;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.searchPersonView1, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(24, 59);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(429, 452);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// SearchPerson
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(471, 604);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Controls.Add(this.button1);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(182, 48);
			this.Name = "SearchPerson";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "xxSearch";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SearchPerson_FormClosing);
			this.Load += new System.EventHandler(this.SearchPerson_Load);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private SearchPersonView searchPersonView1;
        private System.Windows.Forms.Button button1;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}