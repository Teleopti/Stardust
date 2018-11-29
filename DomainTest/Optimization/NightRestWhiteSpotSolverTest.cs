using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class NightRestWhiteSpotSolverTest
    {
		private NightRestWhiteSpotSolver _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3;
        private IScheduleDayPro _scheduleDayPro4;
        private IScheduleDayPro _scheduleDayPro5;
        private IScheduleDayPro _scheduleDayPro6;
        private IScheduleDayPro _scheduleDayPro7;
        private IScheduleDayPro _scheduleDayPro8;

        private IScheduleDay _schedulePartEmpty;
        private IScheduleDay _schedulePartDo;
        private IScheduleDay _schedulePartContractDo;
        private IScheduleDay _schedulePartAbsence;
        private IScheduleDay _schedulePartShift;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IEffectiveRestriction _effectiveRestriction;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro4 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro5 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro6 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro7 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro8 = _mocks.StrictMock<IScheduleDayPro>();
            _schedulePartEmpty = _mocks.StrictMock<IScheduleDay>();
            _schedulePartDo = _mocks.StrictMock<IScheduleDay>();
            _schedulePartContractDo = _mocks.StrictMock<IScheduleDay>();
            _schedulePartAbsence = _mocks.StrictMock<IScheduleDay>();
            _schedulePartShift = _mocks.StrictMock<IScheduleDay>();
			_target = new NightRestWhiteSpotSolver(_effectiveRestrictionCreator);
		}

        [Test]
        public void SolverShouldFindWhiteSpotsAndSuggestDayBefore()
        {
            using (_mocks.Record())
            {
                mockExpectations(allUnlocked());
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).Return(_effectiveRestriction).Repeat.AtLeastOnce().IgnoreArguments();
				simplePeriod();
            }
            NightRestWhiteSpotSolverResult result;

            using(_mocks.Playback())
            {
                result = _target.Resolve(_matrix, new SchedulingOptions());
            }

            Assert.AreEqual(2, result.DaysToDelete.Count());
            Assert.AreEqual(new DateOnly(2010, 1, 2), result.DaysToDelete.ElementAt(0));
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToDelete.ElementAt(1));

            Assert.AreEqual(4, result.DaysToReschedule().Count());
            Assert.AreEqual(new DateOnly(2010, 1, 6), result.DaysToReschedule().ElementAt(0));
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToReschedule().ElementAt(1));
            Assert.AreEqual(new DateOnly(2010, 1, 3), result.DaysToReschedule().ElementAt(2));
            Assert.AreEqual(new DateOnly(2010, 1, 2), result.DaysToReschedule().ElementAt(3));
        }

        [Test]
        public void ShouldSkipIfFirstDayInSchedulePeriod()
        {
            using (_mocks.Record())
            {
                mockExpectations(allUnlocked());
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).Return(_effectiveRestriction).IgnoreArguments();
				firstDay();
            }
            NightRestWhiteSpotSolverResult result;

            using(_mocks.Playback())
            {
                result = _target.Resolve(_matrix, new SchedulingOptions());
            }

            Assert.AreEqual(1, result.DaysToDelete.Count());
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToDelete.ElementAt(0));

            Assert.AreEqual(2, result.DaysToReschedule().Count());
            Assert.AreEqual(new DateOnly(2010, 1, 6), result.DaysToReschedule().ElementAt(0));
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToReschedule().ElementAt(1));
        }

        [Test]
        public void ShouldSkipIfTwoWhiteSpotsInARow()
        {
            using (_mocks.Record())
            {
                mockExpectations(allUnlocked());
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).Return(_effectiveRestriction).IgnoreArguments();
				twoDaysInARow();
            }
            NightRestWhiteSpotSolverResult result;

            using (_mocks.Playback())
            {
                result = _target.Resolve(_matrix, new SchedulingOptions());
            }

            Assert.AreEqual(1, result.DaysToDelete.Count());
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToDelete.ElementAt(0));

            Assert.AreEqual(2, result.DaysToReschedule().Count());
            Assert.AreEqual(new DateOnly(2010, 1, 6), result.DaysToReschedule().ElementAt(0));
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToReschedule().ElementAt(1));
        }

        [Test]
        public void ShouldConsiderIfDayAfterEffectivePeriodIsEmpty()
        {
            using (_mocks.Record())
            {
                mockExpectations(allUnlocked());
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).Return(_effectiveRestriction).IgnoreArguments();
				lastAndNextDayEmpty();
            }
            NightRestWhiteSpotSolverResult result;

            using (_mocks.Playback())
            {
                result = _target.Resolve(_matrix, new SchedulingOptions());
            }

            Assert.AreEqual(1, result.DaysToDelete.Count());
            Assert.AreEqual(new DateOnly(2010, 1, 2), result.DaysToDelete.ElementAt(0));

            Assert.AreEqual(2, result.DaysToReschedule().Count());
            Assert.AreEqual(new DateOnly(2010, 1, 3), result.DaysToReschedule().ElementAt(0));
            Assert.AreEqual(new DateOnly(2010, 1, 2), result.DaysToReschedule().ElementAt(1));
        }

        [Test]
        public void ShouldSkipIfDayBeforeWhiteSpotIsLocked()
        {
            using (_mocks.Record())
            {
                mockExpectations(lockDay2());
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).Return(_effectiveRestriction).IgnoreArguments();
				simplePeriod();
            }
            NightRestWhiteSpotSolverResult result;

            using (_mocks.Playback())
            {
                result = _target.Resolve(_matrix, new SchedulingOptions());
            }

            Assert.AreEqual(1, result.DaysToDelete.Count());
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToDelete.ElementAt(0));

            Assert.AreEqual(2, result.DaysToReschedule().Count());
            Assert.AreEqual(new DateOnly(2010, 1, 6), result.DaysToReschedule().ElementAt(0));
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToReschedule().ElementAt(1));
        }

        [Test]
        public void ShouldSkipIfWhiteSpotIsLocked()
        {
            using (_mocks.Record())
            {
                mockExpectations(lockDay3());
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).Return(_effectiveRestriction).IgnoreArguments();
				simplePeriod();
            }
            NightRestWhiteSpotSolverResult result;

            using (_mocks.Playback())
            {
                result = _target.Resolve(_matrix, new SchedulingOptions());
            }

            Assert.AreEqual(1, result.DaysToDelete.Count());
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToDelete.ElementAt(0));

            Assert.AreEqual(2, result.DaysToReschedule().Count());
            Assert.AreEqual(new DateOnly(2010, 1, 6), result.DaysToReschedule().ElementAt(0));
            Assert.AreEqual(new DateOnly(2010, 1, 5), result.DaysToReschedule().ElementAt(1));
        }

        [Test]
        public void ShouldSkipNotBeDeletedIsNotScheduled()
        {
            using (_mocks.Record())
            {
                mockExpectations(allUnlocked());
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).Return(_effectiveRestriction).IgnoreArguments();
				firstTwoDaysAreEmpty();
            }
            NightRestWhiteSpotSolverResult result;

            using (_mocks.Playback())
            {
                result = _target.Resolve(_matrix, new SchedulingOptions());
            }

            Assert.AreEqual(0, result.DaysToDelete.Count());
        }

        [Test]
        public void ShouldSkipIfDayBeforeWhiteSpotIsDayOff()
        {
            using (_mocks.Record())
            {
                mockExpectations(allUnlocked());
                dayOffBeforeEmptyDay();
            }
            NightRestWhiteSpotSolverResult result;

            using (_mocks.Playback())
            {
                result = _target.Resolve(_matrix, new SchedulingOptions());
            }

            Assert.AreEqual(0, result.DaysToDelete.Count());

            Assert.AreEqual(0, result.DaysToReschedule().Count());
        }

        private void mockExpectations(IScheduleDayPro[] unlockedList)
        {
            IPerson person = PersonFactory.CreatePerson();

            var periodList = new []{
                                                        _scheduleDayPro1,
                                                        _scheduleDayPro2,
                                                        _scheduleDayPro3,
                                                        _scheduleDayPro4,
                                                        _scheduleDayPro5,
                                                        _scheduleDayPro6,
                                                        _scheduleDayPro7
                                                    };
			
            Expect.Call(_matrix.EffectivePeriodDays).Return(periodList).Repeat.Any();
            for (int i = 0; i < 7; i++)
            {
                var day = periodList[i];
                Expect.Call(day.Day).Return(new DateOnly(2010, 1, 1).AddDays(i)).Repeat.Any();
                Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 1).AddDays(i))).Return(periodList[i]).
                    Repeat.Any();
            }
            Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 8))).Return(_scheduleDayPro8).Repeat.Any();
            Expect.Call(_schedulePartEmpty.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
            Expect.Call(_schedulePartDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            Expect.Call(_schedulePartContractDo.SignificantPart()).Return(SchedulePartView.ContractDayOff).Repeat.Any();
            Expect.Call(_schedulePartAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
            Expect.Call(_schedulePartShift.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_schedulePartEmpty.IsScheduled()).Return(false).Repeat.Any();
            Expect.Call(_schedulePartDo.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_schedulePartContractDo.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_schedulePartAbsence.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_schedulePartShift.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_matrix.UnlockedDays).Return(unlockedList).Repeat.Any();
            Expect.Call(_matrix.FullWeeksPeriodDays).Return(periodList).Repeat.Any();
            Expect.Call(_matrix.Person).Return(person).Repeat.Any();
		}

        private void simplePeriod()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
        }

        private void dayOffBeforeEmptyDay()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartDo).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
        }

        private void firstDay()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
        }

        private void twoDaysInARow()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
        }

        private void lastAndNextDayEmpty()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
        }

        private void firstTwoDaysAreEmpty()
        {
            Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_schedulePartEmpty).Repeat.Any();
            Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
            Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_schedulePartShift).Repeat.Any();
        }

        private IScheduleDayPro[] allUnlocked()
        {
	        return new[]
	        {
		        _scheduleDayPro1,
		        _scheduleDayPro2,
		        _scheduleDayPro3,
		        _scheduleDayPro4,
		        _scheduleDayPro5,
		        _scheduleDayPro6,
		        _scheduleDayPro7,
		        _scheduleDayPro8
	        };
        }

        private IScheduleDayPro[] lockDay2()
        {
	        return new[]
	        {
		        _scheduleDayPro1,
		        _scheduleDayPro3,
		        _scheduleDayPro4,
		        _scheduleDayPro5,
		        _scheduleDayPro6,
		        _scheduleDayPro7,
		        _scheduleDayPro8
	        };
        }

        private IScheduleDayPro[] lockDay3()
        {
	        return new[]
	        {
		        _scheduleDayPro1,
		        _scheduleDayPro2,
		        _scheduleDayPro4,
		        _scheduleDayPro5,
		        _scheduleDayPro6,
		        _scheduleDayPro7,
		        _scheduleDayPro8
	        };
        }
    }
}