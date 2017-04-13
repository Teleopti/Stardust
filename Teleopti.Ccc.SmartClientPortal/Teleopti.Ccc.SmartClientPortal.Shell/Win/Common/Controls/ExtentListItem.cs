using System;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    [Serializable]
    public class ExtentListItem : ListViewItem 
    {
        public ExtentListItem() { }
        protected ExtentListItem(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public object TagObject { get; set; }

    }
}
