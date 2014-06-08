using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
    [TestFixture]
    public class DailyTargetValueCalculatorForTeamBlockTest
    {
        private MockRepository _mock;
        private IDailyTargetValueCalculatorForTeamBlock _target;
        private ISkillResolutionProvider _resolutionProvider;
        private ISkillIntervalDataDivider _intervalDataDivider;
        private ISkillIntervalDataAggregator _intervalDataAggregator;
        private IDayIntervalDataCalculator _dayIntervalDataCalculator;
        private ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IGroupPersonSkillAggregator _groupPersonSkillAggregator;
        private IAdvancedPreferences _advancePrefrences;
        private BaseLineData _baseLineData;
        private ITeamInfo _teamInfo;
        private IBlockInfo _blockInfo;
        private ITeamBlockInfo _teamBlockInfo;
        private DateOnlyPeriod _dateOnlyPeriod;
        private IEnumerable<ISkill> _skillList;
        private ISkillDay _skillDay1;
        private ReadOnlyCollection<ISkillStaffPeriod> _skillStaffPeriodCollecion;
        private IList<ISkillStaffPeriod> _skillStaffPeriodList;
        private ISkillStaffPeriod _skillStaffPeriod1;
        private DateTimePeriod _period;
        private ILocateMissingIntervalsIfMidNightBreak _locateMissingIntervalsIfMidNightBreak;
        private IFilterOutIntervalsAfterMidNight _filterOutIntervalsAfterMidNight;
	    private List<DateOnly> _extendedDateOnlyList;

	    [SetUp ]
        public void Setup()
        {
            _mock = new MockRepository();
            _locateMissingIntervalsIfMidNightBreak = _mock.StrictMock<ILocateMissingIntervalsIfMidNightBreak>();
            _filterOutIntervalsAfterMidNight = _mock.StrictMock<IFilterOutIntervalsAfterMidNight>();
            _resolutionProvider = _mock.StrictMock<ISkillResolutionProvider>();
            _intervalDataDivider = _mock.StrictMock<ISkillIntervalDataDivider>();
            _intervalDataAggregator = _mock.StrictMock<ISkillIntervalDataAggregator>();
            _dayIntervalDataCalculator = _mock.StrictMock<IDayIntervalDataCalculator>();
            _skillStaffPeriodToSkillIntervalDataMapper = _mock.StrictMock<ISkillStaffPeriodToSkillIntervalDataMapper>();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _groupPersonSkillAggregator = _mock.StrictMock<IGroupPersonSkillAggregator>();
            _advancePrefrences = new AdvancedPreferences {TargetValueCalculation = TargetValueOptions.StandardDeviation};
            _baseLineData = new BaseLineData();
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>>());
            _dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
            _blockInfo = new BlockInfo(_dateOnlyPeriod);
            _teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            _skillList = new List<ISkill>{_baseLineData.SampleSkill };
            _skillDay1 = _mock.StrictMock<ISkillDay  >() ;

            _skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriodList = new List<ISkillStaffPeriod>();
            _skillStaffPeriodList.Add(_skillStaffPeriod1);
            _skillStaffPeriodCollecion = new ReadOnlyCollection<ISkillStaffPeriod>(_skillStaffPeriodList);
            _period = new DateTimePeriod(DateTime.UtcNow ,DateTime.UtcNow );

	        _target = new DailyTargetValueCalculatorForTeamBlock(_resolutionProvider, _intervalDataDivider,
		        _intervalDataAggregator,
		        _dayIntervalDataCalculator,
		        _skillStaffPeriodToSkillIntervalDataMapper,
		        _schedulingResultStateHolder,
		        _groupPersonSkillAggregator, _locateMissingIntervalsIfMidNightBreak, _filterOutIntervalsAfterMidNight);

	        _extendedDateOnlyList = new List<DateOnly> {DateOnly.Today, DateOnly.Today.AddDays(1)};
        }

        [Test]
        public void TestTargetValueWithOneScheduleDayOneSkill()
        {
            var skillIntervalData1 = new SkillIntervalData(_period, 3.63, 2, 2, 0, 0);
            IDictionary<DateOnly, IList<ISkillIntervalData>> dateToSkillIntervalDic = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            IDictionary<DateTime, ISkillIntervalData> timeToSkillIntervalDic = new Dictionary<DateTime, ISkillIntervalData>();
            timeToSkillIntervalDic.Add(skillIntervalData1.Period.StartDateTime,skillIntervalData1 );
            dateToSkillIntervalDic.Add(DateOnly.Today,new List<ISkillIntervalData>{skillIntervalData1 });
            using (_mock.Record())
            {
                Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_baseLineData.GroupPerson, _dateOnlyPeriod)).IgnoreArguments().Return(_skillList);
                Expect.Call(_resolutionProvider.MinimumResolution(_skillList.ToList() )).Return(15);
	            Expect.Call(
		            _schedulingResultStateHolder.SkillDaysOnDateOnly(_teamBlockInfo.BlockInfo.BlockPeriod.DayCollection()))
		            .Return(new List<ISkillDay> {_skillDay1});
                Expect.Call(_skillDay1.CurrentDate).Return(DateOnly.Today);
                Expect.Call(_skillDay1.Skill ).Return(_baseLineData.SampleSkill );
                Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollecion).Repeat.Twice() ;
				Expect.Call(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(_skillStaffPeriodList, DateOnly.Today, TimeZoneGuard.Instance.TimeZone))
                      .Return(new List<ISkillIntervalData> {skillIntervalData1});
	            Expect.Call(
		            _intervalDataDivider.SplitSkillIntervalData(new List<ISkillIntervalData> {skillIntervalData1}, 15))
		            .Return(new List<ISkillIntervalData> {skillIntervalData1});

				Expect.Call(
					_schedulingResultStateHolder.SkillDaysOnDateOnly(_extendedDateOnlyList))
					.Return(new List<ISkillDay> { _skillDay1 });
				Expect.Call(_skillDay1.CurrentDate).Return(DateOnly.Today.AddDays(1));
				Expect.Call(_skillDay1.Skill).Return(_baseLineData.SampleSkill);
				Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollecion).Repeat.Twice();
				Expect.Call(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(_skillStaffPeriodList, DateOnly.Today.AddDays(1), TimeZoneGuard.Instance.TimeZone))
					  .Return(new List<ISkillIntervalData> { skillIntervalData1 });
				Expect.Call(
					_intervalDataDivider.SplitSkillIntervalData(new List<ISkillIntervalData> { skillIntervalData1 }, 15))
					.Return(new List<ISkillIntervalData> { skillIntervalData1 });


				Expect.Call(_dayIntervalDataCalculator.Calculate(dateToSkillIntervalDic, DateOnly.Today)).IgnoreArguments().Return(timeToSkillIntervalDic);

            }
            Assert.AreEqual(_target.TargetValue(_teamBlockInfo,_advancePrefrences), 0.0);
        }

        [Test]
        public void TestTargetValueWithOneScheduleDayOneSkillMultipleIntervals()
        {
            var skillIntervalData1 = new SkillIntervalData(_period, 5, 3, 3, 0, 0);
            var skillIntervalData2 = new SkillIntervalData(_period.MovePeriod(TimeSpan.FromMinutes(30)), 6, 0, 0, 0, 0);
            var skillIntervalList = new List<ISkillIntervalData> {skillIntervalData1, skillIntervalData2};

            IDictionary<DateOnly, IList<ISkillIntervalData>> dateToSkillIntervalDic = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            IDictionary<DateTime, ISkillIntervalData> timeToSkillIntervalDic = new Dictionary<DateTime, ISkillIntervalData>();

			timeToSkillIntervalDic.Add(skillIntervalData1.Period.StartDateTime, skillIntervalData1);
			timeToSkillIntervalDic.Add(skillIntervalData2.Period.StartDateTime, skillIntervalData2);
            dateToSkillIntervalDic.Add(DateOnly.Today, skillIntervalList);
            
            using (_mock.Record())
            {
	            Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_baseLineData.GroupPerson, _dateOnlyPeriod))
		            .IgnoreArguments()
		            .Return(_skillList);
                Expect.Call(_resolutionProvider.MinimumResolution(_skillList.ToList())).Return(15);
	            Expect.Call(
		            _schedulingResultStateHolder.SkillDaysOnDateOnly(_teamBlockInfo.BlockInfo.BlockPeriod.DayCollection()))
		            .Return(new List<ISkillDay> {_skillDay1});

                skillDayExpectCalls(_skillDay1, skillIntervalList);

				Expect.Call(
					_schedulingResultStateHolder.SkillDaysOnDateOnly(_extendedDateOnlyList))
					.Return(new List<ISkillDay> { _skillDay1 });

				skillDayExpectCalls(_skillDay1, skillIntervalList);

                Expect.Call(
                    _intervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>>
                        {
                            skillIntervalList
                        })).IgnoreArguments().Return(skillIntervalList);

				Expect.Call(_dayIntervalDataCalculator.Calculate(dateToSkillIntervalDic, DateOnly.Today)).IgnoreArguments().Return(timeToSkillIntervalDic);

            }
            Assert.AreEqual(_target.TargetValue(_teamBlockInfo, _advancePrefrences), 0.3);
        }

        [Test]
        public void TestSkillWithMidnightBreak()
        {
            var skillIntervalData1 = new SkillIntervalData(_period, 5, 3, 3, 0, 0);
            var skillIntervalData2 = new SkillIntervalData(_period.MovePeriod(TimeSpan.FromMinutes(30)), 6, 0, 0, 0, 0);
            var skillIntervalList = new List<ISkillIntervalData> { skillIntervalData1, skillIntervalData2 };

            IDictionary<DateOnly, IList<ISkillIntervalData>> dateToSkillIntervalDic = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            IDictionary<DateTime, ISkillIntervalData> timeToSkillIntervalDic = new Dictionary<DateTime, ISkillIntervalData>();

			timeToSkillIntervalDic.Add(skillIntervalData1.Period.StartDateTime, skillIntervalData1);
			timeToSkillIntervalDic.Add(skillIntervalData2.Period.StartDateTime, skillIntervalData2);
            dateToSkillIntervalDic.Add(DateOnly.Today, skillIntervalList);
            _baseLineData.SampleSkill.MidnightBreakOffset = TimeSpan.FromHours(1);
            using (_mock.Record())
            {
	            Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_baseLineData.GroupPerson, _dateOnlyPeriod))
		            .IgnoreArguments()
		            .Return(new List<ISkill> {_baseLineData.SampleSkill});
                Expect.Call(_resolutionProvider.MinimumResolution(_skillList.ToList())).Return(15);
	            Expect.Call(
		            _schedulingResultStateHolder.SkillDaysOnDateOnly(_teamBlockInfo.BlockInfo.BlockPeriod.DayCollection()))
		            .Return(new List<ISkillDay> {_skillDay1});
	            Expect.Call(_locateMissingIntervalsIfMidNightBreak.GetMissingSkillStaffPeriods(DateOnly.Today,
		            _baseLineData.SampleSkill, TimeZoneGuard.Instance.TimeZone))
		            .IgnoreArguments()
		            .Return(new List<ISkillStaffPeriod>());
	            Expect.Call(_filterOutIntervalsAfterMidNight.Filter(_skillStaffPeriodList, DateOnly.Today, TimeZoneInfo.Utc))
		            .IgnoreArguments()
		            .Return(_skillStaffPeriodList);
                skillDayExpectCalls(_skillDay1, skillIntervalList);

				Expect.Call(
					_schedulingResultStateHolder.SkillDaysOnDateOnly(_extendedDateOnlyList))
					.Return(new List<ISkillDay> { _skillDay1 });
				Expect.Call(_locateMissingIntervalsIfMidNightBreak.GetMissingSkillStaffPeriods(DateOnly.Today.AddDays(1),
					_baseLineData.SampleSkill, TimeZoneGuard.Instance.TimeZone))
					.IgnoreArguments()
					.Return(new List<ISkillStaffPeriod>());
				Expect.Call(_filterOutIntervalsAfterMidNight.Filter(_skillStaffPeriodList, DateOnly.Today.AddDays(1), TimeZoneInfo.Utc))
					.IgnoreArguments()
					.Return(_skillStaffPeriodList);
				skillDayExpectCalls(_skillDay1, skillIntervalList);

                Expect.Call(
                    _intervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>>
                        {
                            skillIntervalList
                        })).IgnoreArguments().Return(skillIntervalList);

				Expect.Call(_dayIntervalDataCalculator.Calculate(dateToSkillIntervalDic, DateOnly.Today)).IgnoreArguments().Return(timeToSkillIntervalDic);

            }
            Assert.AreEqual(_target.TargetValue(_teamBlockInfo, _advancePrefrences), 0.3);
        }

	    [Test]
	    public void ShouldHandleMinMaxStaffBoosts()
	    {
			var skillIntervalData1 = new SkillIntervalData(_period, 5, 3, 3, 0, 0);
			var skillIntervalData2 = new SkillIntervalData(_period.MovePeriod(TimeSpan.FromMinutes(30)), 6, 0, 6, 7, null);

			var skillIntervalList = new List<ISkillIntervalData> { skillIntervalData1, skillIntervalData2 };

			IDictionary<DateOnly, IList<ISkillIntervalData>> dateToSkillIntervalDic = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			IDictionary<DateTime, ISkillIntervalData> timeToSkillIntervalDic = new Dictionary<DateTime, ISkillIntervalData>();

			timeToSkillIntervalDic.Add(skillIntervalData1.Period.StartDateTime, skillIntervalData1);
			timeToSkillIntervalDic.Add(skillIntervalData2.Period.StartDateTime, skillIntervalData2);
			dateToSkillIntervalDic.Add(DateOnly.Today, skillIntervalList);
		    var periodWithExtraDay = _dateOnlyPeriod.DayCollection();
			periodWithExtraDay.Add(_dateOnlyPeriod.DayCollection().Max().AddDays(1));
			var skillStaffPeriodCollecion = new ReadOnlyCollection<ISkillStaffPeriod>(_skillStaffPeriodList);

			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupPerson).Return(_baseLineData.GroupPerson);
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(_baseLineData.GroupPerson, _dateOnlyPeriod))
					.Return(_skillList);
				Expect.Call(_resolutionProvider.MinimumResolution(_skillList.ToList())).Return(15);
				
				Expect.Call(
					_schedulingResultStateHolder.SkillDaysOnDateOnly(periodWithExtraDay))
					.Return(new List<ISkillDay> { _skillDay1 });
				Expect.Call(_skillDay1.CurrentDate).Return(DateOnly.Today);
				Expect.Call(_skillDay1.Skill).Return(_baseLineData.SampleSkill);
				Expect.Call(_skillDay1.SkillStaffPeriodCollection).Return(skillStaffPeriodCollecion);
				Expect.Call(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(_skillStaffPeriodList, DateOnly.Today,
					TimeZoneGuard.Instance.TimeZone)).Return(skillIntervalList);
				Expect.Call(_intervalDataDivider.SplitSkillIntervalData(skillIntervalList, 15)).Return(skillIntervalList);

				Expect.Call(_dayIntervalDataCalculator.Calculate(dateToSkillIntervalDic, DateOnly.Today)).IgnoreArguments().Return(timeToSkillIntervalDic);

			}
			using (_mock.Playback())
			{
				Assert.AreEqual(5000.3, _target.TargetValue(_teamBlockInfo, _advancePrefrences), 0.001);
			}
	    }

        private void skillDayExpectCalls(ISkillDay skillDay, List<ISkillIntervalData> skillIntervalList)
        {
            var skillStaffPeriodCollecion = new ReadOnlyCollection<ISkillStaffPeriod>(_skillStaffPeriodList);
            Expect.Call(skillDay.CurrentDate).Return(DateOnly.Today);
            Expect.Call(skillDay.Skill).Return(_baseLineData.SampleSkill);
            Expect.Call(skillDay.SkillStaffPeriodCollection).Return(skillStaffPeriodCollecion).Repeat.Twice();
			Expect.Call(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(_skillStaffPeriodList, DateOnly.Today, TimeZoneGuard.Instance.TimeZone)).Return(skillIntervalList);
            Expect.Call(_intervalDataDivider.SplitSkillIntervalData(skillIntervalList, 15)).Return(skillIntervalList);
        }
    }
}
