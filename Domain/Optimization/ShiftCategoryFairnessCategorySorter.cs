using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IShiftCategoryFairnessCategorySorter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "4#")]
        IEnumerable<IShiftCategoryFairnessCompareValue> GetGroupCategories(
            IShiftCategoryFairnessCompareResult selectedGroup,
            IEnumerable<IShiftCategoryFairnessCompareValue> selectedGroupCategories,
            int numberOfGroups,
            IEnumerable<IShiftCategoryFairnessSwap> blacklist,
            ref List<IShiftCategoryFairnessCompareResult> blacklistedGroups);
    }

    public class ShiftCategoryFairnessCategorySorter : IShiftCategoryFairnessCategorySorter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
        public IEnumerable<IShiftCategoryFairnessCompareValue> GetGroupCategories(
            IShiftCategoryFairnessCompareResult selectedGroup,
            IEnumerable<IShiftCategoryFairnessCompareValue> selectedGroupCategories,
            int numberOfGroups,
            IEnumerable<IShiftCategoryFairnessSwap> blacklist, ref
                                                                   List<IShiftCategoryFairnessCompareResult>
                                                                   blacklistedGroups)
        {

            if (numberOfGroups == int.MaxValue || numberOfGroups == int.MinValue)
                throw new ArgumentOutOfRangeException("numberOfGroups");

            var selectedGroupCategoriesOrdered = selectedGroupCategories.OrderByDescending(c => c.Original);

            var firstCategory =
                selectedGroupCategoriesOrdered.FirstOrDefault(c => c.Original > c.ComparedTo);
            var secondCategory =
                selectedGroupCategoriesOrdered.LastOrDefault(c => c.Original < c.ComparedTo);

            // if all categories are blacklisted for highest
            if (blacklist.Count(g => (g.Group1 == selectedGroup && g.ShiftCategoryFromGroup1 == firstCategory)
                || (g.Group2 == selectedGroup && g.ShiftCategoryFromGroup2 == selectedGroup))
                == ((numberOfGroups - 1) * (selectedGroupCategories.Count() - 1))
                || firstCategory == null
                || secondCategory == null)
            {
                blacklistedGroups.Add(selectedGroup);
                return null;
            }
            return new List<IShiftCategoryFairnessCompareValue> {firstCategory, secondCategory};

        }
    }
}
