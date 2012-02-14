using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common.Collections;

namespace Teleopti.Ccc.WinCode.Common.ControlExtenders
{

    /// <summary>
    /// Extended functionalities to Winform's TreeView control.
    /// </summary>
    public class TreeViewCheckBoxExtender : IDisposable
    {
        #region Variables

        private readonly TreeView _treeView;
        private readonly ITreeItem<TreeNode> _treeItem;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewCheckBoxExtender"/> class.
        /// </summary>
        /// <param name="treeView">The tree view.</param>
        /// <param name="treeItem">The tree item.</param>
        /// <param name="autoChildSelection">if set to <c>true</c> the auto child selection in switched on.</param>
        /// <param name="autoParentSelection">The auto parent selection.</param>
        public TreeViewCheckBoxExtender(TreeView treeView, ITreeItem<TreeNode> treeItem, bool autoChildSelection, AutoParentSelectionOption autoParentSelection)
        {
            _treeView = treeView;
            _treeView.CheckBoxes = true;
            _treeItem = treeItem;
            _treeItem.AutoChildSelection = autoChildSelection;
            _treeItem.AutoParentSelection = autoParentSelection;
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
            _treeView.AfterCheck -= new TreeViewEventHandler(treeView_AfterCheck);
            SetTreeViewCheckBox(e.Node);
            _treeView.AfterCheck += new TreeViewEventHandler(treeView_AfterCheck);
        }

        #endregion

        #region Local

        /// <summary>
        /// Sets the parent check box state.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/16/2007
        /// </remarks>
        private void SetTreeViewCheckBox(TreeNode node)
        {
            _treeView.BeginUpdate();
            if (node == null)
                return;
            ITreeItem<TreeNode> currentItem = _treeItem.FindItem(node, RangeOption.ThisAndRecursiveChildren);

            if(currentItem != null)
            {
                currentItem.Selected = node.Checked;
                foreach (ITreeItem<TreeNode> item in _treeItem.Enumerate(RangeOption.ThisAndRecursiveChildren))
                {
                    item.Node.Checked = item.Selected;
                }
            }
            _treeView.EndUpdate();
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

    }
}