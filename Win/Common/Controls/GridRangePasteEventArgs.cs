using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public class GridRangePasteEventArgs : EventArgs
    {
        public GridRangeInfo GridRange
        {
            get;
            set;
        }
    }
}
