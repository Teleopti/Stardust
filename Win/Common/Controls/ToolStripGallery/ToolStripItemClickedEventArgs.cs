using System;
using Syncfusion.Windows.Forms.Tools;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls.ToolStripGallery
{
    /// <summary>
    /// Provides data for Item Clicked events on a ToolStrip Item.
    /// </summary>
    public class ToolStripItemClickedEventArgs : EventArgs
    {
    	/// <summary>
    	/// Gets or sets the item upon which the click operation was performed.
    	/// </summary>
    	/// <value>
    	/// The item upon which the click operation was performed.
    	/// </value>
    	public ToolStripGalleryItem ClickedItem { get; private set; }

    	/// <summary>
    	/// Gets or sets the mouse button used to perform the click operation.
    	/// </summary>
    	/// <value>
    	/// The mouse button used to perform the click operation.
    	/// </value>
    	public MouseButtons MouseButton { get; private set; }

    	/// <summary>
    	/// Gets or sets the context menu strip instance
    	/// </summary>
    	/// <value>
    	/// The mouse button used to perform the click operation.
    	/// </value>
    	public ContextMenuStrip ContextMenuStrip { get; private set; }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="System.Windows.Forms.ToolStripItemClickedEventArgs" /> class,
        /// using the given clicked item and the button used.
        /// </summary>
        /// <param name="clickedItem">
        /// The item on which the click operation was performed.
        /// </param>
        /// <param name="mouseButton">
        /// The mouse button used to perform the click operation.
        /// </param>
        /// /// <param name="menuStrip">
        /// Context menu strip instance
        /// </param>
        public ToolStripItemClickedEventArgs(ToolStripGalleryItem clickedItem, MouseButtons mouseButton, ContextMenuStrip menuStrip)
        {
            ClickedItem = clickedItem;
            MouseButton = mouseButton;
            ContextMenuStrip = menuStrip;
        }
    }
}
