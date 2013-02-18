using System;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Common.Controls.ToolStripGallery
{
    /// <summary>
    /// Provides data for Item Clicked events on a ToolStrip Item.
    /// </summary>
    public class ToolStripGalleryItemContextMenuClickEventArgs : EventArgs
    {
    	/// <summary>
    	/// Gets or sets the item upon which the click operation was performed.
    	/// </summary>
    	/// <value>
    	/// The item upon which the click operation was performed.
    	/// </value>
    	public ToolStripGalleryItem ClickedItem { get; private set; }

        /// <summary>
        ///  Represents an event with no event data.
        /// </summary>
        public static new ToolStripGalleryItemContextMenuClickEventArgs Empty
        {
            get { return new ToolStripGalleryItemContextMenuClickEventArgs(); }
        }

    	/// <summary>
        /// Initializes a new instance of the 
        /// <see cref="ToolStripGalleryItemContextMenuClickEventArgs" /> class.
        /// </summary>
        public ToolStripGalleryItemContextMenuClickEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="ToolStripGalleryItemContextMenuClickEventArgs" /> class,
        /// using the given clicked item.
        /// </summary>
        public ToolStripGalleryItemContextMenuClickEventArgs(ToolStripGalleryItem clickedItem)
        {
            ClickedItem = clickedItem;
        }
    }
}
