using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Budgeting
{
    partial class EditShrinkageForm
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
			this.textBoxShrinkageName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvSave = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelName = new System.Windows.Forms.Label();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.tableLayoutPanelAbsence = new System.Windows.Forms.TableLayoutPanel();
			this.labelAbsence = new System.Windows.Forms.Label();
			this.buttonAdvAddAll = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvAddSelected = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvRemoveSelected = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvRemoveAll = new Syncfusion.Windows.Forms.ButtonAdv();
			this.listBoxAddedAbsences = new System.Windows.Forms.ListBox();
			this.listBoxAbsences = new System.Windows.Forms.ListBox();
			this.checkBoxInclude = new System.Windows.Forms.CheckBox();
			this.tableLayoutPanelAllowance = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelFields = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.textBoxShrinkageName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.tableLayoutPanelAbsence.SuspendLayout();
			this.tableLayoutPanelAllowance.SuspendLayout();
			this.tableLayoutPanelFields.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxShrinkageName
			// 
			this.textBoxShrinkageName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxShrinkageName.BeforeTouchSize = new System.Drawing.Size(307, 25);
			this.tableLayoutPanelFields.SetColumnSpan(this.textBoxShrinkageName, 3);
			this.textBoxShrinkageName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxShrinkageName.Location = new System.Drawing.Point(108, 14);
			this.textBoxShrinkageName.Margin = new System.Windows.Forms.Padding(3, 14, 3, 3);
			this.textBoxShrinkageName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxShrinkageName.Name = "textBoxShrinkageName";
			this.textBoxShrinkageName.OverflowIndicatorToolTipText = null;
			this.textBoxShrinkageName.Size = new System.Drawing.Size(276, 23);
			this.textBoxShrinkageName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.textBoxShrinkageName.TabIndex = 0;
			this.textBoxShrinkageName.TextChanged += new System.EventHandler(this.textBoxExt1_TextChanged);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(296, 363);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 1;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// buttonAdvSave
			// 
			this.buttonAdvSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvSave.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvSave.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvSave.ForeColor = System.Drawing.Color.White;
			this.buttonAdvSave.IsBackStageButton = false;
			this.buttonAdvSave.Location = new System.Drawing.Point(176, 363);
			this.buttonAdvSave.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvSave.Name = "buttonAdvSave";
			this.buttonAdvSave.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvSave.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvSave.TabIndex = 0;
			this.buttonAdvSave.Text = "xxOk";
			this.buttonAdvSave.UseVisualStyle = true;
			this.buttonAdvSave.UseVisualStyleBackColor = false;
			this.buttonAdvSave.Click += new System.EventHandler(this.buttonAdvSave_Click);
			// 
			// labelName
			// 
			this.labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelName.Location = new System.Drawing.Point(3, 15);
			this.labelName.Margin = new System.Windows.Forms.Padding(3, 14, 3, 0);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(98, 23);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "xxNameColon";
			this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// errorProvider
			// 
			this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider.ContainerControl = this;
			// 
			// tableLayoutPanelAbsence
			// 
			this.tableLayoutPanelAbsence.ColumnCount = 3;
			this.tableLayoutPanelAllowance.SetColumnSpan(this.tableLayoutPanelAbsence, 3);
			this.tableLayoutPanelAbsence.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
			this.tableLayoutPanelAbsence.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanelAbsence.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
			this.tableLayoutPanelAbsence.Controls.Add(this.labelAbsence, 0, 0);
			this.tableLayoutPanelAbsence.Controls.Add(this.buttonAdvAddAll, 1, 2);
			this.tableLayoutPanelAbsence.Controls.Add(this.buttonAdvAddSelected, 1, 3);
			this.tableLayoutPanelAbsence.Controls.Add(this.buttonAdvRemoveSelected, 1, 4);
			this.tableLayoutPanelAbsence.Controls.Add(this.buttonAdvRemoveAll, 1, 5);
			this.tableLayoutPanelAbsence.Controls.Add(this.listBoxAddedAbsences, 2, 1);
			this.tableLayoutPanelAbsence.Controls.Add(this.listBoxAbsences, 0, 1);
			this.tableLayoutPanelAbsence.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelAbsence.Location = new System.Drawing.Point(0, 29);
			this.tableLayoutPanelAbsence.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelAbsence.Name = "tableLayoutPanelAbsence";
			this.tableLayoutPanelAbsence.RowCount = 7;
			this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsence.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsence.Size = new System.Drawing.Size(393, 281);
			this.tableLayoutPanelAbsence.TabIndex = 2;
			// 
			// labelAbsence
			// 
			this.labelAbsence.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelAbsence.AutoSize = true;
			this.labelAbsence.Location = new System.Drawing.Point(3, 7);
			this.labelAbsence.Name = "labelAbsence";
			this.labelAbsence.Size = new System.Drawing.Size(170, 15);
			this.labelAbsence.TabIndex = 12;
			this.labelAbsence.Text = "xxAbsence";
			// 
			// buttonAdvAddAll
			// 
			this.buttonAdvAddAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvAddAll.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvAddAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvAddAll.BeforeTouchSize = new System.Drawing.Size(33, 27);
			this.buttonAdvAddAll.ForeColor = System.Drawing.Color.White;
			this.buttonAdvAddAll.IsBackStageButton = false;
			this.buttonAdvAddAll.Location = new System.Drawing.Point(179, 68);
			this.buttonAdvAddAll.Name = "buttonAdvAddAll";
			this.buttonAdvAddAll.Size = new System.Drawing.Size(33, 27);
			this.buttonAdvAddAll.TabIndex = 7;
			this.buttonAdvAddAll.Text = ">>";
			this.buttonAdvAddAll.UseVisualStyle = true;
			this.buttonAdvAddAll.UseVisualStyleBackColor = false;
			this.buttonAdvAddAll.Click += new System.EventHandler(this.buttonAdvAddAllClick);
			// 
			// buttonAdvAddSelected
			// 
			this.buttonAdvAddSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvAddSelected.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvAddSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvAddSelected.BeforeTouchSize = new System.Drawing.Size(33, 27);
			this.buttonAdvAddSelected.ForeColor = System.Drawing.Color.White;
			this.buttonAdvAddSelected.IsBackStageButton = false;
			this.buttonAdvAddSelected.Location = new System.Drawing.Point(179, 103);
			this.buttonAdvAddSelected.Name = "buttonAdvAddSelected";
			this.buttonAdvAddSelected.Size = new System.Drawing.Size(33, 27);
			this.buttonAdvAddSelected.TabIndex = 8;
			this.buttonAdvAddSelected.Text = ">";
			this.buttonAdvAddSelected.UseVisualStyle = true;
			this.buttonAdvAddSelected.UseVisualStyleBackColor = false;
			this.buttonAdvAddSelected.Click += new System.EventHandler(this.buttonAdvAddSelectedClick);
			// 
			// buttonAdvRemoveSelected
			// 
			this.buttonAdvRemoveSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvRemoveSelected.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvRemoveSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvRemoveSelected.BeforeTouchSize = new System.Drawing.Size(33, 27);
			this.buttonAdvRemoveSelected.ForeColor = System.Drawing.Color.White;
			this.buttonAdvRemoveSelected.IsBackStageButton = false;
			this.buttonAdvRemoveSelected.Location = new System.Drawing.Point(179, 138);
			this.buttonAdvRemoveSelected.Name = "buttonAdvRemoveSelected";
			this.buttonAdvRemoveSelected.Size = new System.Drawing.Size(33, 27);
			this.buttonAdvRemoveSelected.TabIndex = 9;
			this.buttonAdvRemoveSelected.Text = "<";
			this.buttonAdvRemoveSelected.UseVisualStyle = true;
			this.buttonAdvRemoveSelected.UseVisualStyleBackColor = false;
			this.buttonAdvRemoveSelected.Click += new System.EventHandler(this.buttonAdvRemoveSelectedClick);
			// 
			// buttonAdvRemoveAll
			// 
			this.buttonAdvRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvRemoveAll.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvRemoveAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvRemoveAll.BeforeTouchSize = new System.Drawing.Size(33, 27);
			this.buttonAdvRemoveAll.ForeColor = System.Drawing.Color.White;
			this.buttonAdvRemoveAll.IsBackStageButton = false;
			this.buttonAdvRemoveAll.Location = new System.Drawing.Point(179, 173);
			this.buttonAdvRemoveAll.Name = "buttonAdvRemoveAll";
			this.buttonAdvRemoveAll.Size = new System.Drawing.Size(33, 27);
			this.buttonAdvRemoveAll.TabIndex = 10;
			this.buttonAdvRemoveAll.Text = "<<";
			this.buttonAdvRemoveAll.UseVisualStyle = true;
			this.buttonAdvRemoveAll.UseVisualStyleBackColor = false;
			this.buttonAdvRemoveAll.Click += new System.EventHandler(this.buttonAdvRemoveAllClick);
			// 
			// listBoxAddedAbsences
			// 
			this.listBoxAddedAbsences.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxAddedAbsences.FormattingEnabled = true;
			this.listBoxAddedAbsences.HorizontalScrollbar = true;
			this.listBoxAddedAbsences.ItemHeight = 15;
			this.listBoxAddedAbsences.Location = new System.Drawing.Point(218, 32);
			this.listBoxAddedAbsences.Name = "listBoxAddedAbsences";
			this.tableLayoutPanelAbsence.SetRowSpan(this.listBoxAddedAbsences, 6);
			this.listBoxAddedAbsences.Size = new System.Drawing.Size(172, 246);
			this.listBoxAddedAbsences.TabIndex = 13;
			this.listBoxAddedAbsences.DoubleClick += new System.EventHandler(this.listBoxAddedAbsencesDoubleClick);
			// 
			// listBoxAbsences
			// 
			this.listBoxAbsences.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxAbsences.FormattingEnabled = true;
			this.listBoxAbsences.HorizontalScrollbar = true;
			this.listBoxAbsences.ItemHeight = 15;
			this.listBoxAbsences.Location = new System.Drawing.Point(3, 32);
			this.listBoxAbsences.Name = "listBoxAbsences";
			this.tableLayoutPanelAbsence.SetRowSpan(this.listBoxAbsences, 6);
			this.listBoxAbsences.Size = new System.Drawing.Size(170, 246);
			this.listBoxAbsences.TabIndex = 14;
			this.listBoxAbsences.DoubleClick += new System.EventHandler(this.listBoxAbsencesDoubleClick);
			// 
			// checkBoxInclude
			// 
			this.checkBoxInclude.AutoSize = true;
			this.checkBoxInclude.Location = new System.Drawing.Point(108, 6);
			this.checkBoxInclude.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.checkBoxInclude.Name = "checkBoxInclude";
			this.checkBoxInclude.Size = new System.Drawing.Size(189, 19);
			this.checkBoxInclude.TabIndex = 1;
			this.checkBoxInclude.Text = "xxIncludedInRequestAllowance";
			this.checkBoxInclude.UseVisualStyleBackColor = true;
			this.checkBoxInclude.CheckedChanged += new System.EventHandler(this.checkBoxInclude_CheckedChanged);
			// 
			// tableLayoutPanelAllowance
			// 
			this.tableLayoutPanelAllowance.AutoSize = true;
			this.tableLayoutPanelAllowance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanelAllowance.ColumnCount = 2;
			this.tableLayoutPanelFields.SetColumnSpan(this.tableLayoutPanelAllowance, 4);
			this.tableLayoutPanelAllowance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
			this.tableLayoutPanelAllowance.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelAllowance.Controls.Add(this.checkBoxInclude, 1, 0);
			this.tableLayoutPanelAllowance.Controls.Add(this.tableLayoutPanelAbsence, 0, 1);
			this.tableLayoutPanelAllowance.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelAllowance.Location = new System.Drawing.Point(0, 40);
			this.tableLayoutPanelAllowance.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelAllowance.Name = "tableLayoutPanelAllowance";
			this.tableLayoutPanelAllowance.RowCount = 2;
			this.tableLayoutPanelAllowance.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanelAllowance.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelAllowance.Size = new System.Drawing.Size(393, 310);
			this.tableLayoutPanelAllowance.TabIndex = 13;
			// 
			// tableLayoutPanelFields
			// 
			this.tableLayoutPanelFields.ColumnCount = 4;
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelFields.Controls.Add(this.textBoxShrinkageName, 1, 0);
			this.tableLayoutPanelFields.Controls.Add(this.buttonAdvSave, 2, 2);
			this.tableLayoutPanelFields.Controls.Add(this.labelName, 0, 0);
			this.tableLayoutPanelFields.Controls.Add(this.tableLayoutPanelAllowance, 0, 1);
			this.tableLayoutPanelFields.Controls.Add(this.buttonAdvCancel, 3, 2);
			this.tableLayoutPanelFields.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelFields.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelFields.Name = "tableLayoutPanelFields";
			this.tableLayoutPanelFields.RowCount = 3;
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanelFields.Size = new System.Drawing.Size(393, 400);
			this.tableLayoutPanelFields.TabIndex = 14;
			// 
			// EditShrinkageForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(393, 400);
			this.Controls.Add(this.tableLayoutPanelFields);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditShrinkageForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxUpdateShrinkageRow";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.textBoxShrinkageName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.tableLayoutPanelAbsence.ResumeLayout(false);
			this.tableLayoutPanelAbsence.PerformLayout();
			this.tableLayoutPanelAllowance.ResumeLayout(false);
			this.tableLayoutPanelAllowance.PerformLayout();
			this.tableLayoutPanelFields.ResumeLayout(false);
			this.tableLayoutPanelFields.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

		private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxShrinkageName;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSave;
        private System.Windows.Forms.ErrorProvider errorProvider;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFields;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAllowance;
		private System.Windows.Forms.CheckBox checkBoxInclude;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAbsence;
		private System.Windows.Forms.Label labelAbsence;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAddAll;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAddSelected;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRemoveSelected;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRemoveAll;
		private System.Windows.Forms.ListBox listBoxAddedAbsences;
		private System.Windows.Forms.ListBox listBoxAbsences;
    }
}