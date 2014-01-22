using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Collections;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.AgentPortal.Common.Controls.ToolStripGallery
{
    /// <summary>
    /// Represents a ToolStripGallery instance
    /// </summary>
    public class TeleoptiToolStripGallery : Syncfusion.Windows.Forms.Tools.ToolStripGallery
    {
        #region Constructors

        /// <summary>
        /// Default constructor of the TeleoptiToolStripGallery
        /// </summary>
        public TeleoptiToolStripGallery()
        {
            MouseDown += TeliOptiToolStripGallery_MouseDown;
            DropDownOpening += TeleoptiToolStripGallery_DropDownOpening;

            scrollOffsetProperty = GetType().GetProperty("ScrollOffset",
                                                                BindingFlags.NonPublic |
                                                                BindingFlags.Instance |
                                                                BindingFlags.FlattenHierarchy);

            customItems.ItemAdded += CustomItems_ItemAdded;
            customItems.ItemRemoved += CustomItems_ItemRemoved;
        }

        #endregion

    	/// <summary>
        /// Item click event handler
        /// </summary>
        public event EventHandler<ToolStripItemClickedEventArgs> ItemClicked;

        /// <summary>
        /// Item click event handler
        /// </summary>
        public event EventHandler<ToolStripGalleryItemContextMenuClickEventArgs> ContextMenuClicked;

        /// <summary>
        /// Reference to the scroll offset property
        /// </summary>
        private readonly PropertyInfo scrollOffsetProperty;

        /// <summary>
        /// Default ToolStripDropDown
        /// </summary>
        private ToolStripDropDown toolStripDropDown;

        /// <summary>
        /// Status flag of the default ContextMenuStrip
        /// </summary>
        private int contextMenuMode;
        /// <summary>
        /// Status flag to determine whether the context menu is opened for the first time
        /// </summary>
        private bool isFirstTime = true;

        private bool isOpening;

        private ToolStripGalleryItem selectedGalleryItem;

        private readonly ObservableList<ToolStripGalleryItemEx> customItems = new ObservableList<ToolStripGalleryItemEx>();

        [Browsable(true), Category("Teleopti")]
        public ToolStripTabItem ParentRibbonTab
        {
            get;
            set;
        }

    	private void CustomItems_ItemRemoved(object sender, ListItemEventArgs<ToolStripGalleryItemEx> e)
        {
            Items.Remove(e.Item);
        }

        private void CustomItems_ItemAdded(object sender, ListItemEventArgs<ToolStripGalleryItemEx> e)
        {
            Items.Add(e.Item);
        }

        private void TeleoptiToolStripGallery_DropDownOpening(object sender, CancelEventArgs e)
        {
            toolStripDropDown = (ToolStripDropDown)sender;

            if (Parent.Parent.Parent.Parent != null)
            {
                Parent.Parent.Parent.Parent.Click -= Parent_Click;
                Parent.Parent.Parent.Parent.Click += Parent_Click;
            }

            Parent.Parent.Parent.Click -= Parent_Click;
            Parent.Parent.Parent.Click += Parent_Click;

            Parent.Parent.Click -= Parent_Click;
            Parent.Parent.Click += Parent_Click;

            Parent.Click -= Parent_Click;
            Parent.Click += Parent_Click;

            toolStripDropDown.MouseClick -= ToolStripDropDown_MouseDown;
            toolStripDropDown.MouseClick += ToolStripDropDown_MouseDown;

            toolStripDropDown.MouseUp -= ToolStripDropDown_MouseUp;
            toolStripDropDown.MouseUp += ToolStripDropDown_MouseUp;

            toolStripDropDown.Closing -= ToolStripDropDown_Closing;
            toolStripDropDown.Closing += ToolStripDropDown_Closing;

            if (Parent.ContextMenuStrip == null)
                Parent.ContextMenuStrip = new ContextMenuStrip();

            Parent.ContextMenuStrip.Closing -= ContextMenuStrip_Closing;
            Parent.ContextMenuStrip.Closing += ContextMenuStrip_Closing;

            Parent.ContextMenuStrip.Click += ContextMenuStrip_Click;
        }

        private void ToolStripDropDown_MouseUp(object sender, MouseEventArgs e)
        {
            contextMenuMode = 2;
        }

        private void Parent_Click(object sender, EventArgs e)
        {
            //Ugly fix to avoid closed context menu when working with RTL
            if (RightToLeft == RightToLeft.Yes && isOpening)
            {
                isOpening = false;
                return;
            }
            toolStripDropDown.Close();
            Parent.ContextMenuStrip.Close();
        }

        private void ToolStripDropDown_MouseDown(object sender, MouseEventArgs e)
        {
            Parent.ContextMenuStrip.Items.Clear();

            ToolStripDropDown dropDown = (ToolStripDropDown)sender;

            if (e.Button == MouseButtons.Right)
            {
                object dropDownGallery = dropDown.GetType().GetProperty("Gallery", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(dropDown, null);
                int scrollOffset = (int)dropDownGallery.GetType().GetProperty("ScrollOffset", BindingFlags.Instance | BindingFlags.NonPublic)
                                             .GetValue(dropDownGallery, null);

                int remainder;
                int totalItemWidth = ItemSize.Width + ItemMargin.Left + ItemMargin.Right;
                int columnCount = Math.DivRem(dropDown.Width, totalItemWidth, out remainder);

                selectedGalleryItem = GetItemByLocationAndOffset(e.Location, columnCount, scrollOffset);

                if (selectedGalleryItem != null)
                {
                    contextMenuMode = 1;
                    OnItemClicked(e, selectedGalleryItem, Parent.ContextMenuStrip /*contextMenuStrip*/);
                    Parent.ContextMenuStrip.Show(toolStripDropDown, e.Location);
                    contextMenuMode = 2;
                }
            }
        }

        private ToolStripGalleryItem GetItemByLocationAndOffset(Point point, int columnCount, int offset)
        {
            int totalItemWidth = ItemSize.Width + ItemMargin.Left + ItemMargin.Right;

            int remainder;
            int currentRow = Math.DivRem(point.Y + offset, ItemSize.Height + ItemMargin.Top + ItemMargin.Bottom, out remainder);
            int currentColumn = Math.DivRem(point.X, totalItemWidth, out remainder);

            int itemIndex = currentRow * columnCount + currentColumn;
            return Items.Count > itemIndex ? Items[itemIndex] : null;
        }

        private void ToolStripDropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            bool cancel = false;
            if (contextMenuMode == 1)
            {
                cancel = true;
            }
            e.Cancel = cancel;
        }

        private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (isFirstTime)
            {
                isFirstTime = false;
                e.Cancel = true;
            }
            contextMenuMode = 2;
        }

        private void TeliOptiToolStripGallery_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ToolStripGalleryItem toolStripGalleryItem = GetClickedToolStripGalleryItem(e);

				if (Parent.ContextMenuStrip != null)
				{
					Parent.ContextMenuStrip.Click -= ContextMenuStrip_Click;
					Parent.ContextMenuStrip.Click += ContextMenuStrip_Click;

					Parent.ContextMenuStrip.Closing += delegate
					{
						Parent.ContextMenuStrip.Items.Clear();
						Parent.ContextMenuStrip = new ContextMenuStrip();
					};
					isOpening = true;
					OnItemClicked(e, toolStripGalleryItem, Parent.ContextMenuStrip);
				}
            }
        }

        private void ContextMenuStrip_Click(object sender, EventArgs e)
        {
            OnContextMenuClicked();
            contextMenuMode = 2;
        }

    	private int GetScrollOffset()
        {
            return (int)scrollOffsetProperty.GetValue(this, null);
        }

        private void OnItemClicked(MouseEventArgs e, ToolStripGalleryItem toolStripGalleryItem, ContextMenuStrip menuStrip)
        {
            menuStrip.Items.Clear();
            ToolStripItemClickedEventArgs args = new ToolStripItemClickedEventArgs(toolStripGalleryItem, e.Button, menuStrip);
            OnItemClicked(args);
        }

        private ToolStripGalleryItem GetClickedToolStripGalleryItem(MouseEventArgs e)
        {
            return GetItemByLocationAndOffset(e.Location, Dimensions.Width, GetScrollOffset());
        }

        private void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
        	var handler = ItemClicked;
            if (handler != null)
                handler(this, e);
        }

        private void OnContextMenuClicked()
        {
            ToolStripGalleryItemContextMenuClickEventArgs args = new ToolStripGalleryItemContextMenuClickEventArgs();

            var handler = ContextMenuClicked;
            if (handler != null)
                handler(this, args);
        }
    }
}
