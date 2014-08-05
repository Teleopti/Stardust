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
			this.ribbonControlAdvFixed1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.gradientPanelMain = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.btnOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.pictureBoxLoading = new System.Windows.Forms.PictureBox();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvFixed1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).BeginInit();
			this.gradientPanelMain.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxLoading)).BeginInit();
			this.SuspendLayout();
			// 
			// ribbonControlAdvFixed1
			// 
			this.ribbonControlAdvFixed1.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvFixed1.HideMenuButtonToolTip = false;
			this.ribbonControlAdvFixed1.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			resources.ApplyResources(this.ribbonControlAdvFixed1, "ribbonControlAdvFixed1");
			this.ribbonControlAdvFixed1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdvFixed1.MenuButtonEnabled = true;
			this.ribbonControlAdvFixed1.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvFixed1.MenuButtonText = "";
			this.ribbonControlAdvFixed1.MenuButtonWidth = 56;
			this.ribbonControlAdvFixed1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdvFixed1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdvFixed1.Name = "ribbonControlAdvFixed1";
			this.ribbonControlAdvFixed1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			this.ribbonControlAdvFixed1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			// 
			// ribbonControlAdvFixed1.OfficeMenu
			// 
			this.ribbonControlAdvFixed1.OfficeMenu.Name = "OfficeMenu";
			resources.ApplyResources(this.ribbonControlAdvFixed1.OfficeMenu, "ribbonControlAdvFixed1.OfficeMenu");
			this.ribbonControlAdvFixed1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdvFixed1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdvFixed1.QuickPanelVisible = false;
			this.ribbonControlAdvFixed1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdvFixed1.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdvFixed1.SelectedTab = null;
			this.ribbonControlAdvFixed1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdvFixed1.ShowRibbonDisplayOptionButton = false;
			this.ribbonControlAdvFixed1.SystemText.QuickAccessDialogDropDownName = "xxStartMenu";
			this.ribbonControlAdvFixed1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdvFixed1.TitleColor = System.Drawing.Color.Black;
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
			this.btnOk.BeforeTouchSize = new System.Drawing.Size(85, 24);
			this.btnOk.ForeColor = System.Drawing.Color.White;
			this.btnOk.IsBackStageButton = false;
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyle = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnCancel.BeforeTouchSize = new System.Drawing.Size(85, 24);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.ForeColor = System.Drawing.Color.White;
			this.btnCancel.IsBackStageButton = false;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyle = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
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
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
			// 
			// FilterDataView
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Borders = new System.Windows.Forms.Padding(0);
			this.CancelButton = this.btnCancel;
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.ribbonControlAdvFixed1);
			this.Controls.Add(this.pictureBoxLoading);
			this.Controls.Add(this.gradientPanelMain);
			this.Name = "FilterDataView";
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvFixed1)).EndInit();
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
		private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdvFixed1;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private PictureBox pictureBoxLoading;
    }
}