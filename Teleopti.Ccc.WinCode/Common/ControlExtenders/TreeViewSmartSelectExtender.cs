using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common.Collections;

namespace Teleopti.Ccc.WinCode.Common.ControlExtenders
{

    /// <summary>
    /// Extended functionalities to Winform's TreeView control.
    /// </summary>
    public class TreeViewSmartSelectExtender : IDisposable
    {
        #region Variables

        private readonly TreeView _treeView;
        private readonly ITreeItem<TreeNode> _treeItem;
        private Font _normalFont;
        private Font _boldFont;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewCheckBoxExtender"/> class.
        /// </summary>
        /// <param name="treeView">The tree view.</param>
        /// <param name="treeItem">The tree item.</param>
        /// <param name="autoChildSelection">if set to <c>true</c> the auto child selection in switched on.</param>
        /// <param name="autoParentSelection">The auto parent selection.</param>
        public TreeViewSmartSelectExtender(TreeView treeView, ITreeItem<TreeNode> treeItem, bool autoChildSelection, AutoParentSelectionOption autoParentSelection)
        {
            _treeView = treeView;
            _treeView.CheckBoxes = false;
            _normalFont = _treeView.Font;
            _boldFont = new Font(_normalFont.Name, _normalFont.Size, _normalFont.Style | FontStyle.Bold);
            _treeItem = treeItem;
            _treeItem.AutoChildSelection = autoChildSelection;
            _treeItem.AutoParentSelection = autoParentSelection;
            _treeView.AfterSelect +=new TreeViewEventHandler(treeView_AfterSelect);
        }

        #endregion

        #region Component events

        /// <summary>
        /// Handles the AfterSelect event of the _treeView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse)
            {
                SetTreeViewSelectStatus(e.Node);
                _treeView.SelectedNode = null;
            }
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
        protected void SetTreeViewSelectStatus(TreeNode node)
        {
            _treeView.BeginUpdate();
            if (node == null)
                return;
            ITreeItem<TreeNode> currentItem = _treeItem.FindItem(node, RangeOption.ThisAndRecursiveChildren);

            if(currentItem != null)
            {
                currentItem.Selected = !currentItem.Selected;
                foreach (ITreeItem<TreeNode> item in _treeItem.Enumerate(RangeOption.ThisAndRecursiveChildren))
                {
                    SetFontBold(item.Node, item.Selected);
                }
            }
            _treeView.EndUpdate();
        }

        /// <summary>
        /// Sets the font bold.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="value">if <c>true</c> set the font bold.</param>
        protected void SetFontBold(TreeNode node, bool value)
        {
            if (node != null)
            {
                if (value && (node.NodeFont == null || !node.NodeFont.Bold))
                    node.NodeFont = _boldFont;
                else if (!value && (node.NodeFont == null || node.NodeFont.Bold))
                    node.NodeFont = _normalFont;
            }
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
                    if(_normalFont!=null)
                        _normalFont.Dispose();
                    if(_boldFont!=null)
                        _boldFont.Dispose();
                }
            }
            disposed = true;
        }

        #endregion

    }
}