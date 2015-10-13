namespace Teleopti.Ccc.Win.SmartParts.Forecasting
{
    partial class NavigationControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NavigationControl));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonNext = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonPrevious = new Syncfusion.Windows.Forms.ButtonAdv();
            this.autoLabelYear = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.buttonNext, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonPrevious, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.autoLabelYear, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(200, 12);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // buttonNext
            // 
            this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNext.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.WindowsXP;
            this.buttonNext.BackColor = System.Drawing.Color.White;
            this.buttonNext.BorderStyleAdv = Syncfusion.Windows.Forms.ButtonAdvBorderStyle.None;
            this.buttonNext.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.buttonNext.FlatAppearance.BorderSize = 0;
            this.buttonNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonNext.Image = ((System.Drawing.Image)(resources.GetObject("buttonNext.Image")));
            this.buttonNext.Location = new System.Drawing.Point(186, 0);
            this.buttonNext.Margin = new System.Windows.Forms.Padding(0);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(14, 12);
            this.buttonNext.TabIndex = 5;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonPrevious
            // 
            this.buttonPrevious.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.WindowsXP;
            this.buttonPrevious.BackColor = System.Drawing.Color.White;
            this.buttonPrevious.BorderStyleAdv = Syncfusion.Windows.Forms.ButtonAdvBorderStyle.None;
            this.buttonPrevious.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.buttonPrevious.FlatAppearance.BorderSize = 0;
            this.buttonPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPrevious.Image = ((System.Drawing.Image)(resources.GetObject("buttonPrevious.Image")));
            this.buttonPrevious.Location = new System.Drawing.Point(0, 0);
            this.buttonPrevious.Margin = new System.Windows.Forms.Padding(0);
            this.buttonPrevious.Name = "buttonPrevious";
            this.buttonPrevious.Size = new System.Drawing.Size(14, 12);
            this.buttonPrevious.TabIndex = 4;
            this.buttonPrevious.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.buttonPrevious.Click += new System.EventHandler(this.buttonPrevious_Click);
            // 
            // autoLabelYear
            // 
            this.autoLabelYear.BackColor = System.Drawing.Color.White;
            this.autoLabelYear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.autoLabelYear.DX = 0;
            this.autoLabelYear.DY = 0;
            this.autoLabelYear.Location = new System.Drawing.Point(69, 0);
            this.autoLabelYear.Name = "autoLabelYear";
            this.autoLabelYear.Size = new System.Drawing.Size(60, 12);
            this.autoLabelYear.TabIndex = 6;
            this.autoLabelYear.Text = "autoLabel1";
            // 
            // NavigationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "NavigationControl";
            this.Size = new System.Drawing.Size(200, 12);
            this.Load += new System.EventHandler(this.NavigationControl_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonPrevious;
        private Syncfusion.Windows.Forms.ButtonAdv buttonNext;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelYear;


    }
}