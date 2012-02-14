namespace Teleopti.Ccc.AgentPortal.AgentScheduleMessenger
{
    partial class LayerVisualizer
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.gradientPanelTimeIndicator = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelTimeIndicator = new System.Windows.Forms.TableLayoutPanel();
            this.gradientPanelCurrentTime = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelTimeIndicator)).BeginInit();
            this.gradientPanelTimeIndicator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
            this.tableLayoutPanelTimeIndicator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelCurrentTime)).BeginInit();
            this.SuspendLayout();
            // 
            // toolTip1
            // 
            this.toolTip1.ShowAlways = true;
            // 
            // gradientPanelTimeIndicator
            // 
            this.gradientPanelTimeIndicator.BackColor = System.Drawing.Color.White;
            this.gradientPanelTimeIndicator.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.SystemColors.GradientActiveCaption, System.Drawing.SystemColors.HotTrack);
            this.gradientPanelTimeIndicator.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelTimeIndicator.Controls.Add(this.tableLayoutPanelTimeIndicator);
            this.gradientPanelTimeIndicator.Location = new System.Drawing.Point(0, 28);
            this.gradientPanelTimeIndicator.Margin = new System.Windows.Forms.Padding(0);
            this.gradientPanelTimeIndicator.Name = "gradientPanelTimeIndicator";
            this.gradientPanelTimeIndicator.Size = new System.Drawing.Size(201, 8);
            this.gradientPanelTimeIndicator.TabIndex = 3;
            // 
            // gradientPanelHeader
            // 
            this.gradientPanelHeader.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.White, System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224))))));
            this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
            this.gradientPanelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.gradientPanelHeader.Name = "gradientPanelHeader";
            this.gradientPanelHeader.Size = new System.Drawing.Size(396, 20);
            this.gradientPanelHeader.TabIndex = 4;
            this.gradientPanelHeader.Paint += new System.Windows.Forms.PaintEventHandler(this.gradientPanelHeader_Paint);
            // 
            // tableLayoutPanelTimeIndicator
            // 
            this.tableLayoutPanelTimeIndicator.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelTimeIndicator.ColumnCount = 2;
            this.tableLayoutPanelTimeIndicator.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTimeIndicator.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 3F));
            this.tableLayoutPanelTimeIndicator.Controls.Add(this.gradientPanelCurrentTime, 1, 0);
            this.tableLayoutPanelTimeIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelTimeIndicator.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelTimeIndicator.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelTimeIndicator.Name = "tableLayoutPanelTimeIndicator";
            this.tableLayoutPanelTimeIndicator.RowCount = 1;
            this.tableLayoutPanelTimeIndicator.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTimeIndicator.Size = new System.Drawing.Size(201, 8);
            this.tableLayoutPanelTimeIndicator.TabIndex = 0;
            // 
            // gradientPanelCurrentTime
            // 
            this.gradientPanelCurrentTime.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.White, System.Drawing.SystemColors.Highlight);
            this.gradientPanelCurrentTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelCurrentTime.Location = new System.Drawing.Point(198, 0);
            this.gradientPanelCurrentTime.Margin = new System.Windows.Forms.Padding(0);
            this.gradientPanelCurrentTime.Name = "gradientPanelCurrentTime";
            this.gradientPanelCurrentTime.Size = new System.Drawing.Size(3, 8);
            this.gradientPanelCurrentTime.TabIndex = 0;
            // 
            // LayerVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.gradientPanelHeader);
            this.Controls.Add(this.gradientPanelTimeIndicator);
            this.Name = "LayerVisualizer";
            this.Size = new System.Drawing.Size(396, 45);
            this.RightToLeftChanged += new System.EventHandler(this.LayerVisualizer_RightToLeftChanged);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelTimeIndicator)).EndInit();
            this.gradientPanelTimeIndicator.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
            this.tableLayoutPanelTimeIndicator.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelCurrentTime)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelTimeIndicator;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTimeIndicator;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelCurrentTime;
    }
}