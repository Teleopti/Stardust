namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class FindAndReplaceForm
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
				if (_findAndReplaceDialog != null)
					_findAndReplaceDialog.Dispose();
                if (gridListControlSearchResults!=null)
                {
                    gridListControlSearchResults.Dispose();
                }
                _grid = null;
            }

            base.Dispose(disposing);
			//this.findAndReplaceDialog.Dispose();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.ribbonControlAdvSearch = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelTopMain = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonAdvReplaceAll = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvReplace = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvFindAll = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvFindNext = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvClose = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tabControlAdvMain = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageAdvFind = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelFindTabMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelFindTabTop = new System.Windows.Forms.TableLayoutPanel();
			this.comboDropDownFindSearchText = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel1 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanelFindTabBottom = new System.Windows.Forms.TableLayoutPanel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.buttonAdvFindOptions = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panelFindOptions = new System.Windows.Forms.Panel();
			this.checkBoxAdvFindSearchInPersistance = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvFindSearchUp = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvFindMatchWholeCell = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvFindMatchCase = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.comboBoxAdvFindSearchWithin = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel7 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tabPageAdvReplace = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelReplaceMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panel13 = new System.Windows.Forms.Panel();
			this.buttonAdvReplaceOptions = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panelReplaceOptions = new System.Windows.Forms.Panel();
			this.checkBoxAdvReplaceSearchInPersistance = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvReplaceSearchUp = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvReplaceMatchWholeCell = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvReplaceMatchCase = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.comboBoxAdvReplaceSearchWithin = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel19 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel15 = new System.Windows.Forms.TableLayoutPanel();
			this.comboDropDownReplaceReplaceText = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboDropDownReplaceSearchText = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel17 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel18 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.gridListControlSearchResults = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.statusStripExFindReplace = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
			this.tabPageAdvMain = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdv1 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxAdv2 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel2 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel4 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.buttonAdv2 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panel5 = new System.Windows.Forms.Panel();
			this.checkBoxAdv1 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdv2 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdv3 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.comboBoxAdv3 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel5 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.checkBoxAdv4 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdv5 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdv4 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxAdv5 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel6 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel8 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
			this.panel6 = new System.Windows.Forms.Panel();
			this.buttonAdv3 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panel7 = new System.Windows.Forms.Panel();
			this.checkBoxAdv6 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.comboBoxAdv6 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel9 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.checkBoxAdv7 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdv8 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdv7 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxAdv8 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel10 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel11 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
			this.panel8 = new System.Windows.Forms.Panel();
			this.buttonAdv4 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panel9 = new System.Windows.Forms.Panel();
			this.checkBoxAdv9 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.comboBoxAdv9 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel12 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel12 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdv10 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxAdv11 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel13 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel14 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel13 = new System.Windows.Forms.TableLayoutPanel();
			this.panel10 = new System.Windows.Forms.Panel();
			this.buttonAdv5 = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panel11 = new System.Windows.Forms.Panel();
			this.checkBoxAdv10 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdv11 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdv12 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.comboBoxAdv12 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel15 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel14 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdv13 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxAdv14 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabel16 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvSearch)).BeginInit();
			this.tableLayoutPanelMain.SuspendLayout();
			this.tableLayoutPanelTopMain.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdvMain)).BeginInit();
			this.tabControlAdvMain.SuspendLayout();
			this.tabPageAdvFind.SuspendLayout();
			this.tableLayoutPanelFindTabMain.SuspendLayout();
			this.tableLayoutPanelFindTabTop.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboDropDownFindSearchText)).BeginInit();
			this.tableLayoutPanelFindTabBottom.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panelFindOptions.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvFindSearchInPersistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvFindSearchUp)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvFindMatchWholeCell)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvFindMatchCase)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvFindSearchWithin)).BeginInit();
			this.tabPageAdvReplace.SuspendLayout();
			this.tableLayoutPanelReplaceMain.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel13.SuspendLayout();
			this.panelReplaceOptions.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvReplaceSearchInPersistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvReplaceSearchUp)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvReplaceMatchWholeCell)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvReplaceMatchCase)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvReplaceSearchWithin)).BeginInit();
			this.tableLayoutPanel15.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboDropDownReplaceReplaceText)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboDropDownReplaceSearchText)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridListControlSearchResults)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv2)).BeginInit();
			this.tableLayoutPanel4.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv4)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv5)).BeginInit();
			this.tableLayoutPanel5.SuspendLayout();
			this.tableLayoutPanel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv4)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv5)).BeginInit();
			this.tableLayoutPanel7.SuspendLayout();
			this.panel6.SuspendLayout();
			this.panel7.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv6)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv6)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv7)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv8)).BeginInit();
			this.tableLayoutPanel8.SuspendLayout();
			this.tableLayoutPanel9.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv7)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv8)).BeginInit();
			this.tableLayoutPanel10.SuspendLayout();
			this.panel8.SuspendLayout();
			this.panel9.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv9)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv9)).BeginInit();
			this.tableLayoutPanel11.SuspendLayout();
			this.tableLayoutPanel12.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv10)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv11)).BeginInit();
			this.tableLayoutPanel13.SuspendLayout();
			this.panel10.SuspendLayout();
			this.panel11.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv10)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv11)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv12)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv12)).BeginInit();
			this.tableLayoutPanel14.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv13)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv14)).BeginInit();
			this.SuspendLayout();
			// 
			// ribbonControlAdvSearch
			// 
			this.ribbonControlAdvSearch.HideMenuButtonToolTip = false;
			this.ribbonControlAdvSearch.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdvSearch.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdvSearch.MenuButtonEnabled = true;
			this.ribbonControlAdvSearch.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvSearch.MenuButtonText = "";
			this.ribbonControlAdvSearch.MenuButtonVisible = false;
			this.ribbonControlAdvSearch.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdvSearch.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdvSearch.MinimumSize = new System.Drawing.Size(0, 31);
			this.ribbonControlAdvSearch.Name = "ribbonControlAdvSearch";
			this.ribbonControlAdvSearch.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			// 
			// ribbonControlAdvSearch.OfficeMenu
			// 
			this.ribbonControlAdvSearch.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdvSearch.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdvSearch.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdvSearch.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdvSearch.QuickPanelVisible = false;
			this.ribbonControlAdvSearch.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdvSearch.SelectedTab = null;
			this.ribbonControlAdvSearch.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdvSearch.ShowRibbonDisplayOptionButton = true;
			this.ribbonControlAdvSearch.Size = new System.Drawing.Size(534, 38);
			this.ribbonControlAdvSearch.SystemText.QuickAccessDialogDropDownName = "StartMenu";
			this.ribbonControlAdvSearch.TabIndex = 18;
			this.ribbonControlAdvSearch.Text = "yyribbonControlAdv1";
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.ColumnCount = 1;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelTopMain, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this.gridListControlSearchResults, 0, 1);
			this.tableLayoutPanelMain.Controls.Add(this.statusStripExFindReplace, 0, 2);
			this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 3;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 324F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(674, 358);
			this.tableLayoutPanelMain.TabIndex = 21;
			// 
			// tableLayoutPanelTopMain
			// 
			this.tableLayoutPanelTopMain.ColumnCount = 1;
			this.tableLayoutPanelTopMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelTopMain.Controls.Add(this.panel1, 0, 1);
			this.tableLayoutPanelTopMain.Controls.Add(this.tabControlAdvMain, 0, 0);
			this.tableLayoutPanelTopMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelTopMain.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelTopMain.Name = "tableLayoutPanelTopMain";
			this.tableLayoutPanelTopMain.RowCount = 2;
			this.tableLayoutPanelTopMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 254F));
			this.tableLayoutPanelTopMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelTopMain.Size = new System.Drawing.Size(668, 318);
			this.tableLayoutPanelTopMain.TabIndex = 1;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonAdvReplaceAll);
			this.panel1.Controls.Add(this.buttonAdvReplace);
			this.panel1.Controls.Add(this.buttonAdvFindAll);
			this.panel1.Controls.Add(this.buttonAdvFindNext);
			this.panel1.Controls.Add(this.buttonAdvClose);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(35, 257);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(630, 60);
			this.panel1.TabIndex = 0;
			// 
			// buttonAdvReplaceAll
			// 
			this.buttonAdvReplaceAll.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvReplaceAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvReplaceAll.BeforeTouchSize = new System.Drawing.Size(111, 27);
			this.buttonAdvReplaceAll.ForeColor = System.Drawing.Color.White;
			this.buttonAdvReplaceAll.IsBackStageButton = false;
			this.buttonAdvReplaceAll.Location = new System.Drawing.Point(3, 17);
			this.buttonAdvReplaceAll.Name = "buttonAdvReplaceAll";
			this.buttonAdvReplaceAll.Size = new System.Drawing.Size(111, 27);
			this.buttonAdvReplaceAll.TabIndex = 11;
			this.buttonAdvReplaceAll.Text = "xxReplaceAmpersandAll";
			this.buttonAdvReplaceAll.UseVisualStyle = true;
			this.buttonAdvReplaceAll.Click += new System.EventHandler(this.buttonAdvReplaceAllClick);
			// 
			// buttonAdvReplace
			// 
			this.buttonAdvReplace.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvReplace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvReplace.BeforeTouchSize = new System.Drawing.Size(111, 27);
			this.buttonAdvReplace.ForeColor = System.Drawing.Color.White;
			this.buttonAdvReplace.IsBackStageButton = false;
			this.buttonAdvReplace.Location = new System.Drawing.Point(126, 17);
			this.buttonAdvReplace.Name = "buttonAdvReplace";
			this.buttonAdvReplace.Size = new System.Drawing.Size(111, 27);
			this.buttonAdvReplace.TabIndex = 10;
			this.buttonAdvReplace.Text = "xxAmpersandReplace";
			this.buttonAdvReplace.UseVisualStyle = true;
			this.buttonAdvReplace.Click += new System.EventHandler(this.buttonAdvReplaceClick);
			// 
			// buttonAdvFindAll
			// 
			this.buttonAdvFindAll.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvFindAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvFindAll.BeforeTouchSize = new System.Drawing.Size(111, 27);
			this.buttonAdvFindAll.ForeColor = System.Drawing.Color.White;
			this.buttonAdvFindAll.IsBackStageButton = false;
			this.buttonAdvFindAll.Location = new System.Drawing.Point(260, 17);
			this.buttonAdvFindAll.Name = "buttonAdvFindAll";
			this.buttonAdvFindAll.Size = new System.Drawing.Size(111, 27);
			this.buttonAdvFindAll.TabIndex = 9;
			this.buttonAdvFindAll.Text = "xxFAmpersandindAll";
			this.buttonAdvFindAll.UseVisualStyle = true;
			this.buttonAdvFindAll.Click += new System.EventHandler(this.buttonAdvFindAllClick);
			// 
			// buttonAdvFindNext
			// 
			this.buttonAdvFindNext.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvFindNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvFindNext.BeforeTouchSize = new System.Drawing.Size(111, 27);
			this.buttonAdvFindNext.ForeColor = System.Drawing.Color.White;
			this.buttonAdvFindNext.IsBackStageButton = false;
			this.buttonAdvFindNext.Location = new System.Drawing.Point(378, 17);
			this.buttonAdvFindNext.Name = "buttonAdvFindNext";
			this.buttonAdvFindNext.Size = new System.Drawing.Size(111, 27);
			this.buttonAdvFindNext.TabIndex = 8;
			this.buttonAdvFindNext.Text = "xxAmpersandFindNext";
			this.buttonAdvFindNext.UseVisualStyle = true;
			this.buttonAdvFindNext.Click += new System.EventHandler(this.buttonAdvFindNextClick);
			// 
			// buttonAdvClose
			// 
			this.buttonAdvClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvClose.BeforeTouchSize = new System.Drawing.Size(111, 27);
			this.buttonAdvClose.ForeColor = System.Drawing.Color.White;
			this.buttonAdvClose.IsBackStageButton = false;
			this.buttonAdvClose.Location = new System.Drawing.Point(511, 17);
			this.buttonAdvClose.Name = "buttonAdvClose";
			this.buttonAdvClose.Size = new System.Drawing.Size(111, 27);
			this.buttonAdvClose.TabIndex = 12;
			this.buttonAdvClose.Text = "xxClose";
			this.buttonAdvClose.UseVisualStyle = true;
			this.buttonAdvClose.Click += new System.EventHandler(this.buttonAdvCloseClick);
			// 
			// tabControlAdvMain
			// 
			this.tabControlAdvMain.ActiveTabColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.tabControlAdvMain.BeforeTouchSize = new System.Drawing.Size(650, 236);
			this.tabControlAdvMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlAdvMain.BorderWidth = 0;
			this.tabControlAdvMain.Controls.Add(this.tabPageAdvFind);
			this.tabControlAdvMain.Controls.Add(this.tabPageAdvReplace);
			this.tabControlAdvMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlAdvMain.FixedSingleBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.tabControlAdvMain.FocusOnTabClick = false;
			this.tabControlAdvMain.HotTrack = true;
			this.tabControlAdvMain.ImageOffset = 10;
			this.tabControlAdvMain.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlAdvMain.ItemSize = new System.Drawing.Size(79, 24);
			this.tabControlAdvMain.LevelTextAndImage = true;
			this.tabControlAdvMain.Location = new System.Drawing.Point(9, 9);
			this.tabControlAdvMain.Margin = new System.Windows.Forms.Padding(9, 9, 9, 9);
			this.tabControlAdvMain.Name = "tabControlAdvMain";
			this.tabControlAdvMain.PersistTabState = true;
			this.tabControlAdvMain.ShowToolTips = true;
			this.tabControlAdvMain.Size = new System.Drawing.Size(650, 236);
			this.tabControlAdvMain.TabIndex = 1;
			this.tabControlAdvMain.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlAdvMain.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			this.tabControlAdvMain.ThemesEnabled = true;
			this.tabControlAdvMain.UserMoveTabs = true;
			this.tabControlAdvMain.SelectedIndexChanging += new Syncfusion.Windows.Forms.Tools.SelectedIndexChangingEventHandler(this.tabControlAdvMainSelectedIndexChanging);
			// 
			// tabPageAdvFind
			// 
			this.tabPageAdvFind.Controls.Add(this.tableLayoutPanelFindTabMain);
			this.tabPageAdvFind.Image = null;
			this.tabPageAdvFind.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvFind.Location = new System.Drawing.Point(2, 25);
			this.tabPageAdvFind.Name = "tabPageAdvFind";
			this.tabPageAdvFind.ShowCloseButton = true;
			this.tabPageAdvFind.Size = new System.Drawing.Size(646, 209);
			this.tabPageAdvFind.TabIndex = 1;
			this.tabPageAdvFind.Text = "xxFinAmpersandd";
			this.tabPageAdvFind.ThemesEnabled = true;
			// 
			// tableLayoutPanelFindTabMain
			// 
			this.tableLayoutPanelFindTabMain.ColumnCount = 1;
			this.tableLayoutPanelFindTabMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelFindTabMain.Controls.Add(this.tableLayoutPanelFindTabTop, 0, 0);
			this.tableLayoutPanelFindTabMain.Controls.Add(this.tableLayoutPanelFindTabBottom, 0, 1);
			this.tableLayoutPanelFindTabMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelFindTabMain.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelFindTabMain.Name = "tableLayoutPanelFindTabMain";
			this.tableLayoutPanelFindTabMain.RowCount = 2;
			this.tableLayoutPanelFindTabMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 87F));
			this.tableLayoutPanelFindTabMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelFindTabMain.Size = new System.Drawing.Size(646, 209);
			this.tableLayoutPanelFindTabMain.TabIndex = 0;
			// 
			// tableLayoutPanelFindTabTop
			// 
			this.tableLayoutPanelFindTabTop.ColumnCount = 2;
			this.tableLayoutPanelFindTabTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
			this.tableLayoutPanelFindTabTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelFindTabTop.Controls.Add(this.comboDropDownFindSearchText, 1, 0);
			this.tableLayoutPanelFindTabTop.Controls.Add(this.autoLabel1, 0, 0);
			this.tableLayoutPanelFindTabTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelFindTabTop.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelFindTabTop.Name = "tableLayoutPanelFindTabTop";
			this.tableLayoutPanelFindTabTop.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.tableLayoutPanelFindTabTop.RowCount = 2;
			this.tableLayoutPanelFindTabTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelFindTabTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelFindTabTop.Size = new System.Drawing.Size(640, 81);
			this.tableLayoutPanelFindTabTop.TabIndex = 0;
			// 
			// comboDropDownFindSearchText
			// 
			this.comboDropDownFindSearchText.BackColor = System.Drawing.Color.White;
			this.comboDropDownFindSearchText.BeforeTouchSize = new System.Drawing.Size(505, 23);
			this.comboDropDownFindSearchText.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboDropDownFindSearchText.Location = new System.Drawing.Point(126, 9);
			this.comboDropDownFindSearchText.Name = "comboDropDownFindSearchText";
			this.comboDropDownFindSearchText.Size = new System.Drawing.Size(505, 23);
			this.comboDropDownFindSearchText.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboDropDownFindSearchText.TabIndex = 2;
			// 
			// autoLabel1
			// 
			this.autoLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel1.Location = new System.Drawing.Point(9, 8);
			this.autoLabel1.Name = "autoLabel1";
			this.autoLabel1.Size = new System.Drawing.Size(101, 30);
			this.autoLabel1.TabIndex = 1;
			this.autoLabel1.Text = "xxFiAmpersandndWhatColon";
			// 
			// tableLayoutPanelFindTabBottom
			// 
			this.tableLayoutPanelFindTabBottom.ColumnCount = 2;
			this.tableLayoutPanelFindTabBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.85612F));
			this.tableLayoutPanelFindTabBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.14388F));
			this.tableLayoutPanelFindTabBottom.Controls.Add(this.panel2, 1, 0);
			this.tableLayoutPanelFindTabBottom.Controls.Add(this.panelFindOptions, 0, 0);
			this.tableLayoutPanelFindTabBottom.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelFindTabBottom.Location = new System.Drawing.Point(3, 90);
			this.tableLayoutPanelFindTabBottom.Name = "tableLayoutPanelFindTabBottom";
			this.tableLayoutPanelFindTabBottom.RowCount = 1;
			this.tableLayoutPanelFindTabBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelFindTabBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelFindTabBottom.Size = new System.Drawing.Size(640, 122);
			this.tableLayoutPanelFindTabBottom.TabIndex = 1;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.buttonAdvFindOptions);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel2.Location = new System.Drawing.Point(547, 3);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(90, 116);
			this.panel2.TabIndex = 4;
			// 
			// buttonAdvFindOptions
			// 
			this.buttonAdvFindOptions.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvFindOptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvFindOptions.BeforeTouchSize = new System.Drawing.Size(90, 27);
			this.buttonAdvFindOptions.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonAdvFindOptions.ForeColor = System.Drawing.Color.White;
			this.buttonAdvFindOptions.IsBackStageButton = false;
			this.buttonAdvFindOptions.Location = new System.Drawing.Point(0, 0);
			this.buttonAdvFindOptions.Name = "buttonAdvFindOptions";
			this.buttonAdvFindOptions.Size = new System.Drawing.Size(90, 27);
			this.buttonAdvFindOptions.TabIndex = 4;
			this.buttonAdvFindOptions.Text = "xxOpAmpersandtionsGreaterThanGreaterThan";
			this.buttonAdvFindOptions.UseVisualStyle = true;
			this.buttonAdvFindOptions.Click += new System.EventHandler(this.buttonAdvFindOptionsClick);
			// 
			// panelFindOptions
			// 
			this.panelFindOptions.Controls.Add(this.checkBoxAdvFindSearchInPersistance);
			this.panelFindOptions.Controls.Add(this.checkBoxAdvFindSearchUp);
			this.panelFindOptions.Controls.Add(this.checkBoxAdvFindMatchWholeCell);
			this.panelFindOptions.Controls.Add(this.checkBoxAdvFindMatchCase);
			this.panelFindOptions.Controls.Add(this.comboBoxAdvFindSearchWithin);
			this.panelFindOptions.Controls.Add(this.autoLabel7);
			this.panelFindOptions.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelFindOptions.Location = new System.Drawing.Point(3, 3);
			this.panelFindOptions.Name = "panelFindOptions";
			this.panelFindOptions.Size = new System.Drawing.Size(380, 116);
			this.panelFindOptions.TabIndex = 5;
			// 
			// checkBoxAdvFindSearchInPersistance
			// 
			this.checkBoxAdvFindSearchInPersistance.BeforeTouchSize = new System.Drawing.Size(191, 24);
			this.checkBoxAdvFindSearchInPersistance.ForeColor = System.Drawing.SystemColors.ControlText;
			this.checkBoxAdvFindSearchInPersistance.Location = new System.Drawing.Point(9, 76);
			this.checkBoxAdvFindSearchInPersistance.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvFindSearchInPersistance.Name = "checkBoxAdvFindSearchInPersistance";
			this.checkBoxAdvFindSearchInPersistance.Size = new System.Drawing.Size(191, 24);
			this.checkBoxAdvFindSearchInPersistance.TabIndex = 24;
			this.checkBoxAdvFindSearchInPersistance.Text = "xxSearchAll";
			this.checkBoxAdvFindSearchInPersistance.ThemesEnabled = true;
			this.checkBoxAdvFindSearchInPersistance.CheckStateChanged += new System.EventHandler(this.checkBoxAdvFindSearchInPersistanceCheckStateChanged);
			// 
			// checkBoxAdvFindSearchUp
			// 
			this.checkBoxAdvFindSearchUp.BeforeTouchSize = new System.Drawing.Size(164, 24);
			this.checkBoxAdvFindSearchUp.ForeColor = System.Drawing.SystemColors.ControlText;
			this.checkBoxAdvFindSearchUp.Location = new System.Drawing.Point(208, 76);
			this.checkBoxAdvFindSearchUp.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvFindSearchUp.Name = "checkBoxAdvFindSearchUp";
			this.checkBoxAdvFindSearchUp.Size = new System.Drawing.Size(164, 24);
			this.checkBoxAdvFindSearchUp.TabIndex = 7;
			this.checkBoxAdvFindSearchUp.Text = "xxSearchUp";
			this.checkBoxAdvFindSearchUp.ThemesEnabled = true;
			// 
			// checkBoxAdvFindMatchWholeCell
			// 
			this.checkBoxAdvFindMatchWholeCell.BeforeTouchSize = new System.Drawing.Size(164, 24);
			this.checkBoxAdvFindMatchWholeCell.ForeColor = System.Drawing.SystemColors.ControlText;
			this.checkBoxAdvFindMatchWholeCell.Location = new System.Drawing.Point(208, 45);
			this.checkBoxAdvFindMatchWholeCell.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvFindMatchWholeCell.Name = "checkBoxAdvFindMatchWholeCell";
			this.checkBoxAdvFindMatchWholeCell.Size = new System.Drawing.Size(164, 24);
			this.checkBoxAdvFindMatchWholeCell.TabIndex = 6;
			this.checkBoxAdvFindMatchWholeCell.Text = "xxMatchWholeCell";
			this.checkBoxAdvFindMatchWholeCell.ThemesEnabled = true;
			// 
			// checkBoxAdvFindMatchCase
			// 
			this.checkBoxAdvFindMatchCase.BeforeTouchSize = new System.Drawing.Size(191, 24);
			this.checkBoxAdvFindMatchCase.ForeColor = System.Drawing.SystemColors.ControlText;
			this.checkBoxAdvFindMatchCase.Location = new System.Drawing.Point(9, 45);
			this.checkBoxAdvFindMatchCase.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvFindMatchCase.Name = "checkBoxAdvFindMatchCase";
			this.checkBoxAdvFindMatchCase.Size = new System.Drawing.Size(191, 24);
			this.checkBoxAdvFindMatchCase.TabIndex = 5;
			this.checkBoxAdvFindMatchCase.Text = "xxMatchCase";
			this.checkBoxAdvFindMatchCase.ThemesEnabled = true;
			// 
			// comboBoxAdvFindSearchWithin
			// 
			this.comboBoxAdvFindSearchWithin.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvFindSearchWithin.BeforeTouchSize = new System.Drawing.Size(140, 23);
			this.comboBoxAdvFindSearchWithin.Location = new System.Drawing.Point(86, 18);
			this.comboBoxAdvFindSearchWithin.Name = "comboBoxAdvFindSearchWithin";
			this.comboBoxAdvFindSearchWithin.Size = new System.Drawing.Size(140, 23);
			this.comboBoxAdvFindSearchWithin.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvFindSearchWithin.TabIndex = 3;
			// 
			// autoLabel7
			// 
			this.autoLabel7.Location = new System.Drawing.Point(6, 22);
			this.autoLabel7.Name = "autoLabel7";
			this.autoLabel7.Size = new System.Drawing.Size(145, 15);
			this.autoLabel7.TabIndex = 23;
			this.autoLabel7.Text = "xxWitAmpersandhinColon";
			// 
			// tabPageAdvReplace
			// 
			this.tabPageAdvReplace.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tabPageAdvReplace.Controls.Add(this.tableLayoutPanelReplaceMain);
			this.tabPageAdvReplace.Image = null;
			this.tabPageAdvReplace.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvReplace.Location = new System.Drawing.Point(2, 25);
			this.tabPageAdvReplace.Name = "tabPageAdvReplace";
			this.tabPageAdvReplace.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.tabPageAdvReplace.ShowCloseButton = true;
			this.tabPageAdvReplace.Size = new System.Drawing.Size(646, 209);
			this.tabPageAdvReplace.TabIndex = 1;
			this.tabPageAdvReplace.Text = "xxReAmpersandplace";
			this.tabPageAdvReplace.ThemesEnabled = true;
			// 
			// tableLayoutPanelReplaceMain
			// 
			this.tableLayoutPanelReplaceMain.ColumnCount = 1;
			this.tableLayoutPanelReplaceMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelReplaceMain.Controls.Add(this.tableLayoutPanel1, 0, 1);
			this.tableLayoutPanelReplaceMain.Controls.Add(this.tableLayoutPanel15, 0, 0);
			this.tableLayoutPanelReplaceMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelReplaceMain.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelReplaceMain.Name = "tableLayoutPanelReplaceMain";
			this.tableLayoutPanelReplaceMain.RowCount = 2;
			this.tableLayoutPanelReplaceMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 87F));
			this.tableLayoutPanelReplaceMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelReplaceMain.Size = new System.Drawing.Size(646, 209);
			this.tableLayoutPanelReplaceMain.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.85612F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.14388F));
			this.tableLayoutPanel1.Controls.Add(this.panel13, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.panelReplaceOptions, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 90);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(640, 122);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// panel13
			// 
			this.panel13.Controls.Add(this.buttonAdvReplaceOptions);
			this.panel13.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel13.Location = new System.Drawing.Point(547, 3);
			this.panel13.Name = "panel13";
			this.panel13.Size = new System.Drawing.Size(90, 116);
			this.panel13.TabIndex = 4;
			// 
			// buttonAdvReplaceOptions
			// 
			this.buttonAdvReplaceOptions.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvReplaceOptions.BeforeTouchSize = new System.Drawing.Size(90, 27);
			this.buttonAdvReplaceOptions.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonAdvReplaceOptions.IsBackStageButton = false;
			this.buttonAdvReplaceOptions.Location = new System.Drawing.Point(0, 89);
			this.buttonAdvReplaceOptions.Name = "buttonAdvReplaceOptions";
			this.buttonAdvReplaceOptions.Size = new System.Drawing.Size(90, 27);
			this.buttonAdvReplaceOptions.TabIndex = 15;
			this.buttonAdvReplaceOptions.Text = "xxOpAmpersandtionsGreaterThanGreaterThan";
			this.buttonAdvReplaceOptions.UseVisualStyle = true;
			this.buttonAdvReplaceOptions.Click += new System.EventHandler(this.buttonAdvReplaceOptionsClick);
			// 
			// panelReplaceOptions
			// 
			this.panelReplaceOptions.Controls.Add(this.checkBoxAdvReplaceSearchInPersistance);
			this.panelReplaceOptions.Controls.Add(this.checkBoxAdvReplaceSearchUp);
			this.panelReplaceOptions.Controls.Add(this.checkBoxAdvReplaceMatchWholeCell);
			this.panelReplaceOptions.Controls.Add(this.checkBoxAdvReplaceMatchCase);
			this.panelReplaceOptions.Controls.Add(this.comboBoxAdvReplaceSearchWithin);
			this.panelReplaceOptions.Controls.Add(this.autoLabel19);
			this.panelReplaceOptions.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelReplaceOptions.Location = new System.Drawing.Point(3, 3);
			this.panelReplaceOptions.Name = "panelReplaceOptions";
			this.panelReplaceOptions.Size = new System.Drawing.Size(377, 116);
			this.panelReplaceOptions.TabIndex = 5;
			// 
			// checkBoxAdvReplaceSearchInPersistance
			// 
			this.checkBoxAdvReplaceSearchInPersistance.BeforeTouchSize = new System.Drawing.Size(159, 24);
			this.checkBoxAdvReplaceSearchInPersistance.Location = new System.Drawing.Point(42, 76);
			this.checkBoxAdvReplaceSearchInPersistance.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvReplaceSearchInPersistance.Name = "checkBoxAdvReplaceSearchInPersistance";
			this.checkBoxAdvReplaceSearchInPersistance.Size = new System.Drawing.Size(159, 24);
			this.checkBoxAdvReplaceSearchInPersistance.TabIndex = 25;
			this.checkBoxAdvReplaceSearchInPersistance.Text = "xxSearchAll";
			this.checkBoxAdvReplaceSearchInPersistance.ThemesEnabled = true;
			this.checkBoxAdvReplaceSearchInPersistance.CheckStateChanged += new System.EventHandler(this.checkBoxAdvReplaceSearchInPersistanceCheckStateChanged);
			// 
			// checkBoxAdvReplaceSearchUp
			// 
			this.checkBoxAdvReplaceSearchUp.BeforeTouchSize = new System.Drawing.Size(91, 24);
			this.checkBoxAdvReplaceSearchUp.Location = new System.Drawing.Point(224, 76);
			this.checkBoxAdvReplaceSearchUp.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvReplaceSearchUp.Name = "checkBoxAdvReplaceSearchUp";
			this.checkBoxAdvReplaceSearchUp.Size = new System.Drawing.Size(91, 24);
			this.checkBoxAdvReplaceSearchUp.TabIndex = 19;
			this.checkBoxAdvReplaceSearchUp.Text = "xxSearchUp";
			this.checkBoxAdvReplaceSearchUp.ThemesEnabled = true;
			// 
			// checkBoxAdvReplaceMatchWholeCell
			// 
			this.checkBoxAdvReplaceMatchWholeCell.BeforeTouchSize = new System.Drawing.Size(135, 24);
			this.checkBoxAdvReplaceMatchWholeCell.Location = new System.Drawing.Point(224, 45);
			this.checkBoxAdvReplaceMatchWholeCell.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvReplaceMatchWholeCell.Name = "checkBoxAdvReplaceMatchWholeCell";
			this.checkBoxAdvReplaceMatchWholeCell.Size = new System.Drawing.Size(135, 24);
			this.checkBoxAdvReplaceMatchWholeCell.TabIndex = 18;
			this.checkBoxAdvReplaceMatchWholeCell.Text = "xxMatchWholeCell";
			this.checkBoxAdvReplaceMatchWholeCell.ThemesEnabled = true;
			// 
			// checkBoxAdvReplaceMatchCase
			// 
			this.checkBoxAdvReplaceMatchCase.BeforeTouchSize = new System.Drawing.Size(110, 24);
			this.checkBoxAdvReplaceMatchCase.Location = new System.Drawing.Point(224, 14);
			this.checkBoxAdvReplaceMatchCase.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvReplaceMatchCase.Name = "checkBoxAdvReplaceMatchCase";
			this.checkBoxAdvReplaceMatchCase.Size = new System.Drawing.Size(110, 24);
			this.checkBoxAdvReplaceMatchCase.TabIndex = 17;
			this.checkBoxAdvReplaceMatchCase.Text = "xxMatchCase";
			this.checkBoxAdvReplaceMatchCase.ThemesEnabled = true;
			// 
			// comboBoxAdvReplaceSearchWithin
			// 
			this.comboBoxAdvReplaceSearchWithin.BeforeTouchSize = new System.Drawing.Size(140, 21);
			this.comboBoxAdvReplaceSearchWithin.Location = new System.Drawing.Point(59, 14);
			this.comboBoxAdvReplaceSearchWithin.Name = "comboBoxAdvReplaceSearchWithin";
			this.comboBoxAdvReplaceSearchWithin.Size = new System.Drawing.Size(140, 21);
			this.comboBoxAdvReplaceSearchWithin.TabIndex = 16;
			// 
			// autoLabel19
			// 
			this.autoLabel19.Location = new System.Drawing.Point(6, 18);
			this.autoLabel19.Name = "autoLabel19";
			this.autoLabel19.Size = new System.Drawing.Size(145, 15);
			this.autoLabel19.TabIndex = 6;
			this.autoLabel19.Text = "xxWitAmpersandhinColon";
			this.autoLabel19.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanel15
			// 
			this.tableLayoutPanel15.ColumnCount = 2;
			this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
			this.tableLayoutPanel15.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel15.Controls.Add(this.comboDropDownReplaceReplaceText, 1, 1);
			this.tableLayoutPanel15.Controls.Add(this.comboDropDownReplaceSearchText, 1, 0);
			this.tableLayoutPanel15.Controls.Add(this.autoLabel17, 0, 0);
			this.tableLayoutPanel15.Controls.Add(this.autoLabel18, 0, 1);
			this.tableLayoutPanel15.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel15.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel15.Name = "tableLayoutPanel15";
			this.tableLayoutPanel15.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.tableLayoutPanel15.RowCount = 2;
			this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel15.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel15.Size = new System.Drawing.Size(640, 81);
			this.tableLayoutPanel15.TabIndex = 1;
			// 
			// comboDropDownReplaceReplaceText
			// 
			this.comboDropDownReplaceReplaceText.BeforeTouchSize = new System.Drawing.Size(505, 21);
			this.comboDropDownReplaceReplaceText.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboDropDownReplaceReplaceText.Location = new System.Drawing.Point(126, 43);
			this.comboDropDownReplaceReplaceText.Name = "comboDropDownReplaceReplaceText";
			this.comboDropDownReplaceReplaceText.Size = new System.Drawing.Size(505, 21);
			this.comboDropDownReplaceReplaceText.TabIndex = 14;
			// 
			// comboDropDownReplaceSearchText
			// 
			this.comboDropDownReplaceSearchText.BeforeTouchSize = new System.Drawing.Size(505, 21);
			this.comboDropDownReplaceSearchText.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboDropDownReplaceSearchText.Location = new System.Drawing.Point(126, 9);
			this.comboDropDownReplaceSearchText.Name = "comboDropDownReplaceSearchText";
			this.comboDropDownReplaceSearchText.Size = new System.Drawing.Size(505, 21);
			this.comboDropDownReplaceSearchText.TabIndex = 13;
			// 
			// autoLabel17
			// 
			this.autoLabel17.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel17.Location = new System.Drawing.Point(9, 8);
			this.autoLabel17.Name = "autoLabel17";
			this.autoLabel17.Size = new System.Drawing.Size(101, 30);
			this.autoLabel17.TabIndex = 1;
			this.autoLabel17.Text = "xxFiAmpersandndWhatColon";
			// 
			// autoLabel18
			// 
			this.autoLabel18.Location = new System.Drawing.Point(9, 40);
			this.autoLabel18.Name = "autoLabel18";
			this.autoLabel18.Size = new System.Drawing.Size(109, 30);
			this.autoLabel18.TabIndex = 3;
			this.autoLabel18.Text = "xxRAmpesandeplaceWithColon";
			// 
			// gridListControlSearchResults
			// 
			this.gridListControlSearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridListControlSearchResults.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridListControlSearchResults.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridListControlSearchResults.Location = new System.Drawing.Point(9, 333);
			this.gridListControlSearchResults.Margin = new System.Windows.Forms.Padding(9, 9, 9, 9);
			this.gridListControlSearchResults.Name = "gridListControlSearchResults";
			this.gridListControlSearchResults.Office2007ScrollBars = true;
			this.gridListControlSearchResults.RowCount = 0;
			this.gridListControlSearchResults.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridListControlSearchResults.Size = new System.Drawing.Size(656, 1);
			this.gridListControlSearchResults.SmartSizeBox = false;
			this.gridListControlSearchResults.TabIndex = 2;
			this.gridListControlSearchResults.Text = "gridControl1";
			this.gridListControlSearchResults.ThemesEnabled = true;
			this.gridListControlSearchResults.UseRightToLeftCompatibleTextBox = true;
			this.gridListControlSearchResults.Visible = false;
			this.gridListControlSearchResults.CellClick += new Syncfusion.Windows.Forms.Grid.GridCellClickEventHandler(this.gridListControlSearchResultsCellClick);
			// 
			// statusStripExFindReplace
			// 
			this.statusStripExFindReplace.BeforeTouchSize = new System.Drawing.Size(674, 23);
			this.statusStripExFindReplace.Dock = Syncfusion.Windows.Forms.Tools.DockStyleEx.Fill;
			this.statusStripExFindReplace.Location = new System.Drawing.Point(0, 335);
			this.statusStripExFindReplace.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(206)))), ((int)(((byte)(255)))));
			this.statusStripExFindReplace.Name = "statusStripExFindReplace";
			this.statusStripExFindReplace.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
			this.statusStripExFindReplace.Size = new System.Drawing.Size(674, 23);
			this.statusStripExFindReplace.TabIndex = 23;
			this.statusStripExFindReplace.VisualStyle = Syncfusion.Windows.Forms.Tools.StatusStripExStyle.Metro;
			// 
			// tabPageAdvMain
			// 
			this.tabPageAdvMain.Image = null;
			this.tabPageAdvMain.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvMain.Location = new System.Drawing.Point(0, 0);
			this.tabPageAdvMain.Name = "tabPageAdvMain";
			this.tabPageAdvMain.ShowCloseButton = true;
			this.tabPageAdvMain.Size = new System.Drawing.Size(200, 100);
			this.tabPageAdvMain.TabFont = null;
			this.tabPageAdvMain.TabIndex = 0;
			this.tabPageAdvMain.ThemesEnabled = false;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(200, 100);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.Controls.Add(this.comboBoxAdv1, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.comboBoxAdv2, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.autoLabel2, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.autoLabel4, 0, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.Padding = new System.Windows.Forms.Padding(5);
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(194, 14);
			this.tableLayoutPanel3.TabIndex = 0;
			// 
			// comboBoxAdv1
			// 
			this.comboBoxAdv1.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv1.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv1.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv1.Location = new System.Drawing.Point(108, 10);
			this.comboBoxAdv1.Name = "comboBoxAdv1";
			this.comboBoxAdv1.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv1.TabIndex = 27;
			// 
			// comboBoxAdv2
			// 
			this.comboBoxAdv2.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv2.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv2.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv2.Location = new System.Drawing.Point(108, 8);
			this.comboBoxAdv2.Name = "comboBoxAdv2";
			this.comboBoxAdv2.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv2.TabIndex = 26;
			// 
			// autoLabel2
			// 
			this.autoLabel2.Location = new System.Drawing.Point(8, 5);
			this.autoLabel2.Name = "autoLabel2";
			this.autoLabel2.Size = new System.Drawing.Size(56, 2);
			this.autoLabel2.TabIndex = 1;
			this.autoLabel2.Text = "Fi&nd what:";
			// 
			// autoLabel4
			// 
			this.autoLabel4.Location = new System.Drawing.Point(8, 7);
			this.autoLabel4.Name = "autoLabel4";
			this.autoLabel4.Size = new System.Drawing.Size(72, 2);
			this.autoLabel4.TabIndex = 10;
			this.autoLabel4.Text = "R&eplace with:";
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.ColumnCount = 2;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.85612F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.14388F));
			this.tableLayoutPanel4.Controls.Add(this.panel4, 1, 0);
			this.tableLayoutPanel4.Controls.Add(this.panel5, 0, 0);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 78);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 1;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(414, 106);
			this.tableLayoutPanel4.TabIndex = 1;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.buttonAdv2);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel4.Location = new System.Drawing.Point(334, 3);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(77, 100);
			this.panel4.TabIndex = 4;
			// 
			// buttonAdv2
			// 
			this.buttonAdv2.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdv2.BeforeTouchSize = new System.Drawing.Size(77, 23);
			this.buttonAdv2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonAdv2.IsBackStageButton = false;
			this.buttonAdv2.Location = new System.Drawing.Point(0, 77);
			this.buttonAdv2.Name = "buttonAdv2";
			this.buttonAdv2.Size = new System.Drawing.Size(77, 23);
			this.buttonAdv2.TabIndex = 0;
			this.buttonAdv2.Text = "Options >>";
			this.buttonAdv2.UseVisualStyle = true;
			// 
			// panel5
			// 
			this.panel5.Controls.Add(this.checkBoxAdv1);
			this.panel5.Controls.Add(this.checkBoxAdv2);
			this.panel5.Controls.Add(this.checkBoxAdv3);
			this.panel5.Controls.Add(this.comboBoxAdv3);
			this.panel5.Controls.Add(this.autoLabel5);
			this.panel5.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel5.Location = new System.Drawing.Point(3, 3);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(324, 100);
			this.panel5.TabIndex = 5;
			// 
			// checkBoxAdv1
			// 
			this.checkBoxAdv1.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv1.Location = new System.Drawing.Point(192, 66);
			this.checkBoxAdv1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv1.Name = "checkBoxAdv1";
			this.checkBoxAdv1.Size = new System.Drawing.Size(78, 21);
			this.checkBoxAdv1.TabIndex = 31;
			this.checkBoxAdv1.Text = "Search Up";
			this.checkBoxAdv1.ThemesEnabled = true;
			// 
			// checkBoxAdv2
			// 
			this.checkBoxAdv2.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv2.Location = new System.Drawing.Point(192, 39);
			this.checkBoxAdv2.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv2.Name = "checkBoxAdv2";
			this.checkBoxAdv2.Size = new System.Drawing.Size(116, 21);
			this.checkBoxAdv2.TabIndex = 30;
			this.checkBoxAdv2.Text = "Match Whole Cell";
			this.checkBoxAdv2.ThemesEnabled = true;
			// 
			// checkBoxAdv3
			// 
			this.checkBoxAdv3.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv3.Location = new System.Drawing.Point(192, 12);
			this.checkBoxAdv3.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv3.Name = "checkBoxAdv3";
			this.checkBoxAdv3.Size = new System.Drawing.Size(94, 21);
			this.checkBoxAdv3.TabIndex = 29;
			this.checkBoxAdv3.Text = "Match Case";
			this.checkBoxAdv3.ThemesEnabled = true;
			// 
			// comboBoxAdv3
			// 
			this.comboBoxAdv3.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv3.BeforeTouchSize = new System.Drawing.Size(121, 21);
			this.comboBoxAdv3.Location = new System.Drawing.Point(51, 12);
			this.comboBoxAdv3.Name = "comboBoxAdv3";
			this.comboBoxAdv3.Size = new System.Drawing.Size(121, 21);
			this.comboBoxAdv3.TabIndex = 28;
			// 
			// autoLabel5
			// 
			this.autoLabel5.Location = new System.Drawing.Point(5, 16);
			this.autoLabel5.Name = "autoLabel5";
			this.autoLabel5.Size = new System.Drawing.Size(40, 13);
			this.autoLabel5.TabIndex = 23;
			this.autoLabel5.Text = "Within:";
			// 
			// checkBoxAdv4
			// 
			this.checkBoxAdv4.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv4.Location = new System.Drawing.Point(192, 66);
			this.checkBoxAdv4.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv4.Name = "checkBoxAdv4";
			this.checkBoxAdv4.Size = new System.Drawing.Size(78, 21);
			this.checkBoxAdv4.TabIndex = 31;
			this.checkBoxAdv4.Text = "Search Up";
			this.checkBoxAdv4.ThemesEnabled = true;
			// 
			// checkBoxAdv5
			// 
			this.checkBoxAdv5.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv5.Location = new System.Drawing.Point(192, 39);
			this.checkBoxAdv5.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv5.Name = "checkBoxAdv5";
			this.checkBoxAdv5.Size = new System.Drawing.Size(116, 21);
			this.checkBoxAdv5.TabIndex = 30;
			this.checkBoxAdv5.Text = "Match Whole Cell";
			this.checkBoxAdv5.ThemesEnabled = true;
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 1;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel7, 0, 1);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 2;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel5.Size = new System.Drawing.Size(423, 187);
			this.tableLayoutPanel5.TabIndex = 0;
			// 
			// tableLayoutPanel6
			// 
			this.tableLayoutPanel6.ColumnCount = 2;
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel6.Controls.Add(this.comboBoxAdv4, 1, 1);
			this.tableLayoutPanel6.Controls.Add(this.comboBoxAdv5, 1, 0);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel6, 0, 0);
			this.tableLayoutPanel6.Controls.Add(this.autoLabel8, 0, 1);
			this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel6.Name = "tableLayoutPanel6";
			this.tableLayoutPanel6.Padding = new System.Windows.Forms.Padding(5);
			this.tableLayoutPanel6.RowCount = 2;
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel6.Size = new System.Drawing.Size(417, 69);
			this.tableLayoutPanel6.TabIndex = 0;
			// 
			// comboBoxAdv4
			// 
			this.comboBoxAdv4.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv4.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv4.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv4.Location = new System.Drawing.Point(108, 37);
			this.comboBoxAdv4.Name = "comboBoxAdv4";
			this.comboBoxAdv4.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv4.TabIndex = 27;
			// 
			// comboBoxAdv5
			// 
			this.comboBoxAdv5.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv5.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv5.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv5.Location = new System.Drawing.Point(108, 8);
			this.comboBoxAdv5.Name = "comboBoxAdv5";
			this.comboBoxAdv5.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv5.TabIndex = 26;
			// 
			// autoLabel6
			// 
			this.autoLabel6.Location = new System.Drawing.Point(8, 5);
			this.autoLabel6.Name = "autoLabel6";
			this.autoLabel6.Size = new System.Drawing.Size(56, 13);
			this.autoLabel6.TabIndex = 1;
			this.autoLabel6.Text = "Fi&nd what:";
			// 
			// autoLabel8
			// 
			this.autoLabel8.Location = new System.Drawing.Point(8, 34);
			this.autoLabel8.Name = "autoLabel8";
			this.autoLabel8.Size = new System.Drawing.Size(72, 13);
			this.autoLabel8.TabIndex = 10;
			this.autoLabel8.Text = "R&eplace with:";
			// 
			// tableLayoutPanel7
			// 
			this.tableLayoutPanel7.ColumnCount = 2;
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.85612F));
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.14388F));
			this.tableLayoutPanel7.Controls.Add(this.panel6, 1, 0);
			this.tableLayoutPanel7.Controls.Add(this.panel7, 0, 0);
			this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 78);
			this.tableLayoutPanel7.Name = "tableLayoutPanel7";
			this.tableLayoutPanel7.RowCount = 1;
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel7.Size = new System.Drawing.Size(417, 106);
			this.tableLayoutPanel7.TabIndex = 1;
			// 
			// panel6
			// 
			this.panel6.Controls.Add(this.buttonAdv3);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel6.Location = new System.Drawing.Point(337, 3);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(77, 100);
			this.panel6.TabIndex = 4;
			// 
			// buttonAdv3
			// 
			this.buttonAdv3.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdv3.BeforeTouchSize = new System.Drawing.Size(77, 23);
			this.buttonAdv3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonAdv3.IsBackStageButton = false;
			this.buttonAdv3.Location = new System.Drawing.Point(0, 77);
			this.buttonAdv3.Name = "buttonAdv3";
			this.buttonAdv3.Size = new System.Drawing.Size(77, 23);
			this.buttonAdv3.TabIndex = 0;
			this.buttonAdv3.Text = "Options >>";
			this.buttonAdv3.UseVisualStyle = true;
			// 
			// panel7
			// 
			this.panel7.Controls.Add(this.checkBoxAdv4);
			this.panel7.Controls.Add(this.checkBoxAdv5);
			this.panel7.Controls.Add(this.checkBoxAdv6);
			this.panel7.Controls.Add(this.comboBoxAdv6);
			this.panel7.Controls.Add(this.autoLabel9);
			this.panel7.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel7.Location = new System.Drawing.Point(3, 3);
			this.panel7.Name = "panel7";
			this.panel7.Size = new System.Drawing.Size(327, 100);
			this.panel7.TabIndex = 5;
			// 
			// checkBoxAdv6
			// 
			this.checkBoxAdv6.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv6.Location = new System.Drawing.Point(192, 12);
			this.checkBoxAdv6.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv6.Name = "checkBoxAdv6";
			this.checkBoxAdv6.Size = new System.Drawing.Size(94, 21);
			this.checkBoxAdv6.TabIndex = 29;
			this.checkBoxAdv6.Text = "Match Case";
			this.checkBoxAdv6.ThemesEnabled = true;
			// 
			// comboBoxAdv6
			// 
			this.comboBoxAdv6.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv6.BeforeTouchSize = new System.Drawing.Size(121, 21);
			this.comboBoxAdv6.Location = new System.Drawing.Point(51, 12);
			this.comboBoxAdv6.Name = "comboBoxAdv6";
			this.comboBoxAdv6.Size = new System.Drawing.Size(121, 21);
			this.comboBoxAdv6.TabIndex = 28;
			// 
			// autoLabel9
			// 
			this.autoLabel9.Location = new System.Drawing.Point(5, 16);
			this.autoLabel9.Name = "autoLabel9";
			this.autoLabel9.Size = new System.Drawing.Size(40, 13);
			this.autoLabel9.TabIndex = 23;
			this.autoLabel9.Text = "Within:";
			// 
			// checkBoxAdv7
			// 
			this.checkBoxAdv7.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv7.Location = new System.Drawing.Point(192, 66);
			this.checkBoxAdv7.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv7.Name = "checkBoxAdv7";
			this.checkBoxAdv7.Size = new System.Drawing.Size(78, 21);
			this.checkBoxAdv7.TabIndex = 31;
			this.checkBoxAdv7.Text = "Search Up";
			this.checkBoxAdv7.ThemesEnabled = true;
			// 
			// checkBoxAdv8
			// 
			this.checkBoxAdv8.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv8.Location = new System.Drawing.Point(192, 39);
			this.checkBoxAdv8.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv8.Name = "checkBoxAdv8";
			this.checkBoxAdv8.Size = new System.Drawing.Size(116, 21);
			this.checkBoxAdv8.TabIndex = 30;
			this.checkBoxAdv8.Text = "Match Whole Cell";
			this.checkBoxAdv8.ThemesEnabled = true;
			// 
			// tableLayoutPanel8
			// 
			this.tableLayoutPanel8.ColumnCount = 1;
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel8.Controls.Add(this.tableLayoutPanel9, 0, 0);
			this.tableLayoutPanel8.Controls.Add(this.tableLayoutPanel10, 0, 1);
			this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel8.Name = "tableLayoutPanel8";
			this.tableLayoutPanel8.RowCount = 2;
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel8.Size = new System.Drawing.Size(423, 187);
			this.tableLayoutPanel8.TabIndex = 0;
			// 
			// tableLayoutPanel9
			// 
			this.tableLayoutPanel9.ColumnCount = 2;
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel9.Controls.Add(this.comboBoxAdv7, 1, 1);
			this.tableLayoutPanel9.Controls.Add(this.comboBoxAdv8, 1, 0);
			this.tableLayoutPanel9.Controls.Add(this.autoLabel10, 0, 0);
			this.tableLayoutPanel9.Controls.Add(this.autoLabel11, 0, 1);
			this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel9.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel9.Name = "tableLayoutPanel9";
			this.tableLayoutPanel9.Padding = new System.Windows.Forms.Padding(5);
			this.tableLayoutPanel9.RowCount = 2;
			this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel9.Size = new System.Drawing.Size(417, 69);
			this.tableLayoutPanel9.TabIndex = 0;
			// 
			// comboBoxAdv7
			// 
			this.comboBoxAdv7.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv7.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv7.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv7.Location = new System.Drawing.Point(108, 37);
			this.comboBoxAdv7.Name = "comboBoxAdv7";
			this.comboBoxAdv7.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv7.TabIndex = 27;
			// 
			// comboBoxAdv8
			// 
			this.comboBoxAdv8.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv8.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv8.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv8.Location = new System.Drawing.Point(108, 8);
			this.comboBoxAdv8.Name = "comboBoxAdv8";
			this.comboBoxAdv8.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv8.TabIndex = 26;
			// 
			// autoLabel10
			// 
			this.autoLabel10.Location = new System.Drawing.Point(8, 5);
			this.autoLabel10.Name = "autoLabel10";
			this.autoLabel10.Size = new System.Drawing.Size(56, 13);
			this.autoLabel10.TabIndex = 1;
			this.autoLabel10.Text = "Fi&nd what:";
			// 
			// autoLabel11
			// 
			this.autoLabel11.Location = new System.Drawing.Point(8, 34);
			this.autoLabel11.Name = "autoLabel11";
			this.autoLabel11.Size = new System.Drawing.Size(72, 13);
			this.autoLabel11.TabIndex = 10;
			this.autoLabel11.Text = "R&eplace with:";
			// 
			// tableLayoutPanel10
			// 
			this.tableLayoutPanel10.ColumnCount = 2;
			this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.85612F));
			this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.14388F));
			this.tableLayoutPanel10.Controls.Add(this.panel8, 1, 0);
			this.tableLayoutPanel10.Controls.Add(this.panel9, 0, 0);
			this.tableLayoutPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel10.Location = new System.Drawing.Point(3, 78);
			this.tableLayoutPanel10.Name = "tableLayoutPanel10";
			this.tableLayoutPanel10.RowCount = 1;
			this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel10.Size = new System.Drawing.Size(417, 106);
			this.tableLayoutPanel10.TabIndex = 1;
			// 
			// panel8
			// 
			this.panel8.Controls.Add(this.buttonAdv4);
			this.panel8.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel8.Location = new System.Drawing.Point(337, 3);
			this.panel8.Name = "panel8";
			this.panel8.Size = new System.Drawing.Size(77, 100);
			this.panel8.TabIndex = 4;
			// 
			// buttonAdv4
			// 
			this.buttonAdv4.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdv4.BeforeTouchSize = new System.Drawing.Size(77, 23);
			this.buttonAdv4.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonAdv4.IsBackStageButton = false;
			this.buttonAdv4.Location = new System.Drawing.Point(0, 77);
			this.buttonAdv4.Name = "buttonAdv4";
			this.buttonAdv4.Size = new System.Drawing.Size(77, 23);
			this.buttonAdv4.TabIndex = 0;
			this.buttonAdv4.Text = "Options >>";
			this.buttonAdv4.UseVisualStyle = true;
			// 
			// panel9
			// 
			this.panel9.Controls.Add(this.checkBoxAdv7);
			this.panel9.Controls.Add(this.checkBoxAdv8);
			this.panel9.Controls.Add(this.checkBoxAdv9);
			this.panel9.Controls.Add(this.comboBoxAdv9);
			this.panel9.Controls.Add(this.autoLabel12);
			this.panel9.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel9.Location = new System.Drawing.Point(3, 3);
			this.panel9.Name = "panel9";
			this.panel9.Size = new System.Drawing.Size(327, 100);
			this.panel9.TabIndex = 5;
			// 
			// checkBoxAdv9
			// 
			this.checkBoxAdv9.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv9.Location = new System.Drawing.Point(192, 12);
			this.checkBoxAdv9.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv9.Name = "checkBoxAdv9";
			this.checkBoxAdv9.Size = new System.Drawing.Size(94, 21);
			this.checkBoxAdv9.TabIndex = 29;
			this.checkBoxAdv9.Text = "Match Case";
			this.checkBoxAdv9.ThemesEnabled = true;
			// 
			// comboBoxAdv9
			// 
			this.comboBoxAdv9.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv9.BeforeTouchSize = new System.Drawing.Size(121, 21);
			this.comboBoxAdv9.Location = new System.Drawing.Point(51, 12);
			this.comboBoxAdv9.Name = "comboBoxAdv9";
			this.comboBoxAdv9.Size = new System.Drawing.Size(121, 21);
			this.comboBoxAdv9.TabIndex = 28;
			// 
			// autoLabel12
			// 
			this.autoLabel12.Location = new System.Drawing.Point(5, 16);
			this.autoLabel12.Name = "autoLabel12";
			this.autoLabel12.Size = new System.Drawing.Size(40, 13);
			this.autoLabel12.TabIndex = 23;
			this.autoLabel12.Text = "Within:";
			// 
			// tableLayoutPanel11
			// 
			this.tableLayoutPanel11.ColumnCount = 1;
			this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel11.Controls.Add(this.tableLayoutPanel12, 0, 0);
			this.tableLayoutPanel11.Controls.Add(this.tableLayoutPanel13, 0, 1);
			this.tableLayoutPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel11.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel11.Name = "tableLayoutPanel11";
			this.tableLayoutPanel11.RowCount = 2;
			this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
			this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel11.Size = new System.Drawing.Size(423, 187);
			this.tableLayoutPanel11.TabIndex = 0;
			// 
			// tableLayoutPanel12
			// 
			this.tableLayoutPanel12.ColumnCount = 2;
			this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel12.Controls.Add(this.comboBoxAdv10, 1, 1);
			this.tableLayoutPanel12.Controls.Add(this.comboBoxAdv11, 1, 0);
			this.tableLayoutPanel12.Controls.Add(this.autoLabel13, 0, 0);
			this.tableLayoutPanel12.Controls.Add(this.autoLabel14, 0, 1);
			this.tableLayoutPanel12.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel12.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel12.Name = "tableLayoutPanel12";
			this.tableLayoutPanel12.Padding = new System.Windows.Forms.Padding(5);
			this.tableLayoutPanel12.RowCount = 2;
			this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel12.Size = new System.Drawing.Size(417, 69);
			this.tableLayoutPanel12.TabIndex = 0;
			// 
			// comboBoxAdv10
			// 
			this.comboBoxAdv10.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv10.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv10.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv10.Location = new System.Drawing.Point(108, 37);
			this.comboBoxAdv10.Name = "comboBoxAdv10";
			this.comboBoxAdv10.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv10.TabIndex = 27;
			// 
			// comboBoxAdv11
			// 
			this.comboBoxAdv11.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv11.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv11.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv11.Location = new System.Drawing.Point(108, 8);
			this.comboBoxAdv11.Name = "comboBoxAdv11";
			this.comboBoxAdv11.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv11.TabIndex = 26;
			// 
			// autoLabel13
			// 
			this.autoLabel13.Location = new System.Drawing.Point(8, 5);
			this.autoLabel13.Name = "autoLabel13";
			this.autoLabel13.Size = new System.Drawing.Size(56, 13);
			this.autoLabel13.TabIndex = 1;
			this.autoLabel13.Text = "Fi&nd what:";
			// 
			// autoLabel14
			// 
			this.autoLabel14.Location = new System.Drawing.Point(8, 34);
			this.autoLabel14.Name = "autoLabel14";
			this.autoLabel14.Size = new System.Drawing.Size(72, 13);
			this.autoLabel14.TabIndex = 10;
			this.autoLabel14.Text = "R&eplace with:";
			// 
			// tableLayoutPanel13
			// 
			this.tableLayoutPanel13.ColumnCount = 2;
			this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.85612F));
			this.tableLayoutPanel13.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.14388F));
			this.tableLayoutPanel13.Controls.Add(this.panel10, 1, 0);
			this.tableLayoutPanel13.Controls.Add(this.panel11, 0, 0);
			this.tableLayoutPanel13.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel13.Location = new System.Drawing.Point(3, 78);
			this.tableLayoutPanel13.Name = "tableLayoutPanel13";
			this.tableLayoutPanel13.RowCount = 1;
			this.tableLayoutPanel13.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel13.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel13.Size = new System.Drawing.Size(417, 106);
			this.tableLayoutPanel13.TabIndex = 1;
			// 
			// panel10
			// 
			this.panel10.Controls.Add(this.buttonAdv5);
			this.panel10.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel10.Location = new System.Drawing.Point(337, 3);
			this.panel10.Name = "panel10";
			this.panel10.Size = new System.Drawing.Size(77, 100);
			this.panel10.TabIndex = 4;
			// 
			// buttonAdv5
			// 
			this.buttonAdv5.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdv5.BeforeTouchSize = new System.Drawing.Size(77, 23);
			this.buttonAdv5.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonAdv5.IsBackStageButton = false;
			this.buttonAdv5.Location = new System.Drawing.Point(0, 77);
			this.buttonAdv5.Name = "buttonAdv5";
			this.buttonAdv5.Size = new System.Drawing.Size(77, 23);
			this.buttonAdv5.TabIndex = 0;
			this.buttonAdv5.Text = "Options >>";
			this.buttonAdv5.UseVisualStyle = true;
			// 
			// panel11
			// 
			this.panel11.Controls.Add(this.checkBoxAdv10);
			this.panel11.Controls.Add(this.checkBoxAdv11);
			this.panel11.Controls.Add(this.checkBoxAdv12);
			this.panel11.Controls.Add(this.comboBoxAdv12);
			this.panel11.Controls.Add(this.autoLabel15);
			this.panel11.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel11.Location = new System.Drawing.Point(3, 3);
			this.panel11.Name = "panel11";
			this.panel11.Size = new System.Drawing.Size(327, 100);
			this.panel11.TabIndex = 5;
			// 
			// checkBoxAdv10
			// 
			this.checkBoxAdv10.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv10.Location = new System.Drawing.Point(192, 66);
			this.checkBoxAdv10.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv10.Name = "checkBoxAdv10";
			this.checkBoxAdv10.Size = new System.Drawing.Size(78, 21);
			this.checkBoxAdv10.TabIndex = 31;
			this.checkBoxAdv10.Text = "Search Up";
			this.checkBoxAdv10.ThemesEnabled = true;
			// 
			// checkBoxAdv11
			// 
			this.checkBoxAdv11.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv11.Location = new System.Drawing.Point(192, 39);
			this.checkBoxAdv11.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv11.Name = "checkBoxAdv11";
			this.checkBoxAdv11.Size = new System.Drawing.Size(116, 21);
			this.checkBoxAdv11.TabIndex = 30;
			this.checkBoxAdv11.Text = "Match Whole Cell";
			this.checkBoxAdv11.ThemesEnabled = true;
			// 
			// checkBoxAdv12
			// 
			this.checkBoxAdv12.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdv12.Location = new System.Drawing.Point(192, 12);
			this.checkBoxAdv12.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdv12.Name = "checkBoxAdv12";
			this.checkBoxAdv12.Size = new System.Drawing.Size(94, 21);
			this.checkBoxAdv12.TabIndex = 29;
			this.checkBoxAdv12.Text = "Match Case";
			this.checkBoxAdv12.ThemesEnabled = true;
			// 
			// comboBoxAdv12
			// 
			this.comboBoxAdv12.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv12.BeforeTouchSize = new System.Drawing.Size(121, 21);
			this.comboBoxAdv12.Location = new System.Drawing.Point(51, 12);
			this.comboBoxAdv12.Name = "comboBoxAdv12";
			this.comboBoxAdv12.Size = new System.Drawing.Size(121, 21);
			this.comboBoxAdv12.TabIndex = 28;
			// 
			// autoLabel15
			// 
			this.autoLabel15.Location = new System.Drawing.Point(5, 16);
			this.autoLabel15.Name = "autoLabel15";
			this.autoLabel15.Size = new System.Drawing.Size(40, 13);
			this.autoLabel15.TabIndex = 23;
			this.autoLabel15.Text = "Within:";
			// 
			// tableLayoutPanel14
			// 
			this.tableLayoutPanel14.ColumnCount = 2;
			this.tableLayoutPanel14.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel14.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel14.Controls.Add(this.comboBoxAdv13, 1, 1);
			this.tableLayoutPanel14.Controls.Add(this.comboBoxAdv14, 1, 0);
			this.tableLayoutPanel14.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel14.Name = "tableLayoutPanel14";
			this.tableLayoutPanel14.RowCount = 2;
			this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel14.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel14.Size = new System.Drawing.Size(200, 100);
			this.tableLayoutPanel14.TabIndex = 0;
			// 
			// comboBoxAdv13
			// 
			this.comboBoxAdv13.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv13.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv13.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv13.Location = new System.Drawing.Point(103, 23);
			this.comboBoxAdv13.Name = "comboBoxAdv13";
			this.comboBoxAdv13.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv13.TabIndex = 27;
			// 
			// comboBoxAdv14
			// 
			this.comboBoxAdv14.BackColor = System.Drawing.SystemColors.Control;
			this.comboBoxAdv14.BeforeTouchSize = new System.Drawing.Size(301, 21);
			this.comboBoxAdv14.Dock = System.Windows.Forms.DockStyle.Top;
			this.comboBoxAdv14.Location = new System.Drawing.Point(103, 3);
			this.comboBoxAdv14.Name = "comboBoxAdv14";
			this.comboBoxAdv14.Size = new System.Drawing.Size(301, 21);
			this.comboBoxAdv14.TabIndex = 26;
			// 
			// autoLabel16
			// 
			this.autoLabel16.Location = new System.Drawing.Point(8, 5);
			this.autoLabel16.Name = "autoLabel16";
			this.autoLabel16.Size = new System.Drawing.Size(56, 13);
			this.autoLabel16.TabIndex = 1;
			this.autoLabel16.Text = "Fi&nd what:";
			// 
			// FindAndReplaceForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(674, 358);
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Controls.Add(this.ribbonControlAdvSearch);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(686, 392);
			this.Name = "FindAndReplaceForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxFindAndReplace";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.findAndReplaceFormFormClosing);
			this.Load += new System.EventHandler(this.searchFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvSearch)).EndInit();
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelMain.PerformLayout();
			this.tableLayoutPanelTopMain.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdvMain)).EndInit();
			this.tabControlAdvMain.ResumeLayout(false);
			this.tabPageAdvFind.ResumeLayout(false);
			this.tableLayoutPanelFindTabMain.ResumeLayout(false);
			this.tableLayoutPanelFindTabTop.ResumeLayout(false);
			this.tableLayoutPanelFindTabTop.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboDropDownFindSearchText)).EndInit();
			this.tableLayoutPanelFindTabBottom.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panelFindOptions.ResumeLayout(false);
			this.panelFindOptions.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvFindSearchInPersistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvFindSearchUp)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvFindMatchWholeCell)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvFindMatchCase)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvFindSearchWithin)).EndInit();
			this.tabPageAdvReplace.ResumeLayout(false);
			this.tableLayoutPanelReplaceMain.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel13.ResumeLayout(false);
			this.panelReplaceOptions.ResumeLayout(false);
			this.panelReplaceOptions.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvReplaceSearchInPersistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvReplaceSearchUp)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvReplaceMatchWholeCell)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvReplaceMatchCase)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvReplaceSearchWithin)).EndInit();
			this.tableLayoutPanel15.ResumeLayout(false);
			this.tableLayoutPanel15.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboDropDownReplaceReplaceText)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboDropDownReplaceSearchText)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridListControlSearchResults)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv2)).EndInit();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel5.ResumeLayout(false);
			this.panel5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv5)).EndInit();
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanel6.ResumeLayout(false);
			this.tableLayoutPanel6.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv5)).EndInit();
			this.tableLayoutPanel7.ResumeLayout(false);
			this.panel6.ResumeLayout(false);
			this.panel7.ResumeLayout(false);
			this.panel7.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv6)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv6)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv7)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv8)).EndInit();
			this.tableLayoutPanel8.ResumeLayout(false);
			this.tableLayoutPanel9.ResumeLayout(false);
			this.tableLayoutPanel9.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv7)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv8)).EndInit();
			this.tableLayoutPanel10.ResumeLayout(false);
			this.panel8.ResumeLayout(false);
			this.panel9.ResumeLayout(false);
			this.panel9.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv9)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv9)).EndInit();
			this.tableLayoutPanel11.ResumeLayout(false);
			this.tableLayoutPanel12.ResumeLayout(false);
			this.tableLayoutPanel12.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv10)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv11)).EndInit();
			this.tableLayoutPanel13.ResumeLayout(false);
			this.panel10.ResumeLayout(false);
			this.panel11.ResumeLayout(false);
			this.panel11.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv10)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv11)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdv12)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv12)).EndInit();
			this.tableLayoutPanel14.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv13)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv14)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdvSearch;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTopMain;
        private System.Windows.Forms.Panel panel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvClose;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvFindNext;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvFindAll;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvReplaceAll;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvReplace;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdvMain;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvReplace;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvFind;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFindTabMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFindTabTop;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel1;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboDropDownFindSearchText;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFindTabBottom;
        private System.Windows.Forms.Panel panel2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvFindOptions;
        private System.Windows.Forms.Panel panelFindOptions;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel7;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvFindSearchWithin;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvFindMatchCase;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvFindMatchWholeCell;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvFindSearchUp;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelReplaceMain;
        //private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdv2;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv1;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv2;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel2;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Panel panel4;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv2;
        private System.Windows.Forms.Panel panel5;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv1;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv2;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv3;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv3;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel5;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv4;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv4;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv5;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel6;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.Panel panel6;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv3;
        private System.Windows.Forms.Panel panel7;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv6;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv6;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel9;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv7;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv7;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv8;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel10;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel11;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel10;
        private System.Windows.Forms.Panel panel8;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv4;
        private System.Windows.Forms.Panel panel9;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv9;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv9;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel12;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel11;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel12;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv10;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv11;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel13;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel14;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel13;
        private System.Windows.Forms.Panel panel10;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv5;
        private System.Windows.Forms.Panel panel11;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv10;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv11;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdv12;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv12;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel15;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel14;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv13;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv14;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel16;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel15;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboDropDownReplaceReplaceText;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboDropDownReplaceSearchText;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel17;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel18;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel13;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvReplaceOptions;
        private System.Windows.Forms.Panel panelReplaceOptions;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvReplaceSearchUp;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvReplaceMatchWholeCell;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvReplaceMatchCase;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvReplaceSearchWithin;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel19;
        private Syncfusion.Windows.Forms.Grid.GridControl gridListControlSearchResults;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvFindSearchInPersistance;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvReplaceSearchInPersistance;
		private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripExFindReplace;

    }
}