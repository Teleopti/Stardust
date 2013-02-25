using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public interface IShiftCategoryFairnessSwapFinder
    {
        IShiftCategoryFairnessSwap GetGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> groupList,
                                                   IList<IShiftCategoryFairnessSwap> blacklist);

        IList<IShiftCategoryFairnessSwap> GetGroupListOfSwaps(
            IList<IShiftCategoryFairnessCompareResult> groupList,
            IList<IShiftCategoryFairnessSwap> blacklist);

	    IEnumerable<IShiftCategoryFairnessSwap> GetAllGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> groupList);
    }

    public class ShiftCategoryFairnessSwapFinder : IShiftCategoryFairnessSwapFinder
    {
        private static List<IShiftCategoryFairnessCompareResult> _blacklistedGroups =
                new List<IShiftCategoryFairnessCompareResult>();

        private readonly IShiftCategoryFairnessCategorySorter _shiftCategoryFairnessCategorySorter;


        public ShiftCategoryFairnessSwapFinder(IShiftCategoryFairnessCategorySorter shiftCategoryFairnessCategorySorter)
        {
            _shiftCategoryFairnessCategorySorter = shiftCategoryFairnessCategorySorter;
        }


        public IList<IShiftCategoryFairnessSwap> GetGroupListOfSwaps(IList<IShiftCategoryFairnessCompareResult> groupList,
                                                          IList<IShiftCategoryFairnessSwap> blacklist)
        {
            _blacklistedGroups.Clear();
            var worstGroup = groupList.OrderByDescending(g => g.StandardDeviation).FirstOrDefault();
            if (worstGroup == null)
                return null;
            var categoryCount =
                worstGroup.ShiftCategoryFairnessCompareValues.Count(g => g.Original > 0 || g.ComparedTo > 0) - 1;
            var numberOfTries = (categoryCount * (categoryCount + 1) / 2) * (groupList.Count - 1);

            var returnList = new List<IShiftCategoryFairnessSwap>();
            for (var i = 0; i <= numberOfTries; i++)
            {
                var item = GetGroupsToSwap(groupList, blacklist);
                if (item == null)
                    continue;
                if (returnList.Contains(item))
                    blacklist.Add(item);
                else
                {
                    blacklist.Add(item);
                    returnList.Add(item);
                }
            }
            return returnList;
        }

	    public IShiftCategoryFairnessSwap GetGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> groupList,
                                                          IList<IShiftCategoryFairnessSwap> blacklist)
        {
            var ret = getGroupsToSwap(groupList, blacklist);
            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IShiftCategoryFairnessSwap getGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> groupList,
                                                              IList<IShiftCategoryFairnessSwap> blacklist)
        {
            if (groupList.Count() < 2) return null;
            var orderedList = groupList.Except(_blacklistedGroups).OrderByDescending(g => g.StandardDeviation);
            if (!orderedList.Any())
                return null;

            var selectedGroup = orderedList.First();
            var selectedGroupBlacklistedSwapCount = blacklist.Count(b => b.Group1 == selectedGroup);
            var impossibleSwaps =
                selectedGroup.ShiftCategoryFairnessCompareValues.Count(v => !v.Original.Equals(0) &&  v.Original.Equals(v.ComparedTo));
            var categoryCount =
                selectedGroup.ShiftCategoryFairnessCompareValues.Count(g => g.ComparedTo > 0 || g.Original > 0) - 1 -
                impossibleSwaps;

            // if all swaps for selectedGroup have been blacklisted
            if (selectedGroupBlacklistedSwapCount >= (categoryCount*(categoryCount + 1)/2)*(groupList.Count - 1))
            {
                _blacklistedGroups.Add(selectedGroup);
                return getGroupsToSwap(orderedList.ToList(), blacklist);
            }

            // get ordered list, check categories against blacklist
            var selectedGroupCategories =
                _shiftCategoryFairnessCategorySorter.GetGroupCategories(selectedGroup,
                                                                        selectedGroup.
                                                                            ShiftCategoryFairnessCompareValues
                                                                            .Where(
                                                                                g => g.ComparedTo > 0 || g.Original > 0),
                                                                        orderedList.Count(),
                                                                        blacklist, ref _blacklistedGroups).ToList();
            if (!selectedGroupCategories.Any())
                return getGroupsToSwap(groupList, blacklist);

            var selectedGroupHighestCategory = selectedGroupCategories.First().ShiftCategory;
            var selectedGroupLowestCategory = selectedGroupCategories.Last().ShiftCategory;
            var selectedGroupHighestCategoryOriginal = selectedGroupCategories.First().Original;
            var selectedGroupLowestCategoryOriginal = selectedGroupCategories.Last().Original;

            var tempGroup = orderedList.Skip(1).FirstOrDefault();
            var returnGroup = tempGroup;
            if (returnGroup == null) return null;

            var returnGroupHighestCategoryOriginal =
                returnGroup.ShiftCategoryFairnessCompareValues.First(
                    s => s.ShiftCategory.Description.Name == selectedGroupHighestCategory.Description.Name).
                    Original;

            var returnGroupLowestCategoryOrignial =
                returnGroup.ShiftCategoryFairnessCompareValues.First(
                    s => s.ShiftCategory.Description.Name == selectedGroupLowestCategory.Description.Name).
                    Original;
            var haveChanged = false;
            var skippedGroups = 0;

            foreach (var currentGroup in orderedList.Skip(1))
            {
                if (blacklist.Contains(new ShiftCategoryFairnessSwap
                                           {
                                               Group1 = selectedGroup,
                                               Group2 = currentGroup,
                                               ShiftCategoryFromGroup1 = selectedGroupHighestCategory,
                                               ShiftCategoryFromGroup2 = selectedGroupLowestCategory
                                           }))
                {
                    skippedGroups++;
                    continue;
                }

                var currentGroupLowestCategory =
                    currentGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupLowestCategory.Description.Name);
                if (currentGroupLowestCategory.Original <= currentGroupLowestCategory.ComparedTo)
                {
                    skippedGroups++;
                    continue;
                }

                var currentGroupHighestCategory =
                    currentGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupHighestCategory.Description.Name);
                if (currentGroupHighestCategory.Original >= currentGroupHighestCategory.ComparedTo)
                {
                    skippedGroups++;
                    continue;
                }

                var currentGroupHighestCategoryOriginal = currentGroupHighestCategory.Original;
                var currentGroupLowestCategoryOrignial = currentGroupLowestCategory.Original;


                if (haveChanged)
                {
                    returnGroupHighestCategoryOriginal =
                        returnGroup.ShiftCategoryFairnessCompareValues.First(
                            s => s.ShiftCategory.Description.Name == selectedGroupHighestCategory.Description.Name).
                            Original;

                    returnGroupLowestCategoryOrignial =
                        returnGroup.ShiftCategoryFairnessCompareValues.First(
                            s => s.ShiftCategory.Description.Name == selectedGroupLowestCategory.Description.Name).
                            Original;
                }


                if (returnGroup != currentGroup
                    && returnGroupHighestCategoryOriginal - currentGroupHighestCategoryOriginal
                       >= returnGroupLowestCategoryOrignial - currentGroupLowestCategoryOrignial
                    && returnGroupHighestCategoryOriginal >= currentGroupHighestCategoryOriginal
                    && returnGroupLowestCategoryOrignial <= currentGroupLowestCategoryOrignial
                    && currentGroupHighestCategoryOriginal < selectedGroupHighestCategoryOriginal
                    && currentGroupLowestCategoryOrignial > selectedGroupLowestCategoryOriginal)
                {
                    returnGroup = currentGroup;
                    haveChanged = true;
                }

                else
                    haveChanged = false;
            }

            var returnSuggestion = new ShiftCategoryFairnessSwap
                                       {
                                           Group1 = selectedGroup,
                                           Group2 = returnGroup,
                                           ShiftCategoryFromGroup1 = selectedGroupHighestCategory,
                                           ShiftCategoryFromGroup2 = selectedGroupLowestCategory
                                       };
            
            if (skippedGroups == groupList.Count - 1)
            {
                blacklist.Add(returnSuggestion);
                return getGroupsToSwap(orderedList.ToList(), blacklist);
            }

            if (blacklist.Contains(returnSuggestion))
            {
                _blacklistedGroups.Add(selectedGroup);
                return null;
            }

            return returnSuggestion;
        }

		public IEnumerable<IShiftCategoryFairnessSwap> GetAllGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> groupList)
		{
			var ret = new List<IShiftCategoryFairnessSwap>();
			foreach (var selectedGroup in groupList)
			{
				foreach (var otherGroup in groupList)
				{
					if(otherGroup.Equals(selectedGroup)) continue;
					foreach (var shiftCategoryFairnessCompareValue in selectedGroup.ShiftCategoryFairnessCompareValues)
					{
						var selectedCat = shiftCategoryFairnessCompareValue.ShiftCategory;
						foreach (var categoryFairnessCompareValue in otherGroup.ShiftCategoryFairnessCompareValues)
						{
							
							if(selectedCat.Equals(categoryFairnessCompareValue.ShiftCategory)) continue;

							// trade away something there are too few off
							if (shiftCategoryFairnessCompareValue.Original < shiftCategoryFairnessCompareValue.ComparedTo)
							{
								// and the other has too many, no good
								if (hasMoreOfCategory(selectedCat, otherGroup.ShiftCategoryFairnessCompareValues))
								{
									// and the other have too few of what they will trade away
									if (categoryFairnessCompareValue.ComparedTo > categoryFairnessCompareValue.Original)
									{
										// and group one has too many of them, puh
										if (hasMoreOfCategory(categoryFairnessCompareValue.ShiftCategory, selectedGroup.ShiftCategoryFairnessCompareValues))
											continue;
									}
								}
							}
							
							ret.Add(new ShiftCategoryFairnessSwap
								{
									Group1 = selectedGroup,
									Group2 = otherGroup,
									ShiftCategoryFromGroup1 = selectedCat,
									ShiftCategoryFromGroup2 = categoryFairnessCompareValue.ShiftCategory
								});
						}
					}
				}
			}
			return ret;
		}


		private bool hasMoreOfCategory(IShiftCategory shiftCategory, IEnumerable<IShiftCategoryFairnessCompareValue> values)
		{
			return (from shiftCategoryFairnessCompareValue in values where shiftCategoryFairnessCompareValue.ShiftCategory.Equals(shiftCategory) select shiftCategoryFairnessCompareValue.Original > shiftCategoryFairnessCompareValue.ComparedTo).FirstOrDefault();
		}
    }
}