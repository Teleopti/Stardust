namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class SiteControl
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
			if (_columnGridHelper != null)
				_columnGridHelper.Dispose();
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxManageSites"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxEnterPropertiesOnSite"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void InitializeComponent()
        {
			this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
			this.gridControlSites = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanelSubHeader2 = new System.Windows.Forms.TableLayoutPanel();
			this.labelSubHeader2 = new System.Windows.Forms.Label();
			this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
			this.labelHeader = new System.Windows.Forms.Label();
			this.tableLayoutPanelBody.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlSites)).BeginInit();
			this.tableLayoutPanelSubHeader2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
			this.gradientPanelHeader.SuspendLayout();
			this.tableLayoutPanelHeader.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanelBody
			// 
			this.tableLayoutPanelBody.ColumnCount = 1;
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Controls.Add(this.gridControlSites, 0, 1);
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader2, 0, 0);
			this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 55);
			this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
			this.tableLayoutPanelBody.RowCount = 2;
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Size = new System.Drawing.Size(600, 435);
			this.tableLayoutPanelBody.TabIndex = 56;
			// 
			// gridControlSites
			// 
			this.gridControlSites.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlSites.ForeColor = System.Drawing.SystemColors.ControlText;
			this.gridControlSites.Location = new System.Drawing.Point(3, 36);
			this.gridControlSites.Name = "gridControlSites";
			this.gridControlSites.NumberedRowHeaders = false;
			this.gridControlSites.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridControlSites.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.gridControlSites.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlSites.Size = new System.Drawing.Size(594, 396);
			this.gridControlSites.SmartSizeBox = false;
			this.gridControlSites.TabIndex = 4;
			this.gridControlSites.UseRightToLeftCompatibleTextBox = true;
			// 
			// tableLayoutPanelSubHeader2
			// 
			this.tableLayoutPanelSubHeader2.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader2.ColumnCount = 1;
			this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader2.Controls.Add(this.labelSubHeader2, 0, 0);
			this.tableLayoutPanelSubHeader2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSubHeader2.Name = "tableLayoutPanelSubHeader2";
			this.tableLayoutPanelSubHeader2.Padding = new System.Windows.Forms.Padding(0, 2, 0, 3);
			this.tableLayoutPanelSubHeader2.RowCount = 1;
			this.tableLayoutPanelSubHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelSubHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.tableLayoutPanelSubHeader2.Size = new System.Drawing.Size(594, 27);
			this.tableLayoutPanelSubHeader2.TabIndex = 3;
			// 
			// labelSubHeader2
			// 
			this.labelSubHeader2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSubHeader2.AutoSize = true;
			this.labelSubHeader2.BackColor = System.Drawing.Color.Transparent;
			this.labelSubHeader2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelSubHeader2.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelSubHeader2.Location = new System.Drawing.Point(3, 6);
			this.labelSubHeader2.Name = "labelSubHeader2";
			this.labelSubHeader2.Size = new System.Drawing.Size(147, 13);
			this.labelSubHeader2.TabIndex = 0;
			this.labelSubHeader2.Text = "xxEnterPropertiesOnSite";
			this.labelSubHeader2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// gradientPanelHeader
			// 
			this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.gradientPanelHeader.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.gradientPanelHeader.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.White);
			this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
			this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
			this.gradientPanelHeader.Name = "gradientPanelHeader";
			this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(10);
			this.gradientPanelHeader.Size = new System.Drawing.Size(600, 55);
			this.gradientPanelHeader.TabIndex = 55;
			// 
			// tableLayoutPanelHeader
			// 
			this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelHeader.ColumnCount = 1;
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 580F));
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
			this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelHeader.Location = new System.Drawing.Point(10, 10);
			this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
			this.tableLayoutPanelHeader.RowCount = 1;
			this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Size = new System.Drawing.Size(580, 35);
			this.tableLayoutPanelHeader.TabIndex = 0;
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHeader.AutoSize = true;
			this.labelHeader.Font = new System.Drawing.Font("Tahoma", 11.25F);
			this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelHeader.Location = new System.Drawing.Point(3, 8);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.labelHeader.Size = new System.Drawing.Size(111, 18);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "xxManageSites";
			// 
			// SiteControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.tableLayoutPanelBody);
			this.Controls.Add(this.gradientPanelHeader);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.Name = "SiteControl";
			this.Size = new System.Drawing.Size(600, 490);
			this.tableLayoutPanelBody.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridControlSites)).EndInit();
			this.tableLayoutPanelSubHeader2.ResumeLayout(false);
			this.tableLayoutPanelSubHeader2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
			this.gradientPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader2;
		private System.Windows.Forms.Label labelSubHeader2;
		private Syncfusion.Windows.Forms.Grid.GridControl gridControlSites;
    }
}
