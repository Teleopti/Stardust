using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimeLengthDeciderTest
    {
        private IOvertimeLengthDecider _target;
        private MockRepository _mocks;
	    private IOvertimeSkillStaffPeriodToSkillIntervalDataMapper _overtimeSkillStaffPeriodToSkillIntervalDataMapper;
        private IOvertimeSkillIntervalDataDivider _overtimeSkillIntervalDataDivider;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private DateTime _date;
        private ISkillDay _skillDay1;
        private IPerson _person;
        private ISkill _skill1;
        private ISkill _skill2;
        private ICalculateBestOvertime _calculateBestOvertime;
	    private IOvertimeSkillIntervalDataAggregator _overtimeSkillIntervalDataAggregator;
	    private IScheduleDay _scheduleDay;
	    private MinMax<TimeSpan> _overtimeSpecifiedPeriod;
	    private IProjectionService _projectionService;
	    private IVisualLayerCollection _visualLayerCollection;
		
		[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _date = DateTime.SpecifyKind(DateOnly.MinValue.Date, DateTimeKind.Utc);
            _overtimeSkillStaffPeriodToSkillIntervalDataMapper = _mocks.StrictMock<IOvertimeSkillStaffPeriodToSkillIntervalDataMapper>();
            _overtimeSkillIntervalDataDivider = _mocks.StrictMock<IOvertimeSkillIntervalDataDivider>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _skillDay1 = _mocks.StrictMock<ISkillDay>();
            _skill1 = SkillFactory.CreateSkill("1");
            _skill2 = SkillFactory.CreateSkill("2");
            _person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _skill1, _skill2 });
	        _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _overtimeSkillIntervalDataAggregator = _mocks.StrictMock<IOvertimeSkillIntervalDataAggregator>();
            _calculateBestOvertime = _mocks.StrictMock<ICalculateBestOvertime>();
	        _projectionService = _mocks.StrictMock<IProjectionService>();
			_visualLayerCollection = new VisualLayerCollection(_person, new List<IVisualLayer>(), null );
			_overtimeSpecifiedPeriod = new MinMax<TimeSpan>(TimeSpan.Zero, TimeSpan.FromDays(1).Add(TimeSpan.FromHours(6)));
            _target = new OvertimeLengthDecider(_overtimeSkillStaffPeriodToSkillIntervalDataMapper,
                                                _overtimeSkillIntervalDataDivider, ()=>_schedulingResultStateHolder, 
												_calculateBestOvertime,
												_overtimeSkillIntervalDataAggregator, new PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider(new PersonSkillsUseAllForScheduleDaysOvertimeProvider(), new PersonalSkillsProvider()));
        }

		[Test]
		public void ShouldBeZeroIfPersonHasNoSkillOfOneActivity()
		{
			var result = _target.Decide(new OvertimePreferences {SkillActivity = ActivityFactory.CreateActivity("No Match Acitivity") }, _person, DateOnly.Today, _scheduleDay,
										new MinMax<TimeSpan>(TimeSpan.FromHours(1), TimeSpan.FromHours(2)),
										_overtimeSpecifiedPeriod,
										false);
			Assert.That(result.Count(), Is.EqualTo(0));
		}


		[Test]
		public void ShouldNotExtendWhenRelativeDifferenceIsPositive()
		{
			var skillIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(_date, _date.AddMinutes(15)), 0, 1);
			var skillIntervalData2 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(15), _date.AddMinutes(30)), 0, 1);
			var skillIntervalData3 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(30), _date.AddMinutes(45)), 0, 2);
			var skillIntervalData4 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(45), _date.AddMinutes(60)), 0, 3);
			var skillIntervalData5 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(60), _date.AddMinutes(75)), 2, 4);
			var skillIntervalData6 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(75), _date.AddMinutes(90)), 1, 5);
			var skillIntervalData7 = new OvertimeSkillIntervalData(new DateTimePeriod(_date.AddMinutes(90), _date.AddMinutes(105)), -3, 6);
			
			MinMax<TimeSpan> duration = new MinMax<TimeSpan>(TimeSpan.FromHours(1), TimeSpan.FromHours(2));
			var overtimeSkillIntervalDataList = new[]
				{
					skillIntervalData1, skillIntervalData2, skillIntervalData3, skillIntervalData4, skillIntervalData5,
					skillIntervalData6, skillIntervalData7
				};

			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
							  Return(new List<ISkillDay> { _skillDay1 });
				Expect.Call(_overtimeSkillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>()))
					.IgnoreArguments()
					.Return(overtimeSkillIntervalDataList)
					.Repeat.AtLeastOnce();
				Expect.Call(_overtimeSkillIntervalDataDivider.SplitSkillIntervalData(new List<IOvertimeSkillIntervalData>(), 15))
					.IgnoreArguments()
					.Return(overtimeSkillIntervalDataList)
					.Repeat.AtLeastOnce();
				Expect.Call(_skillDay1.Skill).Return(_skill1);
				Expect.Call(_calculateBestOvertime.GetBestOvertime(duration, _overtimeSpecifiedPeriod, _scheduleDay, 15, false, null))
					.IgnoreArguments()
					.Return(new List<DateTimePeriod>());
				Expect.Call(
					_overtimeSkillIntervalDataAggregator.AggregateOvertimeSkillIntervalData(new List<IList<IOvertimeSkillIntervalData>>()))
					.IgnoreArguments()
					.Return(overtimeSkillIntervalDataList);
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			}
			using (_mocks.Playback())
			{

				var resultLength = _target.Decide(new OvertimePreferences { SkillActivity = _skill1.Activity}, _person, new DateOnly(_date), _scheduleDay, duration,_overtimeSpecifiedPeriod, false);

				Assert.That(resultLength.Count(), Is.EqualTo(0));
			}
		}

		[Test]
		public void ShouldNotExtendWhenSpecifiedDurationIsZero()
		{

			var skillIntervalData1 = new OvertimeSkillIntervalData(new DateTimePeriod(_date, _date.AddMinutes(15)), 0, 1);
			var overtimeSkillIntervalDataList = new List<IOvertimeSkillIntervalData>() { skillIntervalData1 };
			MinMax<TimeSpan> duration = new MinMax<TimeSpan>(TimeSpan.FromHours(0), TimeSpan.FromHours(0));
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
							  Return(new List<ISkillDay> { _skillDay1 });
				Expect.Call(_skillDay1.Skill).Return(_skill1);
				Expect.Call(_overtimeSkillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>())).IgnoreArguments().Return(
					 overtimeSkillIntervalDataList).Repeat.AtLeastOnce();
				Expect.Call(_overtimeSkillIntervalDataDivider.SplitSkillIntervalData(new List<IOvertimeSkillIntervalData>(), 15)).IgnoreArguments().
						 Return(overtimeSkillIntervalDataList).Repeat.AtLeastOnce();
				Expect.Call(
					_overtimeSkillIntervalDataAggregator.AggregateOvertimeSkillIntervalData(
						new List<IList<IOvertimeSkillIntervalData>>())).IgnoreArguments().Return(overtimeSkillIntervalDataList);
				Expect.Call(_calculateBestOvertime.GetBestOvertime(duration, _overtimeSpecifiedPeriod, _scheduleDay, 15, false, null))
					  .IgnoreArguments()
					  .Return(new List<DateTimePeriod>());
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			}
			using (_mocks.Playback())
			{
				var resultLength = _target.Decide(new OvertimePreferences {SkillActivity = _skill1.Activity }, _person, new DateOnly(_date), _scheduleDay, duration,_overtimeSpecifiedPeriod, false);

				Assert.That(resultLength.Count(), Is.EqualTo(0));
			}
		}

		[Test]
		public void ShouldReturnZeroIfNoForecastIsAvailable()
		{

			var overtimeSkillIntervalDataList = new List<IOvertimeSkillIntervalData>();
			
			MinMax<TimeSpan> duration = new MinMax<TimeSpan>(TimeSpan.FromHours(0), TimeSpan.FromHours(0));

			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>()))
					.IgnoreArguments()
					.Return(new List<ISkillDay> { _skillDay1 });
				Expect.Call(_overtimeSkillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>()))
					.IgnoreArguments()
					.Return(overtimeSkillIntervalDataList).Repeat.AtLeastOnce();
				Expect.Call(_skillDay1.Skill).Return(_skill1);
				Expect.Call(
					_overtimeSkillIntervalDataAggregator.AggregateOvertimeSkillIntervalData(new List<IList<IOvertimeSkillIntervalData>>()))
					.IgnoreArguments()
					.Return(overtimeSkillIntervalDataList);
				Expect.Call(_calculateBestOvertime.GetBestOvertime(duration, _overtimeSpecifiedPeriod, _scheduleDay, 15, false, new List<IOvertimeSkillIntervalData>{}))
					.IgnoreArguments()
					.Return(new List<DateTimePeriod>());
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			}
			using (_mocks.Playback())
			{
				var resultLength = _target.Decide(new OvertimePreferences {SkillActivity = _skill1.Activity }, _person, new DateOnly(_date), _scheduleDay, duration, _overtimeSpecifiedPeriod, false);

				Assert.That(resultLength.Count(), Is.EqualTo(0));
			}
		}
    }
}
