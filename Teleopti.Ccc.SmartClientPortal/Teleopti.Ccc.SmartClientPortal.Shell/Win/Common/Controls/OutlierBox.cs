using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    public class OutlierBox : BaseUserControl
    {
        private Syncfusion.Windows.Forms.Tools.ContextMenuStripEx contextMenuStripExOutlier;
        private System.Windows.Forms.ToolStripMenuItem xxEditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xxRemoveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xxAddOutlierThreeDotsToolStripMenuItem;
        private Syncfusion.Windows.Forms.Tools.SuperToolTip superToolTipOutliers;
        private System.Windows.Forms.ListBox listBoxOutliers;
    
        public OutlierBox()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public void SetOutliers(IList<IOutlier> outliers)
        {
            listBoxOutliers.Items.Clear();
            foreach (IOutlier outlier in outliers)
            {
                ListBoxItem<IOutlier> listBoxItem = new ListBoxItem<IOutlier>(outlier, outlier.Description.Name);              
                listBoxOutliers.Items.Add(listBoxItem);               
            }
        }

        private void InitializeComponent()
        {
this.listBoxOutliers = new System.Windows.Forms.ListBox();
this.contextMenuStripExOutlier = new Syncfusion.Windows.Forms.Tools.ContextMenuStripEx();
this.xxEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
this.xxRemoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
this.xxAddOutlierThreeDotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
this.superToolTipOutliers = new Syncfusion.Windows.Forms.Tools.SuperToolTip(null);
this.contextMenuStripExOutlier.SuspendLayout();
this.SuspendLayout();
// 
// listBoxOutliers
// 
this.listBoxOutliers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
this.listBoxOutliers.ContextMenuStrip = this.contextMenuStripExOutlier;
this.listBoxOutliers.Dock = System.Windows.Forms.DockStyle.Fill;
this.listBoxOutliers.FormattingEnabled = true;
this.listBoxOutliers.Location = new System.Drawing.Point(0, 0);
this.listBoxOutliers.Name = "listBoxOutliers";
this.listBoxOutliers.Size = new System.Drawing.Size(150, 150);
this.listBoxOutliers.TabIndex = 0;
this.listBoxOutliers.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listBoxOutliers_MouseMove);
// 
// contextMenuStripExOutlier
// 
this.contextMenuStripExOutlier.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xxEditToolStripMenuItem,
            this.xxRemoveToolStripMenuItem,
            this.xxAddOutlierThreeDotsToolStripMenuItem});
this.contextMenuStripExOutlier.Name = "contextMenuStripExOutlier";
this.contextMenuStripExOutlier.Size = new System.Drawing.Size(197, 70);
// 
// xxEditToolStripMenuItem
// 
this.xxEditToolStripMenuItem.Name = "xxEditToolStripMenuItem";
this.xxEditToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
this.xxEditToolStripMenuItem.Text = "xxEdit";
this.xxEditToolStripMenuItem.Click += new System.EventHandler(this.xxEditToolStripMenuItem_Click);
// 
// xxRemoveToolStripMenuItem
// 
this.xxRemoveToolStripMenuItem.Name = "xxRemoveToolStripMenuItem";
this.xxRemoveToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
this.xxRemoveToolStripMenuItem.Text = "xxDelete";
this.xxRemoveToolStripMenuItem.Click += new System.EventHandler(this.xxRemoveToolStripMenuItem_Click);
// 
// xxAddOutlierThreeDotsToolStripMenuItem
// 
this.xxAddOutlierThreeDotsToolStripMenuItem.Name = "xxAddOutlierThreeDotsToolStripMenuItem";
this.xxAddOutlierThreeDotsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
this.xxAddOutlierThreeDotsToolStripMenuItem.Text = "xxAddOutlierThreeDots";
this.xxAddOutlierThreeDotsToolStripMenuItem.Click += new System.EventHandler(this.xxAddOutlierThreeDotsToolStripMenuItem_Click);
// 
// OutlierBox
// 
this.Controls.Add(this.listBoxOutliers);
this.Name = "OutlierBox";
this.contextMenuStripExOutlier.ResumeLayout(false);
this.ResumeLayout(false);

        }

        private void xxEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxOutliers.SelectedItem == null) return;
            IOutlier outlier = ((ListBoxItem<IOutlier>)listBoxOutliers.SelectedItem).Value;

        	var handler = UpdateOutlier;
            if (handler != null)
            {
                handler.Invoke(this, new CustomEventArgs<IOutlier>(outlier));
            }
        }

        public event EventHandler<CustomEventArgs<DateOnly>> AddOutlier;
        public event EventHandler<CustomEventArgs<IOutlier>> DeleteOutlier;
        public event EventHandler<CustomEventArgs<IOutlier>> UpdateOutlier;

        private void xxRemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxOutliers.SelectedItem == null) return;

            IOutlier outlier = ((ListBoxItem<IOutlier>)listBoxOutliers.SelectedItem).Value;
        	var handler = DeleteOutlier;
            if (handler!=null)
            {
                handler.Invoke(this,new CustomEventArgs<IOutlier>(outlier));
            }

            listBoxOutliers.Items.Remove(listBoxOutliers.SelectedItem);
        }

        private void xxAddOutlierThreeDotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        	var handler = AddOutlier;
            if (handler != null)
            {
                handler.Invoke(this, new CustomEventArgs<DateOnly>(DateOnly.Today));
            }
        }

        private void listBoxOutliers_MouseMove(object sender, MouseEventArgs e)
        {
             var listBox = (ListBox)sender;
                var point = new Point(e.X, e.Y);
                int hoverIndex = listBox.IndexFromPoint(point);
                if (hoverIndex >= 0 && hoverIndex < listBox.Items.Count)
                {
                    var info = new ToolTipInfo();
                    var item = ((ListBoxItem<IOutlier>) listBox.Items[hoverIndex]);

					info.Body.Text = string.Join("\n", item.Value.Dates.OrderBy(d => d.Date).Select(d => d.ToShortDateString(CultureInfo.CurrentCulture)));
                    info.Header.Text = item.DisplayText;
                    var font = new Font(info.Header.Font.FontFamily, info.Header.Font.Size,FontStyle.Bold);
                    info.Header.Font = font;
                    superToolTipOutliers.SetToolTip(listBox, info);
                }
        }
    }
}
