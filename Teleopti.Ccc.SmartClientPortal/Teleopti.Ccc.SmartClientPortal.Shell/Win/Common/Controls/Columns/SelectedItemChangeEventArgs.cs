namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public class SelectedItemChangeEventArgs<TData, TItem> : SelectedItemChangeBaseEventArgs<TItem>
    {
        public TData DataItem { get; private set; }

        public SelectedItemChangeEventArgs(TData dataItem, TItem selectedItem) : base(selectedItem)
        {
            DataItem = dataItem;
        }
    }
}