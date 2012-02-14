namespace Teleopti.Ccc.Win.Common
{
    partial class PromptDoubleBox
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PromptDoubleBox));
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.gradientPanelTop = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelFields = new System.Windows.Forms.TableLayoutPanel();
			this.gradientPanelBottom = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvSave = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelName = new System.Windows.Forms.Label();
			this.numericTextBox1 = new Teleopti.Ccc.Win.Common.Controls.NumericTextBox();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelTop)).BeginInit();
			this.gradientPanelTop.SuspendLayout();
			this.tableLayoutPanelFields.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelBottom)).BeginInit();
			this.gradientPanelBottom.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(248, 35);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Startmenu";
			this.ribbonControlAdv1.TabIndex = 0;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			// 
			// gradientPanelTop
			// 
			this.gradientPanelTop.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(252)))), ((int)(((byte)(252))))), System.Drawing.Color.White);
			this.gradientPanelTop.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelTop.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelTop.Controls.Add(this.tableLayoutPanelFields);
			this.gradientPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanelTop.Location = new System.Drawing.Point(6, 36);
			this.gradientPanelTop.Name = "gradientPanelTop";
			this.gradientPanelTop.Size = new System.Drawing.Size(238, 78);
			this.gradientPanelTop.TabIndex = 2;
			// 
			// tableLayoutPanelFields
			// 
			this.tableLayoutPanelFields.AutoSize = true;
			this.tableLayoutPanelFields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanelFields.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelFields.ColumnCount = 2;
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelFields.Controls.Add(this.gradientPanelBottom, 0, 1);
			this.tableLayoutPanelFields.Controls.Add(this.labelName, 0, 0);
			this.tableLayoutPanelFields.Controls.Add(this.numericTextBox1, 1, 0);
			this.tableLayoutPanelFields.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelFields.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelFields.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.tableLayoutPanelFields.Name = "tableLayoutPanelFields";
			this.tableLayoutPanelFields.RowCount = 2;
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanelFields.Size = new System.Drawing.Size(238, 78);
			this.tableLayoutPanelFields.TabIndex = 1;
			// 
			// gradientPanelBottom
			// 
			this.gradientPanelBottom.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))), System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))));
			this.gradientPanelBottom.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelBottom.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tableLayoutPanelFields.SetColumnSpan(this.gradientPanelBottom, 2);
			this.gradientPanelBottom.Controls.Add(this.tableLayoutPanel1);
			this.gradientPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanelBottom.Location = new System.Drawing.Point(0, 46);
			this.gradientPanelBottom.Margin = new System.Windows.Forms.Padding(0);
			this.gradientPanelBottom.Name = "gradientPanelBottom";
			this.gradientPanelBottom.Size = new System.Drawing.Size(238, 32);
			this.gradientPanelBottom.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvSave, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(238, 32);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.Location = new System.Drawing.Point(161, 4);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(74, 23);
			this.buttonAdvCancel.TabIndex = 3;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
			// 
			// buttonAdvSave
			// 
			this.buttonAdvSave.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAdvSave.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvSave.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvSave.Location = new System.Drawing.Point(81, 4);
			this.buttonAdvSave.Name = "buttonAdvSave";
			this.buttonAdvSave.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvSave.Size = new System.Drawing.Size(73, 23);
			this.buttonAdvSave.TabIndex = 2;
			this.buttonAdvSave.Text = "xxOk";
			this.buttonAdvSave.UseVisualStyle = true;
			this.buttonAdvSave.UseVisualStyleBackColor = false;
			this.buttonAdvSave.Click += new System.EventHandler(this.buttonAdvSave_Click);
			// 
			// labelName
			// 
			this.labelName.AutoSize = true;
			this.labelName.Location = new System.Drawing.Point(10, 17);
			this.labelName.Margin = new System.Windows.Forms.Padding(10, 17, 0, 0);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(123, 13);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "xxEnterParameter0Colon";
			// 
			// numericTextBox1
			// 
			this.numericTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.numericTextBox1.Location = new System.Drawing.Point(136, 13);
			this.numericTextBox1.Name = "numericTextBox1";
			this.numericTextBox1.Size = new System.Drawing.Size(99, 20);
			this.numericTextBox1.TabIndex = 1;
			this.numericTextBox1.TextChanged += new System.EventHandler(this.numericTextBox1_TextChanged);
			// 
			// PromptDoubleBox
			// 
			this.AcceptButton = this.buttonAdvSave;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.CancelButton = this.buttonAdvCancel;
			this.ClientSize = new System.Drawing.Size(250, 120);
			this.Controls.Add(this.gradientPanelTop);
			this.Controls.Add(this.ribbonControlAdv1);
			this.HelpButton = false;
			this.HelpButtonImage = ((System.Drawing.Image)(resources.GetObject("$this.HelpButtonImage")));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(250, 120);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(250, 120);
			this.Name = "PromptDoubleBox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxNameType";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelTop)).EndInit();
			this.gradientPanelTop.ResumeLayout(false);
			this.gradientPanelTop.PerformLayout();
			this.tableLayoutPanelFields.ResumeLayout(false);
			this.tableLayoutPanelFields.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelBottom)).EndInit();
			this.gradientPanelBottom.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSave;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFields;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelTop;
		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelBottom;
		private Controls.NumericTextBox numericTextBox1;
    }
}