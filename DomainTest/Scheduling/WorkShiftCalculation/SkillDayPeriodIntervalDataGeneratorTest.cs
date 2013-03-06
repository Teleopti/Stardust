using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class SkillDayPeriodIntervalDataGeneratorTest
    {
        private ISkillDayPeriodIntervalDataGenerator _target;
        private MockRepository _mock;
        private ISkillDay _skillDay1;
        private ISkillDay _skillDay2;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private List<ISkillDay> _skillDayList;
    	private ISkillIntervalDataSkillFactorApplier _factorApplier;
    	private ISkillIntervalDataAggregator _intervalDataAggregator;
    	private IDayIntervalDataCalculator _dayIntervalDataCalculator;
    	private ISkillStaffPeriodToSkillIntervalDataMapper _intervalMapper;
    	private ISkillResolutionProvider _resolutionProvider;
    	private ISkillIntervalDataDivider _intervalDivider;
        private ReadOnlyCollection<ISkillStaffPeriod> _skillStaffPeriodCollection;
        private ISkillStaffPeriod _skillStaffPeriod;
	    private IGroupPersonSkillAggregator _groupPersonSkillAggregator;
	    private IGroupPerson _groupPerson;
	    private DateTime _date;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _skillDay1 = _mock.StrictMock<ISkillDay>();
            _skillDay2 = _mock.StrictMock<ISkillDay>();
            _skillDayList = new List<ISkillDay> {_skillDay1, _skillDay2};
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
        	_factorApplier = _mock.StrictMock<ISkillIntervalDataSkillFactorApplier>();
    		_intervalDataAggregator = _mock.StrictMock<ISkillIntervalDataAggregator>();
    		_dayIntervalDataCalculator = _mock.StrictMock<IDayIntervalDataCalculator>();
    		_intervalMapper = _mock.StrictMock<ISkillStaffPeriodToSkillIntervalDataMapper>();
    		_resolutionProvider =  _mock.StrictMock<ISkillResolutionProvider>();
    		_intervalDivider = _mock.StrictMock<ISkillIntervalDataDivider>();
            _skillStaffPeriod = _mock.StrictMock<ISkillStaffPeriod>();
			_groupPersonSkillAggregator =  _mock.StrictMock<IGroupPersonSkillAggregator>();
			_groupPerson = _mock.StrictMock<IGroupPerson>();
			_date = DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date, DateTimeKind.Utc);
            _skillStaffPeriodCollection = new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>{_skillStaffPeriod});
		    _target = new SkillDayPeriodIntervalDataGenerator(_factorApplier, _resolutionProvider, _intervalDivider,
		                                                      _intervalDataAggregator,
		                                                      _dayIntervalDataCalculator, _intervalMapper,
		                                                      _schedulingResultStateHolder, _groupPersonSkillAggregator);
        }

		[Test]
        public void ShouldCreateIntervalsFromSkillDay()
        {
			var skillIntervalData1 = new SkillIntervalData(new DateTimePeriod(_date, _date.AddMinutes(15)), 6.0, 0, 0, 0, 0);
			var skillIntervalData2 = new SkillIntervalData(new DateTimePeriod(_date, _date.AddMinutes(15)), 12.0, 0, 0, 0, 0);
            var activity1 = ActivityFactory.CreateActivity("phone1");
            var activity2 = ActivityFactory.CreateActivity("phone2");

			var intervalData1 = new Dictionary<TimeSpan, ISkillIntervalData>
                             {
                                 {
                                     _date.TimeOfDay,
                                     skillIntervalData1
                                     }
                             };	
			var intervalData2 = new Dictionary<TimeSpan, ISkillIntervalData>
                             {
                                 {
                                     _date.TimeOfDay,
                                     skillIntervalData2
                                     }
                             };
        	var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>
        	                           	{
        	                           		{activity1, intervalData1},
											{activity2, intervalData2}
        	                           	};
			
        	var skill1 = SkillFactory.CreateSkill("skill1");
        	skill1.Activity = activity1;
        	skill1.DefaultResolution = 15;
			var skill2 = SkillFactory.CreateSkill("skill2");
        	skill2.Activity = activity2;
        	skill2.DefaultResolution = 15;

			var skillIntervalDataList = new[] { skillIntervalData1, skillIntervalData2 };
			
            using (_mock.Record())
            {
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
			   Return(_skillDayList);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_groupPerson,
																		 new DateOnlyPeriod(new DateOnly(_date),
																							new DateOnly(_date))))
					  .Return(new List<ISkill> { skill1, skill2 });
				Expect.Call(_skillDay1.Skill).Return(skill1).Repeat.AtLeastOnce();
				Expect.Call(_skillDay1.CurrentDate).Return(new DateOnly());
				Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollection).Repeat.AtLeastOnce();
				Expect.Call(_skillDay2.Skill).Return(skill2).Repeat.AtLeastOnce();
				Expect.Call(_skillDay2.CurrentDate).Return(new DateOnly());
				Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollection).Repeat.AtLeastOnce();
				Expect.Call(_resolutionProvider.MinimumResolution(new List<ISkill>())).IgnoreArguments().Return(15);
				Expect.Call(_intervalMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>())).IgnoreArguments().Return(
					skillIntervalDataList).Repeat.AtLeastOnce();
				Expect.Call(_intervalDivider.SplitSkillIntervalData(new List<ISkillIntervalData>(), 15)).IgnoreArguments().
					Return(skillIntervalDataList).Repeat.AtLeastOnce();

				Expect.Call(_factorApplier.ApplyFactors(new SkillIntervalData(new DateTimePeriod(), 0, 0, 0, null, null),
														skill1)).IgnoreArguments().Return(skillIntervalData1).Repeat.Times(2);
				Expect.Call(_factorApplier.ApplyFactors(new SkillIntervalData(new DateTimePeriod(), 0, 0, 0, null, null),
														skill2)).IgnoreArguments().Return(skillIntervalData2).Repeat.Times(2);

				Expect.Call(_dayIntervalDataCalculator.Calculate(15, new Dictionary<DateOnly, IList<ISkillIntervalData>>())).
					IgnoreArguments().Return(intervalData1);
				Expect.Call(_dayIntervalDataCalculator.Calculate(15, new Dictionary<DateOnly, IList<ISkillIntervalData>>())).
					IgnoreArguments().Return(intervalData2);
            }
            using(_mock.Playback())
            {
				var calculatedResult = _target.Generate(_groupPerson, new List<DateOnly> { new DateOnly(_date) });
				Assert.That(calculatedResult.Count, Is.EqualTo(2));
				Assert.That(calculatedResult[activity1][_date.TimeOfDay].ForecastedDemand, Is.EqualTo(activityIntervalData[activity1][_date.TimeOfDay].ForecastedDemand));
				Assert.That(calculatedResult[activity2][_date.TimeOfDay].ForecastedDemand, Is.EqualTo(activityIntervalData[activity2][_date.TimeOfDay].ForecastedDemand));
            }
        }

		[Test]
		public void ShouldSkipSkillsNotInAggregatedSkills()
		{
			var skillIntervalData1 = new SkillIntervalData(new DateTimePeriod(_date, _date.AddMinutes(15)), 6.0, 0, 0, 0, 0);
			var skillIntervalData2 = new SkillIntervalData(new DateTimePeriod(_date, _date.AddMinutes(15)), 12.0, 0, 0, 0, 0);
			var activity1 = ActivityFactory.CreateActivity("phone1");

			var intervalData1 = new Dictionary<TimeSpan, ISkillIntervalData>
                             {
                                 {
                                     _date.TimeOfDay,
                                     skillIntervalData1
                                     }
                             };
			var intervalData2 = new Dictionary<TimeSpan, ISkillIntervalData>
                             {
                                 {
                                     _date.TimeOfDay,
                                     skillIntervalData2
                                     }
                             };
			var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>
        	                           	{
        	                           		{activity1, intervalData1}
        	                           	};

			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.Activity = activity1;
			skill1.DefaultResolution = 15;
			var skill2 = SkillFactory.CreateSkill("skill2");

			var skillIntervalDataList = new[] { skillIntervalData1, skillIntervalData2 };

			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
			   Return(_skillDayList);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_groupPerson,
																		 new DateOnlyPeriod(new DateOnly(_date),
																							new DateOnly(_date))))
					  .Return(new List<ISkill> { skill1 });
				Expect.Call(_skillDay1.Skill).Return(skill1).Repeat.AtLeastOnce();
				Expect.Call(_skillDay1.CurrentDate).Return(new DateOnly());
				Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollection).Repeat.AtLeastOnce();
				Expect.Call(_skillDay2.Skill).Return(skill2).Repeat.AtLeastOnce();
				Expect.Call(_skillDay2.CurrentDate).Return(new DateOnly());
				Expect.Call(_resolutionProvider.MinimumResolution(new List<ISkill>())).IgnoreArguments().Return(15);
				Expect.Call(_intervalMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>())).IgnoreArguments().Return(
					skillIntervalDataList).Repeat.AtLeastOnce();
				Expect.Call(_intervalDivider.SplitSkillIntervalData(new List<ISkillIntervalData>(), 15)).IgnoreArguments().
					Return(skillIntervalDataList).Repeat.AtLeastOnce();

				Expect.Call(_factorApplier.ApplyFactors(new SkillIntervalData(new DateTimePeriod(), 0, 0, 0, null, null),
														skill1)).IgnoreArguments().Return(skillIntervalData1).Repeat.Times(2);

				Expect.Call(_dayIntervalDataCalculator.Calculate(15, new Dictionary<DateOnly, IList<ISkillIntervalData>>())).
					IgnoreArguments().Return(intervalData1);
			}
			using (_mock.Playback())
			{
				var calculatedResult = _target.Generate(_groupPerson, new List<DateOnly> { new DateOnly(_date) });
				Assert.That(calculatedResult.Count, Is.EqualTo(1));
				Assert.That(calculatedResult[activity1][_date.TimeOfDay].ForecastedDemand, Is.EqualTo(activityIntervalData[activity1][_date.TimeOfDay].ForecastedDemand));
			}
		}
    }
}
