namespace SdkTestWinGui
{
    partial class UpdatePersonAccountForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelTracker = new System.Windows.Forms.Label();
            this.labelBalanceIn = new System.Windows.Forms.Label();
            this.labelAccrued = new System.Windows.Forms.Label();
            this.textBoxExtra = new System.Windows.Forms.TextBox();
            this.textBoxAccrued = new System.Windows.Forms.TextBox();
            this.textBoxBalanceIn = new System.Windows.Forms.TextBox();
            this.dateTimePickerDateFrom = new System.Windows.Forms.DateTimePicker();
            this.comboBoxTracker = new System.Windows.Forms.ComboBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelDateFrom = new System.Windows.Forms.Label();
            this.labelExtra = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 79F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 97F));
            this.tableLayoutPanel1.Controls.Add(this.labelTracker, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelBalanceIn, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelAccrued, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxExtra, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxAccrued, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxBalanceIn, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.dateTimePickerDateFrom, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxTracker, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonSave, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelDateFrom, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelExtra, 3, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 51.28205F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 48.71795F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(504, 86);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // labelTracker
            // 
            this.labelTracker.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelTracker.Location = new System.Drawing.Point(30, 8);
            this.labelTracker.Name = "labelTracker";
            this.labelTracker.Size = new System.Drawing.Size(53, 11);
            this.labelTracker.TabIndex = 0;
            this.labelTracker.Text = "Tracker";
            // 
            // labelBalanceIn
            // 
            this.labelBalanceIn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelBalanceIn.Location = new System.Drawing.Point(249, 7);
            this.labelBalanceIn.Name = "labelBalanceIn";
            this.labelBalanceIn.Size = new System.Drawing.Size(37, 13);
            this.labelBalanceIn.TabIndex = 0;
            this.labelBalanceIn.Text = "BalanceIn";
            // 
            // labelAccrued
            // 
            this.labelAccrued.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelAccrued.Location = new System.Drawing.Point(440, 7);
            this.labelAccrued.Name = "labelAccrued";
            this.labelAccrued.Size = new System.Drawing.Size(31, 13);
            this.labelAccrued.TabIndex = 0;
            this.labelAccrued.Text = "Accrued";
            // 
            // textBoxExtra
            // 
            this.textBoxExtra.Location = new System.Drawing.Point(310, 30);
            this.textBoxExtra.Name = "textBoxExtra";
            this.textBoxExtra.Size = new System.Drawing.Size(94, 20);
            this.textBoxExtra.TabIndex = 3;
            // 
            // textBoxAccrued
            // 
            this.textBoxAccrued.Location = new System.Drawing.Point(410, 30);
            this.textBoxAccrued.Name = "textBoxAccrued";
            this.textBoxAccrued.Size = new System.Drawing.Size(91, 20);
            this.textBoxAccrued.TabIndex = 4;
            // 
            // textBoxBalanceIn
            // 
            this.textBoxBalanceIn.Location = new System.Drawing.Point(231, 30);
            this.textBoxBalanceIn.Name = "textBoxBalanceIn";
            this.textBoxBalanceIn.Size = new System.Drawing.Size(73, 20);
            this.textBoxBalanceIn.TabIndex = 2;
            // 
            // dateTimePickerDateFrom
            // 
            this.dateTimePickerDateFrom.Location = new System.Drawing.Point(117, 30);
            this.dateTimePickerDateFrom.Name = "dateTimePickerDateFrom";
            this.dateTimePickerDateFrom.Size = new System.Drawing.Size(108, 20);
            this.dateTimePickerDateFrom.TabIndex = 1;
            // 
            // comboBoxTracker
            // 
            this.comboBoxTracker.FormattingEnabled = true;
            this.comboBoxTracker.Location = new System.Drawing.Point(3, 30);
            this.comboBoxTracker.Name = "comboBoxTracker";
            this.comboBoxTracker.Size = new System.Drawing.Size(108, 21);
            this.comboBoxTracker.TabIndex = 0;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(310, 56);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 5;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(410, 56);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelDateFrom
            // 
            this.labelDateFrom.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelDateFrom.Location = new System.Drawing.Point(144, 7);
            this.labelDateFrom.Name = "labelDateFrom";
            this.labelDateFrom.Size = new System.Drawing.Size(54, 13);
            this.labelDateFrom.TabIndex = 0;
            this.labelDateFrom.Text = "Date from";
            // 
            // labelExtra
            // 
            this.labelExtra.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelExtra.Location = new System.Drawing.Point(335, 7);
            this.labelExtra.Name = "labelExtra";
            this.labelExtra.Size = new System.Drawing.Size(44, 13);
            this.labelExtra.TabIndex = 0;
            this.labelExtra.Text = "Extra";
            // 
            // UpdatePersonAccountForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 86);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdatePersonAccountForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "UpdatePersonAccountForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelTracker;
        private System.Windows.Forms.Label labelDateFrom;
        private System.Windows.Forms.Label labelBalanceIn;
        private System.Windows.Forms.Label labelExtra;
        private System.Windows.Forms.Label labelAccrued;
        private System.Windows.Forms.TextBox textBoxExtra;
        private System.Windows.Forms.TextBox textBoxAccrued;
        private System.Windows.Forms.TextBox textBoxBalanceIn;
        private System.Windows.Forms.DateTimePicker dateTimePickerDateFrom;
        private System.Windows.Forms.ComboBox comboBoxTracker;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
    }
}