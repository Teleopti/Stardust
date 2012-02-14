namespace Teleopti.Ccc.Win.Common
{
    partial class ScheduleFilterView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleFilterView));
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.tabControlAdv = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemSearch = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdv)).BeginInit();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ccc_BusinessUnit.png");
			this.imageList1.Images.SetKeyName(1, "ccc_Site.png");
			this.imageList1.Images.SetKeyName(2, "ccc_Team.png");
			this.imageList1.Images.SetKeyName(3, "ccc_Agent.png");
			// 
			// tabControlAdv
			// 
			this.tabControlAdv.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabControlAdv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabControlAdv.BorderVisible = true;
			this.tabControlAdv.BorderWidth = 1;
			this.tabControlAdv.Location = new System.Drawing.Point(7, 6);
			this.tabControlAdv.Name = "tabControlAdv";
			this.tabControlAdv.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.tabControlAdv.ShowScroll = false;
			this.tabControlAdv.Size = new System.Drawing.Size(313, 443);
			this.tabControlAdv.TabIndex = 5;
			this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.FirstTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitiveFirst", ""));
			this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.PreviousTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitivePrevious", ""));
			this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.NextTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitiveNext", ""));
			this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.LastTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitiveLast", ""));
			this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.DropDown, null, System.Drawing.Color.Empty, true, 1, "TabPrimitiveDropDown", ""));
			this.tabControlAdv.TabPrimitivesHost.Visible = true;
			this.tabControlAdv.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererIE7);
			this.tabControlAdv.SelectedIndexChanged += new System.EventHandler(this.tabControlAdv_SelectedIndexChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonCancel.Location = new System.Drawing.Point(240, 455);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOk
			// 
			this.buttonOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Location = new System.Drawing.Point(152, 455);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonOk.Size = new System.Drawing.Size(75, 23);
			this.buttonOk.TabIndex = 3;
			this.buttonOk.Text = "xxOk";
			this.buttonOk.UseVisualStyle = true;
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSearch});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(174, 48);
			// 
			// toolStripMenuItemSearch
			// 
			this.toolStripMenuItemSearch.Name = "toolStripMenuItemSearch";
			this.toolStripMenuItemSearch.Size = new System.Drawing.Size(173, 22);
			this.toolStripMenuItemSearch.Text = "xxSearchThreeDots";
			this.toolStripMenuItemSearch.Click += new System.EventHandler(this.toolStripMenuItemSearch_Click);
			// 
			// ScheduleFilterView
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(339, 486);
			this.ControlBox = false;
			this.Controls.Add(this.tabControlAdv);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ScheduleFilterView";
			this.Text = "xxFilter";
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdv)).EndInit();
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.ButtonAdv buttonOk;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdv;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSearch;
    }
}