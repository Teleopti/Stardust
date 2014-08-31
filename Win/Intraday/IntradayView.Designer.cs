namespace Teleopti.Ccc.Win.Intraday
{
    partial class IntradayView
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
                {
                	components.Dispose();
                }
				if (gradientPanelContent != null)
				{
					gradientPanelContent.Dispose();
					gradientPanelContent = null;
				}
                if (_intradayViewContent != null)
                {
                    _intradayViewContent.Dispose();
                    _intradayViewContent = null;
                }
                if (Presenter!=null)
                {
                    Presenter.ExternalAgentStateReceived -= presenter_ExternalAgentStateReceived;
                    Presenter.Dispose();
                    Presenter = null;
                }
				if (_gridrowInChartSetting!=null)
				{
					_gridrowInChartSetting.LineInChartEnabledChanged -= gridrowInChartSetting_LineInChartEnabledChanged;
					_gridrowInChartSetting.LineInChartSettingsChanged -= gridlinesInChartSettings_LineInChartSettingsChanged;
				}
				if (teleoptiToolStripGalleryViews != null)
				{
					teleoptiToolStripGalleryViews.ItemClicked -= teleoptiToolStripGalleryViews_ItemClicked;
					teleoptiToolStripGalleryViews.GalleryItemClicked -= teleoptiToolStripGalleryViews_GalleryItemClicked;
				}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntradayView));
			this.statusStripExLastUpdate = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
			this.toolStripStatusLabelLastUpdate = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusStripButtonServerUnavailable = new Syncfusion.Windows.Forms.Tools.StatusStripButton();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.toolStripTabItemHome = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExDatePicker = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExChangeForecast = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonChangeForecast = new System.Windows.Forms.ToolStripButton();
			this.toolStripTabItemChart = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExGridRowInChartButtons = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripTabItem1 = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExLayouts = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.contextMenuStripExViews = new Syncfusion.Windows.Forms.Tools.ContextMenuStripEx();
			this.toolStripMenuItemRemove = new System.Windows.Forms.ToolStripMenuItem();
			this.teleoptiToolStripGalleryViews = new Teleopti.Ccc.Win.Common.Controls.ToolStripGallery.TeleoptiToolStripGallery();
			this.imageListLayouts = new System.Windows.Forms.ImageList(this.components);
			this.toolStripButtonNewView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonResetLayout = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonQuickAccessSave = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonExit = new System.Windows.Forms.ToolStripButton();
			this.toolStripExQuickAccess = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.gradientPanelContent = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.backgroundWorkerLoadControls = new System.ComponentModel.BackgroundWorker();
			this.statusStripExLastUpdate.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.ribbonControlAdv1.SuspendLayout();
			this.toolStripTabItemHome.Panel.SuspendLayout();
			this.toolStripExChangeForecast.SuspendLayout();
			this.toolStripTabItemChart.Panel.SuspendLayout();
			this.toolStripTabItem1.Panel.SuspendLayout();
			this.toolStripExLayouts.SuspendLayout();
			this.contextMenuStripExViews.SuspendLayout();
			this.toolStripExQuickAccess.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelContent)).BeginInit();
			this.gradientPanelContent.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStripExLastUpdate
			// 
			this.statusStripExLastUpdate.BeforeTouchSize = new System.Drawing.Size(895, 22);
			this.statusStripExLastUpdate.Dock = Syncfusion.Windows.Forms.Tools.DockStyleEx.Bottom;
			this.statusStripExLastUpdate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelLastUpdate,
            this.statusStripButtonServerUnavailable});
			this.statusStripExLastUpdate.Location = new System.Drawing.Point(6, 653);
			this.statusStripExLastUpdate.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(206)))), ((int)(((byte)(255)))));
			this.statusStripExLastUpdate.Name = "statusStripExLastUpdate";
			this.statusStripExLastUpdate.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Managed;
			this.statusStripExLastUpdate.Size = new System.Drawing.Size(895, 22);
			this.statusStripExLastUpdate.Stretch = false;
			this.statusStripExLastUpdate.TabIndex = 30;
			this.statusStripExLastUpdate.Text = "yystatusStripEx1";
			// 
			// toolStripStatusLabelLastUpdate
			// 
			this.toolStripStatusLabelLastUpdate.AutoSize = false;
			this.toolStripStatusLabelLastUpdate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripStatusLabelLastUpdate.Name = "toolStripStatusLabelLastUpdate";
			this.SetShortcut(this.toolStripStatusLabelLastUpdate, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelLastUpdate.Size = new System.Drawing.Size(180, 15);
			this.toolStripStatusLabelLastUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// statusStripButtonServerUnavailable
			// 
			this.statusStripButtonServerUnavailable.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Refresh_16x16;
			this.statusStripButtonServerUnavailable.Margin = new System.Windows.Forms.Padding(0);
			this.statusStripButtonServerUnavailable.Name = "statusStripButtonServerUnavailable";
			this.SetShortcut(this.statusStripButtonServerUnavailable, System.Windows.Forms.Keys.None);
			this.statusStripButtonServerUnavailable.Size = new System.Drawing.Size(130, 20);
			this.statusStripButtonServerUnavailable.Text = "xxServerUnavailable";
			this.statusStripButtonServerUnavailable.Visible = false;
			this.statusStripButtonServerUnavailable.Click += new System.EventHandler(this.statusStripButtonServerUnavailable_Click);
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemHome);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemChart);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItem1);
			this.ribbonControlAdv1.Header.AddQuickItem(new Syncfusion.Windows.Forms.Tools.QuickButtonReflectable(toolStripButtonQuickAccessSave));
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonHelp});
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(146, 88);
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonExit});
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.SelectedTab = this.toolStripTabItemHome;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = true;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(905, 150);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "StartMenu";
			this.ribbonControlAdv1.TabIndex = 0;
			this.ribbonControlAdv1.Text = "yyribbonControlAdv1";
			this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdv1.TitleColor = System.Drawing.Color.Black;
			// 
			// toolStripTabItemHome
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemHome, "");
			this.toolStripTabItemHome.Name = "toolStripTabItemHome";
			// 
			// ribbonControlAdv1.ribbonPanel1
			// 
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExDatePicker);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExChangeForecast);
			this.toolStripTabItemHome.Panel.Name = "ribbonPanel1";
			this.toolStripTabItemHome.Panel.ScrollPosition = 0;
			this.toolStripTabItemHome.Panel.TabIndex = 2;
			this.toolStripTabItemHome.Panel.Text = "xxHome";
			this.toolStripTabItemHome.Position = 0;
			this.SetShortcut(this.toolStripTabItemHome, System.Windows.Forms.Keys.None);
			this.toolStripTabItemHome.Size = new System.Drawing.Size(51, 19);
			this.toolStripTabItemHome.Text = "xxHome";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemHome, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemHome, false);
			// 
			// toolStripExDatePicker
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExDatePicker, "");
			this.toolStripExDatePicker.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExDatePicker.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExDatePicker.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExDatePicker.Image = null;
			this.toolStripExDatePicker.Location = new System.Drawing.Point(0, 1);
			this.toolStripExDatePicker.Name = "toolStripExDatePicker";
			this.toolStripExDatePicker.ShowLauncher = false;
			this.toolStripExDatePicker.Size = new System.Drawing.Size(106, 88);
			this.toolStripExDatePicker.TabIndex = 0;
			this.toolStripExDatePicker.Text = "xxDateNavigation";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExDatePicker, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExDatePicker, false);
			// 
			// toolStripExChangeForecast
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExChangeForecast, "");
			this.toolStripExChangeForecast.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExChangeForecast.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExChangeForecast.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExChangeForecast.Image = null;
			this.toolStripExChangeForecast.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonChangeForecast});
			this.toolStripExChangeForecast.Location = new System.Drawing.Point(108, 1);
			this.toolStripExChangeForecast.Name = "toolStripExChangeForecast";
			this.toolStripExChangeForecast.ShowCaption = false;
			this.toolStripExChangeForecast.ShowLauncher = false;
			this.toolStripExChangeForecast.Size = new System.Drawing.Size(114, 88);
			this.toolStripExChangeForecast.TabIndex = 1;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExChangeForecast, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExChangeForecast, false);
			this.toolStripExChangeForecast.Visible = false;
			// 
			// toolStripButtonChangeForecast
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChangeForecast, "");
			this.toolStripButtonChangeForecast.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Forecasts;
			this.toolStripButtonChangeForecast.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonChangeForecast.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChangeForecast.Name = "toolStripButtonChangeForecast";
			this.SetShortcut(this.toolStripButtonChangeForecast, System.Windows.Forms.Keys.None);
			this.toolStripButtonChangeForecast.Size = new System.Drawing.Size(76, 85);
			this.toolStripButtonChangeForecast.Text = "xxReforecast";
			this.toolStripButtonChangeForecast.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChangeForecast, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChangeForecast, false);
			this.toolStripButtonChangeForecast.Click += new System.EventHandler(this.toolStripButtonChangeForecastClick);
			// 
			// toolStripTabItemChart
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemChart, "");
			this.toolStripTabItemChart.Name = "toolStripTabItemChart";
			// 
			// ribbonControlAdv1.ribbonPanel2
			// 
			this.toolStripTabItemChart.Panel.Controls.Add(this.toolStripExGridRowInChartButtons);
			this.toolStripTabItemChart.Panel.Name = "ribbonPanel2";
			this.toolStripTabItemChart.Panel.ScrollPosition = 0;
			this.toolStripTabItemChart.Panel.TabIndex = 3;
			this.toolStripTabItemChart.Panel.Text = "xxChart";
			this.toolStripTabItemChart.Position = 1;
			this.SetShortcut(this.toolStripTabItemChart, System.Windows.Forms.Keys.None);
			this.toolStripTabItemChart.Size = new System.Drawing.Size(48, 19);
			this.toolStripTabItemChart.Text = "xxChart";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemChart, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemChart, false);
			// 
			// toolStripExGridRowInChartButtons
			// 
			this.toolStripExGridRowInChartButtons.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripExGridRowInChartButtons, "");
			this.toolStripExGridRowInChartButtons.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExGridRowInChartButtons.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExGridRowInChartButtons.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExGridRowInChartButtons.Image = null;
			this.toolStripExGridRowInChartButtons.Location = new System.Drawing.Point(0, 1);
			this.toolStripExGridRowInChartButtons.Name = "toolStripExGridRowInChartButtons";
			this.toolStripExGridRowInChartButtons.ShowLauncher = false;
			this.toolStripExGridRowInChartButtons.Size = new System.Drawing.Size(233, 0);
			this.toolStripExGridRowInChartButtons.TabIndex = 1;
			this.toolStripExGridRowInChartButtons.Text = "xxGridRowsInChart";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExGridRowInChartButtons, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExGridRowInChartButtons, false);
			// 
			// toolStripTabItem1
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItem1, "");
			this.toolStripTabItem1.Name = "toolStripTabItem1";
			// 
			// ribbonControlAdv1.ribbonPanel3
			// 
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripExLayouts);
			this.toolStripTabItem1.Panel.Name = "ribbonPanel3";
			this.toolStripTabItem1.Panel.ScrollPosition = 0;
			this.toolStripTabItem1.Panel.TabIndex = 4;
			this.toolStripTabItem1.Panel.Text = "xxLayoutViews";
			this.toolStripTabItem1.Position = 2;
			this.SetShortcut(this.toolStripTabItem1, System.Windows.Forms.Keys.None);
			this.toolStripTabItem1.Size = new System.Drawing.Size(83, 19);
			this.toolStripTabItem1.Text = "xxLayoutViews";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItem1, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItem1, false);
			// 
			// toolStripExLayouts
			// 
			this.toolStripExLayouts.AutoSize = false;
			this.toolStripExLayouts.ContextMenuStrip = this.contextMenuStripExViews;
			this.ribbonControlAdv1.SetDescription(this.toolStripExLayouts, "");
			this.toolStripExLayouts.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExLayouts.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExLayouts.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExLayouts.Image = null;
			this.toolStripExLayouts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.teleoptiToolStripGalleryViews,
            this.toolStripButtonNewView,
            this.toolStripButtonResetLayout});
			this.toolStripExLayouts.Location = new System.Drawing.Point(0, 1);
			this.toolStripExLayouts.Name = "toolStripExLayouts";
			this.toolStripExLayouts.ShowLauncher = false;
			this.toolStripExLayouts.Size = new System.Drawing.Size(631, 0);
			this.toolStripExLayouts.TabIndex = 0;
			this.toolStripExLayouts.Text = "Views";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExLayouts, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExLayouts, false);
			// 
			// contextMenuStripExViews
			// 
			this.contextMenuStripExViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRemove});
			this.contextMenuStripExViews.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(236)))), ((int)(((byte)(249)))));
			this.contextMenuStripExViews.Name = "contextMenuStripExViews";
			this.contextMenuStripExViews.Size = new System.Drawing.Size(118, 26);
			this.contextMenuStripExViews.Style = Syncfusion.Windows.Forms.Tools.ContextMenuStripEx.ContextMenuStyle.Default;
			// 
			// toolStripMenuItemRemove
			// 
			this.toolStripMenuItemRemove.Enabled = false;
			this.toolStripMenuItemRemove.Name = "toolStripMenuItemRemove";
			this.SetShortcut(this.toolStripMenuItemRemove, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRemove.Size = new System.Drawing.Size(117, 22);
			this.toolStripMenuItemRemove.Text = "xxDelete";
			// 
			// teleoptiToolStripGalleryViews
			// 
			this.teleoptiToolStripGalleryViews.AutoSize = false;
			this.teleoptiToolStripGalleryViews.BorderStyle = Syncfusion.Windows.Forms.Tools.ToolstripGalleryBorderStyle.None;
			this.teleoptiToolStripGalleryViews.CaptionText = "";
			this.teleoptiToolStripGalleryViews.CheckOnClick = true;
			this.teleoptiToolStripGalleryViews.Dimensions = new System.Drawing.Size(4, 1);
			this.teleoptiToolStripGalleryViews.DropDownDimensions = new System.Drawing.Size(7, 2);
			this.teleoptiToolStripGalleryViews.ImageList = this.imageListLayouts;
			this.teleoptiToolStripGalleryViews.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.teleoptiToolStripGalleryViews.ItemBackColor = System.Drawing.Color.Empty;
			this.teleoptiToolStripGalleryViews.ItemImageSize = new System.Drawing.Size(32, 32);
			this.teleoptiToolStripGalleryViews.ItemPadding = new System.Windows.Forms.Padding(2, 10, 1, 2);
			this.teleoptiToolStripGalleryViews.ItemSize = new System.Drawing.Size(100, 70);
			this.teleoptiToolStripGalleryViews.Margin = new System.Windows.Forms.Padding(5, 7, 0, 0);
			this.teleoptiToolStripGalleryViews.Name = "teleoptiToolStripGalleryViews";
			this.teleoptiToolStripGalleryViews.Padding = new System.Windows.Forms.Padding(0, 7, 0, 0);
			this.teleoptiToolStripGalleryViews.ParentRibbonTab = this.toolStripTabItem1;
			this.teleoptiToolStripGalleryViews.ScrollerType = Syncfusion.Windows.Forms.Tools.ToolStripGalleryScrollerType.Compact;
			this.SetShortcut(this.teleoptiToolStripGalleryViews, System.Windows.Forms.Keys.None);
			this.teleoptiToolStripGalleryViews.ShowToolTip = true;
			this.teleoptiToolStripGalleryViews.Size = new System.Drawing.Size(500, 72);
			this.teleoptiToolStripGalleryViews.Text = "yy";
			// 
			// imageListLayouts
			// 
			this.imageListLayouts.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListLayouts.ImageStream")));
			this.imageListLayouts.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListLayouts.Images.SetKeyName(0, "ccc_defaultview.png");
			this.imageListLayouts.Images.SetKeyName(1, "ccc_customview.png");
			// 
			// toolStripButtonNewView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonNewView, "");
			this.toolStripButtonNewView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripButtonNewView.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonNewView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonNewView.Margin = new System.Windows.Forms.Padding(7, 1, 0, 2);
			this.toolStripButtonNewView.Name = "toolStripButtonNewView";
			this.SetShortcut(this.toolStripButtonNewView, System.Windows.Forms.Keys.None);
			this.toolStripButtonNewView.Size = new System.Drawing.Size(45, 0);
			this.toolStripButtonNewView.Text = "xxNew";
			this.toolStripButtonNewView.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonNewView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonNewView, false);
			this.toolStripButtonNewView.Click += new System.EventHandler(this.toolStripButtonNewView_Click);
			// 
			// toolStripButtonResetLayout
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonResetLayout, "");
			this.toolStripButtonResetLayout.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Undo;
			this.toolStripButtonResetLayout.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonResetLayout.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonResetLayout.Name = "toolStripButtonResetLayout";
			this.SetShortcut(this.toolStripButtonResetLayout, System.Windows.Forms.Keys.None);
			this.toolStripButtonResetLayout.Size = new System.Drawing.Size(85, 51);
			this.toolStripButtonResetLayout.Text = "xxResetLayout";
			this.toolStripButtonResetLayout.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonResetLayout, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonResetLayout, false);
			this.toolStripButtonResetLayout.Click += new System.EventHandler(this.toolStripButtonResetLayout_Click);
			// 
			// toolStripButtonQuickAccessSave
			// 
			this.toolStripButtonQuickAccessSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonQuickAccessSave.Enabled = false;
			this.toolStripButtonQuickAccessSave.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_save_small;
			this.toolStripButtonQuickAccessSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonQuickAccessSave.Name = "toolStripButtonQuickAccessSave";
			this.SetShortcut(this.toolStripButtonQuickAccessSave, System.Windows.Forms.Keys.None);
			this.toolStripButtonQuickAccessSave.Size = new System.Drawing.Size(23, 36);
			this.toolStripButtonQuickAccessSave.Text = "Save";
			this.toolStripButtonQuickAccessSave.Click += new System.EventHandler(this.toolStripButtonQuickAccessSave_Click);
			// 
			// toolStripButtonHelp
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonHelp, "");
			this.toolStripButtonHelp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonHelp.Image")));
			this.toolStripButtonHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonHelp.Name = "toolStripButtonHelp";
			this.SetShortcut(this.toolStripButtonHelp, System.Windows.Forms.Keys.None);
			this.toolStripButtonHelp.Size = new System.Drawing.Size(78, 36);
			this.toolStripButtonHelp.Text = "xxHelp";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonHelp, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonHelp, false);
			this.toolStripButtonHelp.Click += new System.EventHandler(this.toolStripButtonHelp_Click);
			// 
			// toolStripButtonExit
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonExit, "");
			this.toolStripButtonExit.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Exit;
			this.toolStripButtonExit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonExit.Name = "toolStripButtonExit";
			this.SetShortcut(this.toolStripButtonExit, System.Windows.Forms.Keys.None);
			this.toolStripButtonExit.Size = new System.Drawing.Size(130, 20);
			this.toolStripButtonExit.Text = "xxExitTELEOPTICCC";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonExit, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonExit, false);
			this.toolStripButtonExit.Click += new System.EventHandler(this.toolStripButtonExit_Click);
			// 
			// toolStripExQuickAccess
			// 
			this.toolStripExQuickAccess.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExQuickAccess.Image = null;
			this.toolStripExQuickAccess.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonQuickAccessSave});
			this.toolStripExQuickAccess.Location = new System.Drawing.Point(6, 151);
			this.toolStripExQuickAccess.Name = "toolStripExQuickAccess";
			this.toolStripExQuickAccess.Size = new System.Drawing.Size(895, 39);
			this.toolStripExQuickAccess.TabIndex = 16;
			this.toolStripExQuickAccess.Text = "toolStripExQuickAccess";
			this.toolStripExQuickAccess.Visible = false;
			// 
			// gradientPanelContent
			// 
			this.gradientPanelContent.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(252)))), ((int)(((byte)(252))))), System.Drawing.Color.White);
			this.gradientPanelContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelContent.Controls.Add(this.label1);
			this.gradientPanelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanelContent.Location = new System.Drawing.Point(6, 151);
			this.gradientPanelContent.Name = "gradientPanelContent";
			this.gradientPanelContent.Size = new System.Drawing.Size(895, 502);
			this.gradientPanelContent.TabIndex = 31;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(384, 229);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(133, 19);
			this.label1.TabIndex = 0;
			this.label1.Text = "xxLoadingThreeDots";
			// 
			// backgroundWorkerLoadControls
			// 
			this.backgroundWorkerLoadControls.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoadControls_DoWork);
			this.backgroundWorkerLoadControls.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoadControls_RunWorkerCompleted);
			// 
			// IntradayView
			// 
			this.ClientSize = new System.Drawing.Size(907, 681);
			this.Controls.Add(this.gradientPanelContent);
			this.Controls.Add(this.statusStripExLastUpdate);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Controls.Add(this.toolStripExQuickAccess);
			this.HelpButtonImage = ((System.Drawing.Image)(resources.GetObject("$this.HelpButtonImage")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "IntradayView";
			this.Text = "xxTeleoptiRaptorColonIntraday";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IntradayView_FormClosing);
			this.statusStripExLastUpdate.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.ribbonControlAdv1.ResumeLayout(false);
			this.ribbonControlAdv1.PerformLayout();
			this.toolStripTabItemHome.Panel.ResumeLayout(false);
			this.toolStripTabItemHome.Panel.PerformLayout();
			this.toolStripExChangeForecast.ResumeLayout(false);
			this.toolStripExChangeForecast.PerformLayout();
			this.toolStripTabItemChart.Panel.ResumeLayout(false);
			this.toolStripTabItem1.Panel.ResumeLayout(false);
			this.toolStripExLayouts.ResumeLayout(false);
			this.toolStripExLayouts.PerformLayout();
			this.contextMenuStripExViews.ResumeLayout(false);
			this.toolStripExQuickAccess.ResumeLayout(false);
			this.toolStripExQuickAccess.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelContent)).EndInit();
			this.gradientPanelContent.ResumeLayout(false);
			this.gradientPanelContent.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemHome;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExDatePicker;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemChart;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExGridRowInChartButtons;
        private System.Windows.Forms.ToolStripButton toolStripButtonExit;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItem1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExLayouts;
        private Teleopti.Ccc.Win.Common.Controls.ToolStripGallery.TeleoptiToolStripGallery teleoptiToolStripGalleryViews;
        private Syncfusion.Windows.Forms.Tools.ContextMenuStripEx contextMenuStripExViews;
        private System.Windows.Forms.ToolStripButton toolStripButtonNewView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemove;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExQuickAccess;
        private System.Windows.Forms.ToolStripButton toolStripButtonQuickAccessSave;
        private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripExLastUpdate;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelContent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLastUpdate;
        private System.ComponentModel.BackgroundWorker backgroundWorkerLoadControls;
        private System.Windows.Forms.ImageList imageListLayouts;
        private System.Windows.Forms.ToolStripButton toolStripButtonHelp;
        private System.Windows.Forms.ToolStripButton toolStripButtonResetLayout;
        private Syncfusion.Windows.Forms.Tools.StatusStripButton statusStripButtonServerUnavailable;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExChangeForecast;
		private System.Windows.Forms.ToolStripButton toolStripButtonChangeForecast;
    }
}
