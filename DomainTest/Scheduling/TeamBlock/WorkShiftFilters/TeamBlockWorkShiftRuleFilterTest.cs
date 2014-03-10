using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
    [TestFixture]
    public class TeamBlockWorkShiftRuleFilterTest
    {
        private ITeamBlockWorkShiftRuleFilter _target;
        private MockRepository _mock;
        private IRuleSetBag _ruleSetBag1;
        private IRuleSetBag _ruleSetBag2;
        private IWorkShiftRuleSet _workShiftRuleSet1;
        private IWorkShiftRuleSet _workShiftRuleSet2;
        private IWorkShiftRuleSet _workShiftRuleSet3;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _ruleSetBag1 = _mock.StrictMock<IRuleSetBag>();
            _ruleSetBag2 = _mock.StrictMock<IRuleSetBag>();
            _workShiftRuleSet1 = _mock.StrictMock<IWorkShiftRuleSet>();
            _workShiftRuleSet2 = _mock.StrictMock<IWorkShiftRuleSet>();
            _workShiftRuleSet3 = _mock.StrictMock<IWorkShiftRuleSet>();
            _target = new TeamBlockWorkShiftRuleFilter();
        }

        [Test]
        public void ShouldReturnEmptyListIfRuleSetBagIsEmpty()
        {
            var ruleSetBags = new List<IRuleSetBag>();
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
            Assert.AreEqual(0, _target.Filter(blockPeriod, ruleSetBags).Count());

        }

        [Test]
        public void ShouldReturnEmptyListIfBlockIsEmpty()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>());
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(0, _target.Filter(blockPeriod, ruleSetBags).Count());
            }
        }

        [Test]
        public void ReturnSameListWhenNoAccessibilityDefined()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>());
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>());
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.Filter(blockPeriod, ruleSetBags).Count());
            }
        }

        [Test]
        public void ReturnSameListIfAccessibilityDaysAreOutsideSingleDay()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>());
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Thursday, DayOfWeek.Wednesday, DayOfWeek.Tuesday, DayOfWeek.Friday });
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>());
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.Filter(blockPeriod, ruleSetBags).Count());
            }
        }

        [Test]
        public void ReturnSameListIfAccessibilityDaysAreOutsideMultipleDay()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>());
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Thursday, DayOfWeek.Wednesday, DayOfWeek.Friday });
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>());
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.Filter(blockPeriod, ruleSetBags).Count());
            }
        }

        [Test]
        public void ReturnSecondWorkShiftIfAccessibilityDaysAreValid()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Monday });
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>());
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Thursday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>());
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(blockPeriod, ruleSetBags).ToList();
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(_workShiftRuleSet2, result[0]);
            }
        }

        [Test]
        public void TestIfTheReturnListDoesNotHaveDuplicates()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet1 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>());
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Thursday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>());
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(blockPeriod, ruleSetBags).ToList();
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(_workShiftRuleSet1, result[0]);
            }
        }

        [Test]
        public void ReturnSameListIfAccessibilityDateAreOutsideSingleDay()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 11) });
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 09) });
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.Filter(blockPeriod, ruleSetBags).Count());
            }
        }

        [Test]
        public void ReturnSameListIfAccessibilityDateAreOutsideMultipleDay()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 13) });
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 09) });
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.Filter(blockPeriod, ruleSetBags).Count());
            }
        }

        [Test]
        public void ReturnSecondWorkShiftIfAccessibilityDateAreValid()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 11) });
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 12) });
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(blockPeriod, ruleSetBags).ToList();
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(_workShiftRuleSet2, result[0]);
            }
        }

        [Test]
        public void ReturnSameListWithInvalidFullAccessibility()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1, _ruleSetBag2 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet1 = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            var listOfWorkShiftRuleSet2 = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet3, _workShiftRuleSet1 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet1);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 09) });
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Thursday, DayOfWeek.Wednesday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 12) });

                Expect.Call(_ruleSetBag2.RuleSetCollection).Return(listOfWorkShiftRuleSet2);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 09) });
                Expect.Call(_workShiftRuleSet3.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet3.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Thursday, DayOfWeek.Wednesday, DayOfWeek.Friday });
                Expect.Call(_workShiftRuleSet3.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 13) });
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(blockPeriod, ruleSetBags).ToList();
                Assert.AreEqual(3, result.Count());
            }
        }

        [Test]
        public void ReturnEmptyListWithvalidFullAccessibility()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1, _ruleSetBag2 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet1 = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            var listOfWorkShiftRuleSet2 = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet3, _workShiftRuleSet1 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet1);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 11) });
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Thursday, DayOfWeek.Wednesday, DayOfWeek.Sunday, DayOfWeek.Tuesday });
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 12) });

                Expect.Call(_ruleSetBag2.RuleSetCollection).Return(listOfWorkShiftRuleSet2);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 11) });
                Expect.Call(_workShiftRuleSet3.DefaultAccessibility).Return(DefaultAccessibility.Included);
                Expect.Call(_workShiftRuleSet3.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Thursday, DayOfWeek.Wednesday, DayOfWeek.Monday });
                Expect.Call(_workShiftRuleSet3.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 10) });
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(blockPeriod, ruleSetBags).ToList();
                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void TestIfTheReturnListDoesNotHaveDuplicatesWithExcluded()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet1 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Excluded);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday , DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Excluded );
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>());
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Monday, DayOfWeek.Friday, DayOfWeek.Sunday });
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>());
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(blockPeriod, ruleSetBags).ToList();
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(_workShiftRuleSet1, result[0]);
            }
        }

        [Test]
        public void ReturnSameListIfAccessibilityDateAreOutsideSingleDayWithExcluded()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Excluded );
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 10) });
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Excluded );
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 10) });
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.Filter(blockPeriod, ruleSetBags).Count());
            }
        }

        [Test]
        public void ReturnSameListIfAccessibilityDateAreOutsideMultipleDayWithExcluded()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Excluded );
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 10), new DateTime(2014, 03, 11) });
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Excluded );
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 11), new DateTime(2014, 03, 10) });
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.Filter(blockPeriod, ruleSetBags).Count());
            }
        }

        [Test]
        public void EmptyListIfAccessibilityDateAreOutsideMultipleDayWithExcluded()
        {
            var ruleSetBags = new List<IRuleSetBag>() { _ruleSetBag1 };
            var blockPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            var listOfWorkShiftRuleSet = new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1, _workShiftRuleSet2 });
            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection).Return(listOfWorkShiftRuleSet);
                Expect.Call(_workShiftRuleSet1.DefaultAccessibility).Return(DefaultAccessibility.Excluded);
                Expect.Call(_workShiftRuleSet1.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet1.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 08), new DateTime(2014, 03, 09) });
                Expect.Call(_workShiftRuleSet2.DefaultAccessibility).Return(DefaultAccessibility.Excluded);
                Expect.Call(_workShiftRuleSet2.AccessibilityDaysOfWeek).Return(new List<DayOfWeek>());
                Expect.Call(_workShiftRuleSet2.AccessibilityDates).Return(new List<DateTime>() { new DateTime(2014, 03, 11), new DateTime(2014, 03, 12) });
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(0, _target.Filter(blockPeriod, ruleSetBags).Count());
            }
        }

    }


}
