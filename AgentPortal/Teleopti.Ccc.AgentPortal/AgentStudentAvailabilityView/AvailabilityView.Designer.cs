using Teleopti.Ccc.AgentPortal.AgentPreferenceView;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView
{
    partial class AvailabilityView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AvailabilityView));
            this.gridControl1 = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.contextMenuStripStudentAvailability = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemCut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.autoLabelPeriodInformation = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.editStudentAvailabilityView = new Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.EditStudentAvailabilityView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.contextMenuStripStudentAvailability.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridControl1.ColCount = 7;
            this.gridControl1.ContextMenuStrip = this.contextMenuStripStudentAvailability;
            resources.ApplyResources(this.gridControl1, "gridControl1");
            this.gridControl1.ExcelLikeCurrentCell = true;
            this.gridControl1.ExcelLikeSelectionFrame = true;
            this.gridControl1.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridControl1.HScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Automatic;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.NumberedColHeaders = false;
            this.gridControl1.NumberedRowHeaders = false;
            this.gridControl1.Office2007ScrollBars = true;
            this.gridControl1.Properties.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.gridControl1.ResizeColsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
            this.gridControl1.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
            this.gridControl1.RowCount = 1;
            this.gridControl1.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControl1.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.AlwaysVisible;
            this.gridControl1.SmartSizeBox = false;
            this.gridControl1.ThemesEnabled = true;
            this.gridControl1.UseRightToLeftCompatibleTextBox = true;
            this.gridControl1.VScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Automatic;
            this.gridControl1.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.gridControl1_QueryCellInfo);
            // 
            // contextMenuStripStudentAvailability
            // 
            this.contextMenuStripStudentAvailability.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCut,
            this.toolStripMenuItemCopy,
            this.toolStripMenuItemPaste,
            this.toolStripMenuItemDelete});
            this.contextMenuStripStudentAvailability.Name = "contextMenuStripStudentAvailability";
            resources.ApplyResources(this.contextMenuStripStudentAvailability, "contextMenuStripStudentAvailability");
            // 
            // toolStripMenuItemCut
            // 
            this.toolStripMenuItemCut.Name = "toolStripMenuItemCut";
            resources.ApplyResources(this.toolStripMenuItemCut, "toolStripMenuItemCut");
            // 
            // toolStripMenuItemCopy
            // 
            this.toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
            resources.ApplyResources(this.toolStripMenuItemCopy, "toolStripMenuItemCopy");
            // 
            // toolStripMenuItemPaste
            // 
            resources.ApplyResources(this.toolStripMenuItemPaste, "toolStripMenuItemPaste");
            this.toolStripMenuItemPaste.Name = "toolStripMenuItemPaste";
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            resources.ApplyResources(this.toolStripMenuItemDelete, "toolStripMenuItemDelete");
            // 
            // autoLabelPeriodInformation
            // 
            resources.ApplyResources(this.autoLabelPeriodInformation, "autoLabelPeriodInformation");
            this.autoLabelPeriodInformation.Name = "autoLabelPeriodInformation";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.gridControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.editStudentAvailabilityView, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // editStudentAvailabilityView
            // 
            resources.ApplyResources(this.editStudentAvailabilityView, "editStudentAvailabilityView");
            this.editStudentAvailabilityView.EndTimeLimitation = System.TimeSpan.Parse("00:00:00");
            this.editStudentAvailabilityView.EndTimeLimitationErrorMessage = "";
            this.editStudentAvailabilityView.EndTimeLimitationNextDay = false;
            this.editStudentAvailabilityView.Name = "editStudentAvailabilityView";
            this.editStudentAvailabilityView.SaveButtonEnabled = true;
            this.editStudentAvailabilityView.SecondEndTimeLimitation = System.TimeSpan.Parse("00:00:00");
            this.editStudentAvailabilityView.SecondEndTimeLimitationErrorMessage = "";
            this.editStudentAvailabilityView.SecondEndTimeLimitationNextDay = false;
            this.editStudentAvailabilityView.SecondStartTimeLimitation = System.TimeSpan.Parse("00:00:00");
            this.editStudentAvailabilityView.SecondStartTimeLimitationErrorMessage = "";
            this.editStudentAvailabilityView.StartTimeLimitation = System.TimeSpan.Parse("00:00:00");
            this.editStudentAvailabilityView.StartTimeLimitationErrorMessage = "";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.pictureBox1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.autoLabelPeriodInformation, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelInfo, 0, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.tableLayoutPanel2.SetRowSpan(this.pictureBox1, 3);
            this.pictureBox1.TabStop = false;
            // 
            // labelInfo
            // 
            resources.ApplyResources(this.labelInfo, "labelInfo");
            this.labelInfo.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelInfo.Name = "labelInfo";
            // 
            // StudentAvailabilityView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AvailabilityView";
            this.Load += new System.EventHandler(this.StudentAvailabilityView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            this.contextMenuStripStudentAvailability.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Grid.GridControl gridControl1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripStudentAvailability;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopy;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCut;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPaste;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelPeriodInformation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label labelInfo;
        private EditStudentAvailabilityView editStudentAvailabilityView;
    }
}
