using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls.OutlookControls
{
    public class ExtendedToolStripButton : ToolStripButton
    {
        public string EventName { get; set; }

        public ExtendedToolStripButton(Image icon)
        {
            base.Image = icon;
        }
        public ExtendedToolStripButton()
        {
        }
    }
}
