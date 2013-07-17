using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using Teleopti.Common.UI.OutlookControls;
using Teleopti.Common.UI.OutlookControls.Workspaces;

namespace Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces
{
    public partial class OutlookBarWorkspace : UserControl
    {
        private static readonly OutlookBarInfo nullSmartPartInfo = new OutlookBarInfo("Item", null);

        private readonly Dictionary<UserControl, ToolStripButton> _contentUC;
        private readonly Dictionary<ToolStripButton, ToolStripButton> _buttons;
        private readonly Dictionary<string, string> _topics = new Dictionary<string, string>();
        private int _visibleCount = -1;
        private int _lastCount = -1;
        private int _maxHeight;

        public Image ToggleButtonImage
        {
            get
            {
                ToolStripButton htsb = (ToolStripButton)headerStrip2.Items[1];
                return htsb.Image;
            }
            set
            {
                ToolStripButton htsb = (ToolStripButton)headerStrip2.Items[1];
                htsb.Image = value;
            }
        }

        public bool IsCollapsed { get; set; }

        public bool IsRightToLeft { get; set; }

        public ReadOnlyCollection<UserControl> ContentUCCollection
        {
            get
            {
                List<UserControl> result = new List<UserControl>();
                foreach (UserControl ctrl in ContentPanel.Controls) result.Add(ctrl);
                return result.AsReadOnly();
            }
        }


        [Browsable(true),Localizable(true),DefaultValue("Header")]
        public string HeaderText
        {
            get { return _headerLabel.Text; }
            set
            {
                if (!IsCollapsed) _headerLabel.Text = value;
                else headerStrip2.Items[1].Text = value;

                _headerLabel.Text = value;
            }
        }

        public event EventHandler<EventArgs> ToolStripButtonClicked;
        public event EventHandler<EventArgs> ModuleButtonClicked;

        void tsb_Click(object sender, EventArgs e)
        {
            if (ToolStripButtonClicked != null)
            {
                ToolStripButtonClicked(sender, e);
            }
        }

        public OutlookBarWorkspace()
        {
            _contentUC = new Dictionary<UserControl, ToolStripButton>();
            _buttons = new Dictionary<ToolStripButton, ToolStripButton>();
            InitializeComponent();
            this.stackStrip.ItemHeightChanged += new EventHandler(OnStackStripItemHeightChanged);
            // Set Height
            UpdateSplitter();

            //Header Collapsinle button, to collapse Splitter.
            ToolStripButton tsb = new ToolStripButton(Properties.Resources.leftarrow);
            tsb.ToolTipText = Teleopti.Ccc.UserTexts.Resources.Hide;
            tsb.Alignment = ToolStripItemAlignment.Right;
            tsb.ImageTransparentColor = Color.Black;
            tsb.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsb.Click += new EventHandler(tsb_Click);
            headerStrip2.Items.Add(tsb);
            showFewerButtonsToolStripMenuItem.Text = Ccc.UserTexts.Resources.ShowLessPanels;
            showMoreButtonsToolStripMenuItem1.Text = Ccc.UserTexts.Resources.ShowMorePanels;
        }

