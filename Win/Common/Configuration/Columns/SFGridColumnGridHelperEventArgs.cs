using System;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridColumnGridHelperEventArgs<T> : EventArgs
    {
        public T SourceEntity { get; set; }
    }
}