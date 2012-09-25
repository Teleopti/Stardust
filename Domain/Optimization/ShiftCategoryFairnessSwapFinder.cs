using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IShiftCategoryFairnessSwap
    {
        IShiftCategoryFairnessCompareResult Group1 { get; set; }
        IShiftCategoryFairnessCompareResult Group2 { get; set; }
        IShiftCategory ShiftCategoryFromGroup1 { get; set; }
        IShiftCategory ShiftCategoryFromGroup2 { get; set; }
    }


    public interface IShiftCategoryFairnessSwapFinder
    {
        IShiftCategoryFairnessSwap GetGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> inList,
                                                   IList<IShiftCategoryFairnessSwap> blacklist);
    }


    public class ShiftCategoryFairnessSwap : IShiftCategoryFairnessSwap
    {
        public IShiftCategoryFairnessCompareResult Group1 { get; set; }
        public IShiftCategoryFairnessCompareResult Group2 { get; set; }
        public IShiftCategory ShiftCategoryFromGroup1 { get; set; }
        public IShiftCategory ShiftCategoryFromGroup2 { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (ShiftCategoryFairnessSwap) && Equals((ShiftCategoryFairnessSwap) obj);
        }

        public bool Equals(ShiftCategoryFairnessSwap other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return (Equals(other.Group1, Group1) || Equals(other.Group1, Group2)) &&
                   (Equals(other.Group2, Group2) || Equals(other.Group2, Group1)) &&
                   (other.ShiftCategoryFromGroup1.Description.Name == ShiftCategoryFromGroup1.Description.Name
                    || other.ShiftCategoryFromGroup1.Description.Name == ShiftCategoryFromGroup2.Description.Name)
                   &&
                   (other.ShiftCategoryFromGroup2.Description.Name == ShiftCategoryFromGroup2.Description.Name
                    || other.ShiftCategoryFromGroup2.Description.Name == ShiftCategoryFromGroup1.Description.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = (Group1 != null ? Group1.GetHashCode() : 0);
                result = (result*397) ^ (Group2 != null ? Group2.GetHashCode() : 0);
                result = (result*397) ^ (ShiftCategoryFromGroup1 != null ? ShiftCategoryFromGroup1.GetHashCode() : 0);
                result = (result*397) ^ (ShiftCategoryFromGroup2 != null ? ShiftCategoryFromGroup2.GetHashCode() : 0);
                return result;
            }
        }
    }


    public class ShiftCategoryFairnessSwapFinder : IShiftCategoryFairnessSwapFinder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1062:Validate arguments of public methods", MessageId = "1"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
             "CA1062:Validate arguments of public methods", MessageId = "0"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
             MessageId = "blacklist"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IShiftCategoryFairnessSwap GetGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> inList,
                                                          IList<IShiftCategoryFairnessSwap> blacklist)
        {
            if (inList.Count < 2) return null;

            var orderedList = inList.OrderByDescending(g => g.StandardDeviation);
            var selectedGroup = orderedList.First();

            var selectedGroupBlacklistedSwapCount = blacklist.Count(b => b.Group1 == selectedGroup);
            var categoryCount = selectedGroup.ShiftCategoryFairnessCompareValues.Count() - 1;

            // if all swaps for selectedGroup have been blacklisted
            if (selectedGroupBlacklistedSwapCount >= (categoryCount*(categoryCount + 1)/2)*(inList.Count - 1))
                return GetGroupsToSwap(inList.Skip(1).ToList(), blacklist);

            // get ordered list, check categories against blacklist
            var selectedGroupCategories = ShiftCategoryFairnessCategorySorter.GetGroupCategories(selectedGroup,
                                                                                                 selectedGroup.
                                                                                                     ShiftCategoryFairnessCompareValues
                                                                                                     .
                                                                                                     OrderByDescending(
                                                                                                         g => g.Original),
                                                                                                 orderedList.Count(),
                                                                                                 blacklist).ToList();

            var selectedGroupHighestCategory = selectedGroupCategories.First().ShiftCategory;
            var selectedGroupLowestCategory = selectedGroupCategories.Last().ShiftCategory;

            var returnGroup = orderedList.Except(new List<IShiftCategoryFairnessCompareResult>{selectedGroup}).SkipWhile(b => (blacklist.Contains(new ShiftCategoryFairnessSwap
                                                                                 {

                                                                                     Group1 = selectedGroup,
                                                                                     Group2 = b,
                                                                                     ShiftCategoryFromGroup1 =
                                                                                         selectedGroupHighestCategory,
                                                                                     ShiftCategoryFromGroup2 =
                                                                                         selectedGroupLowestCategory
                                                                                 }))).First();

            // I cannot see how this would ever happend, worstcase is that we get a bad swap but returnGroup should never be null
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
