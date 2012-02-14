using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Common.ControlExtenders
{

    /// <summary>
    /// Smart parent select option.
    /// </summary>
    public enum SmartParentSelectingOption
    {
        /// <summary>
        /// Selects the parent node if all childnodes are selected
        /// </summary>
        SelectParentIfAllChildSelected = 0,
        /// <summary>
        /// Selects the parent node if at least one childnode is selected
        /// </summary>
        SelectParentIfAtLeastOneChildSelected,
        /// <summary>
        /// Switched off
        /// </summary>
        Off
    }

    /// <summary>
    /// Smart child selecting option.
    /// </summary>
    public enum SmartChildSelectingOption
    {
        /// <summary>
        /// Selects the all the children nodes recursively if the parent checkbox is selected.
        /// </summary>
        SelectChildrenRecursively = 0,
        /// <summary>
        /// Switched off
        /// </summary>
        Off
    }

    /// <summary>
    /// Extended functionalities to Winform's TreeView control.
    /// </summary>
    public class TreeViewExtender : IDisposable
    {
        #region Variables

        private TreeView _treeView;
        private bool _smartCheckBoxSelectSupport;
        private bool _smartImageSelectSupport;
        private IList<TreeNode> _selectedNode = new List<TreeNode>();
        private SmartParentSelectingOption _smartParentSelectingType;
        private SmartChildSelectingOption _smartChildSelectingType;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewExtender"/> class.
        /// </summary>
        /// <param name="treeView">The tree view.</param>
        public TreeViewExtender(TreeView treeView)
        {
            if (treeView == null)
                throw new ArgumentNullException("treeView");
            _treeView = treeView;
            _treeView.AfterCheck += new TreeViewEventHandler(treeView_AfterCheck);
        }

        #endregion

        #region Component events

        /// <summary>
        /// Handles the AfterSelect event of the _treeView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
        private void treeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_smartCheckBoxSelectSupport)
            {
                StoreSelectedNode(e.Node);
                if (SmartChildSelectingType == SmartChildSelectingOption.SelectChildrenRecursively)
                {
                    foreach (TreeNode childNode in e.Node.Nodes)
                    {
                        if (childNode.Checked != e.Node.Checked)
                            childNode.Checked = e.Node.Checked;
                    }
                }
                if (IsStoredSelectedNode(e.Node))
                {
                    _treeView.AfterCheck -= new TreeViewEventHandler(treeView_AfterCheck);
                    SetParentCheckBoxState(e.Node);
                    _treeView.AfterCheck += new TreeViewEventHandler(treeView_AfterCheck);
                    ReleaseSelectedNode();
                }
            }
        }

        #endregion

        #region Local methods

        /// <summary>
        /// Sets the parent check box state.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        private void SetParentCheckBoxState(TreeNode node)
        {
            if (node == null)
                return;
            TreeNode parentNode = node.Parent;
            if (parentNode != null)
            {
                bool isParentSelected;
                if (SmartParentSelectingType == SmartParentSelectingOption.SelectParentIfAllChildSelected)
                    isParentSelected = IsAllChildrenChecked(parentNode);
                else if (SmartParentSelectingType == SmartParentSelectingOption.SelectParentIfAtLeastOneChildSelected)
                    isParentSelected = IsAtLeastOneChildrenChecked(parentNode);
                else
                    isParentSelected = parentNode.Checked;
                if (parentNode.Checked != isParentSelected)
                    parentNode.Checked = isParentSelected;
                SetParentCheckBoxState(parentNode);
            }
        }

        /// <summary>
        /// Determines whether all children is checked in the specified node].
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// 	<c>true</c> if all children checked in the specified node; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        private static bool IsAllChildrenChecked(TreeNode node)
        {
            if (node == null)
                return false;
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Checked == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether at least one child is checked in the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// 	<c>true</c> if at least a children checked in the specified node; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        private static bool IsAtLeastOneChildrenChecked(TreeNode node)
        {
            if (node == null)
                return false;
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Checked == true)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets ans stores the selected node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        private void StoreSelectedNode(TreeNode node)
        {
            if (_selectedNode.Count == 0)
            {
                _selectedNode.Add(node);
            }
        }

        /// <summary>
        /// Determines whether the top selected rootNode is the specified rootNode.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// 	<c>true</c> if the specified rootNode is the top selected rootNode is; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        private bool IsStoredSelectedNode(TreeNode node)
        {
            if (_selectedNode.Count == 0)
            { 
                return false;
            }
            if (_selectedNode[0] == node)
            {
                _selectedNode.Clear();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Deletes the top selected rootNode.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        private void ReleaseSelectedNode()
        {
            _selectedNode.Clear();
        }

        #endregion

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
                    // do nothing
                }
            }
            disposed = true;
        }

        #endregion

        #region Interface

        /// <summary>
        /// Gets or sets a value indicating whether the control performs a smart branch check meaning that
        /// when a user checks a node, all the sub checkboxes are selected as well, and the parent nodes
        /// will be checked if all the children are selected.
        /// </summary>
        /// <value><c>true</c> if branch check mode is set; otherwise, <c>false</c>.</value>
        public bool SmartCheckBoxSelectSupport
        {
            get { return _smartCheckBoxSelectSupport; }
            set
            {
                if (value && (!_treeView.CheckBoxes || _smartImageSelectSupport) )
                    throw new NotSupportedException("Functionality is not supported if CheckBoxes are not enabled, or the SmartImageSelectSupport is set");
                _smartCheckBoxSelectSupport = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control performs a smart select indication
        /// meaning that when the user selects a node, the imageIndex of the node and the 
        /// imageIndex of the parent node will indicate the changes.
        /// </summary>
        /// <value><c>true</c> if brach select mode is set; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        public bool SmartImageSelectSupport
        {
            get { return _smartImageSelectSupport; }
            set 
            {
                if (value && (_treeView.ImageList == null || _smartCheckBoxSelectSupport) )
                    throw new NotSupportedException("Functionality is not supported if no ImageList is defined in the TreeView, or the SmartCheckBoxSelectSupport is set");
                _smartImageSelectSupport = value; 
            }
        }

        /// <summary>
        /// Gets or sets the smart parent selecting type.
        /// </summary>
        /// <value>The smart select type.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/7/2007
        /// </remarks>
        public SmartParentSelectingOption SmartParentSelectingType
        {
            get { return _smartParentSelectingType; }
            set { _smartParentSelectingType = value; }
        }

        /// <summary>
        /// Gets or sets the smart child selecting type.
        /// </summary>
        /// <value>The smart select type.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/7/2007
        /// </remarks>
        public SmartChildSelectingOption SmartChildSelectingType
        {
            get { return _smartChildSelectingType; }
            set { _smartChildSelectingType = value; }
        }

        #endregion
    }
}