using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Win.Common.Controls.Columns;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class RowHeaderColumn<T> : ColumnBase<T>
    {
        #region IColumn Members

        public override int PreferredWidth
        {
            get { return 25; }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex > 0 && e.ColIndex == 0)
            {
                GridModel gridModel = e.Style.GetGridModel();
                if (gridModel != null)
                {
                    //int headerCount = gridModel.Rows.HeaderCount;
                    //int index = (headerCount > 0) ? (e.RowIndex - 1) : e.RowIndex;
                    //e.Style.CellValue = " ";  // index.ToString(CultureInfo.CurrentCulture);
                }
            }

            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }

        #endregion
    }
}