using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class RelativeDailyStandardDeviationsByAllSkillsExtractorTest
    {
        private RelativeDailyStandardDeviationsByAllSkillsExtractor _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private ISkill[] _skillList;
        private ISkill _skillA;
        private IScheduleDayPro _scheduleDayPro;
        private ISchedulingResultStateHolder _stateHolder;
        private ISkillStaffPeriodHolder _skillStaffPeriodHolder;
        private IList<ISkillStaffPeriod> _skillStaffPeriods;
        private SchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _skillA = SkillFactory.CreateSkill("skillA", SkillTypeFactory.CreateSkillType(), 15);
            _skillList = new []{_skillA};
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _skillStaffPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();
            _schedulingOptions = new SchedulingOptions();
            _target = new RelativeDailyStandardDeviationsByAllSkillsExtractor(_matrix, _schedulingOptions, _stateHolder, TimeZoneInfoFactory.StockholmTimeZoneInfo());
        }

        [Test]
        public void VerifyValueOneDayOneSkillStaffPeriod()
        {
            ISkillStaffPeriod skillStaffPeriod1 = _mocks.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriod1 };
            DateOnly scheduleDay = new DateOnly(2010, 4, 1);
            DateTimePeriod scheduleUtcDateTimePeriod = 
                new DateTimePeriod(
                    new DateTime(2010, 3, 31, 22, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2010, 4, 1, 22, 0, 0, 0, DateTimeKind.Utc));
            /* note: the expression above is equal for the follwing expression in the swedish time zone
            TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
            scheduleDay.Date, scheduleDay.Date.AddDays(1),
            TeleoptiPrincipal.Current.Regional.TimeZone);
            */

            using (_mocks.Record())
            {
                Expect.Call(_matrix.EffectivePeriodDays)
                    .Return(new [] { _scheduleDayPro })
                    .Repeat.AtLeastOnce();

                Expect.Call(_scheduleDayPro.Day)
                    .Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.Skills)
                    .Return(_skillList).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.SkillStaffPeriodHolder)
                    .Return(_skillStaffPeriodHolder).Repeat.AtLeastOnce();
                Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
                    .Return(_skillStaffPeriods).Repeat.AtLeastOnce();
				
                SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);

            }
            IList<double?> ret;
            using (_mocks.Playback())
			{
				_schedulingOptions.UseMinimumPersons = false;
				_schedulingOptions.UseMaximumPersons = false;
				ret = _target.Values();
            }
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(0, ret[0].Value);
        }

        [Test]
        public void VerifyValueOneDayOneSkillStaffPeriodMinStaff()
        {
            ISkillStaffPeriod skillStaffPeriod1;

            skillStaffPeriod1 = _mocks.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriod1 };
            DateOnly scheduleDay = new DateOnly(2010, 4, 1);
            DateTimePeriod scheduleUtcDateTimePeriod =
                new DateTimePeriod(
                    new DateTime(2010, 3, 31, 22, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2010, 4, 1, 22, 0, 0, 0, DateTimeKind.Utc));
            /* note: the expression above is equal for the follwing expression in the swedish time zone
            TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
            scheduleDay.Date, scheduleDay.Date.AddDays(1),
            TeleoptiPrincipal.Current.Regional.TimeZone);
            */

            using (_mocks.Record())
            {
                Expect.Call(_matrix.EffectivePeriodDays)
                    .Return(new [] { _scheduleDayPro })
                    .Repeat.AtLeastOnce();

                Expect.Call(_scheduleDayPro.Day)
                    .Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.Skills)
                    .Return(_skillList).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.SkillStaffPeriodHolder)
                    .Return(_skillStaffPeriodHolder).Repeat.AtLeastOnce();
                Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
                    .Return(_skillStaffPeriods).Repeat.AtLeastOnce();
				
                SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);

            }
            IList<double?> ret;
            using (_mocks.Playback())
            {
	            _schedulingOptions.UseMinimumPersons = true;
				_schedulingOptions.UseMaximumPersons = false;
				ret = _target.Values();
            }
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(0, ret[0].Value);
        }

        [Test]
        public void VerifyValueOneDayOneSkillStaffPeriodMaxStaff()
        {
	        var skillStaffPeriod1 = _mocks.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriod1 };
            DateOnly scheduleDay = new DateOnly(2010, 4, 1);
            DateTimePeriod scheduleUtcDateTimePeriod =
                new DateTimePeriod(
                    new DateTime(2010, 3, 31, 22, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2010, 4, 1, 22, 0, 0, 0, DateTimeKind.Utc));
            /* note: the expression above is equal for the follwing expression in the swedish time zone
            TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
            scheduleDay.Date, scheduleDay.Date.AddDays(1),
            TeleoptiPrincipal.Current.Regional.TimeZone);
            */

            using (_mocks.Record())
            {
                Expect.Call(_matrix.EffectivePeriodDays)
                    .Return(new [] { _scheduleDayPro })
                    .Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day)
                    .Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.Skills)
                    .Return(_skillList).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.SkillStaffPeriodHolder)
                    .Return(_skillStaffPeriodHolder).Repeat.AtLeastOnce();
                Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
                    .Return(_skillStaffPeriods).Repeat.AtLeastOnce();
				
                SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);

            }
            IList<double?> ret;
            using (_mocks.Playback())
			{
				_schedulingOptions.UseMinimumPersons = false;
				_schedulingOptions.UseMaximumPersons = true;
				ret = _target.Values();
            }
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(0, ret[0].Value);
        }

        [Test]
        public void VerifyValueOneDayOneSkillStaffPeriodMinMaxStaff()
        {
            ISkillStaffPeriod skillStaffPeriod1;

            skillStaffPeriod1 = _mocks.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriod1 };
            DateOnly scheduleDay = new DateOnly(2010, 4, 1);
            DateTimePeriod scheduleUtcDateTimePeriod =
                new DateTimePeriod(
                    new DateTime(2010, 3, 31, 22, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2010, 4, 1, 22, 0, 0, 0, DateTimeKind.Utc));
            /* note: the expression above is equal for the follwing expression in the swedish time zone
            TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
            scheduleDay.Date, scheduleDay.Date.AddDays(1),
            TeleoptiPrincipal.Current.Regional.TimeZone);
            */

            using (_mocks.Record())
            {
                Expect.Call(_matrix.EffectivePeriodDays)
                    .Return(new [] { _scheduleDayPro })
                    .Repeat.AtLeastOnce();

                Expect.Call(_scheduleDayPro.Day)
                    .Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.Skills)
                    .Return(_skillList).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.SkillStaffPeriodHolder)
                    .Return(_skillStaffPeriodHolder).Repeat.AtLeastOnce();
                Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
                    .Return(_skillStaffPeriods).Repeat.AtLeastOnce();
				
                SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);

            }
            IList<double?> ret;
            using (_mocks.Playback())
			{
				_schedulingOptions.UseMinimumPersons = true;
				_schedulingOptions.UseMaximumPersons = true;
				ret = _target.Values();
            }
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(0, ret[0].Value);
        }

        [Test]
        public void VerifyValueOneDayMultipleSkillStaffPeriod()
        {
            ISkillStaffPeriod skillStaffPeriod1 = _mocks.StrictMock<ISkillStaffPeriod>();
            ISkillStaffPeriod skillStaffPeriod2 = _mocks.StrictMock<ISkillStaffPeriod>();
            ISkillStaffPeriod skillStaffPeriod3 = _mocks.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2, skillStaffPeriod3 };
            DateOnly scheduleDay = new DateOnly(2010, 4, 1);
            DateTimePeriod scheduleUtcDateTimePeriod =
                new DateTimePeriod(
                    new DateTime(2010, 3, 31, 22, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2010, 4, 1, 22, 0, 0, 0, DateTimeKind.Utc));
            /* note: the expression above is equal for the follwing expression in the swedish time zone
            TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
            scheduleDay.Date, scheduleDay.Date.AddDays(1),
            TeleoptiPrincipal.Current.Regional.TimeZone);
            */

            using (_mocks.Record())
            {
                Expect.Call(_matrix.EffectivePeriodDays)
                    .Return(new [] { _scheduleDayPro })
                    .Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day)
                    .Return(scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.Skills)
                    .Return(_skillList).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.SkillStaffPeriodHolder)
                    .Return(_skillStaffPeriodHolder).Repeat.AtLeastOnce();
                Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
                    .Return(_skillStaffPeriods).Repeat.AtLeastOnce();
				
                SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);
                SetSkillStaffPeriodExpectations(skillStaffPeriod2, 2);
                SetSkillStaffPeriodExpectations(skillStaffPeriod3, 3);

            }
            IList<double?> ret;
            using (_mocks.Playback())
			{
				_schedulingOptions.UseMinimumPersons = false;
				_schedulingOptions.UseMaximumPersons = false;

				ret = _target.Values();
            }
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(0.81d, ret[0].Value, 0.1);
        }

        private static void SetSkillStaffPeriodExpectations(ISkillStaffPeriod skillStaffPeriod, double relativeDifference)
        {
            Expect.Call(skillStaffPeriod.RelativeDifference).Return(relativeDifference).Repeat.Any();
            Expect.Call(skillStaffPeriod.RelativeDifferenceMinStaffBoosted()).Return(relativeDifference).Repeat.Any();
            Expect.Call(skillStaffPeriod.RelativeDifferenceMaxStaffBoosted()).Return(relativeDifference).Repeat.Any();
            Expect.Call(skillStaffPeriod.RelativeDifferenceBoosted()).Return(relativeDifference).Repeat.Any();
            DateTime startDateTime = new DateTime(2010, 3, 31, 22, 0, 0, 0, DateTimeKind.Utc);
            Expect.Call(skillStaffPeriod.Period)
                .Return(new DateTimePeriod(startDateTime, startDateTime.AddMinutes(60)))
                .Repeat.Any();
        }

    }
}
        