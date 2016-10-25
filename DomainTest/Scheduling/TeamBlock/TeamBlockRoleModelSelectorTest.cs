using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockRoleModelSelectorTest
	{
		private ITeamBlockRoleModelSelector _target;
		private MockRepository _mocks;
		private ITeamBlockRestrictionAggregator _restrictionAggregator;
		private IWorkShiftFilterService _workShiftFilterService;
		private DateOnly _dateOnly;
		private ITeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private IWorkShiftSelector _workShiftSelector;
		private ISameOpenHoursInTeamBlock _sameOpenHoursInTeamBlock;
		private IPerson _person;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private List<IPerson> _groupMembers;
		private ITeamInfo _teaminfo;
		private IActivityIntervalDataCreator _activityIntervalDataCreator;
		private PeriodValueCalculationParameters _periodValueCalculationParameters;
		private IMaxSeatInformationGeneratorBasedOnIntervals _maxSeatInformationGeneratorBasedOnIntervals;
		private IMaxSeatSkillAggregator _maxSeatSkillAggregator;
		private IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;
		private IEnumerable<ISkillDay> _skillDays;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_skillDays = Enumerable.Empty<ISkillDay>();
			_restrictionAggregator = _mocks.StrictMock<ITeamBlockRestrictionAggregator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_schedulingResultStateHolder = _mocks.DynamicMock<ISchedulingResultStateHolder>();
			_schedulingResultStateHolder.Expect(x => x.AllSkillDays()).Return(_skillDays).Repeat.Any();
			_sameOpenHoursInTeamBlock = _mocks.StrictMock<ISameOpenHoursInTeamBlock>();
			_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();
			_dateOnly = new DateOnly(2013, 4, 8);
			_person = PersonFactory.CreatePerson("bill");
			_groupMembers = new List<IPerson> { _person };
			_teaminfo = _mocks.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_schedulingOptions = new SchedulingOptions();
			_activityIntervalDataCreator = _mocks.StrictMock<IActivityIntervalDataCreator>();
			_maxSeatInformationGeneratorBasedOnIntervals = _mocks.StrictMock<IMaxSeatInformationGeneratorBasedOnIntervals>();
			_maxSeatSkillAggregator = _mocks.StrictMock<IMaxSeatSkillAggregator>();
			_firstShiftInTeamBlockFinder = _mocks.StrictMock<IFirstShiftInTeamBlockFinder>();
			_target = new TeamBlockRoleModelSelector(_restrictionAggregator, _workShiftFilterService, _sameOpenHoursInTeamBlock,
													 _schedulingResultStateHolder,
													 _activityIntervalDataCreator, 
													 _maxSeatInformationGeneratorBasedOnIntervals, 
													 _maxSeatSkillAggregator,
													 _firstShiftInTeamBlockFinder);
			_periodValueCalculationParameters = new PeriodValueCalculationParameters(
				_schedulingOptions.WorkShiftLengthHintOption,
				_schedulingOptions.UseMinimumPersons,
				_schedulingOptions.UseMaximumPersons, MaxSeatsFeatureOptions.DoNotConsiderMaxSeats, false,
				new Dictionary<DateTime, IntervalLevelMaxSeatInfo>());
		}

		[Test]
		public void ShouldAggregateRestrictions()
		{
			using (_mocks.Record())
			{
				Expect.Call(_restrictionAggregator.Aggregate(_dateOnly, _person, _teamBlockInfo, _schedulingOptions)).Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_workShiftSelector, _teamBlockInfo, _dateOnly, _person, _schedulingOptions, new EffectiveRestriction());

				Assert.That(result, Is.Null);
			}
		}

		[Test]
		public void ShouldFilterCandicateShiftsForRoleModel()
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlockInfo, _person, _dateOnly, _schedulingResultStateHolder))
					.Return(null);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teaminfo);
				Expect.Call(_teaminfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_restrictionAggregator.Aggregate(_dateOnly, _person, _teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_sameOpenHoursInTeamBlock.Check(_skillDays, _teamBlockInfo)).Return(true);
				Expect.Call(_workShiftFilterService.FilterForRoleModel(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
																		new WorkShiftFinderResult(_person, _dateOnly), true, false))
					  .Return(new List<IShiftProjectionCache>());
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_workShiftSelector, _teamBlockInfo, _dateOnly, _person, _schedulingOptions, new EffectiveRestriction());

				Assert.That(result, Is.Null);
			}
		}

		[Ignore("Reason mandatory for NUnit 3")]
		[Test]
		public void ShouldSelectBestShiftAsRoleModel()
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(),
														new WorkTimeLimitation(), null, null, null,
														new List<IActivityRestriction>());
			var shiftProjectionCache = _mocks.StrictMock<IShiftProjectionCache>();
			var shifts = new List<IShiftProjectionCache> { shiftProjectionCache };
			var activityData = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();

			using (_mocks.Record())
			{
				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlockInfo, _person, _dateOnly, _schedulingResultStateHolder))
					.Return(null);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teaminfo).Repeat.Twice();
				Expect.Call(_teaminfo.GroupMembers).Return(_groupMembers).Repeat.Twice();
				Expect.Call(_restrictionAggregator.Aggregate(_dateOnly, _person, _teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_sameOpenHoursInTeamBlock.Check(_skillDays, _teamBlockInfo)).Return(true);
				Expect.Call(_workShiftFilterService.FilterForRoleModel(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
																		new WorkShiftFinderResult(_person, _dateOnly), true, false))
					  .Return(shifts);
				Expect.Call(_activityIntervalDataCreator.CreateFor(_teamBlockInfo, _dateOnly, _skillDays, true))
					  .Return(activityData);
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData, _periodValueCalculationParameters
																		  , TimeZoneGuard.Instance.TimeZone, _schedulingOptions)).IgnoreArguments()
					  .Return(shiftProjectionCache);
				Expect.Call(_maxSeatSkillAggregator.GetAggregatedSkills(_groupMembers,
					new DateOnlyPeriod(_dateOnly, _dateOnly))).Return(new HashSet<ISkill>());
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_workShiftSelector, _teamBlockInfo, _dateOnly, _person, _schedulingOptions, new EffectiveRestriction());

				Assert.That(result, Is.EqualTo(shiftProjectionCache));
			}
		}

		[Test]
		public void ShouldSelectBestShiftAsRoleModelWithMaxSeatFeature()
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(),
														new WorkTimeLimitation(), null, null, null,
														new List<IActivityRestriction>());
			var shiftProjectionCache = _mocks.StrictMock<IShiftProjectionCache>();
			var shifts = new List<IShiftProjectionCache> { shiftProjectionCache };
			var activityData = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();

			using (_mocks.Record())
			{
				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlockInfo, _person, _dateOnly, _schedulingResultStateHolder))
					.Return(null);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teaminfo).Repeat.Twice();
				Expect.Call(_teaminfo.GroupMembers).Return(_groupMembers).Repeat.Twice();
				Expect.Call(_restrictionAggregator.Aggregate(_dateOnly, _person, _teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_sameOpenHoursInTeamBlock.Check(_skillDays, _teamBlockInfo)).Return(true);
				Expect.Call(_workShiftFilterService.FilterForRoleModel(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
																		new WorkShiftFinderResult(_person, _dateOnly), true, false))
					  .Return(shifts);
				Expect.Call(_activityIntervalDataCreator.CreateFor(_teamBlockInfo, _dateOnly, _skillDays, true))
					  .Return(activityData);
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData, _periodValueCalculationParameters
																		  , TimeZoneGuard.Instance.TimeZone, _schedulingOptions)).IgnoreArguments()
					  .Return(shiftProjectionCache);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, _dateOnly,
					_skillDays, TimeZoneGuard.Instance.TimeZone,true)).Return(new Dictionary<DateTime, IntervalLevelMaxSeatInfo>());
				Expect.Call(_maxSeatSkillAggregator.GetAggregatedSkills(_groupMembers,
					new DateOnlyPeriod(_dateOnly, _dateOnly))).Return(new HashSet<ISkill> { SkillFactory.CreateSkill("Skill") });
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_workShiftSelector, _teamBlockInfo, _dateOnly, _person, _schedulingOptions, new EffectiveRestriction());

				Assert.That(result, Is.EqualTo(shiftProjectionCache));
			}
		}
	}
}
