using System;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces
{
    public class OutlookBarInfo 
    {
        public Color ImageTransparentColor { get; set; }

        public string EventTopicName { get; set; }

        public Image Icon { get; set; }

        public string Title { get; set; }

        public bool Focus { get; set; }

        public bool Enable { get; set; }

        public UserControl Client { get; set; }

        public bool Visible { get; set; }

        public OutlookBarInfo()
        {
            ImageTransparentColor = Color.Empty;
        }

        public OutlookBarInfo(string title, Image icon)
            : this (title, icon, String.Empty)
        { }

        public OutlookBarInfo(string title, Image icon, string eventTopic)
        {
            ImageTransparentColor = Color.Empty;
            Title = title;
            Icon = icon;
            EventTopicName = eventTopic;
        }
    }
}