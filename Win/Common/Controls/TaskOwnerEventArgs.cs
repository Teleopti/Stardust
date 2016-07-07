using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public class FromCellEventArgs<T> : EventArgs
    {
        public T Item { get; set; }
        public GridStyleInfo Style { get; set; }
        public object Value { get; set; }
    }
}
