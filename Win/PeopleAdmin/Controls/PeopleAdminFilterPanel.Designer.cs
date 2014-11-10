namespace Teleopti.Ccc.Win.PeopleAdmin.Controls
{
    partial class PeopleAdminFilterPanel
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PeopleAdminFilterPanel));
			this.ximgBUStructure = new System.Windows.Forms.ImageList(this.components);
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ximgBUStructure
			// 
			this.ximgBUStructure.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ximgBUStructure.ImageStream")));
			this.ximgBUStructure.TransparentColor = System.Drawing.Color.Transparent;
			this.ximgBUStructure.Images.SetKeyName(0, "ccc_BusinessUnit.png");
			this.ximgBUStructure.Images.SetKeyName(1, "ccc_Site.png");
			this.ximgBUStructure.Images.SetKeyName(2, "ccc_Team.png");
			this.ximgBUStructure.Images.SetKeyName(3, "ccc_Agent.png");
			this.ximgBUStructure.Images.SetKeyName(4, "ccc_User.png");
			this.ximgBUStructure.Images.SetKeyName(5, "Bu16.ico");
			this.ximgBUStructure.Images.SetKeyName(6, "Home16.ico");
			this.ximgBUStructure.Images.SetKeyName(7, "personSmall.ico");
			this.ximgBUStructure.Images.SetKeyName(8, "onePersonSmall.ico");
			this.ximgBUStructure.Images.SetKeyName(9, "Bu32.ico");
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.17192F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.48424F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.05731F));
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvOk, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvCancel, 2, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 507);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(349, 30);
			this.tableLayoutPanel2.TabIndex = 2;
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvOk.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonAdvOk.Location = new System.Drawing.Point(213, 3);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Size = new System.Drawing.Size(62, 24);
			this.buttonAdvOk.TabIndex = 0;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.ButtonAdvOkClick);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvCancel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonAdvCancel.Location = new System.Drawing.Point(281, 3);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(65, 24);
			this.buttonAdvCancel.TabIndex = 1;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.ButtonAdvCancelClick);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.51852F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.481482F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(355, 540);
			this.tableLayoutPanel1.TabIndex = 3;
			this.tableLayoutPanel1.VisibleChanged += new System.EventHandler(this.tableLayoutPanel1_VisibleChanged);
			// 
			// PeopleAdminFilterPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "PeopleAdminFilterPanel";
			this.Size = new System.Drawing.Size(355, 540);
			this.Load += new System.EventHandler(this.peopleAdminFilterPanelLoad);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList ximgBUStructure;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;



    }
}
