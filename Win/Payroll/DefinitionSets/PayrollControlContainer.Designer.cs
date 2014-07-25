namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    partial class PayrollControlContainer
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonDelete = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAddNew = new Syncfusion.Windows.Forms.ButtonAdv();
            this.labelDefinitionTypeText = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(485, 155);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel4.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel4.Controls.Add(this.buttonDelete, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonAddNew, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.labelDefinitionTypeText, 0, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(479, 34);
            this.tableLayoutPanel4.TabIndex = 18;
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonDelete.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonDelete.BackColor = System.Drawing.Color.White;
            this.buttonDelete.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonDelete.ForeColor = System.Drawing.Color.White;
            this.buttonDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_temp_DeleteGroup10;
            this.buttonDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDelete.IsBackStageButton = false;
            this.buttonDelete.Location = new System.Drawing.Point(438, 3);
            this.buttonDelete.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(28, 28);
            this.buttonDelete.TabIndex = 1;
            this.buttonDelete.TabStop = false;
            this.buttonDelete.UseVisualStyle = true;
            this.buttonDelete.UseVisualStyleBackColor = false;
            this.buttonDelete.Click += new System.EventHandler(this.buttonAdvDeleteClick);
            // 
            // buttonAddNew
            // 
            this.buttonAddNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAddNew.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAddNew.BackColor = System.Drawing.Color.White;
            this.buttonAddNew.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonAddNew.ForeColor = System.Drawing.Color.White;
            this.buttonAddNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.buttonAddNew.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAddNew.IsBackStageButton = false;
            this.buttonAddNew.Location = new System.Drawing.Point(397, 3);
            this.buttonAddNew.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.buttonAddNew.Name = "buttonAddNew";
            this.buttonAddNew.Size = new System.Drawing.Size(28, 28);
            this.buttonAddNew.TabIndex = 0;
            this.buttonAddNew.UseVisualStyle = true;
            this.buttonAddNew.UseVisualStyleBackColor = false;
            this.buttonAddNew.Click += new System.EventHandler(this.buttonNewClick);
            // 
            // labelDefinitionTypeText
            // 
            this.labelDefinitionTypeText.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelDefinitionTypeText.AutoSize = true;
            this.labelDefinitionTypeText.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDefinitionTypeText.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelDefinitionTypeText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelDefinitionTypeText.Location = new System.Drawing.Point(3, 8);
            this.labelDefinitionTypeText.Name = "labelDefinitionTypeText";
            this.labelDefinitionTypeText.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelDefinitionTypeText.Size = new System.Drawing.Size(117, 17);
            this.labelDefinitionTypeText.TabIndex = 2;
            this.labelDefinitionTypeText.Text = "xxDefinitionText";
            // 
            // PayrollControlContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "PayrollControlContainer";
            this.Size = new System.Drawing.Size(485, 155);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private Syncfusion.Windows.Forms.ButtonAdv buttonDelete;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAddNew;
        private System.Windows.Forms.Label labelDefinitionTypeText;
    }
}
