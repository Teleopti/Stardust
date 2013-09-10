using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public static class ColumnCollectionExtensions
    {
        public static void AppendAuditColumns<T>(this IList<SFGridColumnBase<T>> gridColumns)
        {
            gridColumns.Add(new SFGridReadOnlyTextColumn<T>("UpdatedBy.Name", Resources.UpdatedBy));
            gridColumns.Add(new SFGridReadOnlyTextColumn<T>("UpdatedTimeInUserPerspective", Resources.UpdatedOn));
        }
    }
}