using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
    [TestFixture]
    public class ShiftCategoryFairnessSwapFinderTest
    {
        private ShiftCategoryFairnessSwapFinder _target;
        private IList<IShiftCategoryFairnessCompareResult> _groupList;
        private IList<IShiftCategoryFairnessSwap> _blackList;

        
        private ShiftCategoryFairnessCompareResult _group1, _group2, _group3, _group4, _group5;
        private ShiftCategory _shiftCategoryMorning, _shiftCategoryDay, _shiftCategoryNight, _shiftCategoryEvening;
        private readonly Person _person = new Person();
        

        [SetUp]
        public void Setup()
        {
			_person.SetId(Guid.NewGuid());
            _shiftCategoryMorning = new ShiftCategory("Morning");
            _shiftCategoryDay = new ShiftCategory("Day");
            _shiftCategoryNight = new ShiftCategory("Night");
            _shiftCategoryEvening = new ShiftCategory("Evening");

            _group1 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.9, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.1, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.6, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.1,
                             OriginalMembers = new List<IPerson> {_person}
                         };

            _group2 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.4, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.6, ComparedTo = 0.2, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.02,
                             OriginalMembers = new List<IPerson> {_person, _person}
                         };

            _group3 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.8, ComparedTo = 0.2, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.03,
                             OriginalMembers = new List<IPerson> {_person, _person, _person}
                         };

            _group4 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.3, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.7, ComparedTo = 0.2, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.04,
                             OriginalMembers = new List<IPerson> {_person, _person, _person, _person}
                         };

            _group5 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.3, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.4, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.3, ComparedTo = 0.2, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.05,
                             OriginalMembers = new List<IPerson> {_person, _person, _person, _person, _person}
                         };

            _groupList = new List<IShiftCategoryFairnessCompareResult>
                             {
                                 _group1,
                                 _group2,
                                 _group3,
                                 _group4,
                                 _group5
                             };
            _blackList = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group4,
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     }

                             };
            _target = new ShiftCategoryFairnessSwapFinder(new ShiftCategoryFairnessSwapCategorySorter());
        }

        [Test]
        public void ShouldReturnTwoGroupsForSwapping()
        {
            var result = _target.GetGroupsToSwap(_groupList, new List<IShiftCategoryFairnessSwap>());
            Assert.AreEqual(0.1, result.Group1.StandardDeviation);
            Assert.AreEqual("Morning", result.ShiftCategoryFromGroup1.Description.Name);
            Assert.AreEqual(0.04, result.Group2.StandardDeviation);
            Assert.AreEqual("Night", result.ShiftCategoryFromGroup2.Description.Name);
        }

        [Test]
        public void ShouldReturnNullIfGroupListHasOnlyOneItem()
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
            _group1 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.9, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.1,
                             OriginalMembers = new List<IPerson> {_person}
                         };

            _group2 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.5, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.5, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.02,
                             OriginalMembers = new List<IPerson> {_person, _person}
                         };

            _groupList = new List<IShiftCategoryFairnessCompareResult> {_group1, _group2};

            _blackList = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group2,
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryDay
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group2,
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group2,
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     }
                             };

            var result = _target.GetGroupsToSwap(_groupList, _blackList);
            Assert.AreEqual(null, result);
        }

        [Test]
        public void ShouldNotReturnASecondGroupThatHas0InCat2()
        {
            SetupSmallListForBlacklistTests();
            _groupList.Remove(_group3);
            _groupList.Add(
                new ShiftCategoryFairnessCompareResult
                    {
                        ShiftCategoryFairnessCompareValues =
                            new List<IShiftCategoryFairnessCompareValue>
                                {
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.1, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.9, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                    new ShiftCategoryFairnessCompareValue
                                        {Original = 0.0, ComparedTo = 0.2, ShiftCategory = _shiftCategoryNight}
                                },
                        StandardDeviation = 0.03,
                        OriginalMembers = new List<IPerson> {_person, _person, _person}
                    });

            _blackList = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group2,
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
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

            _group1.OriginalMembers = list1;
            _group2.OriginalMembers = list2;

            var result = _group1.Equals(_group2);
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

            _group1.OriginalMembers = list1;
            _group2.OriginalMembers = list2;

            var result = _group1.Equals(_group2);
            Assert.AreEqual(true, result);
        }

        private void SetupSmallListForBlacklistTests()
        {
            _group1 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.9, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.0, ComparedTo = 0.2, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.1,
                             OriginalMembers = new List<IPerson> {_person}
                         };

            _group2 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.2, ComparedTo = 0.5, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.4, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.6, ComparedTo = 0.2, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.02,
                             OriginalMembers = new List<IPerson> {_person, _person}
                         };

            _group3 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.6, ComparedTo = 0.3, ShiftCategory = _shiftCategoryMorning},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.3, ComparedTo = 0.5, ShiftCategory = _shiftCategoryDay},
                                         new ShiftCategoryFairnessCompareValue
                                             {Original = 0.1, ComparedTo = 0.2, ShiftCategory = _shiftCategoryNight}
                                     },
                             StandardDeviation = 0.03,
                             OriginalMembers = new List<IPerson> {_person, _person, _person}
                         };

            _groupList = new List<IShiftCategoryFairnessCompareResult>
                             {
                                 _group1,
                                 _group2,
                                 _group3
                             };
        }

        private void SetupBlacklistForCompleteExhaustOfGroup1Options()
        {
            SetupSmallListForBlacklistTests();

            var blackListItem1 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group2,
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryDay
                                     };
            var blackListItem2 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group2,
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     };
            var blackListItem3 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group2,
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     };
            var blackListItem4 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group3,
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     };
            var blackListItem5 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group3,
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryDay
                                     };
            var blackListItem6 = new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group3,
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
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
                                                         {
                                                             Original = 0.35,
                                                             ComparedTo = 0.1,
                                                             ShiftCategory = _shiftCategoryMorning
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.15,
                                                             ComparedTo = 0.15,
                                                             ShiftCategory = _shiftCategoryDay
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.50,
                                                             ComparedTo = 0.75,
                                                             ShiftCategory = _shiftCategoryNight
                                                         }
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
                                                         {
                                                             Original = 0.1,
                                                             ComparedTo = 0.35,
                                                             ShiftCategory = _shiftCategoryMorning
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.15,
                                                             ComparedTo = 0.15,
                                                             ShiftCategory = _shiftCategoryDay
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.75,
                                                             ComparedTo = 0.50,
                                                             ShiftCategory = _shiftCategoryNight
                                                         }
                                                 },
                                         StandardDeviation = 0.14,
                                         OriginalMembers = new List<IPerson> {_person, _person, _person}
                                     }
                             };

            _blackList = new List<IShiftCategoryFairnessSwap>();

            var result = _target.GetGroupsToSwap(_groupList, _blackList);

            Assert.That(result.ShiftCategoryFromGroup1, Is.EqualTo(_shiftCategoryMorning)); //HERE IT SUGGESTS NIGHT
            Assert.That(result.ShiftCategoryFromGroup2, Is.EqualTo(_shiftCategoryNight));
            //HERE IT SUGGESTS NOON WHICH IS PERFECTLY FAIR SHOULD NEVER BE SWAPPED

            Assert.That(result.Group1, Is.EqualTo(_groupList[0]));
            Assert.That(result.Group2, Is.EqualTo(_groupList[1]));
        }

        [Test]
        public void ReachRecursiveCall()
        {
            var person2 = new Person();
            _group1 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {
                                                 Original = 1,
                                                 ComparedTo = 0.9,
                                                 ShiftCategory = _shiftCategoryMorning
                                             },
                                         new ShiftCategoryFairnessCompareValue
                                             {
                                                 Original = 1,
                                                 ComparedTo = 1,
                                                 ShiftCategory = _shiftCategoryDay
                                             },
                                         new ShiftCategoryFairnessCompareValue
                                             {
                                                 Original = 1,
                                                 ComparedTo = 1,
                                                 ShiftCategory = _shiftCategoryNight
                                             }
                                     },
                             StandardDeviation = 0.1,
                             OriginalMembers = new List<IPerson> {person2, person2, person2}
                         };
            _group2 = new ShiftCategoryFairnessCompareResult
                         {
                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {
                                                 Original = 0.8,
                                                 ComparedTo = 0.8,
                                                 ShiftCategory = _shiftCategoryMorning
                                             },
                                         new ShiftCategoryFairnessCompareValue
                                             {
                                                 Original = 0,
                                                 ComparedTo = 0.9,
                                                 ShiftCategory = _shiftCategoryDay
                                             },
                                         new ShiftCategoryFairnessCompareValue
                                             {
                                                 Original = 0.9,
                                                 ComparedTo = 0.0,
                                                 ShiftCategory = _shiftCategoryNight
                                             }
                                     },
                             StandardDeviation = 0.02,
                             OriginalMembers = new List<IPerson> {_person, _person, _person}
                         };
            _group3 = new ShiftCategoryFairnessCompareResult
                         {

                             ShiftCategoryFairnessCompareValues =
                                 new List<IShiftCategoryFairnessCompareValue>
                                     {
                                         new ShiftCategoryFairnessCompareValue
                                             {
                                                 Original = 0.8,
                                                 ComparedTo = 0.8,
                                                 ShiftCategory = _shiftCategoryMorning
                                             },
                                         new ShiftCategoryFairnessCompareValue
                                             {
                                                 Original = 0.9,
                                                 ComparedTo = 0.0,
                                                 ShiftCategory = _shiftCategoryDay
                                             },
                                         new ShiftCategoryFairnessCompareValue
                                             {
                                                 Original = 0.0,
                                                 ComparedTo = 0.9,
                                                 ShiftCategory = _shiftCategoryNight
                                             }
                                     },
                             StandardDeviation = 0.003,
                             OriginalMembers = new List<IPerson> {_person, person2}
                         };


            _groupList = new List<IShiftCategoryFairnessCompareResult> {_group1, _group2, _group3};
            var result = _target.GetGroupsToSwap(_groupList, new List<IShiftCategoryFairnessSwap>());

            Assert.That(result.Group1, Is.EqualTo(_group2));
            Assert.That(result.Group2, Is.EqualTo(_group3));
            Assert.That(result.ShiftCategoryFromGroup1.Description.Name, Is.EqualTo(_shiftCategoryNight.Description.Name));
            Assert.That(result.ShiftCategoryFromGroup2.Description.Name, Is.EqualTo(_shiftCategoryDay.Description.Name));
        }

        [Test]
        public void ReachingFailsafeWhenReturnGroupIsNull()
        {
            _group1 = new ShiftCategoryFairnessCompareResult
                         {
                             OriginalMembers = new List<IPerson> {_person},
                             ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                      {
                                                                          new ShiftCategoryFairnessCompareValue
                                                                              {
                                                                                  Original = 0.8,
                                                                                  ComparedTo = 0.4,
                                                                                  ShiftCategory = _shiftCategoryMorning
                                                                              },
                                                                          new ShiftCategoryFairnessCompareValue
                                                                              {
                                                                                  Original = 0.2,
                                                                                  ComparedTo = 0.2,
                                                                                  ShiftCategory = _shiftCategoryDay
                                                                              },
                                                                          new ShiftCategoryFairnessCompareValue
                                                                              {
                                                                                  Original = 0.0,
                                                                                  ComparedTo = 0.4,
                                                                                  ShiftCategory = _shiftCategoryNight
                                                                              }

                                                                      },
                             StandardDeviation = 0.1
                         };
            _group2 = new ShiftCategoryFairnessCompareResult
                         {
                             OriginalMembers = new List<IPerson> {_person, _person},
                             ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                      {
                                                                          new ShiftCategoryFairnessCompareValue
                                                                              {
                                                                                  Original = 0.2,
                                                                                  ComparedTo = 0.4,
                                                                                  ShiftCategory = _shiftCategoryMorning
                                                                              },
                                                                          new ShiftCategoryFairnessCompareValue
                                                                              {
                                                                                  Original = 0.2,
                                                                                  ComparedTo = 0.2,
                                                                                  ShiftCategory = _shiftCategoryDay
                                                                              },
                                                                          new ShiftCategoryFairnessCompareValue
                                                                              {
                                                                                  Original = 0.6,
                                                                                  ComparedTo = 0.4,
                                                                                  ShiftCategory = _shiftCategoryNight
                                                                              }

                                                                      },
                             StandardDeviation = 0.02
                         };

            _blackList = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _group1,
                                         Group2 = _group2,
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     }
                             };
            var result = _target.GetGroupsToSwap(new List<IShiftCategoryFairnessCompareResult> {_group1, _group2},
                                                 _blackList);

            Assert.That(result, Is.Null);
        }
        
        [Test]
        public void ShouldReturnListWithThreeSwaps()
        {
            _group1 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.9,
                                                                                   ComparedTo = 0.0,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.1,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.0,
                                                                                   ComparedTo = 0.9,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }

                                                                       },
                              StandardDeviation = 0.1
                          };
            _group2 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person, _person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.8,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.1,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.7,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }

                                                                       },
                              StandardDeviation = 0.02
                          };
            _group3 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person, _person, _person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.3,
                                                                                   ComparedTo = 0.7,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.1,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.6,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }

                                                                       },
                              StandardDeviation = 0.03
                          };

            var result =
                _target.GetGroupListOfSwaps(new List<IShiftCategoryFairnessCompareResult> {_group1, _group2, _group3},
                                            new List<IShiftCategoryFairnessSwap>());

            var firstItem = new ShiftCategoryFairnessSwap
                                {
                                    Group1 = _group1,
                                    Group2 = _group2,
                                    ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                    ShiftCategoryFromGroup2 = _shiftCategoryNight
                                };
            var secondItem = new ShiftCategoryFairnessSwap
                                 {
                                     Group1 = _group1,
                                     Group2 = _group3,
                                     ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                     ShiftCategoryFromGroup2 = _shiftCategoryNight
                                 };

            Assert.That(result[0], Is.EqualTo(firstItem));
            Assert.That(result[1], Is.EqualTo(secondItem));
        }

        [Test]
        public void ShouldReturnSwapsForTwoGroups()
        {
            _group1 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.8,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.0,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.0,
                                                                                   ComparedTo = 0.9,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }

                                                                       },
                              StandardDeviation = 0.1
                          };
            _group2 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person, _person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.8,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.1,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.7,
                                                                                   ComparedTo = 0.0,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }

                                                                       },
                              StandardDeviation = 0.02
                          };


            _group3 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> { _person, _person, _person },
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.3,
                                                                                   ComparedTo = 0.7,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.1,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.6,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }

                                                                       },
                              StandardDeviation = 0.03
                          };

            var result =
                _target.GetGroupListOfSwaps(new List<IShiftCategoryFairnessCompareResult> {_group1, _group2},
                                            new List<IShiftCategoryFairnessSwap>());

            var firstItem = new ShiftCategoryFairnessSwap
                                {
                                    Group1 = _group1,
                                    Group2 = _group2,
                                    ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                    ShiftCategoryFromGroup2 = _shiftCategoryNight
                                };
            var secondItem = new ShiftCategoryFairnessSwap
                                 {
                                     Group1 = _group1,
                                     Group2 = _group2,
                                     ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                     ShiftCategoryFromGroup2 = _shiftCategoryNight
                                 };

            Assert.That(result[0], Is.EqualTo(firstItem));
            Assert.That(result[1], Is.EqualTo(secondItem));
        }

        [Test]
        public void ShouldReturnSwapsForThreeGroups()
        {
            _group1 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.8,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.0,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.0,
                                                                                   ComparedTo = 0.9,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }

                                                                       },
                              StandardDeviation = 0.1
                          };
            _group2 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person, _person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.8,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.1,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.7,
                                                                                   ComparedTo = 0.0,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }

                                                                       },
                              StandardDeviation = 0.02
                          };


            _group3 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person, _person, _person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.3,
                                                                                   ComparedTo = 0.7,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.1,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.6,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }

                                                                       },
                              StandardDeviation = 0.03
                          };

            var result =
                _target.GetGroupListOfSwaps(new List<IShiftCategoryFairnessCompareResult> {_group1, _group2, _group3},
                                            new List<IShiftCategoryFairnessSwap>());

            var firstItem = new ShiftCategoryFairnessSwap
                                {
                                    Group1 = _group1,
                                    Group2 = _group2,
                                    ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                    ShiftCategoryFromGroup2 = _shiftCategoryNight
                                };
            var secondItem = new ShiftCategoryFairnessSwap
                                 {
                                     Group1 = _group1,
                                     Group2 = _group2,
                                     ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                     ShiftCategoryFromGroup2 = _shiftCategoryNight
                                 };

            var thirdItem = new ShiftCategoryFairnessSwap
                                {
                                    Group1 = _group1,
                                    Group2 = _group3,
                                    ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                    ShiftCategoryFromGroup2 = _shiftCategoryNight
                                };

            var fourthItem = new ShiftCategoryFairnessSwap
                                 {
                                     Group1 = _group1,
                                     Group2 = _group3,
                                     ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                     ShiftCategoryFromGroup2 = _shiftCategoryNight
                                 };

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Contains(firstItem), Is.True);
            Assert.That(result.Contains(secondItem), Is.True);
            Assert.That(result.Contains(thirdItem), Is.True);
            Assert.That(result.Contains(fourthItem), Is.True);
        }

        [Test]
        public void ShouldReturnSwapsForTwoGroupsWithFourCategories()
        {
            _group1 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.7,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.3,
                                                                                   ComparedTo = 0.5,
                                                                                   ShiftCategory = _shiftCategoryEvening
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.6,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }
                                                                       },
                              StandardDeviation = 0.1
                          };
            _group2 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person, _person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.7,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.1,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.6,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryEvening
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.5,
                                                                                   ComparedTo = 0.3,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               }
                                                                       },
                              StandardDeviation = 0.02
                          };

            var firstItem = new ShiftCategoryFairnessSwap
            {
                Group1 = _group1,
                Group2 = _group2,
                ShiftCategoryFromGroup1 = _shiftCategoryDay,
                ShiftCategoryFromGroup2 = _shiftCategoryEvening
            };
            var secondItem = new ShiftCategoryFairnessSwap
            {
                Group1 = _group1,
                Group2 = _group2,
                ShiftCategoryFromGroup1 = _shiftCategoryDay,
                ShiftCategoryFromGroup2 = _shiftCategoryNight
            };

            var thirdItem = new ShiftCategoryFairnessSwap
            {
                Group1 = _group1,
                Group2 = _group2,
                ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                ShiftCategoryFromGroup2 = _shiftCategoryEvening
            };

            var fourthItem = new ShiftCategoryFairnessSwap
            {
                Group1 = _group1,
                Group2 = _group2,
                ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                ShiftCategoryFromGroup2 = _shiftCategoryNight
            };

            var result =
                _target.GetGroupListOfSwaps(new List<IShiftCategoryFairnessCompareResult> { _group1, _group2 },
                                            new List<IShiftCategoryFairnessSwap>());

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Contains(firstItem), Is.True);
            Assert.That(result.Contains(secondItem), Is.True);
            Assert.That(result.Contains(thirdItem), Is.True);
            Assert.That(result.Contains(fourthItem), Is.True);
        }

        [Test]
        public void ShouldReturnSwapsForTwoGroupsWithFourCategoriesWhenOneIsUnused()
        {
            var shiftcategoryAllday = new ShiftCategory("Allday");

            _group1 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson> {_person},
                              ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.1,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.7,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.3,
                                                                                   ComparedTo = 0.5,
                                                                                   ShiftCategory = _shiftCategoryEvening
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.6,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0,
                                                                                   ComparedTo = 0,
                                                                                   ShiftCategory = shiftcategoryAllday
                                                                               }
                                                                       },
                              StandardDeviation = 0.1
                          };
            _group2 = new ShiftCategoryFairnessCompareResult
            {
                OriginalMembers = new List<IPerson> { _person, _person },
                ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                       {
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.2,
                                                                                   ComparedTo = 0.7,
                                                                                   ShiftCategory = _shiftCategoryMorning
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.1,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryDay
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.6,
                                                                                   ComparedTo = 0.2,
                                                                                   ShiftCategory = _shiftCategoryEvening
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0.5,
                                                                                   ComparedTo = 0.3,
                                                                                   ShiftCategory = _shiftCategoryNight
                                                                               },
                                                                           new ShiftCategoryFairnessCompareValue
                                                                               {
                                                                                   Original = 0,
                                                                                   ComparedTo = 0,
                                                                                   ShiftCategory = shiftcategoryAllday
                                                                               }
                                                                       },
                StandardDeviation = 0.02
            };

            var firstItem = new ShiftCategoryFairnessSwap
            {
                Group1 = _group1,
                Group2 = _group2,
                ShiftCategoryFromGroup1 = _shiftCategoryDay,
                ShiftCategoryFromGroup2 = _shiftCategoryEvening
            };
            var secondItem = new ShiftCategoryFairnessSwap
            {
                Group1 = _group1,
                Group2 = _group2,
                ShiftCategoryFromGroup1 = _shiftCategoryDay,
                ShiftCategoryFromGroup2 = _shiftCategoryNight
            };

            var thirdItem = new ShiftCategoryFairnessSwap
            {
                Group1 = _group1,
                Group2 = _group2,
                ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                ShiftCategoryFromGroup2 = _shiftCategoryEvening
            };

            var fourthItem = new ShiftCategoryFairnessSwap
            {
                Group1 = _group1,
                Group2 = _group2,
                ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                ShiftCategoryFromGroup2 = _shiftCategoryNight
            };

            var result =
                _target.GetGroupListOfSwaps(new List<IShiftCategoryFairnessCompareResult> { _group1, _group2 },
                                            new List<IShiftCategoryFairnessSwap>());

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Contains(firstItem), Is.True);
            Assert.That(result.Contains(secondItem), Is.True);
            Assert.That(result.Contains(thirdItem), Is.True);
            Assert.That(result.Contains(fourthItem), Is.True);
        }

		[Test]
		public void ShouldReturnAllPossibleSwaps()
		{

			var person2 = new Person();
			person2.SetId(Guid.NewGuid());
			_groupList = new List<IShiftCategoryFairnessCompareResult>
                             {
                                 new ShiftCategoryFairnessCompareResult
                                     {
                                         ShiftCategoryFairnessCompareValues =
                                             new List<IShiftCategoryFairnessCompareValue>
                                                 {
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.35,
                                                             ComparedTo = 0.1,
                                                             ShiftCategory = _shiftCategoryMorning
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.15,
                                                             ComparedTo = 0.15,
                                                             ShiftCategory = _shiftCategoryDay
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.50,
                                                             ComparedTo = 0.75,
                                                             ShiftCategory = _shiftCategoryNight
                                                         }
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
                                                         {
                                                             Original = 0.1,
                                                             ComparedTo = 0.35,
                                                             ShiftCategory = _shiftCategoryMorning
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.15,
                                                             ComparedTo = 0.15,
                                                             ShiftCategory = _shiftCategoryDay
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.75,
                                                             ComparedTo = 0.50,
                                                             ShiftCategory = _shiftCategoryNight
                                                         }
                                                 },
                                         StandardDeviation = 0.14,
                                         OriginalMembers = new List<IPerson> {_person, _person, _person}
                                     }
                             };

			_blackList = new List<IShiftCategoryFairnessSwap>();

			var result = _target.GetAllGroupsToSwap(_groupList);
			Assert.That(result.Count(),Is.EqualTo(3));
			
		}

		[Test]
		public void ShouldNotReturnInpossibleSwaps()
		{

			var person2 = new Person();
			person2.SetId(Guid.NewGuid());
			_groupList = new List<IShiftCategoryFairnessCompareResult>
                             {
                                 new ShiftCategoryFairnessCompareResult
                                     {
                                         ShiftCategoryFairnessCompareValues =
                                             new List<IShiftCategoryFairnessCompareValue>
                                                 {
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.1,
                                                             ComparedTo = 0.35,
                                                             ShiftCategory = _shiftCategoryMorning
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.15,
                                                             ComparedTo = 0.20,
                                                             ShiftCategory = _shiftCategoryDay
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.1,
                                                             ComparedTo = 0.5,
                                                             ShiftCategory = _shiftCategoryNight
                                                         }
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
                                                         {
                                                             Original = 0.15,
                                                             ComparedTo = 0.1,
                                                             ShiftCategory = _shiftCategoryMorning
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.35,
                                                             ComparedTo = 0.2,
                                                             ShiftCategory = _shiftCategoryDay
                                                         },
                                                     new ShiftCategoryFairnessCompareValue
                                                         {
                                                             Original = 0.6,
                                                             ComparedTo = 0.5,
                                                             ShiftCategory = _shiftCategoryNight
                                                         }
                                                 },
                                         StandardDeviation = 0.14,
                                         OriginalMembers = new List<IPerson> {_person, _person, _person}
                                     }
                             };

			_blackList = new List<IShiftCategoryFairnessSwap>();

			var result = _target.GetAllGroupsToSwap(_groupList);
			Assert.That(result.Count(), Is.EqualTo(0));

		}
    }


}
