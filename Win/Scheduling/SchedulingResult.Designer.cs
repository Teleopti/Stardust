namespace Teleopti.Ccc.Win.Scheduling
{
    partial class SchedulingResult
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchedulingResult));
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.masterGrid = new Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl();
			this.detailGrid = new Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl();
			this.splitContainerAdv1 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.gradientPanelSchedulingResult = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.buttonAdvClose = new Syncfusion.Windows.Forms.ButtonAdv();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.masterGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.detailGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).BeginInit();
			this.splitContainerAdv1.Panel1.SuspendLayout();
			this.splitContainerAdv1.Panel2.SuspendLayout();
			this.splitContainerAdv1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelSchedulingResult)).BeginInit();
			this.gradientPanelSchedulingResult.SuspendLayout();
			this.SuspendLayout();
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = true;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(667, 50);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "xxStart menu";
			this.ribbonControlAdv1.TabIndex = 3;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			this.ribbonControlAdv1.TitleColor = System.Drawing.Color.Black;
			// 
			// masterGrid
			// 
			this.masterGrid.BackColor = System.Drawing.SystemColors.Window;
			this.masterGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.masterGrid.FreezeCaption = false;
			this.masterGrid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.masterGrid.Location = new System.Drawing.Point(0, 0);
			this.masterGrid.Name = "masterGrid";
			this.masterGrid.Size = new System.Drawing.Size(657, 183);
			this.masterGrid.TabIndex = 4;
			this.masterGrid.TableOptions.AllowSelection = ((Syncfusion.Windows.Forms.Grid.GridSelectionFlags)((((Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Row | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Column) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Table) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Multiple)));
			this.masterGrid.TableOptions.ListBoxSelectionColorOptions = Syncfusion.Windows.Forms.Grid.Grouping.GridListBoxSelectionColorOptions.None;
			this.masterGrid.TableOptions.ListBoxSelectionMode = System.Windows.Forms.SelectionMode.None;
			this.masterGrid.Text = "gridGroupingControl2";
			this.masterGrid.TopLevelGroupOptions.ShowAddNewRecordBeforeDetails = false;
			this.masterGrid.TopLevelGroupOptions.ShowCaption = false;
			this.masterGrid.VersionInfo = "6.403.0.15";
			// 
			// detailGrid
			// 
			this.detailGrid.BackColor = System.Drawing.SystemColors.Window;
			this.detailGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailGrid.FreezeCaption = false;
			this.detailGrid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.detailGrid.Location = new System.Drawing.Point(0, 0);
			this.detailGrid.Name = "detailGrid";
			this.detailGrid.ShowRelationFields = Syncfusion.Grouping.ShowRelationFields.ShowAllRelatedFields;
			this.detailGrid.Size = new System.Drawing.Size(657, 115);
			this.detailGrid.SortMappingNames = true;
			this.detailGrid.TabIndex = 5;
			this.detailGrid.TableOptions.AllowSelection = ((Syncfusion.Windows.Forms.Grid.GridSelectionFlags)(((Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Row | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Column) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Table)));
			this.detailGrid.Text = "gridGroupingControl3";
			this.detailGrid.TopLevelGroupOptions.ShowAddNewRecordBeforeDetails = false;
			this.detailGrid.TopLevelGroupOptions.ShowCaption = false;
			this.detailGrid.VersionInfo = "6.403.0.15";
			// 
			// splitContainerAdv1
			// 
			this.splitContainerAdv1.BeforeTouchSize = 1;
			this.splitContainerAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdv1.Location = new System.Drawing.Point(6, 51);
			this.splitContainerAdv1.Name = "splitContainerAdv1";
			this.splitContainerAdv1.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// splitContainerAdv1.Panel1
			// 
			this.splitContainerAdv1.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv1.Panel1.Controls.Add(this.masterGrid);
			// 
			// splitContainerAdv1.Panel2
			// 
			this.splitContainerAdv1.Panel2.AutoSize = true;
			this.splitContainerAdv1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdv1.Panel2.Controls.Add(this.detailGrid);
			this.splitContainerAdv1.Panel2.MinimumSize = new System.Drawing.Size(0, 100);
			this.splitContainerAdv1.Size = new System.Drawing.Size(657, 299);
			this.splitContainerAdv1.SplitterDistance = 183;
			this.splitContainerAdv1.SplitterWidth = 1;
			this.splitContainerAdv1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdv1.TabIndex = 6;
			this.splitContainerAdv1.Text = "splitContainerAdv1";
			this.splitContainerAdv1.ThemesEnabled = true;
			// 
			// gradientPanelSchedulingResult
			// 
			this.gradientPanelSchedulingResult.BackColor = System.Drawing.Color.Transparent;
			this.gradientPanelSchedulingResult.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))), System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))));
			this.gradientPanelSchedulingResult.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelSchedulingResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelSchedulingResult.Controls.Add(this.buttonAdvClose);
			this.gradientPanelSchedulingResult.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.gradientPanelSchedulingResult.ForeColor = System.Drawing.SystemColors.ControlText;
			this.gradientPanelSchedulingResult.Location = new System.Drawing.Point(6, 350);
			this.gradientPanelSchedulingResult.Margin = new System.Windows.Forms.Padding(0);
			this.gradientPanelSchedulingResult.Name = "gradientPanelSchedulingResult";
			this.gradientPanelSchedulingResult.Size = new System.Drawing.Size(657, 36);
			this.gradientPanelSchedulingResult.TabIndex = 7;
			// 
			// buttonAdvClose
			// 
			this.buttonAdvClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvClose.BeforeTouchSize = new System.Drawing.Size(87, 23);
			this.buttonAdvClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvClose.ForeColor = System.Drawing.Color.White;
			this.buttonAdvClose.IsBackStageButton = false;
			this.buttonAdvClose.Location = new System.Drawing.Point(539, 7);
			this.buttonAdvClose.Name = "buttonAdvClose";
			this.buttonAdvClose.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvClose.Size = new System.Drawing.Size(87, 23);
			this.buttonAdvClose.TabIndex = 1;
			this.buttonAdvClose.Text = "xxClose";
			this.buttonAdvClose.UseVisualStyle = true;
			this.buttonAdvClose.Click += new System.EventHandler(this.buttonAdvCloseClick);
			// 
			// SchedulingResult
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(669, 392);
			this.Controls.Add(this.splitContainerAdv1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Controls.Add(this.gradientPanelSchedulingResult);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SchedulingResult";
			this.Text = "xxSchedulingResult";
			this.Load += new System.EventHandler(this.SchedulingResult_Load);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.masterGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.detailGrid)).EndInit();
			this.splitContainerAdv1.Panel1.ResumeLayout(false);
			this.splitContainerAdv1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).EndInit();
			this.splitContainerAdv1.ResumeLayout(false);
			this.splitContainerAdv1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelSchedulingResult)).EndInit();
			this.gradientPanelSchedulingResult.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl masterGrid;
        private Syncfusion.Windows.Forms.Grid.Grouping.GridGroupingControl detailGrid;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelSchedulingResult;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvClose;


    }
}