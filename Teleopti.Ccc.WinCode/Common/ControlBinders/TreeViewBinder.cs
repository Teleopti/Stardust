using System;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Common.ControlBinders
{
    /// <summary>
    /// Presentation class to connect Windows.TreeView control and TreeList object
    /// </summary>
    public class TreeViewBinder : ITreeControlBinder<TreeNode>, IDisposable
    {
        #region Variables

        private TreeView _treeView;
        private ITreeItem<TreeNode> _rootItem;
        private bool _synchronized;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewBinder"/> class.
        /// </summary>
        /// <param name="treeView">The tree view.</param>
        /// <param name="rootItem">The rootItem item.</param>
        public TreeViewBinder(TreeView treeView, ITreeItem<TreeNode> rootItem)
        {
            _treeView = treeView;
            _rootItem = rootItem;
            foreach (ITreeItem<TreeNode> item in _rootItem.RecursiveChildCollection)
            {
                item.Added += new EventHandler<TreeItemEventArgs<TreeNode>>(item_Added);
                item.Removed += new EventHandler<TreeItemEventArgs<TreeNode>>(item_Removed);
            }
            SynchronizedWithTreeCollection = true;
        }

        #endregion

        #region Interface

        /// <summary>
        /// Syncronizes the node display informations between the old and the new TreeItem and returns the 
        /// syncronized new TreeItem.
        /// </summary>
        /// <param name="oldRootItem">The old root item.</param>
        /// <param name="newRootItem">The new root item.</param>
        /// <returns>The syncronized TreeItem</returns>
        public static ITreeItem<TreeNode> SynchronizeDisplayInformation(ITreeItem<TreeNode> oldRootItem, ITreeItem<TreeNode> newRootItem)
        {
            if (oldRootItem != null && newRootItem != null)
            {
                // todo: ???
                SaveDisplayInformation(oldRootItem);
                foreach (ITreeItem<TreeNode> item in newRootItem.RecursiveChildCollection)
                {
                    ITreeItem<TreeNode> foundOldItem = FindOldItem(oldRootItem, item);
                    if (foundOldItem != null &&
                        foundOldItem.NodeDisplayInfo as TreeNodeDisplayInfo != null)
                    {
                        item.NodeDisplayInfo = foundOldItem.NodeDisplayInfo;
                    }
                }
            }
            return newRootItem;
        }

        /// <summary>
        /// Finds the old item in the new item collection.
        /// </summary>
        /// <param name="oldRootToSearchIn">The old root to search in.</param>
        /// <param name="itemToFind">The item to find.</param>
        /// <returns></returns>
        private static ITreeItem<TreeNode> FindOldItem(ITreeItem<TreeNode> oldRootToSearchIn, ITreeItem<TreeNode> itemToFind)
        {
            string absolutKey = itemToFind.AbsoluteKey;
            foreach (ITreeItem<TreeNode> subItem in oldRootToSearchIn.Enumerate(RangeOption.ThisAndRecursiveChildren))
            {
                if (subItem.AbsoluteKey == absolutKey)
                    return subItem;
            }
            return null;
        }

        /// <summary>
        /// Saves the node display information.
        /// </summary>
        public static void SaveDisplayInformation(ITreeItem<TreeNode> rootItem)
        {
            foreach (ITreeItem<TreeNode> item in rootItem.RecursiveChildCollection)
            {
                if (item.Node != null)
                    item.NodeDisplayInfo = new TreeNodeDisplayInfo(item.Node);
            }
        }

        /// <summary>
        /// Syncronizes the node display information.
        /// </summary>
        /// <param name="oldRootItem">The old root item.</param>
        public void SynchronizeDisplayInformation(ITreeItem<TreeNode> oldRootItem)
        {
            _rootItem = SynchronizeDisplayInformation(oldRootItem, _rootItem);
        }

        /// <summary>
        /// Gets the inner control.
        /// </summary>
        /// <value>The inner control.</value>
        public object TreeControl
        {
            get { return _treeView; }
        }

        /// <summary>
        /// Gets or sets the tree view.
        /// </summary>
        /// <value>The tree view.</value>
        public TreeView TreeView
        {
            get { return _treeView; }
            set { _treeView = value; }
        }

        /// <summary>
        /// Gets the rootItem dataitem.
        /// </summary>
        /// <value></value>
        public ITreeItem<TreeNode> RootItem
        {
            get { return _rootItem; }
            set { _rootItem = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the TreeView control is syncronized
        /// with the TreeCollection.
        /// </summary>
        /// <value><c>true</c> if syncronized; otherwise, <c>false</c>.</value>
        public bool SynchronizedWithTreeCollection
        {
            get { return _synchronized; }
            set { _synchronized = value; }
        }

        /// <summary>
        /// Displays data.
        /// </summary>
        /// <param name="expandLevel">The expand level.</param>
        public void Display(int expandLevel)
        {
            //  Suppress repainting the TreeView until all the objects have been created.
            _treeView.BeginUpdate();
            //  Clear the TreeView each time the method is called.
            _treeView.Nodes.Clear();
            // get the rootItem
            ITreeItem<TreeNode> rootItem = _rootItem;

            if (IsThisOrAnyRecursiveChildPermitted(rootItem))
            {
                // CreateProjection a new Node and fill its properties
                TreeNode rootNode = new TreeNode();
                _treeView.Nodes.Add(rootNode);
                FillNodeProperties(rootNode, rootItem, expandLevel);
                DisplayTreeItem(rootItem, expandLevel);
            }
            
            _treeView.EndUpdate();

            TreeNode lowestVisibleNode = LowestVisibleNode();
            if (lowestVisibleNode != null)
                lowestVisibleNode.EnsureVisible();
        }

        #region IDisposable Members

        private bool disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">flag to make sure that some components are disposed only once.</param>
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // if (_identity != null)
                    if (_treeView != null && !_treeView.Disposing)
                        _treeView.Dispose();
                }
            }
            disposed = true;
        }

        #endregion

        #endregion

        #region Component events


        /// <summary>
        /// Handles the Removed event of the binded TreeView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Teleopti.Ccc.WinCode.Common.Collections.TreeItemEventArgs{TreeNode}"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/14/2007
        /// </remarks>
        private void item_Removed(object sender, TreeItemEventArgs<TreeNode> e)
        {
            TreeNode childNode = null;
            TreeNode parentNode = null;
            if (e.TreeItem.Node != null)
                childNode = e.TreeItem.Node;
            if (childNode != null)
                parentNode = childNode.Parent;
            if (SynchronizedWithTreeCollection)
            {
                if (childNode != null)
                    childNode.Remove();
                if (parentNode != null)
                {
                    parentNode.TreeView.SelectedNode = parentNode;
                    parentNode.TreeView.Select();
                }
            }
        }

        /// <summary>
        /// Handles the Added event of the binded TreeView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Teleopti.Ccc.WinCode.Common.Collections.TreeItemEventArgs{TreeNode}"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/14/2007
        /// </remarks>
        private void item_Added(object sender, TreeItemEventArgs<TreeNode> e)
        {
            TreeNode childNode = null;
            TreeNode parentNode = null;

            if (e.ParentItem.Node != null
                && e.ParentItem.Node != null)
                parentNode = e.ParentItem.Node;


            if (SynchronizedWithTreeCollection)
            {
                childNode = new TreeNode(e.TreeItem.Text);
                e.TreeItem.Node = childNode;
                childNode.ImageIndex = e.TreeItem.ImageIndex;
                //  add rootNode to the collection
                if (parentNode != null)
                {
                    parentNode.Nodes.Add(childNode);
                }
                childNode.Expand();
                childNode.TreeView.SelectedNode = childNode;
                childNode.TreeView.Select();
            }
        }

        #endregion

        #region Local methods

        /// <summary>
        /// Determines whether is this specified TreeItem item or any of its recursive children are permitted.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is this or any recursive child permitted]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsThisOrAnyRecursiveChildPermitted(ITreeItem<TreeNode> item)
        {
            foreach (ITreeItem<TreeNode> treeItem in item.Enumerate(RangeOption.ThisAndRecursiveChildren))
            {
                if(treeItem.IsPermitted)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the lowest visible node from the stored TreeNodeDispayInfo object.
        /// </summary>
        /// <returns></returns>
        private TreeNode LowestVisibleNode()
        {
            Func<ReadOnlyCollection<ITreeItem<TreeNode>>, int> GetLastVisibleIndex =
                delegate(ReadOnlyCollection<ITreeItem<TreeNode>> collection)
                    {
                        for (int counter = collection.Count - 1;
                             counter > 0;
                             counter--)
                        {
                            ITreeItem<TreeNode> currentItem = collection[counter];
                            TreeNodeDisplayInfo nodeInfo =
                                currentItem.NodeDisplayInfo as TreeNodeDisplayInfo;
                            if (nodeInfo != null
                                && nodeInfo.IsVisible)
                                return counter;
                        }
                        return -1;
                    };

            ReadOnlyCollection<ITreeItem<TreeNode>> childCollection =
                _rootItem.RecursiveChildCollection;

            int lastVisibleIndex = GetLastVisibleIndex(childCollection);

            switch (lastVisibleIndex)
            {
                case -1:
                    return null;
                case 0:
                    return childCollection[0].Node;
                default:
                    // trying to get the most out of a treeview bug:
                    // the last node IsVisible property is True even if the
                    // last visble node is the one before the last.
                    if (childCollection[lastVisibleIndex - 1].Node == null
                        || lastVisibleIndex == childCollection.Count - 1)
                        return childCollection[lastVisibleIndex].Node;
                    else
                        return childCollection[lastVisibleIndex - 1].Node;
            }
        }

        /// <summary>
        /// Displays a tree item.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="expandLevel">The expand level.</param>
        protected static void DisplayTreeItem(ITreeItem<TreeNode> parent, int expandLevel)
        {
            if (parent != null)
            {
                foreach (ITreeItem<TreeNode> childItem in parent.ChildCollection)
                {
                    if (IsThisOrAnyRecursiveChildPermitted(childItem))
                    {
                        //  reveal the reference to the Node
                        TreeNode parentNode = parent.Node;

                        TreeNode childNode = new TreeNode();
                        parentNode.Nodes.Add(childNode);

                        FillNodeProperties(childNode, childItem, expandLevel);

                        if (childItem.ChildCollection.Count > 0)
                        {
                            DisplayTreeItem(childItem, expandLevel);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fills the node properties.
        /// </summary>
        /// <param name="treeNode">The tree node.</param>
        /// <param name="treeItem">The tree item.</param>
        /// <param name="expandLevel">The expand level.</param>
        private static void FillNodeProperties(TreeNode treeNode, ITreeItem<TreeNode> treeItem, int expandLevel)
        {
            // display data info
            treeNode.Text = treeItem.Text;
            treeNode.ToolTipText = treeItem.DescriptionText;

            // fill display info
            TreeNodeDisplayInfo displayInfo = treeItem.NodeDisplayInfo as TreeNodeDisplayInfo;
            if (displayInfo == null)
            {
                treeNode.ImageIndex = treeItem.ImageIndex;
                treeNode.SelectedImageIndex = treeItem.ImageIndex;
                if (treeItem.Parent != null 
                    && treeItem.Parent.Node != null 
                    && treeItem.Parent.Level < expandLevel)
                    treeItem.Parent.Node.Expand();
            }
            else
            {
                treeNode.ImageIndex = displayInfo.ImageIndex;
                treeNode.SelectedImageIndex = displayInfo.ImageIndex;
                if (treeItem.Parent != null
                    && treeItem.Parent.Node != null)
                {
                    TreeNodeDisplayInfo parentDisplayInfo = treeItem.Parent.NodeDisplayInfo as TreeNodeDisplayInfo;
                    if (parentDisplayInfo != null 
                        && parentDisplayInfo.IsExpanded)
                        treeItem.Parent.Node.Expand();
                }
                treeNode.Checked = displayInfo.IsChecked;
            }
            //  save a reference to the Node
            treeItem.Node = treeNode;
        }

        #endregion
    }
}