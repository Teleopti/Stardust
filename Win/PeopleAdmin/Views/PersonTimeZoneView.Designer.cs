namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
	partial class PersonTimeZoneView
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
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.comboBoxAdvTimeZones = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTimeZones)).BeginInit();
			this.SuspendLayout();
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.ShowItemToolTips = true;
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(298, 33);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdv1.TabIndex = 1;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvOk.Location = new System.Drawing.Point(118, 120);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvOk.TabIndex = 6;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvCancel.Location = new System.Drawing.Point(207, 120);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvCancel.TabIndex = 7;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// comboBoxAdvTimeZones
			// 
			this.comboBoxAdvTimeZones.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvTimeZones.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdvTimeZones.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvTimeZones.Location = new System.Drawing.Point(72, 66);
			this.comboBoxAdvTimeZones.Name = "comboBoxAdvTimeZones";
			this.comboBoxAdvTimeZones.Size = new System.Drawing.Size(210, 21);
			this.comboBoxAdvTimeZones.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdvTimeZones.TabIndex = 8;
			// 
			// PersonTimeZoneView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(300, 161);
			this.Controls.Add(this.comboBoxAdvTimeZones);
			this.Controls.Add(this.buttonAdvOk);
			this.Controls.Add(this.buttonAdvCancel);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Name = "PersonTimeZoneView";
			this.Text = "xxTimeZone";
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTimeZones)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvTimeZones;
	}
}