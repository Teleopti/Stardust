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
		private SwapFariness _target;
		private IList<ShiftCategoryFairnessCompareResult> _groupList;
        private IList<SwapFairnessGroupAndCategory> _blackList;

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
                                                 { Original = 0.5, ComparedTo = 0.5, ShiftCategory = new ShiftCategory("Noon") },
                                             new ShiftCategoryFairnessCompareValue
                                                 { Original = 0.5, ComparedTo = 0.2, ShiftCategory = new ShiftCategory("Night") } 
                                         },
                                 StandardDeviation = 0.04
                             };


            _groupList = new List<ShiftCategoryFairnessCompareResult>
                             {
                                 group1,
                                 group2,
                                 group3,
                                 group4
                             };

            _blackList = new List<SwapFairnessGroupAndCategory>
                             {
                                 new SwapFairnessGroupAndCategory
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = new ShiftCategory("Day"),
                                         ShiftCategoryFromGroup2 = new ShiftCategory("Night")
                                     }
                             };

            _target = new SwapFariness();
        }

	    [Test]
		public void ShouldReturnTwoGroupsForSwapping()
		{
            var result = _target.GetGroupsToSwap(_groupList, new List<SwapFairnessGroupAndCategory>());
            Assert.AreEqual(0.1, result.Group1.StandardDeviation);
            Assert.AreEqual("Day",result.ShiftCategoryFromGroup1.Description.Name);
            Assert.AreEqual(0.02, result.Group2.StandardDeviation);
            Assert.AreEqual("Night", result.ShiftCategoryFromGroup2.Description.Name);
		}

	    [Test]
	    public void ShouldReturnEmptyListIfGroupListHasOnlyOneItem()
	    {
	        var result = _target.GetGroupsToSwap(
	            new List<ShiftCategoryFairnessCompareResult>
	                {
	                    new ShiftCategoryFairnessCompareResult()
	                }, new List<SwapFairnessGroupAndCategory>());
            Assert.AreEqual(null, result.Group1);
	    }

        [Test]
        public void ShouldNotReturnBlacklistedGroupSwap()
        {
            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreEqual(0.1,result.Group1.StandardDeviation);
            Assert.AreEqual(0.04,result.Group2.StandardDeviation);
        }

        [Test]
        public void ShouldReturnTwoBestGroupToSwapWith()
        {
            
        }
	}

    public class SwapFairnessGroupAndCategory
    {
        public ShiftCategoryFairnessCompareResult Group1;
        public ShiftCategoryFairnessCompareResult Group2;
        public IShiftCategory ShiftCategoryFromGroup1;
        public IShiftCategory ShiftCategoryFromGroup2;
    }

    public class SwapFariness
	{
        public SwapFairnessGroupAndCategory GetGroupsToSwap(IList<ShiftCategoryFairnessCompareResult> inList, IList<SwapFairnessGroupAndCategory> blackList)
        {
            if (inList.Count < 2) return new SwapFairnessGroupAndCategory();

            var orderedList = inList.OrderByDescending(g => g.StandardDeviation);
            var selectedGroup = orderedList.First();
            var returnGroup = orderedList.Skip(1).First();

            var selectedGroupCategories =
                selectedGroup.ShiftCategoryFairnessCompareValues.OrderByDescending(g => g.Original);
            var selectedGroupHighestCategory = selectedGroupCategories.First().ShiftCategory;
            var selectedGroupLowestCategory = selectedGroupCategories.Last().ShiftCategory;

            foreach (var currentGroup in orderedList.Skip(1))
            {
                var currentGroupHighestCategoryOriginal =
                    currentGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupHighestCategory.Description.Name).Original;

                var currentGroupLowestCategoryOrignial =
                    currentGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupLowestCategory.Description.Name).Original;


                var returnGroupHighestCategoryOriginal =
                    returnGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupHighestCategory.Description.Name).Original;

                var returnGroupLowestCategoryOrignial =
                    returnGroup.ShiftCategoryFairnessCompareValues.First(
                        s => s.ShiftCategory.Description.Name == selectedGroupLowestCategory.Description.Name).Original;

                returnGroup = currentGroupHighestCategoryOriginal <= returnGroupHighestCategoryOriginal
                              && currentGroupLowestCategoryOrignial > returnGroupLowestCategoryOrignial
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
