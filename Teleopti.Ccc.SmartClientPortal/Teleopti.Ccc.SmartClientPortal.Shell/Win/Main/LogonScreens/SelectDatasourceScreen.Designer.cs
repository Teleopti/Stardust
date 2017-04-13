namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	partial class SelectDatasourceScreen
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
			this.buttonLogOnOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnBack = new Syncfusion.Windows.Forms.ButtonAdv();
			this.radioButtonAdvWindows = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonAdvApplication = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvWindows)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvApplication)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonLogOnOK
			// 
			this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonLogOnOK.BackColor = System.Drawing.Color.White;
			this.buttonLogOnOK.BeforeTouchSize = new System.Drawing.Size(35, 35);
			this.buttonLogOnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonLogOnOK.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnOK.ForeColor = System.Drawing.Color.White;
			this.buttonLogOnOK.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.right_round_32;
			this.buttonLogOnOK.IsBackStageButton = false;
			this.buttonLogOnOK.Location = new System.Drawing.Point(250, 107);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(35, 35);
			this.buttonLogOnOK.TabIndex = 3;
			this.buttonLogOnOK.UseVisualStyleBackColor = false;
			this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
			// 
			// btnBack
			// 
			this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBack.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnBack.BackColor = System.Drawing.Color.White;
			this.btnBack.BeforeTouchSize = new System.Drawing.Size(35, 35);
			this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBack.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBack.ForeColor = System.Drawing.Color.White;
			this.btnBack.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.left_round_32;
			this.btnBack.IsBackStageButton = false;
			this.btnBack.Location = new System.Drawing.Point(209, 107);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(35, 35);
			this.btnBack.TabIndex = 4;
			this.btnBack.UseVisualStyleBackColor = false;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			// 
			// radioButtonAdvWindows
			// 
			this.radioButtonAdvWindows.BeforeTouchSize = new System.Drawing.Size(211, 21);
			this.radioButtonAdvWindows.Checked = true;
			this.radioButtonAdvWindows.DrawFocusRectangle = false;
			this.radioButtonAdvWindows.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.radioButtonAdvWindows.Location = new System.Drawing.Point(3, 65);
			this.radioButtonAdvWindows.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAdvWindows.Name = "radioButtonAdvWindows";
			this.radioButtonAdvWindows.Size = new System.Drawing.Size(211, 21);
			this.radioButtonAdvWindows.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonAdvWindows.TabIndex = 1;
			this.radioButtonAdvWindows.Text = "&Windows logon";
			this.radioButtonAdvWindows.ThemesEnabled = false;
			this.radioButtonAdvWindows.CheckChanged += new System.EventHandler(this.radioButtonAdvWindows_CheckChanged);
			// 
			// radioButtonAdvApplication
			// 
			this.radioButtonAdvApplication.BeforeTouchSize = new System.Drawing.Size(211, 21);
			this.radioButtonAdvApplication.DrawFocusRectangle = false;
			this.radioButtonAdvApplication.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.radioButtonAdvApplication.Location = new System.Drawing.Point(3, 85);
			this.radioButtonAdvApplication.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAdvApplication.Name = "radioButtonAdvApplication";
			this.radioButtonAdvApplication.Size = new System.Drawing.Size(211, 21);
			this.radioButtonAdvApplication.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonAdvApplication.TabIndex = 2;
			this.radioButtonAdvApplication.TabStop = false;
			this.radioButtonAdvApplication.Text = "&Application logon";
			this.radioButtonAdvApplication.ThemesEnabled = false;
			// 
			// SelectDatasourceScreen
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.btnBack);
			this.Controls.Add(this.radioButtonAdvApplication);
			this.Controls.Add(this.radioButtonAdvWindows);
			this.Controls.Add(this.buttonLogOnOK);
			this.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SelectDatasourceScreen";
			this.Size = new System.Drawing.Size(285, 142);
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvWindows)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvApplication)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnOK;
		private Syncfusion.Windows.Forms.ButtonAdv btnBack;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAdvWindows;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAdvApplication;


	}
}
