using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class SwapFairnessTest
	{
		private SwapFairness _target;
		private IList<ShiftCategoryFairnessCompareResult> _groupList;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private Dictionary<Guid, IList<SwapFairnessGroupAndCategory>> _blackList;

        [SetUp]
        public void Setup()
        {
            var group1 = new ShiftCategoryFairnessCompareResult
                             {
                                 ShiftCategoryFairnessCompareValues =
                                     new List<ShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.9, ComparedTo = 0.3, ShiftCategory = new ShiftCategory("Day") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.1, ComparedTo = 0.5, ShiftCategory = new ShiftCategory("Noon") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.0, ComparedTo = 0.2, ShiftCategory = new ShiftCategory("Night") }
                                         },
                                 StandardDeviation = 0.1
                             };

            var group2 = new ShiftCategoryFairnessCompareResult
                             {
                                 ShiftCategoryFairnessCompareValues =
                                     new List<ShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.0, ComparedTo = 0.3, ShiftCategory = new ShiftCategory("Day") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.4, ComparedTo = 0.5, ShiftCategory = new ShiftCategory("Noon") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.6, ComparedTo = 0.2, ShiftCategory = new ShiftCategory("Night") }
                                         },
                                 StandardDeviation = 0.02
                             };

            var group3 = new ShiftCategoryFairnessCompareResult
                             {
                                 ShiftCategoryFairnessCompareValues =
                                     new List<ShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.1, ComparedTo = 0.3, ShiftCategory = new ShiftCategory("Day") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.1, ComparedTo = 0.5, ShiftCategory = new ShiftCategory("Noon") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.8, ComparedTo = 0.2, ShiftCategory = new ShiftCategory("Night") }
                                         },
                                 StandardDeviation = 0.03
                             };

            var group4 = new ShiftCategoryFairnessCompareResult
                             {
                                 ShiftCategoryFairnessCompareValues =
                                     new List<ShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.0, ComparedTo = 0.3, ShiftCategory = new ShiftCategory("Day") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.3, ComparedTo = 0.5, ShiftCategory = new ShiftCategory("Noon") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.7, ComparedTo = 0.2, ShiftCategory = new ShiftCategory("Night") } 
                                         },
                                 StandardDeviation = 0.04
                             };

            var group5 = new ShiftCategoryFairnessCompareResult
            {
                ShiftCategoryFairnessCompareValues =
                    new List<ShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.1, ComparedTo = 0.3, ShiftCategory = new ShiftCategory("Day") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.2, ComparedTo = 0.5, ShiftCategory = new ShiftCategory("Noon") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.7, ComparedTo = 0.2, ShiftCategory = new ShiftCategory("Night") } 
                                         },
                StandardDeviation = 0.05
            };

            _groupList = new List<ShiftCategoryFairnessCompareResult>
                             {
                                 group1,
                                 group2,
                                 group3,
                                 group4,
                                 group5
                             };

            _blackList = new Dictionary<Guid, IList<SwapFairnessGroupAndCategory>>
                             {
                                 {
                                     group1.Id, new List<SwapFairnessGroupAndCategory>
                                                    {
                                                        new SwapFairnessGroupAndCategory
                                                            {
                                                                Group1 = group1,
                                                                Group2 = group4,
                                                                ShiftCategoryFromGroup1 = new ShiftCategory("Day"),
                                                                ShiftCategoryFromGroup2 = new ShiftCategory("Night")
                                                            }
                                                    }
                                     }
                             };

        	_target = new SwapFairness();
        }

	    [Test]
		public void ShouldReturnTwoGroupsForSwapping()
		{
            var result = _target.GetGroupsToSwap(_groupList, new Dictionary<Guid, IList<SwapFairnessGroupAndCategory>>());
            Assert.AreEqual(0.1, result.Group1.StandardDeviation);
            Assert.AreEqual("Day",result.ShiftCategoryFromGroup1.Description.Name);
            Assert.AreEqual(0.04, result.Group2.StandardDeviation);
            Assert.AreEqual("Night", result.ShiftCategoryFromGroup2.Description.Name);
		}

	    [Test]
	    public void ShouldReturnNullIfGroupListHasOnlyOneItem()
	    {
	        var result = _target.GetGroupsToSwap(
	            new List<ShiftCategoryFairnessCompareResult>
	                {
	                    new ShiftCategoryFairnessCompareResult()
	                }, new Dictionary<Guid, IList<SwapFairnessGroupAndCategory>>());
            Assert.AreEqual(null, result.Group1);
	    }
        /*
        [Test]
        public void ShouldNotReturnBlacklistedGroupSwap()
        {
            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreEqual(0.1,result.Group1.StandardDeviation);
            Assert.AreEqual(0.03,result.Group2.StandardDeviation);
        }*/
	}

    public class SwapFairnessGroupAndCategory
    {
        public ShiftCategoryFairnessCompareResult Group1 { get; set; }

        public ShiftCategoryFairnessCompareResult Group2 { get; set; }
        public IShiftCategory ShiftCategoryFromGroup1 { get; set; }
        public IShiftCategory ShiftCategoryFromGroup2 { get; set; }
    }

    public class SwapFairness
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private bool IsEligibleForSwap(Guid groupId, IShiftCategory shiftCategory ,Dictionary<Guid, IList<IShiftCategory>> blackList)
		{
			if(blackList.ContainsKey(groupId))
			{
				var categoriesList = blackList[groupId];
				return !categoriesList.Contains(shiftCategory);
			}

			return true;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "blacklist"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public SwapFairnessGroupAndCategory GetGroupsToSwap(IList<ShiftCategoryFairnessCompareResult> inList, Dictionary<Guid, IList<SwapFairnessGroupAndCategory>> blacklist)
        {
            if (inList.Count < 2) return new SwapFairnessGroupAndCategory();


            var orderedList = inList.OrderByDescending(g => g.StandardDeviation);
            var selectedGroup = orderedList.First();
            var returnGroup = orderedList.Skip(1).First();

            var selectedGroupCategories = selectedGroup.ShiftCategoryFairnessCompareValues.OrderByDescending(g => g.Original);
            var selectedGroupHighestCategory = selectedGroupCategories.First().ShiftCategory;
            var selectedGroupLowestCategory = selectedGroupCategories.Last().ShiftCategory;

            foreach (var currentGroup in orderedList.Skip(1))
            {
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

                // retHigh - currHigh >= retLow - currLow
                // && retHigh >= currHigh
                // && retLow <= currLow
                returnGroup = returnGroupHighestCategoryOriginal - currentGroupHighestCategoryOriginal
                    >= returnGroupLowestCategoryOrignial - currentGroupLowestCategoryOrignial
                    && returnGroupHighestCategoryOriginal >= currentGroupHighestCategoryOriginal
                    && returnGroupLowestCategoryOrignial <= currentGroupLowestCategoryOrignial
                                  ? currentGroup
                                  : returnGroup;
            }

            return new SwapFairnessGroupAndCategory
                       {
                           Group1 = selectedGroup,
                           Group2 = returnGroup,
                           ShiftCategoryFromGroup1 = selectedGroupHighestCategory,
                           ShiftCategoryFromGroup2 = selectedGroupLowestCategory
                       };
        }
	}
}
