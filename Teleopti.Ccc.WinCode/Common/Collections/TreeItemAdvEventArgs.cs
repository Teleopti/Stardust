using System;

//using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Tree item event args
    /// </summary>
    [CLSCompliant(false)]
    public class TreeItemAdvEventArgs : EventArgs
    {
        private  ITreeItemAdv _treeItem;
        private  ITreeItemAdv _parentItem;
        private bool _cancel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdvEventArgs"/> class.
        /// </summary>
        /// <remarks>
        /// Use only in test.
        /// Created by: tamasb
        /// Created date: 11/14/2007
        /// </remarks>
        protected TreeItemAdvEventArgs()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemAdvEventArgs"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="parentItem">The parent item.</param>
        public TreeItemAdvEventArgs(ITreeItemAdv item, ITreeItemAdv parentItem)
        {
            _treeItem = item;
            _parentItem = parentItem;
        }

        /// <summary>
        /// Gets the parent item.
        /// </summary>
        /// <value>The parent item.</value>
        public ITreeItemAdv ParentItem
        {
            get { return _parentItem; }
            protected set { _parentItem = value; }
        }

        /// <summary>
        /// Gets the tree item.
        /// </summary>
        /// <value>The tree item.</value>
        public ITreeItemAdv TreeItem
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