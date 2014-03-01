using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{

    public class ActivityUpdatedReadOnlyTextColumn<T> : SFGridColumnBase<T>
    {
        private readonly int _preferredWidth;
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

        public ActivityUpdatedReadOnlyTextColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        { }

        public ActivityUpdatedReadOnlyTextColumn(string bindingProperty, int preferredWidth, string headerText)
            : base(bindingProperty, headerText)
        {
            _preferredWidth = preferredWidth;
        }

        public override int PreferredWidth
        {
            get { return (_preferredWidth > 0) ? _preferredWidth : 150; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            var inf = currentItem as IChangeInfo;
            if(inf == null) return;
            if (BindingProperty == "UpdatedTimeInUserPerspective")
                e.Style.CellValue = _localizer.UpdatedTimeInUserPerspective(inf);
            else if (inf.UpdatedBy != null)
                e.Style.CellValue = inf.UpdatedBy.Name;
            else
                e.Style.CellValue = "";
            e.Style.ReadOnly = true;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            // Commented: To avoid getting exceptions when doing copy past 
            //throw new InvalidOperationException("Attempt to set value of read-only cell");
        }
    }
}