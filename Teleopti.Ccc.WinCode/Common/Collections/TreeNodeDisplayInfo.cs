using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common.Collections;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Contains information about the node on how to display.
    /// </summary>
    public class TreeNodeDisplayInfo : ITreeNodeDisplayInfo
    {

        #region Variables

        private bool _isVisible;
        private bool _isExpanded;
        private bool _isChecked;
        private bool _isSelected;
        private int _imageIndex;
        private Color _foreColor;
        private readonly TreeNode _node;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeDisplayInfo"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        public TreeNodeDisplayInfo(TreeNode node)
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
                    IsChecked = _node.Checked;
                    IsExpanded = _node.IsExpanded;
                    IsVisible = _node.IsVisible;
                    ForeColor = _node.ForeColor;
                    IsSelected = _node.IsSelected;
                    ImageIndex = _node.ImageIndex;
             }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the node is visible.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if node is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

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
        /// Gets or sets a value indicating whether the node is checked.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the node is checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; }
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

        /// <summary>
        /// Gets or sets the index of the image.
        /// </summary>
        /// <value>The index of the image.</value>
        public int ImageIndex
        {
            get { return _imageIndex; }
            set { _imageIndex = value; }
        }

        /// <summary>
        /// Gets or sets the ForeColor of the node.
        /// </summary>
        /// <value>The ForeColor.</value>
        public Color ForeColor
        {
            get { return _foreColor; }
            set { _foreColor = value; }
        }

        #endregion
    }
}
