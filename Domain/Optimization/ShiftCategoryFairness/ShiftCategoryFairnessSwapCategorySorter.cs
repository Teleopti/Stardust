using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public interface IShiftCategoryFairnessCategorySorter
    {
        IEnumerable<IShiftCategoryFairnessCompareValue> GetGroupCategories(
            IShiftCategoryFairnessCompareResult selectedGroup,
            IEnumerable<IShiftCategoryFairnessCompareValue> selectedGroupCategories,
            int numberOfGroups,
            IEnumerable<IShiftCategoryFairnessSwap> blacklist,
            ref List<IShiftCategoryFairnessCompareResult> blacklistedGroups);
    }

    public class ShiftCategoryFairnessSwapCategorySorter : IShiftCategoryFairnessCategorySorter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
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
                selectedGroupCategoriesOrdered.FirstOrDefault();
            var secondCategory =
                selectedGroupCategoriesOrdered.LastOrDefault();
            
            if (firstCategory == null || secondCategory == null)
            {
                if (!blacklistedGroups.Contains(selectedGroup)) 
                    blacklistedGroups.Add(selectedGroup);
                return new List<IShiftCategoryFairnessCompareValue>();
            }

            List<IShiftCategoryFairnessCompareValue> exceptList;

            //  if all swaps for selected group highest category are blacklisted
            var availableSwaps = (numberOfGroups - 1) * selectedGroupCategoriesOrdered.Count(g => g.Original < g.ComparedTo);
            var selectedGroupBlackListItems =
                blacklist.Where(
                    g => (g.Group1 == selectedGroup && g.ShiftCategoryFromGroup1 == firstCategory.ShiftCategory)
                         || (g.Group2 == selectedGroup && g.ShiftCategoryFromGroup2 == firstCategory.ShiftCategory))
                    .ToList();
            if (availableSwaps == 0 || selectedGroupBlackListItems.Count() >= availableSwaps)
            {
                exceptList = new List<IShiftCategoryFairnessCompareValue> {firstCategory};
                if (selectedGroupCategoriesOrdered.Except(exceptList).Any(b => b.Original > b.ComparedTo))
                    return GetGroupCategories(selectedGroup, selectedGroupCategoriesOrdered.Except(exceptList),
                                              numberOfGroups, blacklist.Except(selectedGroupBlackListItems),
                                              ref blacklistedGroups);

                if (!blacklistedGroups.Contains(selectedGroup)) blacklistedGroups.Add(selectedGroup);
                return new List<IShiftCategoryFairnessCompareValue>();
            }
            
            //  if all swaps for selected group lowest category are blacklisted
            availableSwaps = (numberOfGroups - 1)*selectedGroupCategoriesOrdered.Count(g => g.Original > g.ComparedTo);
            selectedGroupBlackListItems = blacklist.Where(
                g => (g.Group1 == selectedGroup && g.ShiftCategoryFromGroup2 == secondCategory.ShiftCategory)
                     || (g.Group2 == selectedGroup && g.ShiftCategoryFromGroup1 == secondCategory.ShiftCategory))
                .ToList();
            if (availableSwaps == 0 || selectedGroupBlackListItems.Count() >= availableSwaps)
            {
                exceptList = new List<IShiftCategoryFairnessCompareValue> {secondCategory};
                if (selectedGroupCategoriesOrdered.Except(exceptList).Any(b => b.Original < b.ComparedTo))
                    return GetGroupCategories(selectedGroup, selectedGroupCategoriesOrdered.Except(exceptList),
                                              numberOfGroups, blacklist.Except(selectedGroupBlackListItems),
                                              ref blacklistedGroups);

                if (!blacklistedGroups.Contains(selectedGroup)) blacklistedGroups.Add(selectedGroup);
                return new List<IShiftCategoryFairnessCompareValue>();
            }

            // if current swap is blacklisted and we need to reselect category
            selectedGroupBlackListItems = blacklist.Where(
                b => (b.Group1 == selectedGroup
                     && b.ShiftCategoryFromGroup1 == firstCategory.ShiftCategory
                     && b.ShiftCategoryFromGroup2 == secondCategory.ShiftCategory)).ToList();
            var selectedGroupBlackListItemsCount = selectedGroupBlackListItems.Count();
            if (selectedGroupBlackListItemsCount == numberOfGroups - 1)
            {
                var firstCategoryCompare =
                    selectedGroupCategoriesOrdered.Skip(1).FirstOrDefault();
                var secondCategoryCompare =
                    selectedGroupCategoriesOrdered.Take(selectedGroupCategoriesOrdered.Count() - 1).LastOrDefault();
                if (firstCategoryCompare == null || secondCategoryCompare == null) return new List<IShiftCategoryFairnessCompareValue>();
                var firstDiff = firstCategoryCompare.Original - firstCategoryCompare.ComparedTo;
                var secondDiff = secondCategoryCompare.ComparedTo - secondCategory.Original;

                var exceptedCategory = firstDiff > secondDiff ? firstCategory : secondCategory;
                exceptList = new List<IShiftCategoryFairnessCompareValue> { exceptedCategory };

                if (selectedGroupCategoriesOrdered.Except(exceptList).Any())
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
