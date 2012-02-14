using System.Drawing;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Interface for Display info
    /// </summary>
    public interface ITreeNodeDisplayInfo
    {
        /// <summary>
        /// Updates the inner display properties.
        /// </summary>
        void UpdateDisplayProperties();

        /// <summary>
        /// Gets or sets a value indicating whether the node is visible.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if node is visible; otherwise, <c>false</c>.
        /// </value>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is expanded.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the node is expanded; otherwise, <c>false</c>.
        /// </value>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is checked.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the node is checked; otherwise, <c>false</c>.
        /// </value>
        bool IsChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is selected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the node is selected; otherwise, <c>false</c>.
        /// </value>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the index of the image.
        /// </summary>
        /// <value>The index of the image.</value>
        int ImageIndex { get; set; }

        /// <summary>
        /// Gets or sets the ForeColor of the node.
        /// </summary>
        /// <value>The ForeColor.</value>
        Color ForeColor { get; set; }
    }
}