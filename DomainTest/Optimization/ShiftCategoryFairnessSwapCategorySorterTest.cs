using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ShiftCategoryFairnessSwapCategorySorterTest
    {
        private ShiftCategoryFairnessCompareResult _selectedGroup;
        private IOrderedEnumerable<IShiftCategoryFairnessCompareValue> _selectedGroupCategories;
        private int _numberOfGroups;
        private IEnumerable<IShiftCategoryFairnessSwap> _blacklist;
        private readonly ShiftCategoryFairnessCategorySorter _target = new ShiftCategoryFairnessCategorySorter();

        private ShiftCategory _shiftCategoryMorning = new ShiftCategory("Morning");
        private ShiftCategory _shiftCategoryDay = new ShiftCategory("Day");
        private ShiftCategory _shiftCategoryNight = new ShiftCategory("Night");

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
                                     StandardDeviation = 0.1

                                 };

            _selectedGroupCategories =
                _selectedGroup.ShiftCategoryFairnessCompareValues.OrderByDescending(g => g.Original);

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
        public void ShouldNotReturnBlacklistedCategory()
        {
            var result = _target.GetGroupCategories(_selectedGroup, _selectedGroupCategories,_numberOfGroups, _blacklist);
            Assert.AreNotEqual(_shiftCategoryDay, result.First());
        }
    }
}
