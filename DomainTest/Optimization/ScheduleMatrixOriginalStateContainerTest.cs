using System;
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
                Expect.Call(_part.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
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
                Expect.Call(_part.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
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
                Expect.Call(_part.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                _target = new ScheduleMatrixOriginalStateContainer(_matrix, _scheduleDayEquator);
                Assert.IsFalse(_target.IsFullyScheduled());
            }

        }

        [Test]
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
            var periodDays = new [] {_scheduleDayPro, scheduleDayPro2};

            int periodDayCount = periodDays.Length;

            using(_mocks.Record())
            {
                Expect.Call(_matrix.EffectivePeriodDays).Return(periodDays).Repeat.Any();
                Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 10)).Repeat.Any();
                Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 10))).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
                Expect.Call(scheduleDayPro2.Day).Return(new DateOnly(2010, 1, 11)).Repeat.Any();
                Expect.Call(_matrix.GetScheduleDayByKey(new DateOnly(2010, 1, 11))).Return(_scheduleDayPro);
                Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayEquator.MainShiftEquals(_part, _part)).IgnoreArguments()
                    .Return(true)
                    .Repeat.Times(periodDayCount);

            }

            using(_mocks.Playback())
            {
                _target = new ScheduleMatrixOriginalStateContainer(_matrix, _scheduleDayEquator);
                _target.ChangedWorkShiftsPercent();
            }

        }

		[Test]
		public void VerifyChangedDayOffsPercent()
		{
			IScheduleDayPro scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();

			var periodDays = new [] {_scheduleDayPro, scheduleDayPro2};

			using(_mocks.Record())
			{
				Expect.Call(_matrix.EffectivePeriodDays).Return(periodDays).Repeat.Any();
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 10)).Repeat.Any();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro2.Day).Return(new DateOnly(2010, 1, 11)).Repeat.Any();
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
				Expect.Call(_part.Clone()).Return(_part).Repeat.Twice();
				Expect.Call(_part.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_part.SignificantPart()).Return(SchedulePartView.DayOff);

				Expect.Call(_scheduleDayEquator.DayOffEquals(_part, _part)).Return(false);
				Expect.Call(_scheduleDayEquator.DayOffEquals(_part, _part)).Return(true);
			}

			double ret;

			using (_mocks.Playback())
			{
				_target = new ScheduleMatrixOriginalStateContainer(_matrix, _scheduleDayEquator);
				ret = _target.ChangedDayOffsPercent();
			}

			Assert.AreEqual(.5, ret);
		}
		
        private void mockExpectations()
        {
            
            var periodDays = new []{_scheduleDayPro};
            
            Expect.Call(_matrix.EffectivePeriodDays).Return(periodDays).Repeat.Any();
            Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 1)).Repeat.Any();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_part).Repeat.AtLeastOnce();
            Expect.Call(_part.Clone()).Return(_part).Repeat.Once();
            
        }
    }
}