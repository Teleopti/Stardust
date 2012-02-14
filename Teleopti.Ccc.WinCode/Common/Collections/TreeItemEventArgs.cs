using System;

//using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Tree item event args
    /// </summary>
    public class TreeItemEventArgs<TNode> : EventArgs
    {
        private ITreeItem<TNode> _treeItem;
        private ITreeItem<TNode> _parentItem;
        private bool _cancel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemEventArgs{TNode}"/> class.
        /// </summary>
        /// <remarks>
        /// Use only in test.
        /// Created by: tamasb
        /// Created date: 11/14/2007
        /// </remarks>
        protected TreeItemEventArgs()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemEventArgs{TNode}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentItem">The parent item.</param>
        public TreeItemEventArgs(ITreeItem<TNode> item, ITreeItem<TNode> parentItem)
        {
            _treeItem = item;
            _parentItem = parentItem;
        }

        /// <summary>
        /// Gets the parent item.
        /// </summary>
        /// <value>The parent item.</value>
        public ITreeItem<TNode> ParentItem
        {
            get { return _parentItem; }
            protected set { _parentItem = value; }
        }

        /// <summary>
        /// Gets the tree item.
        /// </summary>
        /// <value>The tree item.</value>
        public ITreeItem<TNode> TreeItem
        {
            get { return _treeItem; }
            protected set { _treeItem = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation is cancelled by the user.
        /// </summary>
        /// <value><c>true</c> if user wants to cancel; otherwise, <c>false</c>.</value>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }
}