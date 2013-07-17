using System;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class SelectedItemChangeBaseEventArgs<T> : EventArgs
    {
        public T SelectedItem { get; private set; }

        public SelectedItemChangeBaseEventArgs(T selectedItem)
        {
            SelectedItem = selectedItem;
        }
    }
}