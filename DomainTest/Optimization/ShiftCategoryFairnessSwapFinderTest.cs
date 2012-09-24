using System;
using System.Collections;
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
        private ShiftCategoryFairnessSwapFinder _target;
        private IList<ShiftCategoryFairnessCompareResult> _groupList;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IList<ShiftCategoryFairnessSwap> _blackList;

// ReSharper disable InconsistentNaming
        private ShiftCategoryFairnessCompareResult group1, group2, group3, group4, group5;
        private ShiftCategory shiftCategoryDay, shiftCategoryNoon, shiftCategoryNight;
// ReSharper restore InconsistentNaming

        [SetUp]
        public void Setup()
        {
            shiftCategoryDay = new ShiftCategory("Day");
            shiftCategoryNoon = new ShiftCategory("Noon");
            shiftCategoryNight = new ShiftCategory("Night");

            group1 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<ShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.9, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.1
                         };

            group2 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<ShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.4, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.6, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.02
                         };

            group3 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<ShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.8, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.03
                         };

            group4 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<ShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.3, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.7, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.04
                         };

            group5 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<ShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.2, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.7, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
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
            _blackList = new List<ShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group4,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     }

                             };
            _target = new ShiftCategoryFairnessSwapFinder();
        }

        [Test]
        public void ShouldReturnTwoGroupsForSwapping()
        {
            var result = _target.GetGroupsToSwap(_groupList, new List<ShiftCategoryFairnessSwap>());
            Assert.AreEqual(0.1, result.Group1.StandardDeviation);
            Assert.AreEqual("Day", result.ShiftCategoryFromGroup1.Description.Name);
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
	                }, new List<ShiftCategoryFairnessSwap>());
            Assert.AreEqual(null, result.Group1);
        }

        [Test]
        public void ShouldNotReturnBlacklistedGroupSwap()
        {
            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreEqual(0.1, result.Group1.StandardDeviation);
            Assert.AreEqual(0.03, result.Group2.StandardDeviation);
        }

        [Test]
        public void ShouldSelectNewFirstGroupWhenAllSuggestionsAreExhausted()
        {
            SetupBlacklistForCompleteExhaustOfGroup1Options();
            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreEqual(0.03, result.Group1.StandardDeviation);
            Assert.AreEqual(0.02, result.Group2.StandardDeviation);
        }

        [Test]
        public void ShouldSelectNewShiftCategoriesForGroup1()
        {
            SetupBlacklistForCompleteExhaustOfGroup1ShiftCategoryDayOptions();
            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreEqual(0.1, result.Group1.StandardDeviation);
            Assert.AreEqual(0.03, result.Group2.StandardDeviation);
            Assert.AreEqual(shiftCategoryNoon, result.ShiftCategoryFromGroup1);
            Assert.AreEqual(shiftCategoryNight, result.ShiftCategoryFromGroup2);
        }

        [Test]
        public void ShouldNeverReturnIdenticalGroupOrShiftInSwap()
        {
            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreNotEqual(result.Group1.StandardDeviation, result.Group2.StandardDeviation);
            Assert.AreNotEqual(result.ShiftCategoryFromGroup1, result.ShiftCategoryFromGroup2);

            SetupBlacklistForCompleteExhaustOfGroup1ShiftCategoryDayOptions();
            result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreNotEqual(result.Group1.StandardDeviation, result.Group2.StandardDeviation);
            Assert.AreNotEqual(result.ShiftCategoryFromGroup1, result.ShiftCategoryFromGroup2);

            SetupBlacklistForCompleteExhaustOfGroup1Options();
            result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreNotEqual(result.Group1.StandardDeviation, result.Group2.StandardDeviation);
            Assert.AreNotEqual(result.ShiftCategoryFromGroup1, result.ShiftCategoryFromGroup2);
        }

        [Test]
        public void ShouldReturnNullIfNoSwapIsLeft()
        {
            group1 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<ShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.9, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.1
                         };

            group2 = new ShiftCategoryFairnessCompareResult
                    {
                        ShiftCategoryFairnessCompareValues =
                            new List<ShiftCategoryFairnessCompareValue>
                                {
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.5, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.5, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNight}
                                },
                        StandardDeviation = 0.02
                    };

            _groupList = new List<ShiftCategoryFairnessCompareResult> {group1, group2};

            _blackList = new List<ShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNoon
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = shiftCategoryNoon,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     }
                             };

            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreEqual(null, result);
        }

        private void SetupSmallListForBlacklistTests()
        {
            group1 = new ShiftCategoryFairnessCompareResult
            {
                ShiftCategoryFairnessCompareValues =
                    new List<ShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.9, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.0, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                         },
                StandardDeviation = 0.1
            };

            group2 = new ShiftCategoryFairnessCompareResult
            {
                ShiftCategoryFairnessCompareValues =
                    new List<ShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.5, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.4, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.1, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                         },
                StandardDeviation = 0.02
            };

            group3 = new ShiftCategoryFairnessCompareResult
            {
                ShiftCategoryFairnessCompareValues =
                    new List<ShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.6, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.3, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.1, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                         },
                StandardDeviation = 0.03
            };

            _groupList = new List<ShiftCategoryFairnessCompareResult>
                             {
                                 group1,
                                 group2,
                                 group3
                             };
        }

        private void SetupBlacklistForCompleteExhaustOfGroup1ShiftCategoryDayOptions()
        {
            SetupSmallListForBlacklistTests();

            var blackListItem1 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNoon
                                     };
            var blackListItem2 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     };
            var blackListItem3 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group3,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNoon
                                     };
            var blackListItem4 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group3,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     };
            _blackList = new List<ShiftCategoryFairnessSwap>
                             {
                                 blackListItem1,
                                 blackListItem2,
                                 blackListItem3,
                                 blackListItem4
                             };
        }

        private void SetupBlacklistForCompleteExhaustOfGroup1Options()
        {
            SetupSmallListForBlacklistTests();

            var blackListItem1 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNoon
                                     };
            var blackListItem2 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     };
            var blackListItem3 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = shiftCategoryNoon,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     };
            var blackListItem4 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group3,
                                         ShiftCategoryFromGroup1 = shiftCategoryNoon,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     };
            var blackListItem5 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group3,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNoon
                                     };
            var blackListItem6 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group3,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     };

            _blackList = new List<ShiftCategoryFairnessSwap>
                             {
                                 blackListItem1,
                                 blackListItem2,
                                 blackListItem3,
                                 blackListItem4,
                                 blackListItem5,
                                 blackListItem6
                             };

        }
    }
}
