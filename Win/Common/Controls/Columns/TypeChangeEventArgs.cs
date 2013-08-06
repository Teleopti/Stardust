using System;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class TypeChangeEventArgs : EventArgs
    {
        public Type NewType { get; set; }

        public object DataItem { get; set; }

        public bool IsDataItemValid { get; set; }
    }
}