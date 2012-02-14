using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GridTest;
using Syncfusion.Windows.Forms.Grid;

namespace SdkTestWinGui
{
    public class ScheduleGridVisualProjectionColumn<T> : ScheduleGridColumnBase<T>
    {
        private readonly IComparer<T> _columnComparer;
        private TimePeriod _period;

        public ScheduleGridVisualProjectionColumn(string bindingProperty, string headerText, IComparer<T> columnComparer)
            : base(bindingProperty, headerText)
        {
            _columnComparer = columnComparer;
        }

        public override IComparer<T> ColumnComparer
        {
            get
            {
                return _columnComparer;
            }
        }

        public TimePeriod Period
        {
            get { return _period; }
            set { _period = value; }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {

            e.Style.Tag = Period;
            if (e.RowIndex <= e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                
                e.Style.CellType = "VisualProjectionColumnHeaderCell";
                e.Style.CellValue = "2008-12-31";
            }
            else
            {
                e.Style.CellType = "VisualProjectionCell";
                GetCellValue(e, dataItems, dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)]);
            }

            e.Handled = true;
        }


        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "VisualProjectionCell";
            e.Style.Tag = Period;
            e.Style.CellValueType = typeof(VisualLayer);
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }

        //private TimePeriod period(IEnumerable<T> dataItems)
        //{

        //    TimeSpan min = TimeSpan.MaxValue;
        //    TimeSpan max = TimeSpan.MinValue;

        //    foreach (T dataItem in dataItems)
        //    {
        //        VisualProjection projection = dataItem as VisualProjection;
        //        if (projection != null)
        //        {
        //            TimePeriod period =  projection.Period();
        //            if (period.StartTime < min)
        //                min = period.StartTime;

        //            if (period.EndTime > max)
        //                max = period.EndTime;
        //        }
        //    }

        //    return new TimePeriod(min, max);
        //}
    }
}