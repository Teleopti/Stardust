using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Common.UI.OutlookControls
{
    public class ExtendedToolStripButton : ToolStripButton
    {
        private string eventName;
        public string EventName
        {
            get { return eventName; }
            set { eventName = value; }
        }

        public ExtendedToolStripButton(Image icon)
        {
            base.Image = icon;
        }
        public ExtendedToolStripButton()
        {
        }
    }
}
