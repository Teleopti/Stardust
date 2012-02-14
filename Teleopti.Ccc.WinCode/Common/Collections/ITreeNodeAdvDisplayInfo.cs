using System.Drawing;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Interface for Display info for <see cref="TreeNodeAdv"/>.
    /// </summary>
    public interface ITreeNodeAdvDisplayInfo
    {
        /// <summary>
        /// Updates the inner display properties.
        /// </summary>
        void UpdateDisplayProperties();

        /// <summary>
        /// Gets or sets a value indicating whether the node is expanded.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the node is expanded; otherwise, <c>false</c>.
        /// </value>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is selected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the node is selected; otherwise, <c>false</c>.
        /// </value>
        bool IsSelected { get; set; }
    }
}