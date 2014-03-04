using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class RelativeDailyStandardDeviationsByPersonalSkillsExtractorTest
    {
        private RelativeDailyValueByPersonalSkillsExtractor _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private IPerson _person;
        private HashSet<ISkill> _skillList;
        private ISkill _skillA;
        private IScheduleDayPro _scheduleDayPro;
        private ISchedulingResultStateHolder _stateHolder;
        private ISkillStaffPeriodHolder _skillStaffPeriodHolder;
        private IList<ISkillStaffPeriod> _skillStaffPeriods;
        private IAdvancedPreferences _advancedPreferences;
		private ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private ISkillIntervalDataDivider _skillIntervalDataDivider;
		private ISkillIntervalDataAggregator _skillIntervalDataAggregator;
	    private IList<ISkillIntervalData> _skillIntervalDatas;
			
			
		[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _skillList = new HashSet<ISkill>();
            _skillA = SkillFactory.CreateSkill("skillA", SkillTypeFactory.CreateSkillType(), 15);
            _skillList.Add(_skillA);
            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2010, 1, 1), _skillList);
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _skillStaffPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();
            _advancedPreferences = new AdvancedPreferences();
			_skillStaffPeriodToSkillIntervalDataMapper = _mocks.StrictMock<ISkillStaffPeriodToSkillIntervalDataMapper>();
			_skillIntervalDataDivider = _mocks.StrictMock<ISkillIntervalDataDivider>();
			_skillIntervalDataAggregator = _mocks.StrictMock<ISkillIntervalDataAggregator>();
	        _target = new RelativeDailyValueByPersonalSkillsExtractor(_matrix, _advancedPreferences,
	                                                                  _skillStaffPeriodToSkillIntervalDataMapper,
	                                                                  _skillIntervalDataDivider,
	                                                                  _skillIntervalDataAggregator);
			_skillIntervalDatas = new List<ISkillIntervalData>();
        }

		[Test, Ignore]
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

			_skillIntervalDatas.Add(new SkillIntervalData(new DateTimePeriod(), 10, -10, 10, null, null));

			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person).Repeat.Any();
				Expect.Call(_matrix.EffectivePeriodDays)
					.Return(new ReadOnlyCollection<IScheduleDayPro>
						(new List<IScheduleDayPro> { _scheduleDayPro }))
					.Repeat.Any();
				Expect.Call(_matrix.SchedulingStateHolder)
					.Return(_stateHolder).Repeat.Any();

				Expect.Call(_scheduleDayPro.Day)
					.Return(scheduleDay).Repeat.Any();
				Expect.Call(_stateHolder.SkillStaffPeriodHolder)
					.Return(_skillStaffPeriodHolder).Repeat.Any();
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
					.Return(_skillStaffPeriods).Repeat.Any();
				Expect.Call(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(_skillStaffPeriods))
					  .Return(_skillIntervalDatas);
				Expect.Call(_skillIntervalDataDivider.SplitSkillIntervalData(_skillIntervalDatas, 15))
					  .Return(_skillIntervalDatas);
				Expect.Call(
					_skillIntervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>>
			            {
				            _skillIntervalDatas
			            })).Return(_skillIntervalDatas);

				SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);

			}
			IList<double?> ret;
			using (_mocks.Playback())
			{
				ret = _target.Values();
			}
			Assert.AreEqual(1, ret.Count);
			Assert.AreEqual(0, ret[0].Value);
		}

		[Test, Ignore]
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
			_advancedPreferences.UseMaximumStaffing = false;
			_skillIntervalDatas.Add(new SkillIntervalData(new DateTimePeriod(), 10, -10, 10, null, 10));
			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person).Repeat.Any();
				Expect.Call(_matrix.EffectivePeriodDays)
					.Return(new ReadOnlyCollection<IScheduleDayPro>
						(new List<IScheduleDayPro> { _scheduleDayPro }))
					.Repeat.Any();
				Expect.Call(_matrix.SchedulingStateHolder)
					.Return(_stateHolder).Repeat.Any();

				Expect.Call(_scheduleDayPro.Day)
					.Return(scheduleDay).Repeat.Any();
				Expect.Call(_stateHolder.SkillStaffPeriodHolder)
					.Return(_skillStaffPeriodHolder).Repeat.Any();
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
					.Return(_skillStaffPeriods).Repeat.Any();
				Expect.Call(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(_skillStaffPeriods))
					  .Return(_skillIntervalDatas);
				Expect.Call(_skillIntervalDataDivider.SplitSkillIntervalData(_skillIntervalDatas, 15))
					  .Return(_skillIntervalDatas);
				Expect.Call(
					_skillIntervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>>
			            {
				            _skillIntervalDatas
			            })).Return(_skillIntervalDatas);


				SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);

			}
			IList<double?> ret;
			using (_mocks.Playback())
			{
				ret = _target.Values();
			}
			Assert.AreEqual(1, ret.Count);
			Assert.AreEqual(0, ret[0].Value);
		}

		[Test, Ignore]
		public void VerifyValueOneDayOneSkillStaffPeriodMaxStaff()
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

			_skillIntervalDatas.Add(new SkillIntervalData(new DateTimePeriod(), 10, -10, 10, 10, null));
			_advancedPreferences.UseMinimumStaffing = false;
			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person).Repeat.Any();
				Expect.Call(_matrix.EffectivePeriodDays)
					.Return(new ReadOnlyCollection<IScheduleDayPro>
						(new List<IScheduleDayPro> { _scheduleDayPro }))
					.Repeat.Any();
				Expect.Call(_matrix.SchedulingStateHolder)
					.Return(_stateHolder).Repeat.Any();

				Expect.Call(_scheduleDayPro.Day)
					.Return(scheduleDay).Repeat.Any();
				Expect.Call(_stateHolder.SkillStaffPeriodHolder)
					.Return(_skillStaffPeriodHolder).Repeat.Any();
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
					.Return(_skillStaffPeriods).Repeat.Any();
				Expect.Call(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(_skillStaffPeriods))
					  .Return(_skillIntervalDatas);
				Expect.Call(_skillIntervalDataDivider.SplitSkillIntervalData(_skillIntervalDatas, 15))
					  .Return(_skillIntervalDatas);
				Expect.Call(
					_skillIntervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>>
			            {
				            _skillIntervalDatas
			            })).Return(_skillIntervalDatas);


				SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);

			}
			IList<double?> ret;
			using (_mocks.Playback())
			{
				ret = _target.Values();
			}
			Assert.AreEqual(1, ret.Count);
			Assert.AreEqual(0, ret[0].Value);
		}

		[Test, Ignore]
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
			_skillIntervalDatas.Add(new SkillIntervalData(new DateTimePeriod(), 10, -10, 10, 10, 10));
			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person).Repeat.Any();
				Expect.Call(_matrix.EffectivePeriodDays)
					.Return(new ReadOnlyCollection<IScheduleDayPro>
						(new List<IScheduleDayPro> { _scheduleDayPro }))
					.Repeat.Any();
				Expect.Call(_matrix.SchedulingStateHolder)
					.Return(_stateHolder).Repeat.Any();

				Expect.Call(_scheduleDayPro.Day)
					.Return(scheduleDay).Repeat.Any();
				Expect.Call(_stateHolder.SkillStaffPeriodHolder)
					.Return(_skillStaffPeriodHolder).Repeat.Any();
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
					.Return(_skillStaffPeriods).Repeat.Any();
				Expect.Call(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(_skillStaffPeriods))
					  .Return(_skillIntervalDatas);
				Expect.Call(_skillIntervalDataDivider.SplitSkillIntervalData(_skillIntervalDatas, 15))
					  .Return(_skillIntervalDatas);
				Expect.Call(
					_skillIntervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>>
			            {
				            _skillIntervalDatas
			            })).Return(_skillIntervalDatas);


				SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);

			}
			IList<double?> ret;
			using (_mocks.Playback())
			{
				ret = _target.Values();
			}
			Assert.AreEqual(1, ret.Count);
			Assert.AreEqual(0, ret[0].Value);
		}

		[Test, Ignore]
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
			_advancedPreferences.UseMinimumStaffing = false;
			_advancedPreferences.UseMaximumStaffing = false;

			_skillIntervalDatas.Add(new SkillIntervalData(new DateTimePeriod(), 10, -10, 10, null, null));
			_skillIntervalDatas.Add(new SkillIntervalData(new DateTimePeriod(), 10, -20, 10, null, null));
			_skillIntervalDatas.Add(new SkillIntervalData(new DateTimePeriod(), 10, -30, 10, null, null));
			using (_mocks.Record())
			{
				Expect.Call(_matrix.Person).Return(_person).Repeat.Any();
				Expect.Call(_matrix.EffectivePeriodDays)
					.Return(new ReadOnlyCollection<IScheduleDayPro>
						(new List<IScheduleDayPro> { _scheduleDayPro }))
					.Repeat.Any();
				Expect.Call(_matrix.SchedulingStateHolder)
					.Return(_stateHolder).Repeat.Any();

				Expect.Call(_scheduleDayPro.Day)
					.Return(scheduleDay).Repeat.Any();
				Expect.Call(_stateHolder.SkillStaffPeriodHolder)
					.Return(_skillStaffPeriodHolder).Repeat.Any();
				Expect.Call(_skillStaffPeriodHolder.SkillStaffPeriodList(_skillList, scheduleUtcDateTimePeriod))
					.Return(_skillStaffPeriods).Repeat.Any();
				Expect.Call(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(_skillStaffPeriods))
					  .Return(_skillIntervalDatas);
				Expect.Call(_skillIntervalDataDivider.SplitSkillIntervalData(_skillIntervalDatas, 15))
					  .Return(_skillIntervalDatas);
				Expect.Call(
					_skillIntervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>>
			            {
				            _skillIntervalDatas
			            })).Return(_skillIntervalDatas);


				SetSkillStaffPeriodExpectations(skillStaffPeriod1, 1);
				SetSkillStaffPeriodExpectations(skillStaffPeriod2, 2);
				SetSkillStaffPeriodExpectations(skillStaffPeriod3, 3);

			}
			IList<double?> ret;
			using (_mocks.Playback())
			{
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
        