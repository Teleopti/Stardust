using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Budgeting
{
    partial class SetEfficiencyShrinkageForm
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
			this.buttonAdvSave = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanelFields = new System.Windows.Forms.TableLayoutPanel();
			this.textBoxExt1 = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.labelName = new System.Windows.Forms.Label();
			this.checkBoxInclude = new System.Windows.Forms.CheckBox();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.tableLayoutPanelFields.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExt1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonAdvSave
			// 
			this.buttonAdvSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvSave.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvSave.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvSave.ForeColor = System.Drawing.Color.White;
			this.buttonAdvSave.IsBackStageButton = false;
			this.buttonAdvSave.Location = new System.Drawing.Point(231, 146);
			this.buttonAdvSave.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.buttonAdvSave.Name = "buttonAdvSave";
			this.buttonAdvSave.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvSave.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvSave.TabIndex = 0;
			this.buttonAdvSave.Text = "xxOk";
			this.buttonAdvSave.UseVisualStyle = true;
			this.buttonAdvSave.UseVisualStyleBackColor = false;
			this.buttonAdvSave.Click += new System.EventHandler(this.buttonAdvSave_Click);
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
			this.buttonAdvCancel.Location = new System.Drawing.Point(344, 146);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 1;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
			// 
			// tableLayoutPanelFields
			// 
			this.tableLayoutPanelFields.AutoSize = true;
			this.tableLayoutPanelFields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanelFields.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelFields.ColumnCount = 4;
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 119F));
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelFields.Controls.Add(this.buttonAdvCancel, 3, 2);
			this.tableLayoutPanelFields.Controls.Add(this.textBoxExt1, 1, 0);
			this.tableLayoutPanelFields.Controls.Add(this.labelName, 0, 0);
			this.tableLayoutPanelFields.Controls.Add(this.checkBoxInclude, 1, 1);
			this.tableLayoutPanelFields.Controls.Add(this.buttonAdvSave, 2, 2);
			this.tableLayoutPanelFields.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelFields.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelFields.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
			this.tableLayoutPanelFields.Name = "tableLayoutPanelFields";
			this.tableLayoutPanelFields.RowCount = 2;
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelFields.Size = new System.Drawing.Size(441, 183);
			this.tableLayoutPanelFields.TabIndex = 1;
			// 
			// textBoxExt1
			// 
			this.textBoxExt1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxExt1.BackColor = System.Drawing.Color.White;
			this.textBoxExt1.BeforeTouchSize = new System.Drawing.Size(307, 25);
			this.textBoxExt1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExt1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tableLayoutPanelFields.SetColumnSpan(this.textBoxExt1, 3);
			this.textBoxExt1.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExt1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.textBoxExt1.Location = new System.Drawing.Point(122, 17);
			this.textBoxExt1.Margin = new System.Windows.Forms.Padding(3, 17, 3, 3);
			this.textBoxExt1.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExt1.Name = "textBoxExt1";
			this.textBoxExt1.OverflowIndicatorToolTipText = null;
			this.textBoxExt1.Size = new System.Drawing.Size(307, 25);
			this.textBoxExt1.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExt1.TabIndex = 0;
			this.textBoxExt1.TextChanged += new System.EventHandler(this.textBoxExt1_TextChanged);
			// 
			// labelName
			// 
			this.labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelName.AutoSize = true;
			this.labelName.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.labelName.Location = new System.Drawing.Point(3, 19);
			this.labelName.Margin = new System.Windows.Forms.Padding(3, 14, 3, 0);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(89, 17);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "xxNameColon";
			this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkBoxInclude
			// 
			this.checkBoxInclude.AutoSize = true;
			this.checkBoxInclude.Checked = true;
			this.checkBoxInclude.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tableLayoutPanelFields.SetColumnSpan(this.checkBoxInclude, 3);
			this.checkBoxInclude.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.checkBoxInclude.Location = new System.Drawing.Point(122, 48);
			this.checkBoxInclude.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.checkBoxInclude.Name = "checkBoxInclude";
			this.checkBoxInclude.Size = new System.Drawing.Size(203, 21);
			this.checkBoxInclude.TabIndex = 1;
			this.checkBoxInclude.Text = "xxIncludedInRequestAllowance";
			this.checkBoxInclude.UseVisualStyleBackColor = true;
			this.checkBoxInclude.Visible = false;
			// 
			// errorProvider
			// 
			this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider.ContainerControl = this;
			// 
			// SetEfficiencyShrinkageForm
			// 
			this.AcceptButton = this.buttonAdvSave;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(441, 183);
			this.Controls.Add(this.tableLayoutPanelFields);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SetEfficiencyShrinkageForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxUpdateEfficiencyShrinkageRow";
			this.TopMost = true;
			this.tableLayoutPanelFields.ResumeLayout(false);
			this.tableLayoutPanelFields.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExt1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.Label labelName;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExt1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFields;
        private System.Windows.Forms.CheckBox checkBoxInclude;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSave;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}