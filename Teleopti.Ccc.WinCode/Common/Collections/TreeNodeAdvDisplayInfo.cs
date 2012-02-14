using System;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common.Collections;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Contains information about the node on how to display.
    /// </summary>
    public class TreeNodeAdvDisplayInfo : ITreeNodeAdvDisplayInfo
    {

        #region Variables

        private bool _isExpanded;
        private bool _isSelected;
        private TreeNodeAdv _node;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeAdvDisplayInfo"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        public TreeNodeAdvDisplayInfo(TreeNodeAdv node)
        {
            _node = node;
            UpdateDisplayProperties();
        }

        #endregion

        #region Interface

        /// <summary>
        /// Updates the inner display properties.
        /// </summary>
        public void UpdateDisplayProperties()
        {
             if (_node != null)
             {
                 IsExpanded = _node.Expanded; 
                 IsSelected = _node.IsSelected;
             }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the node is expanded.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the node is expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node is selected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the node is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; }
        }

        #endregion
    }
}
