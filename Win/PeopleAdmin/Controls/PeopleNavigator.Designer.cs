using Syncfusion.Windows.Forms.Tools.Enums;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls
{
    partial class PeopleNavigator
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxFindThreeDots")]
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.splitContainerAdv1 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.toolStripPeople = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonAddPerson = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsAddGroupPage = new System.Windows.Forms.ToolStripButton();
			this.tsRenameGroupPage = new System.Windows.Forms.ToolStripButton();
			this.tsEditGroupPage = new System.Windows.Forms.ToolStripButton();
			this.tsDeleteGroupPage = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonSendInstantMessage = new System.Windows.Forms.ToolStripButton();
			this.toolStripRefresh = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).BeginInit();
			this.splitContainerAdv1.Panel2.SuspendLayout();
			this.splitContainerAdv1.SuspendLayout();
			this.toolStripPeople.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// splitContainerAdv1
			// 
			this.splitContainerAdv1.BackColor = System.Drawing.Color.Transparent;
			this.splitContainerAdv1.BeforeTouchSize = 4;
			this.splitContainerAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdv1.Location = new System.Drawing.Point(0, 0);
			this.splitContainerAdv1.Name = "splitContainerAdv1";
			this.splitContainerAdv1.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.splitContainerAdv1.Panel1MinSize = 150;
			// 
			// splitContainerAdv1.Panel2
			// 
			this.splitContainerAdv1.Panel2.AutoScroll = true;
			this.splitContainerAdv1.Panel2.BackColor = System.Drawing.Color.White;
			this.splitContainerAdv1.Panel2.Controls.Add(this.toolStripPeople);
			this.splitContainerAdv1.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.splitContainerAdv1.Size = new System.Drawing.Size(262, 606);
			this.splitContainerAdv1.SplitterDistance = 399;
			this.splitContainerAdv1.SplitterWidth = 4;
			this.splitContainerAdv1.TabIndex = 1;
			this.splitContainerAdv1.Text = "splitContainerAdv1";
			this.splitContainerAdv1.SplitterMoved += new Syncfusion.Windows.Forms.Tools.Events.SplitterMoveEventHandler(this.splitContainerAdv1SplitterMoved);
			// 
			// toolStripPeople
			// 
			this.toolStripPeople.BackColor = System.Drawing.Color.Transparent;
			this.toolStripPeople.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.toolStripPeople.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.toolStripPeople.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.toolStripPeople.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripPeople.Image = null;
			this.toolStripPeople.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripButtonOpen,
            this.toolStripButtonAddPerson,
            this.toolStripSeparator2,
            this.tsAddGroupPage,
            this.tsRenameGroupPage,
            this.tsEditGroupPage,
            this.tsDeleteGroupPage,
            this.toolStripSeparator5,
            this.toolStripButtonSendInstantMessage,
            this.toolStripRefresh,
            this.toolStripButtonSearch});
			this.toolStripPeople.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripPeople.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripPeople.Location = new System.Drawing.Point(10, 0);
			this.toolStripPeople.Name = "toolStripPeople";
			this.toolStripPeople.Office12Mode = false;
			this.toolStripPeople.ShowCaption = false;
			this.toolStripPeople.Size = new System.Drawing.Size(235, 341);
			this.toolStripPeople.TabIndex = 0;
			this.toolStripPeople.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripLabel1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(233, 17);
			this.toolStripLabel1.Text = "xxActions";
			// 
			// toolStripButtonOpen
			// 
			this.toolStripButtonOpen.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Open;
			this.toolStripButtonOpen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonOpen.Name = "toolStripButtonOpen";
			this.toolStripButtonOpen.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonOpen.Size = new System.Drawing.Size(233, 29);
			this.toolStripButtonOpen.Text = "xxOpenPeople";
			this.toolStripButtonOpen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripButtonAddPerson
			// 
			this.toolStripButtonAddPerson.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Open;
			this.toolStripButtonAddPerson.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonAddPerson.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonAddPerson.Name = "toolStripButtonAddPerson";
			this.toolStripButtonAddPerson.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonAddPerson.Size = new System.Drawing.Size(233, 29);
			this.toolStripButtonAddPerson.Text = "xxAddPerson";
			this.toolStripButtonAddPerson.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(233, 6);
			// 
			// tsAddGroupPage
			// 
			this.tsAddGroupPage.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New_Group_32x32;
			this.tsAddGroupPage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tsAddGroupPage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsAddGroupPage.Name = "tsAddGroupPage";
			this.tsAddGroupPage.Padding = new System.Windows.Forms.Padding(4);
			this.tsAddGroupPage.Size = new System.Drawing.Size(233, 29);
			this.tsAddGroupPage.Text = "xxAddNewGroupPage";
			this.tsAddGroupPage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tsAddGroupPage.Click += new System.EventHandler(this.tsAddGroupPageClick);
			// 
			// tsRenameGroupPage
			// 
			this.tsRenameGroupPage.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Rename_group_32x32;
			this.tsRenameGroupPage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tsRenameGroupPage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsRenameGroupPage.Name = "tsRenameGroupPage";
			this.tsRenameGroupPage.Padding = new System.Windows.Forms.Padding(4);
			this.tsRenameGroupPage.Size = new System.Drawing.Size(233, 29);
			this.tsRenameGroupPage.Text = "xxRenameGroupPage";
			this.tsRenameGroupPage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tsRenameGroupPage.Click += new System.EventHandler(this.tsRenameGroupPageClick);
			// 
			// tsEditGroupPage
			// 
			this.tsEditGroupPage.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Edit_group_32x32;
			this.tsEditGroupPage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tsEditGroupPage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsEditGroupPage.Name = "tsEditGroupPage";
			this.tsEditGroupPage.Padding = new System.Windows.Forms.Padding(4);
			this.tsEditGroupPage.Size = new System.Drawing.Size(233, 29);
			this.tsEditGroupPage.Text = "xxEditGroupPage";
			this.tsEditGroupPage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tsEditGroupPage.Click += new System.EventHandler(this.tsEditGroupPageClick);
			// 
			// tsDeleteGroupPage
			// 
			this.tsDeleteGroupPage.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.tsDeleteGroupPage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tsDeleteGroupPage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsDeleteGroupPage.Name = "tsDeleteGroupPage";
			this.tsDeleteGroupPage.Padding = new System.Windows.Forms.Padding(4);
			this.tsDeleteGroupPage.Size = new System.Drawing.Size(233, 29);
			this.tsDeleteGroupPage.Text = "xxDeleteGroupPage";
			this.tsDeleteGroupPage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tsDeleteGroupPage.Click += new System.EventHandler(this.tsDeleteGroupPageClick);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(233, 6);
			// 
			// toolStripButtonSendInstantMessage
			// 
			this.toolStripButtonSendInstantMessage.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_ListRequests_32x32;
			this.toolStripButtonSendInstantMessage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonSendInstantMessage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSendInstantMessage.Name = "toolStripButtonSendInstantMessage";
			this.toolStripButtonSendInstantMessage.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonSendInstantMessage.Size = new System.Drawing.Size(233, 29);
			this.toolStripButtonSendInstantMessage.Text = "xxMessages";
			this.toolStripButtonSendInstantMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonSendInstantMessage.Click += new System.EventHandler(this.toolStripButtonSendInstantMessageClick);
			// 
			// toolStripRefresh
			// 
			this.toolStripRefresh.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Refresh;
			this.toolStripRefresh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripRefresh.Name = "toolStripRefresh";
			this.toolStripRefresh.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripRefresh.Size = new System.Drawing.Size(233, 29);
			this.toolStripRefresh.Text = "xxRefresh";
			this.toolStripRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripRefresh.Click += new System.EventHandler(this.toolStripRefreshClick);
			// 
			// toolStripButtonSearch
			// 
			this.toolStripButtonSearch.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_FindAgent;
			this.toolStripButtonSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSearch.Name = "toolStripButtonSearch";
			this.toolStripButtonSearch.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonSearch.Size = new System.Drawing.Size(233, 29);
			this.toolStripButtonSearch.Text = "xxFindThreeDots";
			this.toolStripButtonSearch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonSearch.Click += new System.EventHandler(this.toolStripButtonSearchClick);
			// 
			// PeopleNavigator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainerAdv1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "PeopleNavigator";
			this.Size = new System.Drawing.Size(262, 606);
			this.splitContainerAdv1.Panel2.ResumeLayout(false);
			this.splitContainerAdv1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).EndInit();
			this.splitContainerAdv1.ResumeLayout(false);
			this.toolStripPeople.ResumeLayout(false);
			this.toolStripPeople.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv1;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripPeople;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripButton toolStripRefresh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsAddGroupPage;
        private System.Windows.Forms.ToolStripButton tsEditGroupPage;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton toolStripButtonAddPerson;
        private System.Windows.Forms.ToolStripButton toolStripButtonSendInstantMessage;
        private System.Windows.Forms.ToolStripButton toolStripButtonSearch;
        private System.Windows.Forms.ToolStripButton tsRenameGroupPage;
		private System.Windows.Forms.ToolStripButton tsDeleteGroupPage;
    }
}
