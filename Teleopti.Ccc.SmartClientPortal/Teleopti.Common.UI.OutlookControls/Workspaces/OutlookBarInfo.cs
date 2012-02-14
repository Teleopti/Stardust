using System;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Common.UI.OutlookControls.Workspaces
{
    public class OutlookBarInfo 
    {
        private string _eventTopicName;
        private Color _imageTransparentColor = Color.Empty;

        public Color ImageTransparentColor
        {
            get { return _imageTransparentColor; }
            set { _imageTransparentColor = value; }
        }

        public string EventTopicName
        {
            get { return _eventTopicName; }
            set { _eventTopicName = value; }
        }

        public Image Icon { get; set; }

        public string Title { get; set; }

        public bool Focus { get; set; }

        public bool Enable { get; set; }

        public UserControl Client { get; set; }

        public bool Visible { get; set; }

        public OutlookBarInfo()           
        { }

        public OutlookBarInfo(string title, Image icon)
            : this (title, icon, String.Empty)
        { }

        public OutlookBarInfo(string title, Image icon, string eventTopic)
        {
            Title = title;
            Icon = icon;
            _eventTopicName = eventTopic;
        }
    }
}