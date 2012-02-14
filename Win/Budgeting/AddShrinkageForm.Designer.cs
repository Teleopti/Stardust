using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Budgeting
{
    partial class AddShrinkageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddShrinkageForm));
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.gradientPanelTop = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelFields = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxShrinkageName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.gradientPanelBottom = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.buttonAdvSave = new Syncfusion.Windows.Forms.ButtonAdv();
            this.tableLayoutPanelAllowance = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxInclude = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanelAbsence = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvAddAll = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvAddSelected = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvRemoveSelected = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvRemoveAll = new Syncfusion.Windows.Forms.ButtonAdv();
            this.listBoxAddedAbsences = new System.Windows.Forms.ListBox();
            this.listBoxAbsences = new System.Windows.Forms.ListBox();
            this.labelAbsence = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelTop)).BeginInit();
            this.gradientPanelTop.SuspendLayout();
            this.tableLayoutPanelFields.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxShrinkageName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelBottom)).BeginInit();
            this.gradientPanelBottom.SuspendLayout();
            this.tableLayoutPanelAllowance.SuspendLayout();
            this.tableLayoutPanelAbsence.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvCancel.Location = new System.Drawing.Point(272, 10);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvCancel.TabIndex = 1;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
            // 
            // gradientPanelTop
            // 
            this.gradientPanelTop.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(252)))), ((int)(((byte)(252))))), System.Drawing.Color.White);
            this.gradientPanelTop.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanelTop.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelTop.Controls.Add(this.tableLayoutPanelFields);
            this.gradientPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanelTop.Location = new System.Drawing.Point(6, 34);
            this.gradientPanelTop.Name = "gradientPanelTop";
            this.gradientPanelTop.Size = new System.Drawing.Size(369, 310);
            this.gradientPanelTop.TabIndex = 2;
            // 
            // tableLayoutPanelFields
            // 
            this.tableLayoutPanelFields.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelFields.ColumnCount = 2;
            this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelFields.Controls.Add(this.textBoxShrinkageName, 1, 0);
            this.tableLayoutPanelFields.Controls.Add(this.gradientPanelBottom, 0, 2);
            this.tableLayoutPanelFields.Controls.Add(this.tableLayoutPanelAllowance, 0, 1);
            this.tableLayoutPanelFields.Controls.Add(this.labelName, 0, 0);
            this.tableLayoutPanelFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelFields.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelFields.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.tableLayoutPanelFields.Name = "tableLayoutPanelFields";
            this.tableLayoutPanelFields.RowCount = 3;
            this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 238F));
            this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanelFields.Size = new System.Drawing.Size(369, 310);
            this.tableLayoutPanelFields.TabIndex = 1;
            // 
            // textBoxShrinkageName
            // 
            this.textBoxShrinkageName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxShrinkageName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxShrinkageName.Location = new System.Drawing.Point(93, 12);
            this.textBoxShrinkageName.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.textBoxShrinkageName.Name = "textBoxShrinkageName";
            this.textBoxShrinkageName.OverflowIndicatorToolTipText = null;
            this.textBoxShrinkageName.Size = new System.Drawing.Size(264, 20);
            this.textBoxShrinkageName.TabIndex = 0;
            this.textBoxShrinkageName.TextChanged += new System.EventHandler(this.textBoxExt1_TextChanged);
            // 
            // gradientPanelBottom
            // 
            this.gradientPanelBottom.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))), System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))));
            this.gradientPanelBottom.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanelBottom.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tableLayoutPanelFields.SetColumnSpan(this.gradientPanelBottom, 2);
            this.gradientPanelBottom.Controls.Add(this.buttonAdvCancel);
            this.gradientPanelBottom.Controls.Add(this.buttonAdvSave);
            this.gradientPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanelBottom.Location = new System.Drawing.Point(0, 274);
            this.gradientPanelBottom.Margin = new System.Windows.Forms.Padding(0);
            this.gradientPanelBottom.Name = "gradientPanelBottom";
            this.gradientPanelBottom.Size = new System.Drawing.Size(369, 36);
            this.gradientPanelBottom.TabIndex = 0;
            // 
            // buttonAdvSave
            // 
            this.buttonAdvSave.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvSave.Location = new System.Drawing.Point(181, 10);
            this.buttonAdvSave.Name = "buttonAdvSave";
            this.buttonAdvSave.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvSave.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvSave.TabIndex = 0;
            this.buttonAdvSave.Text = "xxOk";
            this.buttonAdvSave.UseVisualStyle = true;
            this.buttonAdvSave.UseVisualStyleBackColor = false;
            this.buttonAdvSave.Click += new System.EventHandler(this.buttonAdvSave_Click);
            // 
            // tableLayoutPanelAllowance
            // 
            this.tableLayoutPanelAllowance.ColumnCount = 2;
            this.tableLayoutPanelFields.SetColumnSpan(this.tableLayoutPanelAllowance, 2);
            this.tableLayoutPanelAllowance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanelAllowance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelAllowance.Controls.Add(this.checkBoxInclude, 1, 0);
            this.tableLayoutPanelAllowance.Controls.Add(this.tableLayoutPanelAbsence, 0, 1);
            this.tableLayoutPanelAllowance.Location = new System.Drawing.Point(0, 36);
            this.tableLayoutPanelAllowance.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelAllowance.Name = "tableLayoutPanelAllowance";
            this.tableLayoutPanelAllowance.RowCount = 2;
            this.tableLayoutPanelAllowance.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelAllowance.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelAllowance.Size = new System.Drawing.Size(369, 238);
            this.tableLayoutPanelAllowance.TabIndex = 13;
            // 
            // checkBoxInclude
            // 
            this.checkBoxInclude.AutoSize = true;
            this.checkBoxInclude.Location = new System.Drawing.Point(93, 5);
            this.checkBoxInclude.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.checkBoxInclude.Name = "checkBoxInclude";
            this.checkBoxInclude.Size = new System.Drawing.Size(175, 17);
            this.checkBoxInclude.TabIndex = 1;
            this.checkBoxInclude.Text = "xxIncludedInRequestAllowance";
            this.checkBoxInclude.UseVisualStyleBackColor = true;
            this.checkBoxInclude.CheckedChanged += new System.EventHandler(this.checkBoxInclude_CheckedChanged);
            // 
            // tableLayoutPanelAbsence
            // 
            this.tableLayoutPanelAbsence.ColumnCount = 3;
            this.tableLayoutPanelAllowance.SetColumnSpan(this.tableLayoutPanelAbsence, 3);
            this.tableLayoutPanelAbsence.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanelAbsence.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelAbsence.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanelAbsence.Controls.Add(this.buttonAdvAddAll, 1, 2);
            this.tableLayoutPanelAbsence.Controls.Add(this.buttonAdvAddSelected, 1, 3);
            this.tableLayoutPanelAbsence.Controls.Add(this.buttonAdvRemoveSelected, 1, 4);
            this.tableLayoutPanelAbsence.Controls.Add(this.buttonAdvRemoveAll, 1, 5);
            this.tableLayoutPanelAbsence.Controls.Add(this.listBoxAddedAbsences, 2, 1);
            this.tableLayoutPanelAbsence.Controls.Add(this.listBoxAbsences, 0, 1);
            this.tableLayoutPanelAbsence.Controls.Add(this.labelAbsence, 0, 0);
            this.tableLayoutPanelAbsence.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanelAbsence.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelAbsence.Name = "tableLayoutPanelAbsence";
            this.tableLayoutPanelAbsence.RowCount = 7;
            this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelAbsence.Size = new System.Drawing.Size(369, 213);
            this.tableLayoutPanelAbsence.TabIndex = 2;
            // 
            // buttonAdvAddAll
            // 
            this.buttonAdvAddAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvAddAll.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvAddAll.Location = new System.Drawing.Point(169, 58);
            this.buttonAdvAddAll.Name = "buttonAdvAddAll";
            this.buttonAdvAddAll.Size = new System.Drawing.Size(30, 23);
            this.buttonAdvAddAll.TabIndex = 7;
            this.buttonAdvAddAll.Text = ">>";
            this.buttonAdvAddAll.UseVisualStyle = true;
            this.buttonAdvAddAll.UseVisualStyleBackColor = false;
            this.buttonAdvAddAll.Click += new System.EventHandler(this.buttonAdvAddAll_Click);
            // 
            // buttonAdvAddSelected
            // 
            this.buttonAdvAddSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvAddSelected.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvAddSelected.Location = new System.Drawing.Point(169, 88);
            this.buttonAdvAddSelected.Name = "buttonAdvAddSelected";
            this.buttonAdvAddSelected.Size = new System.Drawing.Size(30, 23);
            this.buttonAdvAddSelected.TabIndex = 8;
            this.buttonAdvAddSelected.Text = ">";
            this.buttonAdvAddSelected.UseVisualStyle = true;
            this.buttonAdvAddSelected.UseVisualStyleBackColor = false;
            this.buttonAdvAddSelected.Click += new System.EventHandler(this.buttonAdvAddSelected_Click);
            // 
            // buttonAdvRemoveSelected
            // 
            this.buttonAdvRemoveSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvRemoveSelected.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvRemoveSelected.Location = new System.Drawing.Point(169, 118);
            this.buttonAdvRemoveSelected.Name = "buttonAdvRemoveSelected";
            this.buttonAdvRemoveSelected.Size = new System.Drawing.Size(30, 23);
            this.buttonAdvRemoveSelected.TabIndex = 9;
            this.buttonAdvRemoveSelected.Text = "<";
            this.buttonAdvRemoveSelected.UseVisualStyle = true;
            this.buttonAdvRemoveSelected.UseVisualStyleBackColor = false;
            this.buttonAdvRemoveSelected.Click += new System.EventHandler(this.buttonAdvRemoveSelected_Click);
            // 
            // buttonAdvRemoveAll
            // 
            this.buttonAdvRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvRemoveAll.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvRemoveAll.Location = new System.Drawing.Point(169, 148);
            this.buttonAdvRemoveAll.Name = "buttonAdvRemoveAll";
            this.buttonAdvRemoveAll.Size = new System.Drawing.Size(30, 23);
            this.buttonAdvRemoveAll.TabIndex = 10;
            this.buttonAdvRemoveAll.Text = "<<";
            this.buttonAdvRemoveAll.UseVisualStyle = true;
            this.buttonAdvRemoveAll.UseVisualStyleBackColor = false;
            this.buttonAdvRemoveAll.Click += new System.EventHandler(this.buttonAdvRemoveAll_Click);
            // 
            // listBoxAddedAbsences
            // 
            this.listBoxAddedAbsences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxAddedAbsences.FormattingEnabled = true;
            this.listBoxAddedAbsences.HorizontalScrollbar = true;
            this.listBoxAddedAbsences.Location = new System.Drawing.Point(205, 28);
            this.listBoxAddedAbsences.Name = "listBoxAddedAbsences";
            this.tableLayoutPanelAbsence.SetRowSpan(this.listBoxAddedAbsences, 6);
            this.listBoxAddedAbsences.Size = new System.Drawing.Size(161, 182);
            this.listBoxAddedAbsences.TabIndex = 13;
            this.listBoxAddedAbsences.DoubleClick += new System.EventHandler(this.listBoxAddedAbsences_DoubleClick);
            // 
            // listBoxAbsences
            // 
            this.listBoxAbsences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxAbsences.FormattingEnabled = true;
            this.listBoxAbsences.HorizontalScrollbar = true;
            this.listBoxAbsences.Location = new System.Drawing.Point(3, 28);
            this.listBoxAbsences.Name = "listBoxAbsences";
            this.tableLayoutPanelAbsence.SetRowSpan(this.listBoxAbsences, 6);
            this.listBoxAbsences.Size = new System.Drawing.Size(160, 182);
            this.listBoxAbsences.TabIndex = 14;
            this.listBoxAbsences.DoubleClick += new System.EventHandler(this.listBoxAbsences_DoubleClick);
            // 
            // labelAbsence
            // 
            this.labelAbsence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelAbsence.AutoSize = true;
            this.labelAbsence.Location = new System.Drawing.Point(3, 6);
            this.labelAbsence.Name = "labelAbsence";
            this.labelAbsence.Size = new System.Drawing.Size(59, 13);
            this.labelAbsence.TabIndex = 12;
            this.labelAbsence.Text = "xxAbsence";
            // 
            // labelName
            // 
            this.labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelName.Location = new System.Drawing.Point(3, 14);
            this.labelName.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(84, 20);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "xxNameColon";
            this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.ribbonControlAdv1.SelectedTab = null;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(379, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Startmenu";
            this.ribbonControlAdv1.TabIndex = 0;
            this.ribbonControlAdv1.Text = "xxAddShrinkageRow";
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // AddShrinkageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAdvCancel;
            this.ClientSize = new System.Drawing.Size(381, 350);
            this.Controls.Add(this.gradientPanelTop);
            this.Controls.Add(this.ribbonControlAdv1);
            this.HelpButtonImage = ((System.Drawing.Image)(resources.GetObject("$this.HelpButtonImage")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddShrinkageForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxAddShrinkageRow";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelTop)).EndInit();
            this.gradientPanelTop.ResumeLayout(false);
            this.tableLayoutPanelFields.ResumeLayout(false);
            this.tableLayoutPanelFields.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxShrinkageName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelBottom)).EndInit();
            this.gradientPanelBottom.ResumeLayout(false);
            this.tableLayoutPanelAllowance.ResumeLayout(false);
            this.tableLayoutPanelAllowance.PerformLayout();
            this.tableLayoutPanelAbsence.ResumeLayout(false);
            this.tableLayoutPanelAbsence.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private System.Windows.Forms.Label labelName;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxShrinkageName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFields;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelTop;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelBottom;
        private System.Windows.Forms.CheckBox checkBoxInclude;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSave;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAddAll;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAddSelected;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRemoveSelected;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRemoveAll;
        private System.Windows.Forms.Label labelAbsence;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAllowance;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAbsence;
        private System.Windows.Forms.ListBox listBoxAddedAbsences;
        private System.Windows.Forms.ListBox listBoxAbsences;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}