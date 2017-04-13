namespace Teleopti.Ccc.Win.Grouping
{
	partial class PersonSelectorViewNew
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
			this.tabControlAdv1 = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.Main = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdv1)).BeginInit();
			this.tabControlAdv1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlAdv1
			// 
			this.tabControlAdv1.ActiveTabFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabControlAdv1.BeforeTouchSize = new System.Drawing.Size(286, 319);
			this.tabControlAdv1.Controls.Add(this.Main);
			this.tabControlAdv1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabControlAdv1.Location = new System.Drawing.Point(218, 77);
			this.tabControlAdv1.Name = "tabControlAdv1";
			this.tabControlAdv1.Size = new System.Drawing.Size(286, 319);
			this.tabControlAdv1.TabIndex = 0;
			this.tabControlAdv1.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			// 
			// Main
			// 
			this.Main.Image = null;
			this.Main.ImageSize = new System.Drawing.Size(16, 16);
			this.Main.Location = new System.Drawing.Point(1, 24);
			this.Main.Name = "Main";
			this.Main.ShowCloseButton = true;
			this.Main.Size = new System.Drawing.Size(283, 293);
			this.Main.TabIndex = 1;
			this.Main.Text = "xxName";
			this.Main.ThemesEnabled = false;
			// 
			// PersonSelectorViewNew
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tabControlAdv1);
			this.Name = "PersonSelectorViewNew";
			this.Size = new System.Drawing.Size(613, 494);
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdv1)).EndInit();
			this.tabControlAdv1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdv1;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv Main;
	}
}
