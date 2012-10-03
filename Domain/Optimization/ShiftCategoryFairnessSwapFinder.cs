﻿using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IShiftCategoryFairnessSwapFinder
    {
        IShiftCategoryFairnessSwap GetGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> groupList,
                                                   IList<IShiftCategoryFairnessSwap> blacklist);
    }

    public class ShiftCategoryFairnessSwapFinder : IShiftCategoryFairnessSwapFinder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<IShiftCategoryFairnessCompareResult> BlacklistedGroups = new List<IShiftCategoryFairnessCompareResult>(); 
        private readonly IShiftCategoryFairnessCategorySorter _shiftCategoryFairnessCategorySorter;

        public ShiftCategoryFairnessSwapFinder(IShiftCategoryFairnessCategorySorter shiftCategoryFairnessCategorySorter)
        {
            _shiftCategoryFairnessCategorySorter = shiftCategoryFairnessCategorySorter;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design","CA1062:Validate arguments of public methods", MessageId = "1"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design","CA1062:Validate arguments of public methods", MessageId = "0")]
        public IShiftCategoryFairnessSwap GetGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> groupList,
                                                          IList<IShiftCategoryFairnessSwap> blacklist)
        {
            var orderedList = groupList.Except(BlacklistedGroups).OrderByDescending(g => g.StandardDeviation);
            if (groupList.Count < 2) return null;

            var selectedGroup = orderedList.First();
            var selectedGroupBlacklistedSwapCount = blacklist.Count(b => b.Group1 == selectedGroup);
            var categoryCount = selectedGroup.ShiftCategoryFairnessCompareValues.Count(g => g.ComparedTo > 0 || g.Original > 0) - 1;

            // ES: if all swaps for selectedGroup have been blacklisted
            if (selectedGroupBlacklistedSwapCount >= (categoryCount*(categoryCount + 1)/2)*(groupList.Count - 1))
            {
                if (!BlacklistedGroups.Contains(selectedGroup)) BlacklistedGroups.Add(selectedGroup);
                return GetGroupsToSwap(groupList.Skip(1).ToList(), blacklist);
            }

		    var firstCount = BlacklistedGroups.Count;
            // ES: get ordered list, check categories against blacklist
            var selectedGroupCategories =
                _shiftCategoryFairnessCategorySorter.GetGroupCategories(selectedGroup,
                                                                        selectedGroup.
                                                                            ShiftCategoryFairnessCompareValues.Where(
                                                                                g => g.ComparedTo > 0 || g.Original > 0),
                                                                        orderedList.Count(),
                                                                        blacklist, ref BlacklistedGroups).ToList();
            if (firstCount < BlacklistedGroups.Count)
                return GetGroupsToSwap(groupList, blacklist);

            var selectedGroupHighestCategory = selectedGroupCategories.First().ShiftCategory;
            var selectedGroupLowestCategory = selectedGroupCategories.Last().ShiftCategory;
		    var selectedGroupHighestCategoryOriginal = selectedGroupCategories.First().Original;
		    var selectedGroupLowestCategoryOriginal = selectedGroupCategories.Last().Original;

            var returnGroup =
                orderedList.Except(new List<IShiftCategoryFairnessCompareResult> {selectedGroup}).SkipWhile(
                    b => (blacklist.Contains(new ShiftCategoryFairnessSwap
                                                 {
                                                     Group1 = selectedGroup,
                                                     Group2 = b,
                                                     ShiftCategoryFromGroup1 =
                                                         selectedGroupHighestCategory,
                                                     ShiftCategoryFromGroup2 =
                                                         selectedGroupLowestCategory
                                                 }))).FirstOrDefault();

            if (returnGroup == null) return null;

            foreach (var currentGroup in orderedList.Skip(1))
            {
                if (blacklist.Contains(new ShiftCategoryFairnessSwap
                                           {
                                               Group1 = selectedGroup,
                                               Group2 = currentGroup,
                                               ShiftCategoryFromGroup1 = selectedGroupHighestCategory,
                                               ShiftCategoryFromGroup2 = selectedGroupLowestCategory
                                           })) continue;

                var currentGroupLowestCategoryOrignial =
                    currentGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupLowestCategory.Description.Name).Original;
                
                var currentGroupHighestCategoryOriginal =
                    currentGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupHighestCategory.Description.Name).Original;

                var returnGroupHighestCategoryOriginal =
                    returnGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupHighestCategory.Description.Name).Original;

                var returnGroupLowestCategoryOrignial =
                    returnGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupLowestCategory.Description.Name).Original;

                returnGroup = returnGroupHighestCategoryOriginal - currentGroupHighestCategoryOriginal
                              >= returnGroupLowestCategoryOrignial - currentGroupLowestCategoryOrignial
                              && returnGroupHighestCategoryOriginal >= currentGroupHighestCategoryOriginal
                              && returnGroupLowestCategoryOrignial <= currentGroupLowestCategoryOrignial
                              && currentGroupHighestCategoryOriginal < selectedGroupHighestCategoryOriginal
                              && currentGroupLowestCategoryOrignial > selectedGroupLowestCategoryOriginal
                                  ? currentGroup
                                  : returnGroup;
            }

            return new ShiftCategoryFairnessSwap
                       {
                           Group1 = selectedGroup,
                           Group2 = returnGroup,
                           ShiftCategoryFromGroup1 = selectedGroupHighestCategory,
                           ShiftCategoryFromGroup2 = selectedGroupLowestCategory
                       };
        }
    }
}