using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces
{
    public class OutlookBarWorkspaceModel
    {
        private readonly IList<OutlookBarInfo> _itemCollection = new List<OutlookBarInfo>();

        public int NumberOfVisibleGroupBars { get; set; }
        public string StartupModule { get; set; }
        public string LastModule { get; set; }

        public IList<OutlookBarInfo> ItemCollection
        {
            get { return new ReadOnlyCollection<OutlookBarInfo>(_itemCollection); }
        }

        public void Add(OutlookBarInfo outlookBarSmartPartInfo)
        {
            _itemCollection.Add(outlookBarSmartPartInfo);
        }
    }
}
