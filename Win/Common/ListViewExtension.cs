using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Contains all the extension methods which belongs to ListView.
    /// </summary>
    /// <remarks>
    /// Created By: kosalanp
    /// Created Date: 17-04-2008
    /// </remarks>
    public static class ListViewExtension
    {
        /// <summary>
        /// Finds the list view item which contains the value in it's Tag property.
        /// </summary>
        /// <param name="listView">The list view.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>The list view item that contains the value.</returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 17-04-2008
        /// </remarks>
        public static ListViewItem FindItemWithTag(this ListView listView, object value)
        {
            int count = listView.Items.Count;
            if (count == 0) return null;

            for (int index = 0; index < count; index++)
            {
                ListViewItem item = listView.Items[index];
                if (item.Tag != null && value == item.Tag) return item;
            }

            return null;
        }

        /// <summary>
        /// Finds the list view item which contains the value in it's TagObject property.
        /// </summary>
        /// <param name="listView">The list view.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>The list view item that contains the value.</returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 17-04-2008
        /// </remarks>
        public static Controls.ExtentListItem FindItemWithTagObject(this ListView listView, object value)
        {
            int count = listView.Items.Count;
            if (count == 0) return null;

            for (int index = 0; index < count; index++)
            {
                Controls.ExtentListItem item = listView.Items[index] as Controls.ExtentListItem;
                if (item.TagObject != null && value == item.TagObject) return item;
            }

            return null;
        }
    }
}