        public void ToggleButton()
        {
            ToolStripButton htsb = (ToolStripButton)headerStrip2.Items[1];
            ToolStripButton htsbRtL = null;
            if (headerStrip2.Items.Count > 2)   
                htsbRtL = (ToolStripButton)headerStrip2.Items[2];
            if (!IsCollapsed)
            {
                BaseStackStrip bss = new BaseStackStrip();
                htsb.ToolTipText = Teleopti.Ccc.UserTexts.Resources.Show;
                bss.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
                bss.Dock = DockStyle.Left;
                bss.Width = 28;
                foreach (ToolStripItem tsi in stackStrip.Items)
                {
                    if (tsi.GetType() == typeof(ExtendedToolStripButton))
                    {
                        ExtendedToolStripButton tsb = (ExtendedToolStripButton)tsi;
                        ExtendedToolStripButton newTsb = new ExtendedToolStripButton();
                        newTsb.Image = tsb.Image;
                        newTsb.ImageTransparentColor = Color.Black;
                        newTsb.Alignment = ToolStripItemAlignment.Left;
                        newTsb.DisplayStyle = ToolStripItemDisplayStyle.Image;
                        newTsb.Text = tsb.Text;
                        newTsb.Tag = tsb.Tag;
                        newTsb.Click += new EventHandler(this.OnClick);
                        newTsb.EventName = tsb.EventName;
                        newTsb.Padding = new System.Windows.Forms.Padding(3);
                        newTsb.Margin = new System.Windows.Forms.Padding(3);
                        bss.Items.Add(newTsb);
                    }
                }

                htsb.Text = headerStrip2.Items[0].Text;
                headerStrip2.Items[0].Text = "";
                stackStripSplitter.Panel2.Controls.Add(bss);
                if (htsbRtL != null)
                htsbRtL.Visible = false;
                IsCollapsed = true;
            }
            else
            {
                htsb.ToolTipText = Teleopti.Ccc.UserTexts.Resources.Hide;
                headerStrip2.Items[0].Text = htsb.Text;
                stackStripSplitter.Panel2.Controls.RemoveAt(stackStripSplitter.Panel2.Controls.Count - 1);
                if(htsbRtL != null)
                    htsbRtL.Visible = true;
                IsCollapsed = false;
            }
        }

        public void Close(UserControl contentUC)
        {
            if (_contentUC.ContainsKey(contentUC))
            {
                this.stackStrip.Items.Remove(_contentUC[contentUC]);
                this.overflowStrip.Items.Remove(_buttons[_contentUC[contentUC]]);
                UpdateSplitter();
            }
            ContentPanel.Controls.Remove(contentUC);
        }

        public void Add(OutlookBarInfo outlookBarInfo)
        {
            OutlookBarInfo info = outlookBarInfo;
            if (info == null) info = nullSmartPartInfo;

            // Add new button; 
            ExtendedToolStripButton stackStripButton = new ExtendedToolStripButton();
            ExtendedToolStripButton overflowStripButton = new ExtendedToolStripButton(info.Icon);

            //Add new button method take out paramenters those must forced to initialized for later use.
            AddNewButton(info, stackStripButton, overflowStripButton);

            if ((!string.IsNullOrEmpty(info.EventTopicName)) &&
               (!_topics.ContainsKey(stackStripButton.Text)))
            {
                _topics.Add(stackStripButton.Text, info.EventTopicName);
            }
            _buttons.Add(stackStripButton, overflowStripButton);
        }

        public void AddContentControl(UserControl contentUC)
        {
            if (ContentPanel.Controls.Count > 0) ContentPanel.Controls.Clear();
            contentUC.SuspendLayout();
            ContentPanel.Controls.Add(contentUC);
            contentUC.ResumeLayout(true);
        }

        public string FindTitleByModulePath(string modulePath)
        {
            return _topics.FirstOrDefault(t => t.Value == modulePath).Key;
        }

        public void EnableButtonByModulePath(string modulePath, bool isEnabled)
        {
            ToolStripButton button = _buttons.FirstOrDefault(t => t.Key.Name == modulePath).Value;
            if (button != null)
            {
                button.Enabled = isEnabled;
                button = _buttons.FirstOrDefault(t => t.Key.Name == modulePath).Key;
                button.Enabled = isEnabled;
            }
        }

        private void OnClick(object sender, EventArgs e)
        {
            foreach (ToolStripButton item in stackStrip.Items)
            {
                item.Checked = false;
            }

            ExtendedToolStripButton button = (ExtendedToolStripButton)sender;
            
            //Add coressponding UC
            UserControl LoadUC = (UserControl)button.Tag;
            if (LoadUC != null)
            {
                if (ContentPanel.Controls.Count > 0) ContentPanel.Controls.Clear();
                ContentPanel.Controls.Add(LoadUC);
                return;
            }
            if (_topics.ContainsKey(button.Text))
            {
                string eventTopic = _topics[button.Text];
                if (eventTopic != null)
                {
                    if (ModuleButtonClicked != null)
                    {
                        ModuleButtonClicked(sender, e);
                    }
                }
            }
        }

