using System;
using System.Drawing;

namespace Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces
{
    public class OutlookBarInfo 
    {
        private Color ImageTransparentColor { get; set; }

        public string EventTopicName { get; set; }

        public Image Icon { get; set; }

        public string Title { get; set; }

        public bool Enable { get; set; }
        public Uri PreviewUrl { get; set; }
        public string PreviewText { get; set; }

        public OutlookBarInfo()
        {
            ImageTransparentColor = Color.Empty;
        }
    }
}