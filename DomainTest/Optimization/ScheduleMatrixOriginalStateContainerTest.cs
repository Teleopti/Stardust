using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ScheduleMatrixOriginalStateContainerTest
    {
        private IScheduleMatrixOriginalStateContainer _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _part;
        private IScheduleDayEquator _scheduleDayEquator;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _part = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
        }

        [Test]
        public void VerifyConstructor()
        {
            using (_mocks.Record())
            {
                mockExpectations();
            }
            using (_mocks.Playback())
            {
                _target = new ScheduleMatrixOriginalStateContainer(_matrix, _scheduleDayEquator);
                Assert.AreSame(_matrix, _target.ScheduleMatrix);
                Assert.AreEqual(1, _target.OldPeriodDaysState.Count);
            }
            
        }

        [Test]
        public void VerifyIsMatrixFullyScheduledReturnsTrueIfMainShiftAbsenceOrDayOff()
        {
            using (_mocks.Record())
            {
                mockExpectations();
				Expect.Call(_part.IsScheduled()).Return(true);
            }
            using (_mocks.Playback())
            {
                _target = new ScheduleMatrixOriginalStateContainer(_matrix, _scheduleDayEquator);
                Assert.IsTrue(_target.IsFullyScheduled());
            }
            
        }

        [Test]
        public void VerifyIsMatrixFullyScheduledReturnsFalseIfNotScheduled()
        {
            using (_mocks.Record())
            {
                mockExpectations();
            	Expect.Call(_part.IsScheduled()).Return(false);
            }
            using (_mocks.Playback())
            {
                _target = new ScheduleMatrixOriginalStateContainer(_matrix, _scheduleDayEquator);
                Assert.IsFalse(_target.IsFullyScheduled());
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InEffective"), Test]
        public void VerifyScheduleDayEquatorCalledForEachDayInEffectivePeriodDays()
        {
            IPerson person = PersonFactory.CreatePerson();
            IScenario scenario = new Scenario("TestScenario");
            DateTime startDate = new DateTime(2010, 01, 10, 0, 0, 0, DateTimeKind.Utc);
            DateTime endDate = startDate.AddDays(10);
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDate, endDate);
            ISkill skill = SkillFactory.CreateSkill("TestSkill");

            IScheduleDayPro scheduleDayPro2 =  _mocks.StrictMock<IScheduleDayPro>();

            SchedulePartFactoryForDomain schedulePartFactory = new SchedulePartFactoryForDomain(person, scenario, dateTimePeriod, skill);
            
            _part = schedulePartFactory.CreatePartWithMainShift();
            IList<IScheduleDayPro> periodDays = new List<IScheduleDayPro> {_scheduleDayPro, scheduleDayPro2};

            int periodDayCount = periodDays.Count;

            using(_mocks.Record())
            {
                Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodDays)).Repeat.Any();
                Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 10)).Repeat.Any();
                Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 10))).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
                Expect.Call(scheduleDayPro2.Day).Return(new DateOnly(2010, 1, 11)).Repeat.Any();
                Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 11))).Return(_scheduleDayPro);
                Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayEquator.MainShiftEquals(null, null)).IgnoreArguments()
                    .Return(true)
                    .Repeat.Times(periodDayCount);

            }

            using(_mocks.Playback())
            {
                _target = new ScheduleMatrixOriginalStateContainer(_matrix, _scheduleDayEquator);
                _target.CountChangedWorkShifts();
            }

        }

        [Test]
        public void VerifyScheduleDayEquatorShouldReturnTheNumberOfDayEquatorReturnTrue()
        {
            IPerson person = PersonFactory.CreatePerson();
            IScenario scenario = new Scenario("TestScenario");
            DateTime startDate = new DateTime(2010, 01, 10, 0, 0, 0, DateTimeKind.Utc);
            DateTime endDate = startDate.AddDays(10);
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDate, endDate);
            ISkill skill = SkillFactory.CreateSkill("TestSkill");

            IScheduleDayPro scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();

            SchedulePartFactoryForDomain schedulePartFactory = new SchedulePartFactoryForDomain(person, scenario, dateTimePeriod, skill);

            _part = schedulePartFactory.CreatePartWithMainShift();
            IList<IScheduleDayPro> periodDays = new List<IScheduleDayPro> { _scheduleDayPro, scheduleDayPro2 };

            using (_mocks.Record())
            {
                Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodDays)).Repeat.Any();
                Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 10)).Repeat.Any();
                Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 10))).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
                Expect.Call(scheduleDayPro2.Day).Return(new DateOnly(2010, 1, 11)).Repeat.Any();
                Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 11))).Return(scheduleDayPro2);
                Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayEquator.MainShiftEquals(null, null)).IgnoreArguments()
                    .Return(true)
                    .Repeat.Once();
                Expect.Call(_scheduleDayEquator.MainShiftEquals(null, null)).IgnoreArguments()
                    .Return(false)
                    .Repeat.Once();

            }

            int result;
            using (_mocks.Playback())
            {
                _target = new ScheduleMatrixOriginalStateContainer(_matrix, _scheduleDayEquator);
                result = _target.CountChangedWorkShifts();
            }

            Assert.AreEqual(1, result);
        }

        private void mockExpectations()
        {
            
            IList<IScheduleDayPro> periodDays = new List<IScheduleDayPro>{_scheduleDayPro};
            
            Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodDays)).Repeat.Any();
            Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 1)).Repeat.Any();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
            Expect.Call(_part.Clone()).Return(_part).Repeat.Once();
        }
    }
}