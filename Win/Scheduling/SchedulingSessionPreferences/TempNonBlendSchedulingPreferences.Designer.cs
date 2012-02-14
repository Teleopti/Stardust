namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    partial class TempNonBlendSchedulingPreferences
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Skillens"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "gruppering"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "från"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "activitet"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TempNonBlendSchedulingPreferences"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Behov"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
        private void InitializeComponent()
        {
            this.comboBoxGrouping = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.integerTextBox1 = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxActivities = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxGrouping
            // 
            this.comboBoxGrouping.FormattingEnabled = true;
            this.comboBoxGrouping.Location = new System.Drawing.Point(131, 6);
            this.comboBoxGrouping.Name = "comboBoxGrouping";
            this.comboBoxGrouping.Size = new System.Drawing.Size(231, 21);
            this.comboBoxGrouping.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Skill från gruppering:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(304, 312);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "ok";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Behov:";
            // 
            // integerTextBox1
            // 
            this.integerTextBox1.IntegerValue = ((long)(20));
            this.integerTextBox1.Location = new System.Drawing.Point(131, 73);
            this.integerTextBox1.MaxValue = ((long)(10000));
            this.integerTextBox1.MinValue = ((long)(1));
            this.integerTextBox1.Name = "integerTextBox1";
            this.integerTextBox1.NullString = "0";
            this.integerTextBox1.Size = new System.Drawing.Size(100, 20);
            this.integerTextBox1.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Skillens activitet:";
            // 
            // comboBoxActivities
            // 
            this.comboBoxActivities.FormattingEnabled = true;
            this.comboBoxActivities.Location = new System.Drawing.Point(131, 29);
            this.comboBoxActivities.Name = "comboBoxActivities";
            this.comboBoxActivities.Size = new System.Drawing.Size(231, 21);
            this.comboBoxActivities.TabIndex = 6;
            // 
            // TempNonBlendSchedulingPreferences
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 347);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxActivities);
            this.Controls.Add(this.integerTextBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxGrouping);
            this.Name = "TempNonBlendSchedulingPreferences";
            this.Text = "TempNonBlendSchedulingPreferences";
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxGrouping;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxActivities;
    }
}