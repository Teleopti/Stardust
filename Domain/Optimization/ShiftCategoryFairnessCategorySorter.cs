using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
    public static class ShiftCategoryFairnessCategorySorter
    {
        public static IOrderedEnumerable<IShiftCategoryFairnessCompareValue> GetGroupCategories(
               IShiftCategoryFairnessCompareResult selectedGroup,
               IOrderedEnumerable<IShiftCategoryFairnessCompareValue> selectedGroupCategories,
               int numberOfGroups,
               IEnumerable<IShiftCategoryFairnessSwap> blacklist)
        {
            if (numberOfGroups == int.MaxValue || numberOfGroups == int.MinValue) throw new ArgumentOutOfRangeException("numberOfGroups");

            return blacklist.Count(g => g.Group1 == selectedGroup || g.Group2 == selectedGroup)
                   == ((numberOfGroups - 1) * (selectedGroupCategories.Count() - 1))
                       ? GetGroupCategories(selectedGroup,
                                            selectedGroupCategories.Skip(1).OrderByDescending(c => c.Original),
                                            numberOfGroups, blacklist)
                       : selectedGroupCategories.OrderByDescending(c => c.Original);
        }
    }
}
