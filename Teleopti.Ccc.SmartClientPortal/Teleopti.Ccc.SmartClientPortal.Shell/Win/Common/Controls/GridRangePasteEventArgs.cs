using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
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