        protected virtual void AddNewButton(OutlookBarInfo info, UserControl contentUC, ExtendedToolStripButton stackStripButton, ExtendedToolStripButton overflowStripButton)
        {
            //stackStripButton = new ExtendedToolStripButton();
            stackStripButton.ImageTransparentColor = info.ImageTransparentColor == Color.Empty ? Color.Black : info.ImageTransparentColor;
            stackStripButton.CheckOnClick = true;
            stackStripButton.Image = info.Icon;
            stackStripButton.ImageScaling = ToolStripItemImageScaling.None;
            stackStripButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            stackStripButton.Margin = new System.Windows.Forms.Padding(3);
            stackStripButton.Padding = new System.Windows.Forms.Padding(2);
            stackStripButton.Size = new System.Drawing.Size(311, 32);
            stackStripButton.Text = info.Title;
            stackStripButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            stackStripButton.Tag = contentUC;
            stackStripButton.Checked = info.Focus;
            stackStripButton.Click += new EventHandler(this.OnClick);
            stackStripButton.EventName = info.EventTopicName;
            stackStripButton.Enabled = info.Enable;

            overflowStripButton.ImageTransparentColor = Color.Black;
            overflowStripButton.Alignment = ToolStripItemAlignment.Right;
            overflowStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            overflowStripButton.Text = info.Title;
            overflowStripButton.Tag = contentUC;
            overflowStripButton.Click += new EventHandler(this.OnClick);
            overflowStripButton.EventName = info.EventTopicName;
            overflowStripButton.Enabled = info.Enable;

            this.overflowStrip.Items.Add(overflowStripButton);
            this.stackStrip.Items.Add(stackStripButton);
            UpdateSplitter();
        }

        protected virtual void AddNewButton(OutlookBarInfo info, ExtendedToolStripButton stackStripButton, ExtendedToolStripButton overflowStripButton)
        {
            stackStripButton.ImageTransparentColor = info.ImageTransparentColor == Color.Empty ? Color.Black : info.ImageTransparentColor;
            stackStripButton.CheckOnClick = true;
            stackStripButton.Image = info.Icon;
            stackStripButton.ImageScaling = ToolStripItemImageScaling.None;
            stackStripButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            stackStripButton.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            stackStripButton.Padding = new System.Windows.Forms.Padding(6);
            stackStripButton.Size = new System.Drawing.Size(311, 35);
            stackStripButton.Text = info.Title;
            stackStripButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            stackStripButton.Checked = info.Focus;
            stackStripButton.Click += new EventHandler(this.OnClick);
            stackStripButton.EventName = info.EventTopicName;
            stackStripButton.Name = info.EventTopicName;
            stackStripButton.Enabled = info.Enable;

            overflowStripButton.ImageTransparentColor = Color.Black;
            overflowStripButton.Alignment = ToolStripItemAlignment.Right;
            overflowStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            overflowStripButton.Text = info.Title;
            overflowStripButton.Click += new EventHandler(this.OnClick);
            overflowStripButton.EventName = info.EventTopicName;
            overflowStripButton.Name = info.EventTopicName;
            overflowStripButton.Enabled = info.Enable;

            this.overflowStrip.Items.Add(overflowStripButton);
            this.stackStrip.Items.Add(stackStripButton);
            UpdateSplitter();
        }

        private void UpdateSplitter()
        {
            // Set slider increment
            if (this.stackStrip.ItemHeight > 0)
            {
                this.stackStripSplitter.SplitterIncrement = this.stackStrip.ItemHeight;

                // Find visible count                
                _visibleCount = this.stackStrip.Items.Count;

                // Setup BaseStackStrip
                this.overflowStrip.Height = this.stackStrip.ItemHeight;

                // Set splitter distance
                int min = this.stackStrip.ItemHeight + this.overflowStrip.Height;
                int distance = this.stackStripSplitter.Height - this.stackStripSplitter.SplitterWidth - ((_visibleCount * this.stackStrip.ItemHeight) + this.overflowStrip.Height);

                // Set Max
                _maxHeight = (this.stackStrip.ItemHeight * this.stackStrip.ItemCount) + this.overflowStrip.Height + this.stackStripSplitter.SplitterWidth;

                // In case it's sized too small on startup
                if (distance < 0)
                {
                    distance = min;
                }

                // Set Min/Max
                this.stackStripSplitter.SplitterDistance = distance;
                this.stackStripSplitter.Panel1MinSize = min;
            }
        }

