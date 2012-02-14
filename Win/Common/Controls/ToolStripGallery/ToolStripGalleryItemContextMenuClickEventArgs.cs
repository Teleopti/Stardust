#region Imports

using System;
using Syncfusion.Windows.Forms.Tools;

#endregion

namespace Teleopti.Ccc.Win.Common.Controls.ToolStripGallery
{

    /// <summary>
    /// Provides data for Item Clicked events on a ToolStrip Item.
    /// </summary>
    public class ToolStripGalleryItemContextMenuClickEventArgs : EventArgs
    {

        #region Fields - Static Member

        #endregion

        #region Fields - Instance Member

        /// <summary>
        /// The item upon which the click operation was performed.
        /// </summary>
        private ToolStripGalleryItem clickedItem;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - ToolStripGalleryItemContextMenuClickEventArgs Members

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
        ///  Represents an event with no event data.
        /// </summary>
        public static new ToolStripGalleryItemContextMenuClickEventArgs Empty
        {
            get { return new ToolStripGalleryItemContextMenuClickEventArgs(); }
        }

        #endregion

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - ToolStripGalleryItemContextMenuClickEventArgs Members

        #region Methods - Instance Member - ToolStripGalleryItemContextMenuClickEventArgs Members - (constructors)

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
            this.clickedItem = clickedItem;
        }

        #endregion

        #endregion

        #endregion

    }

}
