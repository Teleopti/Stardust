using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.ScheduleFilter
{
    public class CccTreeNode
    {
        private IList<CccTreeNode> _nodes = new List<CccTreeNode>();
        private object _tag;
        private int _imageIndex;
        private bool _isChecked;
        private string _displayName;
        private CccTreeNode _parent;
        private bool _displayExpanded;

        public CccTreeNode(string displayName, object tag, bool isChecked, int imageIndex)
        {
            _displayName = displayName;
            _tag = tag;
            _isChecked = isChecked;
            _imageIndex = imageIndex;
        }
        public IList<CccTreeNode> Nodes
        {
            get { return _nodes; }
        }

        public CccTreeNode Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public string DisplayName
        {
            get { return _displayName; }
        }

        public int ImageIndex
        {
            get { return _imageIndex; }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
        }

        public bool DisplayExpanded
        {
            get { return _displayExpanded; }
            set { _displayExpanded = value; }
        }

        public object Tag
        {
            get { return _tag; }
        }
    }
}
