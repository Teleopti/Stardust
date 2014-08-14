using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
	partial class FilterDataView
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
				if (components != null)
					components.Dispose();

			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterDataView));
			this.gradientPanelMain = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.btnOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.pictureBoxLoading = new System.Windows.Forms.PictureBox();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).BeginInit();
			this.gradientPanelMain.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxLoading)).BeginInit();
			this.SuspendLayout();
			// 
			// gradientPanelMain
			// 
			this.gradientPanelMain.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.None, System.Drawing.Color.White, System.Drawing.Color.White);
			this.gradientPanelMain.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.gradientPanelMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelMain.Controls.Add(this.tableLayoutPanel1);
			resources.ApplyResources(this.gradientPanelMain, "gradientPanelMain");
			this.gradientPanelMain.Name = "gradientPanelMain";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.Controls.Add(this.btnOk, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 1);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnOk.ForeColor = System.Drawing.Color.White;
			this.btnOk.IsBackStageButton = false;
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyle = true;
			this.btnOk.Click += new System.EventHandler(this.btnOkClick);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.ForeColor = System.Drawing.Color.White;
			this.btnCancel.IsBackStageButton = false;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyle = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancelClick);
			// 
			// pictureBoxLoading
			// 
			this.pictureBoxLoading.AccessibleRole = System.Windows.Forms.AccessibleRole.Graphic;
			resources.ApplyResources(this.pictureBoxLoading, "pictureBoxLoading");
			this.pictureBoxLoading.BackColor = System.Drawing.Color.Transparent;
			this.pictureBoxLoading.Image = global::Teleopti.Ccc.Win.Properties.Resources.hierarchipanel_loader;
			this.pictureBoxLoading.Name = "pictureBoxLoading";
			this.pictureBoxLoading.TabStop = false;
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1DoWork);
			this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1RunWorkerCompleted);
			// 
			// FilterDataView
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CancelButton = this.btnCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.Controls.Add(this.pictureBoxLoading);
			this.Controls.Add(this.gradientPanelMain);
			this.Name = "FilterDataView";
			this.ShowIcon = false;
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).EndInit();
			this.gradientPanelMain.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxLoading)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelMain;
		private TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.ButtonAdv btnOk;
		private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private PictureBox pictureBoxLoading;
	}
}