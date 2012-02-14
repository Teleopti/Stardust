using System.Collections.Generic;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Grouping
{
    public class GroupPageLoaderInformation
    {
        public IList<IGroupPage> GroupPageCollection { get; private set; }
        public ITreeItem<TreeNodeAdv> TreeItem { get; private set; }
        public bool LoadFromDatabase { get; private set; }
        public bool DoSetFocusOnMainTabTree { get; private set; }

        public GroupPageLoaderInformation(bool loadFromDatabase, bool doSetFocusOnMainTabTree)
        {
            LoadFromDatabase = loadFromDatabase;
            DoSetFocusOnMainTabTree = doSetFocusOnMainTabTree;
        }

        public void SetResultData(IList<IGroupPage> groupPageCollection, ITreeItem<TreeNodeAdv> treeItem)
        {
            GroupPageCollection = groupPageCollection;
            TreeItem = treeItem;
        }
    }
}