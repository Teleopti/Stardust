using System;
using Syncfusion.Windows.Forms.Tools;
using System.Windows.Forms;

namespace Teleopti.Ccc.AgentPortal.Common.Controls.ToolStripGallery
{

    /// <summary>
    /// Provides data for Item Clicked events on a ToolStrip Item.
    /// </summary>
    public class ToolStripItemClickedEventArgs : EventArgs
    {
        /// <summary>
        /// The item upon which the click operation was performed.
        /// </summary>
        private readonly ToolStripGalleryItem clickedItem;
        /// <summary>
        /// The mouse button used to perform the click operation.
        /// </summary>
        private readonly MouseButtons mouseButton = MouseButtons.None;
        /// <summary>
        /// The context menu strip instance
        /// </summary>
        private readonly ContextMenuStrip contextMenuStrip;

        /// <summary>
        /// Gets or sets the item upon which the click operation was performed.
        /// </summary>
        /// <value>
        /// The item upon which the click operation was performed.
        /// </value>
        public ToolStripGalleryItem ClickedItem
        {
            get
            {
                return clickedItem;
            }
        }

        /// <summary>
        /// Gets or sets the context menu strip instance
        /// </summary>
        /// <value>
        /// The mouse button used to perform the click operation.
        /// </value>
        public ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return contextMenuStrip;
            }
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="System.Windows.Forms.ToolStripItemClickedEventArgs" /> class.
        /// </summary>
        public ToolStripItemClickedEventArgs()
        {
        }

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
            : this()
        {
            // set instance fields
            this.clickedItem = clickedItem;
            this.mouseButton = mouseButton;
            contextMenuStrip = menuStrip;
        }
    }
}
