#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Teleopti.Ccc.Win.Common
{
    public interface ISortColumn<T>
    {
        string BindingProperty { get;}
        bool? IsAscending { get; set; }
        IComparer<T> Comparer { get; }
        SortCompare<T> SortCompare { get; set; }
    }
}
