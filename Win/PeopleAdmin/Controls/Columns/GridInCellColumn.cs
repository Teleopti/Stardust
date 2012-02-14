﻿using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls.Columns
{
    public class GridInCellColumn<T> : ColumnBase<T>
    {
        private string _bindingProperty;
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        public GridInCellColumn(string bindingProperty)
        {
            _bindingProperty = bindingProperty;
        }

        public override int PreferredWidth
        {
            get { return 0; }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = "-";
                e.Handled = true;
            }
            
            if (e.ColIndex > 0 && IsContentRow(e.RowIndex,dataItems.Count))
            {
                e.Style.CellType = "GridInCell";
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.Control = (GridControl)_propertyReflector.GetValue(dataItem, _bindingProperty);
                e.Handled = true;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0 && e.RowIndex <= dataItems.Count)
            {
                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, _bindingProperty, e.Style.Control);
                e.Handled = true;
            }
        }
    }
}