namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class OptionDialog
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
                if (components != null) components.Dispose();
                if (Core != null) Core.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionDialog));
            this.gradientPanelBottom = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.ribbonControlAdvForm = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelBottom)).BeginInit();
            this.gradientPanelBottom.SuspendLayout();
            this.tableLayoutPanelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvForm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
            this.SuspendLayout();
            // 
            // gradientPanelBottom
            // 
            this.gradientPanelBottom.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelBottom.Controls.Add(this.tableLayoutPanelBottom);
            this.gradientPanelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gradientPanelBottom.Location = new System.Drawing.Point(6, 149);
            this.gradientPanelBottom.Name = "gradientPanelBottom";
            this.gradientPanelBottom.Size = new System.Drawing.Size(341, 43);
            this.gradientPanelBottom.TabIndex = 4;
            // 
            // tableLayoutPanelBottom
            // 
            this.tableLayoutPanelBottom.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelBottom.ColumnCount = 4;
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelBottom.Controls.Add(this.buttonAdvOK, 1, 0);
            this.tableLayoutPanelBottom.Controls.Add(this.buttonAdvCancel, 3, 0);
            this.tableLayoutPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelBottom.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
            this.tableLayoutPanelBottom.Padding = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanelBottom.RowCount = 1;
            this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBottom.Size = new System.Drawing.Size(341, 43);
            this.tableLayoutPanelBottom.TabIndex = 4;
            // 
            // buttonAdvOK
            // 
            this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvOK.Location = new System.Drawing.Point(177, 8);
            this.buttonAdvOK.Name = "buttonAdvOK";
            this.buttonAdvOK.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.buttonAdvOK.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvOK.TabIndex = 0;
            this.buttonAdvOK.Text = "xxOk";
            this.buttonAdvOK.UseVisualStyle = true;
            this.buttonAdvOK.Click += new System.EventHandler(this.ButtonAdvOkClick);
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvCancel.CausesValidation = false;
            this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvCancel.Location = new System.Drawing.Point(258, 8);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvCancel.TabIndex = 1;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            // 
            // ribbonControlAdvForm
            // 
            this.ribbonControlAdvForm.CaptionFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControlAdvForm.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdvForm.MenuButtonVisible = false;
            this.ribbonControlAdvForm.Name = "ribbonControlAdvForm";
            // 
            // ribbonControlAdvForm.OfficeMenu
            // 
            this.ribbonControlAdvForm.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdvForm.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdvForm.QuickPanelVisible = false;
            this.ribbonControlAdvForm.ShowContextMenu = false;
            this.ribbonControlAdvForm.ShowLauncher = false;
            this.ribbonControlAdvForm.Size = new System.Drawing.Size(351, 33);
            this.ribbonControlAdvForm.SystemText.QuickAccessDialogDropDownName = "xxStartMenu";
            this.ribbonControlAdvForm.TabIndex = 5;
            this.ribbonControlAdvForm.Text = "ribbonControlAdv1";
            this.ribbonControlAdvForm.TitleFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // gradientPanel1
            // 
            this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanel1.Location = new System.Drawing.Point(6, 34);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Size = new System.Drawing.Size(341, 115);
            this.gradientPanel1.TabIndex = 6;
            // 
            // OptionDialog
            // 
            this.AcceptButton = this.buttonAdvOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAdvCancel;
            this.ClientSize = new System.Drawing.Size(353, 198);
            this.Controls.Add(this.gradientPanel1);
            this.Controls.Add(this.ribbonControlAdvForm);
            this.Controls.Add(this.gradientPanelBottom);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxOptions";
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelBottom)).EndInit();
            this.gradientPanelBottom.ResumeLayout(false);
            this.tableLayoutPanelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvForm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelBottom;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottom;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdvForm;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;

    }
}