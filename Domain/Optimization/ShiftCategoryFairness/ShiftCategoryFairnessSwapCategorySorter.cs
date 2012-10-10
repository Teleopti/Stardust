using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public interface IShiftCategoryFairnessCategorySorter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference",
            MessageId = "4#")]
        IEnumerable<IShiftCategoryFairnessCompareValue> GetGroupCategories(
            IShiftCategoryFairnessCompareResult selectedGroup,
            IEnumerable<IShiftCategoryFairnessCompareValue> selectedGroupCategories,
            int numberOfGroups,
            IEnumerable<IShiftCategoryFairnessSwap> blacklist,
            ref List<IShiftCategoryFairnessCompareResult> blacklistedGroups);
    }

    public class ShiftCategoryFairnessSwapCategorySorter : IShiftCategoryFairnessCategorySorter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")
        , System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
        public IEnumerable<IShiftCategoryFairnessCompareValue> GetGroupCategories(
            IShiftCategoryFairnessCompareResult selectedGroup,
            IEnumerable<IShiftCategoryFairnessCompareValue> selectedGroupCategories,
            int numberOfGroups,
            IEnumerable<IShiftCategoryFairnessSwap> blacklist, ref
                                                                   List<IShiftCategoryFairnessCompareResult>
                                                                   blacklistedGroups)
        {
            blacklist = blacklist.ToList();
            if (numberOfGroups == int.MaxValue || numberOfGroups == int.MinValue)
                throw new ArgumentOutOfRangeException("numberOfGroups");

            var selectedGroupCategoriesOrdered =
                selectedGroupCategories.OrderByDescending(c => c.Original - c.ComparedTo);

            var firstCategory =
                selectedGroupCategoriesOrdered.FirstOrDefault(c => c.Original > c.ComparedTo);
            var secondCategory =
                selectedGroupCategoriesOrdered.LastOrDefault(c => c.Original < c.ComparedTo);
            var categoryCount = selectedGroupCategoriesOrdered.Count();

            if (firstCategory == null || secondCategory == null)
            {
                if (!blacklistedGroups.Contains(selectedGroup)) blacklistedGroups.Add(selectedGroup);
                return new List<IShiftCategoryFairnessCompareValue>();
            }

            var availableSwaps = ((numberOfGroups - 1)*(categoryCount - 1));
            var selectedGroupBlackListItems =
                blacklist.Where(
                    g => (g.Group1 == selectedGroup && g.ShiftCategoryFromGroup1 == firstCategory.ShiftCategory)
                         || (g.Group2 == selectedGroup && g.ShiftCategoryFromGroup2 == firstCategory.ShiftCategory)).
                    ToList();

            List<IShiftCategoryFairnessCompareValue> exceptList;

            // ES: if all swaps for selected group highest category is blacklisted
            if (selectedGroupBlackListItems.Count() == availableSwaps)
            {
                exceptList = new List<IShiftCategoryFairnessCompareValue> {firstCategory};
                if (selectedGroupCategoriesOrdered.Except(exceptList).Any(b => b.Original > b.ComparedTo))
                    return GetGroupCategories(selectedGroup, selectedGroupCategoriesOrdered.Except(exceptList),
                                              numberOfGroups, blacklist.Except(selectedGroupBlackListItems),
                                              ref blacklistedGroups);

                if (!blacklistedGroups.Contains(selectedGroup)) blacklistedGroups.Add(selectedGroup);
                return new List<IShiftCategoryFairnessCompareValue>();

            }

            var category = secondCategory;
            selectedGroupBlackListItems = blacklist.Where(
                g => (g.Group1 == selectedGroup && g.ShiftCategoryFromGroup2 == category.ShiftCategory)
                     || (g.Group2 == selectedGroup && g.ShiftCategoryFromGroup1 == category.ShiftCategory)).ToList();

            
            // ES: if all swaps for selected group lowest category is blacklisted
            if (selectedGroupBlackListItems.Count() == availableSwaps)
            {
                exceptList = new List<IShiftCategoryFairnessCompareValue> {secondCategory};
                if (selectedGroupCategoriesOrdered.Except(exceptList).Any(b => b.Original < b.ComparedTo))
                    return GetGroupCategories(selectedGroup, selectedGroupCategoriesOrdered.Except(exceptList),
                                              numberOfGroups, blacklist.Except(selectedGroupBlackListItems),
                                              ref blacklistedGroups);

                if (!blacklistedGroups.Contains(selectedGroup)) blacklistedGroups.Add(selectedGroup);
                return new List<IShiftCategoryFairnessCompareValue>();

            }
            return new List<IShiftCategoryFairnessCompareValue> {firstCategory, secondCategory};
        }
    }
}
