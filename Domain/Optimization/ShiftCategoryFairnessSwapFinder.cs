using System.Collections.Generic;
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
        private readonly IShiftCategoryFairnessCategorySorter _shiftCategoryFairnessCategorySorter;

        public ShiftCategoryFairnessSwapFinder(IShiftCategoryFairnessCategorySorter shiftCategoryFairnessCategorySorter)
        {
            _shiftCategoryFairnessCategorySorter = shiftCategoryFairnessCategorySorter;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")
        , System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1062:Validate arguments of public methods", MessageId = "1"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
             "CA1062:Validate arguments of public methods", MessageId = "0"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IShiftCategoryFairnessSwap GetGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> groupList,
                                                          IList<IShiftCategoryFairnessSwap> blacklist)
        {
            if (groupList.Count < 2) return null;

            var orderedList = groupList.OrderByDescending(g => g.StandardDeviation);
            var selectedGroup = orderedList.First();

            var selectedGroupBlacklistedSwapCount = blacklist.Count(b => b.Group1 == selectedGroup);
            var categoryCount = selectedGroup.ShiftCategoryFairnessCompareValues.Count(g => g.ComparedTo > 0 || g.Original > 0) - 1;

            // ES: if all swaps for selectedGroup have been blacklisted
            if (selectedGroupBlacklistedSwapCount >= (categoryCount*(categoryCount + 1)/2)*(groupList.Count - 1))
                return GetGroupsToSwap(groupList.Skip(1).ToList(), blacklist);

            // ES: get ordered list, check categories against blacklist
            var selectedGroupCategories =
                _shiftCategoryFairnessCategorySorter.GetGroupCategories(selectedGroup,
                                                                        selectedGroup.
                                                                            ShiftCategoryFairnessCompareValues.Where(
                                                                                g => g.ComparedTo > 0 || g.Original > 0)
                                                                                .OrderByDescending(
                                                                                g => g.Original),
                                                                        orderedList.Count(),
                                                                        blacklist).ToList();

            var selectedGroupHighestCategory = selectedGroupCategories.First().ShiftCategory;
            var selectedGroupLowestCategory = selectedGroupCategories.Last().ShiftCategory;
            
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

            // ES: I cannot see how this would ever happend, worstcase is that we get a bad swap but returnGroup should never be null
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

                if (currentGroupLowestCategoryOrignial.Equals(0)) continue;

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