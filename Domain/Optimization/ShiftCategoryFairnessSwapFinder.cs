using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IShiftCategoryFairnessSwap
    {
        ShiftCategoryFairnessCompareResult Group1 { get; set; }
        ShiftCategoryFairnessCompareResult Group2 { get; set; }
        IShiftCategory ShiftCategoryFromGroup1 { get; set; }
        IShiftCategory ShiftCategoryFromGroup2 { get; set; }
    }


    public interface IShiftCategoryFairnessSwapFinder
    {
        ShiftCategoryFairnessSwap GetGroupsToSwap(IList<ShiftCategoryFairnessCompareResult> inList,
                                                  IList<ShiftCategoryFairnessSwap> blacklist);
    }


    public class ShiftCategoryFairnessSwap : IShiftCategoryFairnessSwap
    {
        public ShiftCategoryFairnessCompareResult Group1 { get; set; }
        public ShiftCategoryFairnessCompareResult Group2 { get; set; }
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
        private static IOrderedEnumerable<ShiftCategoryFairnessCompareValue> GetGroupCategories(
            ShiftCategoryFairnessCompareResult selectedGroup,
            IOrderedEnumerable<ShiftCategoryFairnessCompareValue> selectedGroupCategories,
            int orderedListCount,
            IEnumerable<ShiftCategoryFairnessSwap> blacklist)
        {
            if (orderedListCount == int.MaxValue || orderedListCount == int.MinValue) throw new ArgumentOutOfRangeException("orderedListCount");
            return blacklist.Count(g => g.Group1 == selectedGroup || g.Group2 == selectedGroup)
                   == ((orderedListCount - 1)*(selectedGroupCategories.Count() - 1))
                       ? GetGroupCategories(selectedGroup,
                                            selectedGroupCategories.Skip(1).OrderByDescending(c => c.Original),
                                            orderedListCount, blacklist)
                       : selectedGroupCategories.OrderByDescending(c => c.Original);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "blacklist"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ShiftCategoryFairnessSwap GetGroupsToSwap(IList<ShiftCategoryFairnessCompareResult> inList,
                                                         IList<ShiftCategoryFairnessSwap> blacklist)
        {
            if (inList.Count < 2) return null;

            var orderedList = inList.OrderByDescending(g => g.StandardDeviation);
            var selectedGroup = orderedList.First();

            var selectedGroupBlacklistedSwapCount = blacklist.Count(b => b.Group1 == selectedGroup);
            var categoryCount = selectedGroup.ShiftCategoryFairnessCompareValues.Count() - 1;

            // if all swaps for selectedGroup have been blacklisted
            if (selectedGroupBlacklistedSwapCount >= (categoryCount*(categoryCount + 1)/2)*(inList.Count - 1))
                return GetGroupsToSwap(inList.Skip(1).ToList(), blacklist);

            // get ordered list, checked against blacklist
            var selectedGroupCategories = GetGroupCategories(selectedGroup,
                                                             selectedGroup.ShiftCategoryFairnessCompareValues.
                                                                 OrderByDescending(g => g.Original), orderedList.Count(),
                                                             blacklist);

            var selectedGroupHighestCategory = selectedGroupCategories.First().ShiftCategory;
            var selectedGroupLowestCategory = selectedGroupCategories.Last().ShiftCategory;

            var returnGroup = orderedList.Skip(1).First();
            foreach (var currentGroup in orderedList.Skip(1))
            {
                if (blacklist.Contains(new ShiftCategoryFairnessSwap
                                           {
                                               Group1 = selectedGroup,
                                               Group2 = currentGroup,
                                               ShiftCategoryFromGroup1 = selectedGroupHighestCategory,
                                               ShiftCategoryFromGroup2 = selectedGroupLowestCategory
                                           })) continue;

                var currentGroupHighestCategoryOriginal =
                    currentGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupHighestCategory.Description.Name).Original;

                var returnGroupHighestCategoryOriginal =
                    returnGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupHighestCategory.Description.Name).Original;

                var currentGroupLowestCategoryOrignial =
                    currentGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupLowestCategory.Description.Name).Original;

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
