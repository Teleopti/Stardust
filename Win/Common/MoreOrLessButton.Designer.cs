namespace Teleopti.Ccc.Win.Common
{
    partial class MoreOrLessButton
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
            this.buttonMoreOrLess = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonMoreOrLess
            // 
            this.buttonMoreOrLess.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMoreOrLess.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonMoreOrLess.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonMoreOrLess.Location = new System.Drawing.Point(0, 0);
            this.buttonMoreOrLess.Margin = new System.Windows.Forms.Padding(0);
            this.buttonMoreOrLess.Name = "buttonMoreOrLess";
            this.buttonMoreOrLess.Size = new System.Drawing.Size(70, 19);
            this.buttonMoreOrLess.TabIndex = 11;
            this.buttonMoreOrLess.Text = "xxAdvanced";
            this.buttonMoreOrLess.UseVisualStyleBackColor = true;
            this.buttonMoreOrLess.Click += new System.EventHandler(this.buttonMoreOrLess_Click);
            // 
            // MoreOrLessButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonMoreOrLess);
            this.Name = "MoreOrLessButton";
            this.Size = new System.Drawing.Size(70, 19);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonMoreOrLess;
    }
}