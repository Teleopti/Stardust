namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	partial class PickColorControl
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
				this.colorPickerUIAdv1.Picked -= new Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv.ColorPickedEventHandler(this.colorPickerUiAdv1Picked);
				if (components != null)
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
			this.popupControlContainer1 = new Syncfusion.Windows.Forms.PopupControlContainer(this.components);
			this.colorPickerUIAdv1 = new Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv();
			this.xpToolBar1 = new Syncfusion.Windows.Forms.Tools.XPMenus.XPToolBar();
			this.dropDownBarItem2 = new Syncfusion.Windows.Forms.Tools.XPMenus.DropDownBarItem();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.popupControlContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// popupControlContainer1
			// 
			this.popupControlContainer1.BackColor = System.Drawing.Color.Transparent;
			this.popupControlContainer1.Controls.Add(this.colorPickerUIAdv1);
			this.popupControlContainer1.Location = new System.Drawing.Point(710, 509);
			this.popupControlContainer1.Name = "popupControlContainer1";
			this.popupControlContainer1.Size = new System.Drawing.Size(173, 195);
			this.popupControlContainer1.TabIndex = 122;
			this.popupControlContainer1.Visible = false;
			// 
			// colorPickerUIAdv1.RecentGroup
			// 
			this.colorPickerUIAdv1.RecentGroup.Name = "RecentColors";
			this.colorPickerUIAdv1.RecentGroup.Visible = false;
			// 
			// colorPickerUIAdv1.StandardGroup
			// 
			this.colorPickerUIAdv1.StandardGroup.Name = "StandardColors";
			// 
			// colorPickerUIAdv1.ThemeGroup
			// 
			this.colorPickerUIAdv1.ThemeGroup.IsSubItemsVisible = true;
			this.colorPickerUIAdv1.ThemeGroup.Name = "ThemeColors";
			// 
			// colorPickerUIAdv1
			// 
			this.colorPickerUIAdv1.BeforeTouchSize = new System.Drawing.Size(13, 13);
			this.colorPickerUIAdv1.ColorItemSize = new System.Drawing.Size(13, 13);
			this.colorPickerUIAdv1.Dock = System.Windows.Forms.DockStyle.Top;
			this.colorPickerUIAdv1.Location = new System.Drawing.Point(0, 0);
			this.colorPickerUIAdv1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.colorPickerUIAdv1.MinimumSize = new System.Drawing.Size(136, 195);
			this.colorPickerUIAdv1.Name = "colorPickerUIAdv1";
			this.colorPickerUIAdv1.SelectedColor = System.Drawing.Color.Empty;
			this.colorPickerUIAdv1.Size = new System.Drawing.Size(173, 195);
			this.colorPickerUIAdv1.Style = Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv.visualstyle.Default;
			this.colorPickerUIAdv1.TabIndex = 0;
			this.colorPickerUIAdv1.Text = "yycolorPickerUIAdv1";
			this.colorPickerUIAdv1.UseOffice2007Style = false;
			this.colorPickerUIAdv1.Picked += new Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv.ColorPickedEventHandler(this.colorPickerUiAdv1Picked);
			// 
			// xpToolBar1
			// 
			// 
			// 
			// 
			this.xpToolBar1.Bar.BarName = "";
			this.xpToolBar1.Bar.Items.AddRange(new Syncfusion.Windows.Forms.Tools.XPMenus.BarItem[] {
				this.dropDownBarItem2});
			this.xpToolBar1.Bar.Manager = null;
			this.xpToolBar1.Location = new System.Drawing.Point(0, 35);
			this.xpToolBar1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.xpToolBar1.Name = "xpToolBar1";
			this.xpToolBar1.ShowHighlightRectangle = false;
			this.xpToolBar1.Size = new System.Drawing.Size(105, 23);
			this.xpToolBar1.TabIndex = 0;
			// 
			// dropDownBarItem2
			// 
			this.dropDownBarItem2.BarName = "dropDownBarItem2";
			this.dropDownBarItem2.CustomTextFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dropDownBarItem2.ID = "Custom Colors";
			this.dropDownBarItem2.ParentStyle = Syncfusion.Windows.Forms.Tools.XPMenus.ParentBarItemStyle.Default;
			this.dropDownBarItem2.PopupControlContainer = this.popupControlContainer1;
			this.dropDownBarItem2.ShowToolTipInPopUp = false;
			this.dropDownBarItem2.SizeToFit = true;
			this.dropDownBarItem2.Text = "xxSelectColor";
			this.dropDownBarItem2.BeforePopupItemPaint += new Syncfusion.Windows.Forms.Tools.XPMenus.PopupItemPaintEventHandler(this.dropDownBarItem2BeforePopupItemPaint);
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.White;
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Location = new System.Drawing.Point(28, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(29, 29);
			this.pictureBox1.TabIndex = 124;
			this.pictureBox1.TabStop = false;
			// 
			// PickColorControl
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.xpToolBar1);
			this.Name = "PickColorControl";
			this.Size = new System.Drawing.Size(100, 66);
			this.popupControlContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.XPMenus.XPToolBar xpToolBar1;
		private Syncfusion.Windows.Forms.Tools.XPMenus.DropDownBarItem dropDownBarItem2;
		private Syncfusion.Windows.Forms.PopupControlContainer popupControlContainer1;
		private Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv colorPickerUIAdv1;
		private System.Windows.Forms.PictureBox pictureBox1;

	}
}
