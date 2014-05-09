namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
    partial class UserCredentialConflicts
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
			this.listViewConflicts = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// listViewConflicts
			// 
			this.listViewConflicts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader5});
			this.listViewConflicts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewConflicts.Location = new System.Drawing.Point(0, 0);
			this.listViewConflicts.Name = "listViewConflicts";
			this.listViewConflicts.Size = new System.Drawing.Size(963, 453);
			this.listViewConflicts.TabIndex = 0;
			this.listViewConflicts.UseCompatibleStateImageBehavior = false;
			this.listViewConflicts.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "xxPerson";
			this.columnHeader1.Width = 176;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "xxLogOn";
			this.columnHeader2.Width = 378;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "xxApplicationLogInName";
			this.columnHeader4.Width = 231;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "xxTerminalDate";
			this.columnHeader5.Width = 136;
			// 
			// UserCredentialConflicts
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(963, 453);
			this.Controls.Add(this.listViewConflicts);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UserCredentialConflicts";
			this.Text = "xxErrorMsgCaptionDuplicateUserCredentials";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.userCredentialConflictsKeyDown);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewConflicts;
        private System.Windows.Forms.ColumnHeader columnHeader1;
		  private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
    }
}