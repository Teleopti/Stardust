#region Using

using System.Collections.Generic;

#endregion

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    public interface ISortColumn<T>
    {
        string BindingProperty { get;}
        bool? IsAscending { get; set; }
        IComparer<T> Comparer { get; }
        SortCompare<T> SortCompare { get; set; }
    }
}
