using System;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.WinCode.Common.ControlBinders
{
    /// <summary>
    /// This will implement Treeview binder for synfusion tree view
    /// </summary>
    public class TreeViewAdvBinder
    {
        private TreeViewAdv _treeView;
        private ITreeItem<TreeNodeAdv> _rootItem;
        private bool _syncronized;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewBinder"/> class.
        /// </summary>
        /// <param name="treeView">The tree view.</param>
        /// <param name="rootItem">The rootItem item.</param>
        public TreeViewAdvBinder(TreeViewAdv treeView, ITreeItem<TreeNodeAdv> rootItem)
        {
            _treeView = treeView;
            _rootItem = rootItem;

            SynchronizedWithTreeCollection = true;
        }

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">flag to make sure that some components are disposed only once.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_treeView != null && !_treeView.Disposing)
                        _treeView.Dispose();
                }
            }
            _disposed = true;
        }

        /// <summary>
        /// Syncronizes the node display informations between the old and the new TreeItem and returns the 
        /// syncronized new TreeItem.
        /// </summary>
        /// <param name="oldRootItem">The old root item.</param>
        /// <param name="newRootItem">The new root item.</param>
        /// <returns>The syncronized TreeItem</returns>
        public static ITreeItem<TreeNodeAdv> SynchronizeDisplayInformation(ITreeItem<TreeNodeAdv> oldRootItem, ITreeItem<TreeNodeAdv> newRootItem)
        {
            if (oldRootItem != null && newRootItem != null)
            {
                // todo: ???
                SaveDisplayInformation(oldRootItem);
                foreach (ITreeItem<TreeNodeAdv> item in newRootItem.RecursiveChildCollection)
                {
                    ITreeItem<TreeNodeAdv> foundOldItem = FindOldItem(oldRootItem, item);
                    if (foundOldItem != null &&
                        foundOldItem.NodeDisplayInfo as TreeNodeAdvDisplayInfo != null)
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
        private static ITreeItem<TreeNodeAdv> FindOldItem(ITreeItem<TreeNodeAdv> oldRootToSearchIn, ITreeItem<TreeNodeAdv> itemToFind)
        {
            string absolutKey = itemToFind.AbsoluteKey;
            foreach (ITreeItem<TreeNodeAdv> subItem in oldRootToSearchIn.Enumerate(RangeOption.ThisAndRecursiveChildren))
            {
                if (subItem.AbsoluteKey == absolutKey)
                    return subItem;
            }
            return null;
        }

        /// <summary>
        /// Saves the node display information.
        /// </summary>
        public static void SaveDisplayInformation(ITreeItem<TreeNodeAdv> rootItem)
        {
            foreach (ITreeItem<TreeNodeAdv> item in rootItem.RecursiveChildCollection)
            {
                if (item.Node != null)
                    item.NodeDisplayInfo = new TreeNodeAdvDisplayInfo(item.Node);
            }
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
        public TreeViewAdv TreeView
        {
            get { return _treeView; }
            set { _treeView = value; }
        }

        /// <summary>
        /// Sets the tree selected nodes.
        /// </summary>
        /// <param name="checkedItemCollection">The checked item collection.</param>
        public void SetTreeSelectedNodes(IList<ITreeItem<TreeNodeAdv>> checkedItemCollection)
        {
            _treeView.SelectedNodes.Clear();
            foreach (TreeNodeAdv node in _treeView.Root.Nodes)
            {
                RecursivelyAddFilterNodes(node, checkedItemCollection);
            }
            _treeView.ExpandAll();
            _treeView.Focus();
        }

        /// <summary>
        /// Recursivelies the add filter nodes.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="checkedItemCollection">The checked item collection.</param>
        public void RecursivelyAddFilterNodes(TreeNodeAdv node, IList<ITreeItem<TreeNodeAdv>> checkedItemCollection)
        {
            if (!node.HasChildren)
            {
                for (int i = 0; i < checkedItemCollection.Count; i++)
                {
                    if (checkedItemCollection[i].Key.Equals(((ITreeItem<TreeNodeAdv>)node.Tag).Key))
                    {
                        _treeView.SelectedNodes.Add(node);
                    }
                }
            }
            else
                foreach (TreeNodeAdv childNode in node.Nodes)
                {
                    RecursivelyAddFilterNodes(childNode, checkedItemCollection);
                }
        }


        /// <summary>
        /// Gets the selected items.
        /// </summary>
        /// <value>The selected item collection.</value>
        public ReadOnlyCollection<ITreeItem<TreeNodeAdv>> SelectedItemCollection
        {
            get
            {
                IList<ITreeItem<TreeNodeAdv>> selectedList = new List<ITreeItem<TreeNodeAdv>>();

                foreach (TreeNodeAdv item in TreeView.SelectedNodes)
                {
                    RecursivelyAddSelectedNodes(item, selectedList);
                }

                return new ReadOnlyCollection<ITreeItem<TreeNodeAdv>>(selectedList);
            }
        }

        /// <summary>
        /// Recursivelies the add selected nodes.
        /// </summary>
        /// <param name="treeNode">The tree node.</param>
        /// <param name="selectedList">The selected list.</param>
        public void RecursivelyAddSelectedNodes(TreeNodeAdv treeNode,
           IList<ITreeItem<TreeNodeAdv>> selectedList)
        {
            if (!treeNode.HasChildren)
            {
                ITreeItem<TreeNodeAdv> item = (ITreeItem<TreeNodeAdv>)treeNode.Tag;

                if (!selectedList.Contains(item))
                { selectedList.Add((ITreeItem<TreeNodeAdv>)treeNode.Tag); }

            }
            else
                foreach (TreeNodeAdv childNode in treeNode.Nodes)
                {
                    RecursivelyAddSelectedNodes(childNode, selectedList);
                }
        }


        /// <summary>
        /// Gets the rootItem dataitem.
        /// </summary>
        /// <value></value>
        public ITreeItem<TreeNodeAdv> RootItem
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
            get { return _syncronized; }
            set { _syncronized = value; }
        }

        /// <summary>
        /// Displays data.
        /// </summary>
        /// <param name="expandLevel">The expand level.</param>
        public void Display(int expandLevel)
        {
            try
            {
                //  Suppress repainting the TreeView until all the objects have been created.
                _treeView.BeginUpdate();
                //  Clear the TreeView each time the method is called.
                _treeView.Nodes.Clear();
                // get the rootItem
                ITreeItem<TreeNodeAdv> rootItem = _rootItem;
                TreeNodeAdv rootNode = BindRootItemToNode(rootItem);

                if (rootNode != null)
                    _treeView.Nodes.Add(rootNode);

                foreach (ITreeItem<TreeNodeAdv> treeItem in _rootItem.Enumerate(RangeOption.ThisAndRecursiveChildren))
                {
                    if (treeItem.Node != null
                        && treeItem.Level < expandLevel)
                        treeItem.Node.Expand();
                }
                _treeView.EndUpdate();
            }
            catch (NullReferenceException)
            {
                //Some times different threads try to call above method in strange manner. Need to manage in a good behaviour.
            }
        }

        /// <summary>
        /// Binds root item to a root node.
        /// </summary>
        /// <returns></returns>
        public static TreeNodeAdv BindRootItemToNode(ITreeItem<TreeNodeAdv> rootItem)
        {
            TreeNodeAdv rootNode = null;

            if (IsThisOrAnyRecursiveChildPermitted(rootItem))
            {
                rootNode = new TreeNodeAdv();
                FillNodeProperties(rootNode, rootItem);
                DisplayTreeItem(rootItem);
            }
            return rootNode;
        }

        /// <summary>
        /// Determines whether is this specified TreeItem item or any of its recursive children are permitted.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is this or any recursive child permitted]; otherwise, <c>false</c>.
        /// </returns>
        protected static bool IsThisOrAnyRecursiveChildPermitted(ITreeItem<TreeNodeAdv> item)
        {
            foreach (ITreeItem<TreeNodeAdv> treeItem in item.Enumerate(RangeOption.ThisAndRecursiveChildren))
            {
                if (treeItem.IsPermitted)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Displays a tree item.
        /// </summary>
        /// <param name="parent">The parent.</param>
        protected static void DisplayTreeItem(ITreeItem<TreeNodeAdv> parent)
        {
            if (parent != null)
            {
                foreach (ITreeItem<TreeNodeAdv> childItem in parent.ChildCollection)
                {
                    if (IsThisOrAnyRecursiveChildPermitted(childItem))
                    {
                        //  reveal the reference to the Node
                        TreeNodeAdv parentNode = parent.Node;

                        TreeNodeAdv childNode = new TreeNodeAdv();
                        parentNode.Nodes.Add(childNode);

                        FillNodeProperties(childNode, childItem);

                        if (childItem.ChildCollection.Count > 0)
                        {
                            DisplayTreeItem(childItem);
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
        protected static void FillNodeProperties(TreeNodeAdv treeNode, ITreeItem<TreeNodeAdv> treeItem)
        {
            // display data info
            treeNode.Text = treeItem.Text;
            treeNode.HelpText = treeItem.DescriptionText;
            treeNode.Tag = treeItem;

            // fill display info
            TreeNodeAdvDisplayInfo displayInfo = treeItem.NodeDisplayInfo as TreeNodeAdvDisplayInfo;
            if (displayInfo == null)
            {
                treeNode.Checked = treeItem.Selected;
                if (treeNode.Checked)
                {
                    treeNode.CheckState = CheckState.Checked;
                    TreeNodeAdv itsParent = treeNode.Parent;
                    while (itsParent != null)
                    {
                        itsParent.Expand();
                        itsParent = itsParent.Parent;
                    }
                }
            }
            else
            {
                if (treeItem.Parent != null
                   && treeItem.Parent.Node != null)
                {
                    TreeNodeAdvDisplayInfo parentDisplayInfo = treeItem.Parent.NodeDisplayInfo as TreeNodeAdvDisplayInfo;
                    if (parentDisplayInfo != null
                        && parentDisplayInfo.IsExpanded)
                        treeItem.Parent.Node.Expand();
                }
            }
            treeNode.TagObject = treeItem.Data;
            if (treeItem.StoredDataDictionary.ContainsKey("Grouping"))
                treeNode.Tag = treeItem.StoredDataDictionary["Grouping"];
            treeNode.LeftImageIndices = new[] { treeItem.ImageIndex };
            treeItem.Node = treeNode;
        }
    }
}
