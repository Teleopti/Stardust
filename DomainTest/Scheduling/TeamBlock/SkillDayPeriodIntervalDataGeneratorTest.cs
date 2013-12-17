using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
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
	    private BlockInfo _blockInfo;
        private ITeamBlockInfo _teamBlockInfo2;
        private ITeamInfo _teamInfo2;
        private BaseLineData _baseLineData;
        private ISkill _skill1;
        private ISkill _skill2;
        private Activity _activity1;
        private Activity _activity2;
        private IOpenHourRestrictionForTeamBlock _openHourrestrcitionForTeamBlock;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            //TODO Should refactor these tests as most of the classes are finished
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
            _openHourrestrcitionForTeamBlock = _mock.StrictMock<IOpenHourRestrictionForTeamBlock>();
		    _target = new SkillDayPeriodIntervalDataGenerator(_factorApplier, _resolutionProvider, _intervalDivider,
		                                                      _intervalDataAggregator,
		                                                      _dayIntervalDataCalculator, _intervalMapper,
		                                                      _schedulingResultStateHolder, _groupPersonSkillAggregator,_openHourrestrcitionForTeamBlock);
		    _blockInfo = new BlockInfo(new DateOnlyPeriod(new DateOnly(_date), new DateOnly(_date)));

            _baseLineData = new BaseLineData();
            _teamInfo2 = new TeamInfo(_baseLineData.GroupPerson,new List<IList<IScheduleMatrixPro>>() );
            _teamBlockInfo2 = new TeamBlockInfo(_teamInfo2,_blockInfo  );

            _activity1 = ActivityFactory.CreateActivity("phone1");
            _activity2 = ActivityFactory.CreateActivity("phone2");
            _skill1 = SkillFactory.CreateSkill("skill1");
            _skill2 = SkillFactory.CreateSkill("skill2");

            _skill1.Activity = _activity1;
            _skill1.DefaultResolution = 15;

            _skill2.Activity = _activity2;
            _skill2.DefaultResolution = 15;
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnForEmptySkillStaffPeriodCollection()
        {
            var openHour = new Dictionary<IActivity, TimePeriod>();
            openHour.Add(_activity1,new TimePeriod(0,0,0,0));
            using (_mock.Record())
            {
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
                       Return(_skillDayList);
                Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_groupPerson,
                                                                         new DateOnlyPeriod(new DateOnly(_date),
                                                                                            new DateOnly(_date)))).IgnoreArguments()
                      .Return(new List<ISkill> { _skill1, _skill2 });
                Expect.Call(_skillDay1.Skill).Return(_skill1).Repeat.AtLeastOnce();
                Expect.Call(_skillDay1.CurrentDate).Return(new DateOnly());
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>())).Repeat.AtLeastOnce();
                Expect.Call(_skillDay2.Skill).Return(_skill2).Repeat.AtLeastOnce();
                Expect.Call(_skillDay2.CurrentDate).Return(new DateOnly());
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>())).Repeat.AtLeastOnce();
                Expect.Call(_resolutionProvider.MinimumResolution(new List<ISkill>())).IgnoreArguments().Return(15);
                Expect.Call(_openHourrestrcitionForTeamBlock.GetOpenHoursPerActivity(_teamBlockInfo2)).Return(openHour);

            }
            using (_mock.Playback())
            {
                var calculatedResult = _target.GeneratePerDay(_teamBlockInfo2);
                Assert.That(calculatedResult.Count, Is.EqualTo(0));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldNotSkipSkills()
        {
            var openHour = new Dictionary<IActivity, TimePeriod>();
            openHour.Add(_activity1, new TimePeriod(_date.Hour , _date.Minute , _date.Hour , _date.AddMinutes(15).Minute  ));
            var skillIntervalData1 = createSkillIntervalData(6.0);
            var skillIntervalData2 = createSkillIntervalData(12.0);
            var intervalData1 = getIntervalData(_date.TimeOfDay, skillIntervalData1);

            var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>{{_activity1, intervalData1}};

            var skill1 = SkillFactory.CreateSkill("skill1");
            skill1.Activity = _activity1;
            skill1.DefaultResolution = 15;
            var skill2 = skill1;

            var skillIntervalDataList = new[] { skillIntervalData1, skillIntervalData2 };

            using (_mock.Record())
            {
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
                       Return(_skillDayList);
                Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_groupPerson,
                                                                         new DateOnlyPeriod(new DateOnly(_date),
                                                                                            new DateOnly(_date)))).IgnoreArguments()
                      .Return(new List<ISkill> { skill1 });
                Expect.Call(_skillDay1.Skill).Return(skill1).Repeat.AtLeastOnce();
                Expect.Call(_skillDay1.CurrentDate).Return(new DateOnly()).Repeat.AtLeastOnce();
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollection).Repeat.AtLeastOnce();
                Expect.Call(_skillDay2.Id).Return(null);
                Expect.Call(_skillDay2.Skill).Return(skill2).Repeat.AtLeastOnce();
                Expect.Call(_skillDay2.CurrentDate).Return(new DateOnly()).Repeat.AtLeastOnce();
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollection).Repeat.AtLeastOnce();
                Expect.Call(_resolutionProvider.MinimumResolution(new List<ISkill>())).IgnoreArguments().Return(15);
                Expect.Call(_intervalMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>())).IgnoreArguments().Return(
                    skillIntervalDataList).Repeat.AtLeastOnce();
                Expect.Call(_intervalDivider.SplitSkillIntervalData(new List<ISkillIntervalData>(), 15)).IgnoreArguments().
                       Return(skillIntervalDataList).Repeat.AtLeastOnce();

                Expect.Call(_factorApplier.ApplyFactors(new SkillIntervalData(new DateTimePeriod(), 0, 0, 0, null, null),
                                                        skill1)).IgnoreArguments().Return(skillIntervalData1).Repeat.Times(2);
                Expect.Call(_factorApplier.ApplyFactors(new SkillIntervalData(new DateTimePeriod(), 0, 0, 0, null, null),
                                                        skill2)).IgnoreArguments().Return(skillIntervalData1).Repeat.Times(2);

                Expect.Call(_dayIntervalDataCalculator.Calculate(15, new Dictionary<DateOnly, IList<ISkillIntervalData>>())).
                       IgnoreArguments().Return(intervalData1);
                Expect.Call(_intervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>>(0)))
                      .IgnoreArguments()
                      .Return(skillIntervalDataList);
                Expect.Call(_openHourrestrcitionForTeamBlock.GetOpenHoursPerActivity(_teamBlockInfo2)).Return(openHour);
                Expect.Call(_skillStaffPeriod.Period).Return(new DateTimePeriod(_date, _date.AddMinutes(15))).Repeat.AtLeastOnce()  ;
            }
            using (_mock.Playback())
            {
                var calculatedResult = _target.GeneratePerDay(_teamBlockInfo2);
                Assert.That(calculatedResult.Count, Is.EqualTo(1));
                Assert.That(calculatedResult[_activity1][_date.TimeOfDay].ForecastedDemand, Is.EqualTo(activityIntervalData[_activity1][_date.TimeOfDay].ForecastedDemand));
                Assert.That(calculatedResult[_activity1][_date.TimeOfDay].CurrentDemand , Is.EqualTo(activityIntervalData[_activity1][_date.TimeOfDay].CurrentDemand ));
                Assert.That(calculatedResult[_activity1][_date.TimeOfDay].CurrentHeads  , Is.EqualTo(activityIntervalData[_activity1][_date.TimeOfDay].CurrentHeads  ));
            }
        }

        private static  Dictionary<TimeSpan, ISkillIntervalData> getIntervalData(TimeSpan timeSpan,
                                                                         SkillIntervalData skillIntervalData)
        {
            return new Dictionary<TimeSpan, ISkillIntervalData>
                             {
                                 {
                                     timeSpan,
                                     skillIntervalData
                                     }
                             };
        }

        private SkillIntervalData createSkillIntervalData(double forcastedDemand)
        {
            return new SkillIntervalData(new DateTimePeriod(_date, _date.AddMinutes(15)), forcastedDemand, 0, 0, 0, 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldCreateIntervalsFromSkillDay()
        {
            var openHour = new Dictionary<IActivity, TimePeriod>();
            openHour.Add(_activity1, new TimePeriod(_date.Hour, _date.Minute, _date.Hour, _date.AddMinutes(15).Minute));
            openHour.Add(_activity2, new TimePeriod(_date.Hour, _date.Minute, _date.Hour, _date.AddMinutes(15).Minute));
            var skillIntervalData1 = createSkillIntervalData(6.0);
            var skillIntervalData2 = createSkillIntervalData(12.0);
            var intervalData1 = getIntervalData(_date.TimeOfDay, skillIntervalData1);
			var intervalData2 =getIntervalData(_date.TimeOfDay,skillIntervalData2);
        	var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>
        	                           	{
        	                           		{_activity1, intervalData1},
											{_activity2, intervalData2}
        	                           	};
			
            var skillIntervalDataList = new[] { skillIntervalData1, skillIntervalData2 };
			
            using (_mock.Record())
            {
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
                       Return(_skillDayList);
                Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_groupPerson,
                                                                         new DateOnlyPeriod(new DateOnly(_date),
                                                                                            new DateOnly(_date)))).IgnoreArguments() 
                      .Return(new List<ISkill> { _skill1, _skill2 });
                Expect.Call(_skillDay1.Skill).Return(_skill1).Repeat.AtLeastOnce();
                Expect.Call(_skillDay1.CurrentDate).Return(new DateOnly());
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollection).Repeat.AtLeastOnce();
                Expect.Call(_skillDay2.Skill).Return(_skill2).Repeat.AtLeastOnce();
                Expect.Call(_skillDay2.CurrentDate).Return(new DateOnly());
                Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollection).Repeat.AtLeastOnce();
                Expect.Call(_resolutionProvider.MinimumResolution(new List<ISkill>())).IgnoreArguments().Return(15);
                Expect.Call(_intervalMapper.MapSkillIntervalData(new List<ISkillStaffPeriod>())).IgnoreArguments().Return(
                    skillIntervalDataList).Repeat.AtLeastOnce();
                Expect.Call(_intervalDivider.SplitSkillIntervalData(new List<ISkillIntervalData>(), 15)).IgnoreArguments().
                       Return(skillIntervalDataList).Repeat.AtLeastOnce();

                Expect.Call(_factorApplier.ApplyFactors(new SkillIntervalData(new DateTimePeriod(), 0, 0, 0, null, null),
                                                        _skill1)).IgnoreArguments().Return(skillIntervalData1).Repeat.Times(2);
                Expect.Call(_factorApplier.ApplyFactors(new SkillIntervalData(new DateTimePeriod(), 0, 0, 0, null, null),
                                                        _skill2)).IgnoreArguments().Return(skillIntervalData2).Repeat.Times(2);

                Expect.Call(_dayIntervalDataCalculator.Calculate(15, new Dictionary<DateOnly, IList<ISkillIntervalData>>())).
                       IgnoreArguments().Return(intervalData1);
                Expect.Call(_dayIntervalDataCalculator.Calculate(15, new Dictionary<DateOnly, IList<ISkillIntervalData>>())).
                       IgnoreArguments().Return(intervalData2);
                Expect.Call(_openHourrestrcitionForTeamBlock.GetOpenHoursPerActivity(_teamBlockInfo2)).Return(openHour);
                Expect.Call(_skillStaffPeriod.Period).Return(new DateTimePeriod(_date, _date.AddMinutes(15))).Repeat.AtLeastOnce();
            }
            using(_mock.Playback())
            {
				var calculatedResult = _target.GeneratePerDay(_teamBlockInfo2);
				Assert.That(calculatedResult.Count, Is.EqualTo(2));
				Assert.That(calculatedResult[_activity1][_date.TimeOfDay].ForecastedDemand, Is.EqualTo(activityIntervalData[_activity1][_date.TimeOfDay].ForecastedDemand));
				Assert.That(calculatedResult[_activity2][_date.TimeOfDay].ForecastedDemand, Is.EqualTo(activityIntervalData[_activity2][_date.TimeOfDay].ForecastedDemand));
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSkipSkillsNotInAggregatedSkills()
		{
            var openHour = new Dictionary<IActivity, TimePeriod>();
            openHour.Add(_activity1, new TimePeriod(_date.Hour, _date.Minute, _date.Hour, _date.AddMinutes(15).Minute));
            openHour.Add(_activity2, new TimePeriod(_date.Hour, _date.Minute, _date.Hour, _date.AddMinutes(15).Minute));
            var skillIntervalData1 = createSkillIntervalData(6.0);
            var skillIntervalData2 = createSkillIntervalData(12.0);
			var intervalData1 = getIntervalData(_date.TimeOfDay, skillIntervalData1);
			
			var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>
        	                           	{
        	                           		{_activity1, intervalData1}
        	                           	};

            var skill2 = SkillFactory.CreateSkill("skill2");

			var skillIntervalDataList = new[] { skillIntervalData1, skillIntervalData2 };

			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>())).IgnoreArguments().
                       Return(_skillDayList);
                Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_groupPerson,
                                                                         new DateOnlyPeriod(new DateOnly(_date),
                                                                                            new DateOnly(_date)))).IgnoreArguments() 
                      .Return(new List<ISkill> { _skill1 });
                Expect.Call(_skillDay1.Skill).Return(_skill1).Repeat.AtLeastOnce();
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
                                                        _skill1)).IgnoreArguments().Return(skillIntervalData1).Repeat.Times(2);

                Expect.Call(_dayIntervalDataCalculator.Calculate(15, new Dictionary<DateOnly, IList<ISkillIntervalData>>())).
                       IgnoreArguments().Return(intervalData1);
                Expect.Call(_openHourrestrcitionForTeamBlock.GetOpenHoursPerActivity(_teamBlockInfo2)).Return(openHour);
                Expect.Call(_skillStaffPeriod.Period).Return(new DateTimePeriod(_date, _date.AddMinutes(15))).Repeat.AtLeastOnce();
			}
			using (_mock.Playback())
			{
                var calculatedResult = _target.GeneratePerDay(_teamBlockInfo2);
				Assert.That(calculatedResult.Count, Is.EqualTo(1));
				Assert.That(calculatedResult[_activity1][_date.TimeOfDay].ForecastedDemand, Is.EqualTo(activityIntervalData[_activity1][_date.TimeOfDay].ForecastedDemand));
				Assert.That(calculatedResult[_activity1][_date.TimeOfDay].CurrentDemand , Is.EqualTo(activityIntervalData[_activity1][_date.TimeOfDay].CurrentDemand ));
				Assert.That(calculatedResult[_activity1][_date.TimeOfDay].CurrentHeads , Is.EqualTo(activityIntervalData[_activity1][_date.TimeOfDay].CurrentHeads ));
			}
		}


		
    }
}
