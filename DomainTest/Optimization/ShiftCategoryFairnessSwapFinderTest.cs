using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ShiftCategoryFairnessSwapFinderTest
    {
        private ShiftCategoryFairnessSwapFinder _target;
        private IList<IShiftCategoryFairnessCompareResult> _groupList;
        private IList<IShiftCategoryFairnessSwap> _blackList;

// ReSharper disable InconsistentNaming
        private ShiftCategoryFairnessCompareResult group1, group2, group3, group4, group5;
        private ShiftCategory shiftCategoryDay, shiftCategoryNoon, shiftCategoryNight;
        private readonly Person person = new Person();
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
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.9, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.1, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.6, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.1,
                             OriginalMembers = new List<IPerson> { person }
                         };

            group2 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.4, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.6, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.02,
                             OriginalMembers = new List<IPerson> { person, person }
                         };

            group3 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.8, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.03,
                             OriginalMembers = new List<IPerson> { person, person, person }
                         };

            group4 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.3, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.7, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.04,
                             OriginalMembers = new List<IPerson> { person, person, person, person }
                         };

            group5 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.2, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.7, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.05,
                             OriginalMembers = new List<IPerson> { person, person, person, person, person }
                         };

            _groupList = new List<IShiftCategoryFairnessCompareResult>
                             {
                                 group1,
                                 group2,
                                 group3,
                                 group4,
                                 group5
                             };
            _blackList = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group4,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     }

                             };
            _target = new ShiftCategoryFairnessSwapFinder(new ShiftCategoryFairnessCategorySorter());
        }

        [Test]
        public void ShouldReturnTwoGroupsForSwapping()
        {
            var result = _target.GetGroupsToSwap(_groupList, new List<IShiftCategoryFairnessSwap>());
            Assert.AreEqual(0.1, result.Group1.StandardDeviation);
            Assert.AreEqual("Day", result.ShiftCategoryFromGroup1.Description.Name);
            Assert.AreEqual(0.04, result.Group2.StandardDeviation);
            Assert.AreEqual("Night", result.ShiftCategoryFromGroup2.Description.Name);
        }

        [Test]
        public void  ShouldReturnNullIfGroupListHasOnlyOneItem()
        {
            var result = _target.GetGroupsToSwap(
                new List<IShiftCategoryFairnessCompareResult>
	                {
	                    new ShiftCategoryFairnessCompareResult()
	                }, new List<IShiftCategoryFairnessSwap>());
            Assert.That(result, Is.Null);
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
        public void ShouldNeverReturnIdenticalGroupOrShiftInSwap()
        {
            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreNotEqual(result.Group1, result.Group2);
            Assert.AreNotEqual(result.ShiftCategoryFromGroup1, result.ShiftCategoryFromGroup2);

            SetupBlacklistForCompleteExhaustOfGroup1Options();
            result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreNotEqual(result.Group1, result.Group2);
            Assert.AreNotEqual(result.ShiftCategoryFromGroup1, result.ShiftCategoryFromGroup2);
        }

        [Test]
        public void ShouldReturnNullIfNoSwapIsLeft()
        {
            group1 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.9, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNight}
                                     },
                             StandardDeviation = 0.1,
                             OriginalMembers = new List<IPerson> { person }
                         };

            group2 = new ShiftCategoryFairnessCompareResult
                    {
                        ShiftCategoryFairnessCompareValues =
                            new List<IShiftCategoryFairnessCompareValue>
                                {
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.5, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.5, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNight}
                                },
                        StandardDeviation = 0.02,
                        OriginalMembers = new List<IPerson> { person, person }
                    };

            _groupList = new List<IShiftCategoryFairnessCompareResult> {group1, group2};

            _blackList = new List<IShiftCategoryFairnessSwap>
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

        [Test]
        public void ShouldNotReturnASecondGroupThatHas0InCat2()
        {
            SetupSmallListForBlacklistTests();
            _groupList.Remove(group3);
            _groupList.Add(
                new ShiftCategoryFairnessCompareResult
                    {
                        ShiftCategoryFairnessCompareValues =
                            new List<IShiftCategoryFairnessCompareValue>
                                {
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.1, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.9, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.0, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                },
                        StandardDeviation = 0.03,
                        OriginalMembers = new List<IPerson> { person, person, person }
                    });

            _blackList = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = group1,
                                         Group2 = group2,
                                         ShiftCategoryFromGroup1 = shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = shiftCategoryNight
                                     }
                             };

            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreNotEqual(0, result.ShiftCategoryFromGroup2);
        }

        [Test]
        public void SameItemsDifferentListsShouldBeEqual()
        {
            var firstName = new Name("First", "Person");
            var secondName = new Name("Second", "Person");
            var list1 = new List<IPerson>
                            {
                                new Person
                                    {
                                        Name = firstName
                                    },
                                new Person
                                    {
                                        Name = secondName
                                    }
                            };

            var list2 = new List<IPerson>
                            {
                                new Person
                                    {
                                        Name = firstName
                                    },
                                new Person
                                    {
                                        Name = secondName
                                    }
                            };

            group1.OriginalMembers = list1;
            group2.OriginalMembers = list2;

            var result = group1.Equals(group2);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void SameItemsDifferentOrderShouldStillBeEqual()
        {
            var firstName = new Name("First", "Person");
            var secondName = new Name("Second", "Person");
            var list1 = new List<IPerson>
                            {
                                new Person
                                    {
                                        Name = firstName
                                    },
                                new Person
                                    {
                                        Name = secondName
                                    }
                            };

            var list2 = new List<IPerson>
                            {
                                new Person
                                    {
                                        Name = secondName
                                    },
                                new Person
                                    {
                                        Name = firstName
                                    }
                            };

            group1.OriginalMembers = list1;
            group2.OriginalMembers = list2;

            var result = group1.Equals(group2);
            Assert.AreEqual(true, result);
        }

        private void SetupSmallListForBlacklistTests()
        {
            group1 = new ShiftCategoryFairnessCompareResult
            {
                ShiftCategoryFairnessCompareValues =
                    new List<IShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.9, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.1, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.0, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                         },
                StandardDeviation = 0.1,
                OriginalMembers = new List<IPerson> { person }
            };

            group2 = new ShiftCategoryFairnessCompareResult
            {
                ShiftCategoryFairnessCompareValues =
                    new List<IShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.5, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.4, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.1, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                         },
                StandardDeviation = 0.02,
                OriginalMembers = new List<IPerson> { person, person }
            };

            group3 = new ShiftCategoryFairnessCompareResult
            {
                ShiftCategoryFairnessCompareValues =
                    new List<IShiftCategoryFairnessCompareValue>
                                         {
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.6, ComparedTo = 0.3, ShiftCategory = shiftCategoryDay},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.3, ComparedTo = 0.5, ShiftCategory = shiftCategoryNoon},
                                             new ShiftCategoryFairnessCompareValue
                                                 {Original = 0.1, ComparedTo = 0.2, ShiftCategory = shiftCategoryNight}
                                         },
                StandardDeviation = 0.03,
                OriginalMembers = new List<IPerson> { person, person, person }
            };

            _groupList = new List<IShiftCategoryFairnessCompareResult>
                             {
                                 group1,
                                 group2,
                                 group3
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

            _blackList = new List<IShiftCategoryFairnessSwap>
                             {
                                 blackListItem1,
                                 blackListItem2,
                                 blackListItem3,
                                 blackListItem4,
                                 blackListItem5,
                                 blackListItem6
                             };

        }

		[Test]
		public void ShouldReturnWhatIWant()
		{
			// In this test Group1 has to few Night and to many Day compared to others
			// Group2 has to many Nights and too few days, we want a suggestion back that we should swap so it gets better
			// to extend this test:
			// if I can't use this for some reason this suggestion will come in the blacklist and the next suggestion should be
			// swap night from group2 with noon in group1
			var person2 = new Person();
			_groupList = new List<IShiftCategoryFairnessCompareResult>
			             	{
			             		new ShiftCategoryFairnessCompareResult
			             			{
			             				ShiftCategoryFairnessCompareValues =
			             					new List<IShiftCategoryFairnessCompareValue>
			             						{
			             							new ShiftCategoryFairnessCompareValue
			             								{Original = 0.35, ComparedTo = 0.1, ShiftCategory = shiftCategoryDay},
			             							new ShiftCategoryFairnessCompareValue 
			             								{Original = 0.15, ComparedTo = 0.15, ShiftCategory = shiftCategoryNoon},
			             							new ShiftCategoryFairnessCompareValue
			             								{Original = 0.50, ComparedTo = 0.75, ShiftCategory = shiftCategoryNight}
			             						},
			             				StandardDeviation = 0.15,
			             				OriginalMembers = new List<IPerson> {person2, person2, person2}
			             			},
			             		new ShiftCategoryFairnessCompareResult
			             			{
			             				ShiftCategoryFairnessCompareValues =
			             					new List<IShiftCategoryFairnessCompareValue>
			             						{
			             							new ShiftCategoryFairnessCompareValue
			             								{Original = 0.1, ComparedTo = 0.35, ShiftCategory = shiftCategoryDay},
			             							new ShiftCategoryFairnessCompareValue
			             								{Original = 0.15, ComparedTo = 0.15, ShiftCategory = shiftCategoryNoon},
			             							new ShiftCategoryFairnessCompareValue
			             								{Original = 0.75, ComparedTo = 0.50, ShiftCategory = shiftCategoryNight}
			             						},
			             				StandardDeviation = 0.14,
			             				OriginalMembers = new List<IPerson> {person, person, person}
			             			}
			             	};

			_blackList = new List<IShiftCategoryFairnessSwap>();

			var result = _target.GetGroupsToSwap(_groupList, _blackList);

            Assert.That(result.ShiftCategoryFromGroup1, Is.EqualTo(shiftCategoryDay)); //HERE IT SUGGESTS NIGHT
            Assert.That(result.ShiftCategoryFromGroup2, Is.EqualTo(shiftCategoryNight)); //HERE IT SUGGESTS NOON WHICH IS PERFECTLY FAIR SHOULD NEVER BE SWAPPED
			
			Assert.That(result.Group1,Is.EqualTo(_groupList[0]));
			Assert.That(result.Group2,Is.EqualTo(_groupList[1]));
		}
    }
}
