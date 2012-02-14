using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Common.ControlExtenders;

namespace Teleopti.Ccc.WinCodeTest.Common.ControlExtenders
{

    /// <summary>
    /// Extended functionalities to Winform's TreeView control.
    /// </summary>
    public class TreeViewSmartSelectExtenderTestClass : TreeViewSmartSelectExtender
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewCheckBoxExtender"/> class.
        /// </summary>
        /// <param name="treeView">The tree view.</param>
        /// <param name="treeItem">The tree item.</param>
        /// <param name="autoChildSelection">if set to <c>true</c> the auto child selection in switched on.</param>
        /// <param name="autoParentSelection">The auto parent selection.</param>
        public TreeViewSmartSelectExtenderTestClass(TreeView treeView, ITreeItem<TreeNode> treeItem, bool autoChildSelection, AutoParentSelectionOption autoParentSelection) 
            : base(treeView, treeItem, autoChildSelection, autoParentSelection)
        {}

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
        public new void SetTreeViewSelectStatus(TreeNode node)
        {
            base.SetTreeViewSelectStatus(node);
        }

        #endregion

    }
}