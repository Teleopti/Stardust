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
			this.xpToolBar1 = new Syncfusion.Windows.Forms.Tools.XPMenus.XPToolBar();
			this.dropDownBarItem2 = new Syncfusion.Windows.Forms.Tools.XPMenus.DropDownBarItem();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.colorPickerUIAdv1 = new Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
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
			this.dropDownBarItem2.ShowToolTipInPopUp = false;
			this.dropDownBarItem2.SizeToFit = true;
			this.dropDownBarItem2.Text = "xxSelectColor";
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
			// colorPickerUIAdv1.RecentGroup
			// 
			// 
			// colorPickerUIAdv1.StandardGroup
			// 
			// 
			// colorPickerUIAdv1.ThemeGroup
			// 
			// 
			// colorPickerUIAdv1
			// 
			this.colorPickerUIAdv1.BeforeTouchSize = new System.Drawing.Size(13, 13);
			this.colorPickerUIAdv1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.colorPickerUIAdv1.ColorItemSize = new System.Drawing.Size(13, 13);
			this.colorPickerUIAdv1.Dock = System.Windows.Forms.DockStyle.Top;
			this.colorPickerUIAdv1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.colorPickerUIAdv1.Location = new System.Drawing.Point(0, 0);
			this.colorPickerUIAdv1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.colorPickerUIAdv1.MinimumSize = new System.Drawing.Size(136, 163);
			this.colorPickerUIAdv1.Name = "colorPickerUIAdv1";
			this.colorPickerUIAdv1.SelectedColor = System.Drawing.Color.Empty;
			this.colorPickerUIAdv1.Size = new System.Drawing.Size(173, 195);
			this.colorPickerUIAdv1.Style = Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv.visualstyle.Metro;
			this.colorPickerUIAdv1.TabIndex = 0;
			this.colorPickerUIAdv1.Text = "yycolorPickerUIAdv1";
			this.colorPickerUIAdv1.UseOffice2007Style = false;
			this.colorPickerUIAdv1.Picked += new Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv.ColorPickedEventHandler(this.colorPickerUiAdv1Picked);
			// 
			// PickColorControl
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.xpToolBar1);
			this.Name = "PickColorControl";
			this.Size = new System.Drawing.Size(100, 66);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.XPMenus.XPToolBar xpToolBar1;
		private Syncfusion.Windows.Forms.Tools.XPMenus.DropDownBarItem dropDownBarItem2;
		private System.Windows.Forms.PictureBox pictureBox1;
		private Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv colorPickerUIAdv1;

	}
}
