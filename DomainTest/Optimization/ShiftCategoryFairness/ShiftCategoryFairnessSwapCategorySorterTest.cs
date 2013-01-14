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
    public class ShiftCategoryFairnessSwapCategorySorterTest
    {
        private ShiftCategoryFairnessCompareResult _selectedGroup;
        private int _numberOfGroups;
        private IList<IShiftCategoryFairnessSwap> _blacklist;
        private readonly ShiftCategoryFairnessSwapCategorySorter _target = new ShiftCategoryFairnessSwapCategorySorter();
        private List<IShiftCategoryFairnessCompareResult> _list = new List<IShiftCategoryFairnessCompareResult>();

        private ShiftCategory _shiftCategoryMorning = new ShiftCategory("Morning");
        private ShiftCategory _shiftCategoryDay = new ShiftCategory("Day");
        private ShiftCategory _shiftCategoryNight = new ShiftCategory("Night");
        private readonly ShiftCategory _shiftCategoryEvening = new ShiftCategory("Evening");

        [SetUp]
        public void Setup()
        {
            _shiftCategoryMorning = new ShiftCategory("Morning");
            _shiftCategoryDay = new ShiftCategory("Day");
            _shiftCategoryNight = new ShiftCategory("Night");

            _selectedGroup = new ShiftCategoryFairnessCompareResult
                                 {
                                     ShiftCategoryFairnessCompareValues =
                                         new List<IShiftCategoryFairnessCompareValue>
                                             {
                                                 new ShiftCategoryFairnessCompareValue
                                                     {
                                                         Original = 0.9,
                                                         ComparedTo = 0.3,
                                                         ShiftCategory = _shiftCategoryMorning
                                                     },
                                                 new ShiftCategoryFairnessCompareValue
                                                     {
                                                         Original = 0.1,
                                                         ComparedTo = 0.6,
                                                         ShiftCategory = _shiftCategoryDay
                                                     },
                                                 new ShiftCategoryFairnessCompareValue
                                                     {
                                                         Original = 0.0,
                                                         ComparedTo = 0.1,
                                                         ShiftCategory = _shiftCategoryNight
                                                     }
                                             },
                                     StandardDeviation = 0.1,
                                     OriginalMembers = new List<IPerson>
                                                           {
                                                               new Person
                                                                   {
                                                                       Name = new Name("Ashlee", "Andeen")
                                                                   }
                                                           }

                                 };

            _numberOfGroups = 2;

            _blacklist = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     }
                             };
        }

        [Test]
        public void ShouldReturnBestSwap()
        {
            _selectedGroup.ShiftCategoryFairnessCompareValues =
                new List<IShiftCategoryFairnessCompareValue>
                    {
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.6,
                                ComparedTo = 0.5,
                                ShiftCategory = _shiftCategoryMorning
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.3,
                                ComparedTo = 0.1,
                                ShiftCategory = _shiftCategoryDay
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.1,
                                ComparedTo = 0.3,
                                ShiftCategory = _shiftCategoryNight
                            }
                    };
            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    _numberOfGroups,
                                                    _blacklist, ref _list).ToList();

            Assert.AreEqual(_shiftCategoryDay, result.First().ShiftCategory);
            Assert.AreEqual(_shiftCategoryNight, result.Last().ShiftCategory);
        }

        [Test]
        public void ShouldReturnNextBestSwapForFirstCategory()
        {
            _selectedGroup.ShiftCategoryFairnessCompareValues =
                new List<IShiftCategoryFairnessCompareValue>
                    {
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.6,
                                ComparedTo = 0.5,
                                ShiftCategory = _shiftCategoryMorning
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.3,
                                ComparedTo = 0.1,
                                ShiftCategory = _shiftCategoryDay
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.1,
                                ComparedTo = 0.3,
                                ShiftCategory = _shiftCategoryNight
                            }
                    };

            _blacklist = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     },
                                     new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryMorning
                                     }
                             };

            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    _numberOfGroups,
                                                    _blacklist, ref _list).ToList();

            Assert.AreEqual(_shiftCategoryMorning.Description.Name, result.First().ShiftCategory.Description.Name);
            Assert.AreEqual(_shiftCategoryNight.Description.Name, result.Last().ShiftCategory.Description.Name);
        }

        [Test]
        public void ShouldReturnThirdBestSwapForFirstCategory()
        {
            _selectedGroup.ShiftCategoryFairnessCompareValues =
                new List<IShiftCategoryFairnessCompareValue>
                    {
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.9,
                                ComparedTo = 0,
                                ShiftCategory = _shiftCategoryMorning
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.8,
                                ComparedTo = 0.0,
                                ShiftCategory = _shiftCategoryDay
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.7,
                                ComparedTo = 0.0,
                                ShiftCategory = _shiftCategoryEvening
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0,
                                ComparedTo = 15,
                                ShiftCategory = _shiftCategoryNight
                            }
                    };

            _blacklist = new List<IShiftCategoryFairnessSwap>
                             {
                                 // block morning
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryDay
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryEvening
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     },
                                 // block day
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryEvening
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     }
                             };
            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    _numberOfGroups,
                                                    _blacklist, ref _list).ToList();

            Assert.AreEqual(_shiftCategoryEvening.Description.Name, result.First().ShiftCategory.Description.Name);
            Assert.AreEqual(_shiftCategoryNight.Description.Name, result.Last().ShiftCategory.Description.Name);
        }

        [Test]
        public void ShouldReturnNextBestSwapForSecondCategory()
        {
            _selectedGroup.ShiftCategoryFairnessCompareValues =
                new List<IShiftCategoryFairnessCompareValue>
                    {
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.6,
                                ComparedTo = 0.3,
                                ShiftCategory = _shiftCategoryMorning
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.3,
                                ComparedTo = 0.4,
                                ShiftCategory = _shiftCategoryDay
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.1,
                                ComparedTo = 0.3,
                                ShiftCategory = _shiftCategoryNight
                            }
                    };

            _blacklist = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     }
                             };

            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    _numberOfGroups,
                                                    _blacklist, ref _list).ToList();

            Assert.AreEqual(_shiftCategoryMorning.Description.Name, result.First().ShiftCategory.Description.Name);
            Assert.AreEqual(_shiftCategoryDay.Description.Name, result.Last().ShiftCategory.Description.Name);
        }

        [Test]
        public void ShouldReturnThirdBestSwapForSecondCategory()
        {
            _selectedGroup.ShiftCategoryFairnessCompareValues =
                new List<IShiftCategoryFairnessCompareValue>
                    {
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.15,
                                ComparedTo = 0,
                                ShiftCategory = _shiftCategoryMorning
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.0,
                                ComparedTo = 0.9,
                                ShiftCategory = _shiftCategoryDay
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.0,
                                ComparedTo = 0.8,
                                ShiftCategory = _shiftCategoryEvening
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0,
                                ComparedTo = 7,
                                ShiftCategory = _shiftCategoryNight
                            }
                    };

            _blacklist = new List<IShiftCategoryFairnessSwap>
                             {
                                 // block day
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryDay
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryEvening,
                                         ShiftCategoryFromGroup2 = _shiftCategoryDay
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryNight,
                                         ShiftCategoryFromGroup2 = _shiftCategoryDay
                                     },
                                 // block evening
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryEvening
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryNight,
                                         ShiftCategoryFromGroup2 = _shiftCategoryEvening
                                     }
                             };
            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    _numberOfGroups,
                                                    _blacklist, ref _list).ToList();

            Assert.AreEqual(_shiftCategoryMorning.Description.Name, result.First().ShiftCategory.Description.Name);
            Assert.AreEqual(_shiftCategoryNight.Description.Name, result.Last().ShiftCategory.Description.Name);
        }

        [Test]
        public void ShouldNotReturnBlacklistedCategory()
        {
            _list = new List<IShiftCategoryFairnessCompareResult>();
            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    _numberOfGroups,
                                                    _blacklist, ref _list);
            Assert.AreNotEqual(_shiftCategoryMorning, result.First());
        }

        [Test]
        public void ShouldAddGroupToBlacklistAllCategoriesBlacklisted()
        {
            _blacklist.Add(
                new ShiftCategoryFairnessSwap
                    {
                        Group1 = _selectedGroup,
                        Group2 = new ShiftCategoryFairnessCompareResult(),
                        ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                        ShiftCategoryFromGroup2 = _shiftCategoryDay
                    });
            var count = _list.Count;

            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    _numberOfGroups,
                                                    _blacklist, ref _list);
            Assert.That(result, Is.Empty);
            Assert.AreNotEqual(count, _list.Count);
            Assert.AreEqual(_selectedGroup, _list.First());
        }

        [Test]
        public void ShouldAddGroupToBlacklistNoGoodSwapsLeft()
        {
            _selectedGroup.ShiftCategoryFairnessCompareValues =
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
                                Original = 0.0,
                                ComparedTo = 0.0,
                                ShiftCategory = _shiftCategoryDay
                            },
                        new ShiftCategoryFairnessCompareValue
                            {
                                Original = 0.0,
                                ComparedTo = 0.0,
                                ShiftCategory = _shiftCategoryNight
                            }
                    };
            _list = new List<IShiftCategoryFairnessCompareResult>();
            var count = _list.Count;
            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    _numberOfGroups,
                                                    _blacklist, ref _list);

            Assert.That(result, Is.Empty);
            Assert.AreNotEqual(count, _list.Count);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals",
            MessageId = "result"), Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void ShouldThrowException()
        {

            var maxInt = int.MaxValue;
            maxInt++;
            // ReSharper disable UnusedVariable
            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    // ReSharper restore UnusedVariable
                                                    maxInt,
                                                    _blacklist, ref _list);

        }

        [Test]
        public void ShouldReturnNullWhenNoGoodSwapsLeft()
        {
            _selectedGroup.ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue>
                                                                    {
                                                                        new ShiftCategoryFairnessCompareValue
                                                                            {
                                                                                Original = 0.8,
                                                                                ComparedTo = 0.0,
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
                                                                                ComparedTo = 0.8,
                                                                                ShiftCategory = _shiftCategoryNight
                                                                            }

                                                                    };
            _blacklist = new List<IShiftCategoryFairnessSwap>
                             {
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryMorning,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     },
                                 new ShiftCategoryFairnessSwap
                                     {
                                         Group1 = _selectedGroup,
                                         Group2 = new ShiftCategoryFairnessCompareResult(),
                                         ShiftCategoryFromGroup1 = _shiftCategoryDay,
                                         ShiftCategoryFromGroup2 = _shiftCategoryNight
                                     }
                             };
            _list = new List<IShiftCategoryFairnessCompareResult>();
            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroup.ShiftCategoryFairnessCompareValues,
                                                    _numberOfGroups,
                                                    _blacklist, ref _list);
            Assert.That(_list.Contains(_selectedGroup), Is.True);
            Assert.That(result, Is.Empty);
        }
    }
}