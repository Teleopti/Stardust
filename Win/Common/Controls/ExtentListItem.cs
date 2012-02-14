using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Win.Common.Controls
{
    [Serializable]
    public class ExtentListItem : ListViewItem 
    {
        public ExtentListItem() { }
        public ExtentListItem(ListViewGroup group) : base(group) { }
        public ExtentListItem(string text) : base(text) { }
        public ExtentListItem(string text, int imageIndex) : base(text, imageIndex) { }
        public ExtentListItem(string text, string imageKey) : base(text, imageKey) { }
        protected ExtentListItem(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public object TagObject { get; set; }

    }
}
