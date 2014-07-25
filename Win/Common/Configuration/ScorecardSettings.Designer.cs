namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class ScorecardSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScorecardSettings));
            this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
            this.labelHeader = new System.Windows.Forms.Label();
            this.labelSubHeader3 = new System.Windows.Forms.Label();
            this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelSubHeader2 = new System.Windows.Forms.TableLayoutPanel();
            this.labelSubHeader2 = new System.Windows.Forms.Label();
            this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonNew = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvDeleteScorecard = new Syncfusion.Windows.Forms.ButtonAdv();
            this.labelSubHeader1 = new System.Windows.Forms.Label();
            this.checkedListBoxKpi = new System.Windows.Forms.CheckedListBox();
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxScorecardName = new System.Windows.Forms.TextBox();
            this.comboBoxAdvScorecard = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.comboBoxAdvType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.labelMeasurePeriod = new System.Windows.Forms.Label();
            this.labelSelectScorecard = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
            this.gradientPanelHeader.SuspendLayout();
            this.tableLayoutPanelHeader.SuspendLayout();
            this.tableLayoutPanelBody.SuspendLayout();
            this.tableLayoutPanelSubHeader2.SuspendLayout();
            this.tableLayoutPanelSubHeader1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvScorecard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvType)).BeginInit();
            this.SuspendLayout();
            // 
            // gradientPanelHeader
            // 
            this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gradientPanelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
            this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.gradientPanelHeader.ForeColor = System.Drawing.Color.White;
            this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
            this.gradientPanelHeader.Name = "gradientPanelHeader";
            this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(12);
            this.gradientPanelHeader.Size = new System.Drawing.Size(716, 62);
            this.gradientPanelHeader.TabIndex = 4;
            // 
            // tableLayoutPanelHeader
            // 
            this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelHeader.ColumnCount = 1;
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 692F));
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
            this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelHeader.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
            this.tableLayoutPanelHeader.RowCount = 1;
            this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHeader.Size = new System.Drawing.Size(692, 38);
            this.tableLayoutPanelHeader.TabIndex = 0;
            // 
            // labelHeader
            // 
            this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.ForeColor = System.Drawing.Color.White;
            this.labelHeader.Location = new System.Drawing.Point(3, 6);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelHeader.Size = new System.Drawing.Size(209, 25);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "xxManageScorecards";
            // 
            // labelSubHeader3
            // 
            this.tableLayoutPanelBody.SetColumnSpan(this.labelSubHeader3, 2);
            this.labelSubHeader3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSubHeader3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelSubHeader3.Location = new System.Drawing.Point(3, 205);
            this.labelSubHeader3.Margin = new System.Windows.Forms.Padding(3);
            this.labelSubHeader3.Name = "labelSubHeader3";
            this.labelSubHeader3.Size = new System.Drawing.Size(710, 29);
            this.labelSubHeader3.TabIndex = 8;
            this.labelSubHeader3.Text = "xxKPIsIncludedInScorecardColon";
            this.labelSubHeader3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanelBody
            // 
            this.tableLayoutPanelBody.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanelBody.ColumnCount = 2;
            this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 204F));
            this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader2, 0, 3);
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
            this.tableLayoutPanelBody.Controls.Add(this.checkedListBoxKpi, 0, 7);
            this.tableLayoutPanelBody.Controls.Add(this.labelSubHeader3, 0, 6);
            this.tableLayoutPanelBody.Controls.Add(this.labelName, 0, 2);
            this.tableLayoutPanelBody.Controls.Add(this.textBoxScorecardName, 1, 2);
            this.tableLayoutPanelBody.Controls.Add(this.comboBoxAdvScorecard, 1, 1);
            this.tableLayoutPanelBody.Controls.Add(this.comboBoxAdvType, 1, 4);
            this.tableLayoutPanelBody.Controls.Add(this.labelMeasurePeriod, 0, 4);
            this.tableLayoutPanelBody.Controls.Add(this.labelSelectScorecard, 0, 1);
            this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 62);
            this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
            this.tableLayoutPanelBody.RowCount = 8;
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBody.Size = new System.Drawing.Size(716, 502);
            this.tableLayoutPanelBody.TabIndex = 0;
            // 
            // tableLayoutPanelSubHeader2
            // 
            this.tableLayoutPanelSubHeader2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.tableLayoutPanelSubHeader2.ColumnCount = 1;
            this.tableLayoutPanelBody.SetColumnSpan(this.tableLayoutPanelSubHeader2, 2);
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanelSubHeader2.Controls.Add(this.labelSubHeader2, 0, 0);
            this.tableLayoutPanelSubHeader2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSubHeader2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.tableLayoutPanelSubHeader2.Location = new System.Drawing.Point(3, 113);
            this.tableLayoutPanelSubHeader2.Name = "tableLayoutPanelSubHeader2";
            this.tableLayoutPanelSubHeader2.RowCount = 1;
            this.tableLayoutPanelSubHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanelSubHeader2.Size = new System.Drawing.Size(710, 34);
            this.tableLayoutPanelSubHeader2.TabIndex = 19;
            // 
            // labelSubHeader2
            // 
            this.labelSubHeader2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSubHeader2.AutoSize = true;
            this.labelSubHeader2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelSubHeader2.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelSubHeader2.Location = new System.Drawing.Point(0, 8);
            this.labelSubHeader2.Margin = new System.Windows.Forms.Padding(0);
            this.labelSubHeader2.Name = "labelSubHeader2";
            this.labelSubHeader2.Size = new System.Drawing.Size(209, 17);
            this.labelSubHeader2.TabIndex = 16;
            this.labelSubHeader2.Text = "xxEnterSettingsForThisScorecard";
            // 
            // tableLayoutPanelSubHeader1
            // 
            this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.DimGray;
            this.tableLayoutPanelSubHeader1.ColumnCount = 3;
            this.tableLayoutPanelBody.SetColumnSpan(this.tableLayoutPanelSubHeader1, 2);
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonNew, 0, 0);
            this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonAdvDeleteScorecard, 2, 0);
            this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
            this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
            this.tableLayoutPanelSubHeader1.RowCount = 1;
            this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(710, 34);
            this.tableLayoutPanelSubHeader1.TabIndex = 18;
            // 
            // buttonNew
            // 
            this.buttonNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonNew.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonNew.BackColor = System.Drawing.Color.White;
            this.buttonNew.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonNew.ForeColor = System.Drawing.Color.White;
            this.buttonNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.buttonNew.IsBackStageButton = false;
            this.buttonNew.Location = new System.Drawing.Point(640, 3);
            this.buttonNew.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(28, 28);
            this.buttonNew.TabIndex = 18;
            this.buttonNew.TabStop = false;
            this.toolTip1.SetToolTip(this.buttonNew, "xxDelete the selected Scorecard.");
            this.buttonNew.UseVisualStyle = true;
            this.buttonNew.UseVisualStyleBackColor = false;
            this.buttonNew.Click += new System.EventHandler(this.buttonNewClick);
            // 
            // buttonAdvDeleteScorecard
            // 
            this.buttonAdvDeleteScorecard.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdvDeleteScorecard.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvDeleteScorecard.BackColor = System.Drawing.Color.White;
            this.buttonAdvDeleteScorecard.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonAdvDeleteScorecard.ForeColor = System.Drawing.Color.White;
            this.buttonAdvDeleteScorecard.Image = ((System.Drawing.Image)(resources.GetObject("buttonAdvDeleteScorecard.Image")));
            this.buttonAdvDeleteScorecard.IsBackStageButton = false;
            this.buttonAdvDeleteScorecard.Location = new System.Drawing.Point(675, 3);
            this.buttonAdvDeleteScorecard.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.buttonAdvDeleteScorecard.Name = "buttonAdvDeleteScorecard";
            this.buttonAdvDeleteScorecard.Size = new System.Drawing.Size(28, 28);
            this.buttonAdvDeleteScorecard.TabIndex = 2;
            this.buttonAdvDeleteScorecard.TabStop = false;
            this.toolTip1.SetToolTip(this.buttonAdvDeleteScorecard, "xxDelete the selected Scorecard.");
            this.buttonAdvDeleteScorecard.UseVisualStyle = true;
            this.buttonAdvDeleteScorecard.UseVisualStyleBackColor = false;
            this.buttonAdvDeleteScorecard.Click += new System.EventHandler(this.buttonAdvDeleteScorecardClick);
            // 
            // labelSubHeader1
            // 
            this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSubHeader1.AutoSize = true;
            this.labelSubHeader1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubHeader1.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelSubHeader1.Location = new System.Drawing.Point(3, 8);
            this.labelSubHeader1.Name = "labelSubHeader1";
            this.labelSubHeader1.Size = new System.Drawing.Size(165, 17);
            this.labelSubHeader1.TabIndex = 17;
            this.labelSubHeader1.Text = "xxChooseScorecardToEdit";
            this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkedListBoxKpi
            // 
            this.checkedListBoxKpi.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBoxKpi.CheckOnClick = true;
            this.tableLayoutPanelBody.SetColumnSpan(this.checkedListBoxKpi, 2);
            this.checkedListBoxKpi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBoxKpi.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkedListBoxKpi.FormattingEnabled = true;
            this.checkedListBoxKpi.Location = new System.Drawing.Point(10, 247);
            this.checkedListBoxKpi.Margin = new System.Windows.Forms.Padding(10);
            this.checkedListBoxKpi.Name = "checkedListBoxKpi";
            this.checkedListBoxKpi.Size = new System.Drawing.Size(696, 245);
            this.checkedListBoxKpi.TabIndex = 9;
            // 
            // labelName
            // 
            this.labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelName.Location = new System.Drawing.Point(3, 80);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(169, 24);
            this.labelName.TabIndex = 6;
            this.labelName.Text = "xxNameColon";
            this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxScorecardName
            // 
            this.textBoxScorecardName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxScorecardName.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.textBoxScorecardName.Location = new System.Drawing.Point(207, 82);
            this.textBoxScorecardName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.textBoxScorecardName.MaxLength = 100;
            this.textBoxScorecardName.Name = "textBoxScorecardName";
            this.textBoxScorecardName.Size = new System.Drawing.Size(251, 22);
            this.textBoxScorecardName.TabIndex = 5;
            // 
            // comboBoxAdvScorecard
            // 
            this.comboBoxAdvScorecard.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBoxAdvScorecard.BackColor = System.Drawing.Color.White;
            this.comboBoxAdvScorecard.BeforeTouchSize = new System.Drawing.Size(252, 23);
            this.comboBoxAdvScorecard.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.comboBoxAdvScorecard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvScorecard.Location = new System.Drawing.Point(207, 47);
            this.comboBoxAdvScorecard.Name = "comboBoxAdvScorecard";
            this.comboBoxAdvScorecard.Size = new System.Drawing.Size(252, 23);
            this.comboBoxAdvScorecard.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
            this.comboBoxAdvScorecard.TabIndex = 1;
            // 
            // comboBoxAdvType
            // 
            this.comboBoxAdvType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBoxAdvType.BackColor = System.Drawing.Color.White;
            this.comboBoxAdvType.BeforeTouchSize = new System.Drawing.Size(252, 23);
            this.comboBoxAdvType.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.comboBoxAdvType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvType.Items.AddRange(new object[] {
            "xxDag",
            "xxVecka",
            "xxMånad",
            "xxKvartal",
            "xxÅr"});
            this.comboBoxAdvType.ItemsImageIndexes.Add(new Syncfusion.Windows.Forms.Tools.ComboBoxAdv.ImageIndexItem(this.comboBoxAdvType, "xxDag"));
            this.comboBoxAdvType.ItemsImageIndexes.Add(new Syncfusion.Windows.Forms.Tools.ComboBoxAdv.ImageIndexItem(this.comboBoxAdvType, "xxVecka"));
            this.comboBoxAdvType.ItemsImageIndexes.Add(new Syncfusion.Windows.Forms.Tools.ComboBoxAdv.ImageIndexItem(this.comboBoxAdvType, "xxMånad"));
            this.comboBoxAdvType.ItemsImageIndexes.Add(new Syncfusion.Windows.Forms.Tools.ComboBoxAdv.ImageIndexItem(this.comboBoxAdvType, "xxKvartal"));
            this.comboBoxAdvType.ItemsImageIndexes.Add(new Syncfusion.Windows.Forms.Tools.ComboBoxAdv.ImageIndexItem(this.comboBoxAdvType, "xxÅr"));
            this.comboBoxAdvType.Location = new System.Drawing.Point(207, 157);
            this.comboBoxAdvType.Name = "comboBoxAdvType";
            this.comboBoxAdvType.Size = new System.Drawing.Size(252, 23);
            this.comboBoxAdvType.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
            this.comboBoxAdvType.TabIndex = 16;
            this.comboBoxAdvType.Text = "xxDag";
            // 
            // labelMeasurePeriod
            // 
            this.labelMeasurePeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMeasurePeriod.Location = new System.Drawing.Point(3, 155);
            this.labelMeasurePeriod.Name = "labelMeasurePeriod";
            this.labelMeasurePeriod.Size = new System.Drawing.Size(169, 24);
            this.labelMeasurePeriod.TabIndex = 6;
            this.labelMeasurePeriod.Text = "xxPeriodColon";
            this.labelMeasurePeriod.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelSelectScorecard
            // 
            this.labelSelectScorecard.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSelectScorecard.Location = new System.Drawing.Point(3, 45);
            this.labelSelectScorecard.Name = "labelSelectScorecard";
            this.labelSelectScorecard.Size = new System.Drawing.Size(169, 24);
            this.labelSelectScorecard.TabIndex = 0;
            this.labelSelectScorecard.Text = "xxScorecardColon";
            this.labelSelectScorecard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ScorecardSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelBody);
            this.Controls.Add(this.gradientPanelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ScorecardSettings";
            this.Size = new System.Drawing.Size(716, 564);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
            this.gradientPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.PerformLayout();
            this.tableLayoutPanelBody.ResumeLayout(false);
            this.tableLayoutPanelBody.PerformLayout();
            this.tableLayoutPanelSubHeader2.ResumeLayout(false);
            this.tableLayoutPanelSubHeader2.PerformLayout();
            this.tableLayoutPanelSubHeader1.ResumeLayout(false);
            this.tableLayoutPanelSubHeader1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvScorecard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvType)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.TextBox textBoxScorecardName;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvScorecard;
        private System.Windows.Forms.Label labelSelectScorecard;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
        private System.Windows.Forms.Label labelMeasurePeriod;
        private System.Windows.Forms.Label labelSubHeader3;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.Label labelSubHeader2;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvType;
		private System.Windows.Forms.Label labelSubHeader1;
        private System.Windows.Forms.CheckedListBox checkedListBoxKpi;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDeleteScorecard;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonNew;
    }
}