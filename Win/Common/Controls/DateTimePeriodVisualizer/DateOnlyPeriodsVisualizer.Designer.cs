namespace Teleopti.Ccc.Win.Common.Controls.DateTimePeriodVisualizer
{
    partial class DateOnlyPeriodsVisualizer
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
            this.gradientLabelColumnHeader = new DateOnlyPeriodVisualizerHeader();
            this.gradientLabelTextTemplate = new Syncfusion.Windows.Forms.Tools.GradientLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                                  | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.gradientLabelColumnHeader, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gradientLabelTextTemplate, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(607, 20);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // gradientLabelColumnHeader
            // 
            this.gradientLabelColumnHeader.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(240)))), ((int)(((byte)(247))))), System.Drawing.Color.LightSkyBlue);
            this.gradientLabelColumnHeader.BorderSides = System.Windows.Forms.Border3DSide.Top;
            this.gradientLabelColumnHeader.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.gradientLabelColumnHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientLabelColumnHeader.Location = new System.Drawing.Point(102, 1);
            this.gradientLabelColumnHeader.Margin = new System.Windows.Forms.Padding(0);
            this.gradientLabelColumnHeader.Name = "gradientLabelColumnHeader";
            this.gradientLabelColumnHeader.Size = new System.Drawing.Size(504, 18);
            this.gradientLabelColumnHeader.TabIndex = 1;
            this.gradientLabelColumnHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gradientLabelTextTemplate
            // 
            this.gradientLabelTextTemplate.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(240)))), ((int)(((byte)(247))))), System.Drawing.Color.LightSkyBlue);
            this.gradientLabelTextTemplate.BorderSides = System.Windows.Forms.Border3DSide.Top;
            this.gradientLabelTextTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientLabelTextTemplate.Location = new System.Drawing.Point(1, 1);
            this.gradientLabelTextTemplate.Margin = new System.Windows.Forms.Padding(0);
            this.gradientLabelTextTemplate.Name = "gradientLabelTextTemplate";
            this.gradientLabelTextTemplate.Size = new System.Drawing.Size(100, 18);
            this.gradientLabelTextTemplate.TabIndex = 0;
            this.gradientLabelTextTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DateOnlyPeriodsVisualizer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Name = "DateOnlyPeriodsVisualizer";
            this.Size = new System.Drawing.Size(613, 95);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.GradientLabel gradientLabelTextTemplate;
        private DateOnlyPeriodVisualizerHeader gradientLabelColumnHeader;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}