        private void OnStackStripSplitterPaint(object sender, PaintEventArgs e)
        {
            ProfessionalColorTable pct = new ProfessionalColorTable();
            Rectangle bounds = (sender as SplitContainer).SplitterRectangle;

            int squares;
            int maxSquares = 9;
            int squareSize = 4;
            int boxSize = 2;

            // Make sure we need to do work
            if ((bounds.Width > 0) && (bounds.Height > 0))
            {
                Graphics g = e.Graphics;

                // Setup colors from the provided renderer
                Color begin = pct.OverflowButtonGradientMiddle;
                Color end = pct.OverflowButtonGradientEnd;

                // Make sure we need to do work
                using (Brush b = new LinearGradientBrush(bounds, begin, end, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(b, bounds);
                }

                // Calculate squares
                if ((bounds.Width > squareSize) && (bounds.Height > squareSize))
                {
                    squares = Math.Min((bounds.Width / squareSize), maxSquares);

                    // Calculate start
                    int start = (bounds.Width - (squares * squareSize)) / 2;
                    int Y = bounds.Y + 1;

                    // Get brushes
                    Brush dark = new SolidBrush(pct.GripDark);
                    Brush middle = new SolidBrush(pct.ToolStripBorder);
                    Brush light = new SolidBrush(pct.ToolStripDropDownBackground);

                    // Draw
                    for (int idx = 0; idx < squares; idx++)
                    {
                        // Draw grips
                        g.FillRectangle(dark, start, Y, boxSize, boxSize);
                        g.FillRectangle(light, start + 1, Y + 1, boxSize, boxSize);
                        g.FillRectangle(middle, start + 1, Y + 1, 1, 1);
                        start += squareSize;
                    }

                    dark.Dispose();
                    middle.Dispose();
                    light.Dispose();
                }
            }
        }

        private void OnStackStripSplitterMoved(object sender, SplitterEventArgs e)
        {
            if ((_maxHeight > 0) && ((this.stackStripSplitter.Height - e.SplitY) >= _maxHeight))
            {
                // Limit to max height
                this.stackStripSplitter.SplitterDistance = this.stackStripSplitter.Height - _maxHeight;

                // Set to max
                _visibleCount = this.stackStrip.ItemCount;
                int count = this.overflowStrip.Items.Count;

                // Update overflow items
                for (int idx = 1; idx < count; idx++)
                {
                    this.overflowStrip.Items[idx].Visible = false;
                }
                _lastCount = _visibleCount;

            }
            else if ((_visibleCount >= 0) && (this.stackStrip.ItemCount > 0))
            {
                // Make sure overflow is correct
                _visibleCount = (this.stackStrip.Height / this.stackStrip.ItemHeight);

                // See if this changed
                if (_lastCount != _visibleCount)
                {
                    int count = this.overflowStrip.Items.Count;

                    // Update overflow items
                    for (int idx = 1; idx < count; idx++)
                    {
                        this.overflowStrip.Items[count - idx].Visible = (idx < (count - _visibleCount));
                    }

                    // Update last
                    _lastCount = _visibleCount;
                }
            }
        }

        public int ModuleSelectorHeight
        {
            get { return stackStripSplitter.Height - stackStripSplitter.SplitterDistance; }
            set { stackStripSplitter.SplitterDistance = stackStripSplitter.Height - value; }
        }

        private void OnStackStripItemHeightChanged(object sender, EventArgs e)
        {
            UpdateSplitter();
        }

        private void showMoreButtonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_visibleCount < this.stackStrip.Items.Count)
            {
                this.stackStripSplitter.SplitterDistance -= this.stackStrip.ItemHeight;
                stackStripSplitter.Refresh();
            }
        }

        private void showFewerButtonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_visibleCount > 0)
            {
                this.stackStripSplitter.SplitterDistance += this.stackStrip.ItemHeight;
                stackStripSplitter.Refresh();
            }
        }
    }
}