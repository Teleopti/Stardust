using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class TeamBlockRoleModelSelectorTest
	{
		private TeamBlockRoleModelSelector _target;
		private MockRepository _mocks;
		private ITeamBlockRestrictionAggregator _restrictionAggregator;
		private IWorkShiftFilterService _workShiftFilterService;
		private DateOnly _dateOnly;
		private ITeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private IWorkShiftSelector _workShiftSelector;
		private ISameOpenHoursInTeamBlock _sameOpenHoursInTeamBlock;
		private IPerson _person;
		private IScheduleDictionary _schedules;
		private List<IPerson> _groupMembers;
		private ITeamInfo _teaminfo;
		private IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;
		private IEnumerable<ISkillDay> _skillDays;
		private readonly GroupPersonSkillAggregator groupPersonSkillAggregator = new GroupPersonSkillAggregator(new PersonalSkillsProvider());
		private IScheduleDay _scheduleDay;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_skillDays = Enumerable.Empty<ISkillDay>();
			_restrictionAggregator = _mocks.StrictMock<ITeamBlockRestrictionAggregator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_schedules= _mocks.DynamicMock<IScheduleDictionary>();
			_sameOpenHoursInTeamBlock = _mocks.StrictMock<ISameOpenHoursInTeamBlock>();
			_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();
			_dateOnly = new DateOnly(2013, 4, 8);
			_person = PersonFactory.CreatePerson("bill");
			_groupMembers = new List<IPerson> { _person };
			_teaminfo = _mocks.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_schedulingOptions = new SchedulingOptions();
			_firstShiftInTeamBlockFinder = _mocks.StrictMock<IFirstShiftInTeamBlockFinder>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_target = new TeamBlockRoleModelSelector(_restrictionAggregator, _workShiftFilterService, _sameOpenHoursInTeamBlock, _firstShiftInTeamBlockFinder, new EffectiveRestrictionStartTimeDeciderOff());
			
		}

		[Test]
		public void ShouldAggregateRestrictions()
		{
			using (_mocks.Record())
			{
				Expect.Call(_restrictionAggregator.Aggregate(null, _dateOnly, _person, _teamBlockInfo, _schedulingOptions)).Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(null, null, _workShiftSelector, _teamBlockInfo, _dateOnly, _person, _schedulingOptions, new EffectiveRestriction(), groupPersonSkillAggregator);

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
				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlockInfo, _person, _dateOnly, _schedules))
					.Return(null);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teaminfo);
				Expect.Call(_teaminfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_restrictionAggregator.Aggregate(_schedules, _dateOnly, _person, _teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_sameOpenHoursInTeamBlock.Check(_skillDays, _teamBlockInfo, groupPersonSkillAggregator)).Return(true);
				Expect.Call(_workShiftFilterService.FilterForRoleModel(groupPersonSkillAggregator, _schedules, _dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
																		new WorkShiftFinderResult(_person, _dateOnly), true, false, _skillDays))
				
					  .Return(new List<ShiftProjectionCache>());
				Expect.Call(_schedules[_person].ScheduledDay(_dateOnly)).Return(_scheduleDay);
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_schedules, _skillDays, _workShiftSelector, _teamBlockInfo, _dateOnly, _person, _schedulingOptions, new EffectiveRestriction(), groupPersonSkillAggregator);

				Assert.That(result, Is.Null);
			}
		}

		[Test]
		public void ShouldSelectBestShiftAsRoleModelWithMaxSeatFeature()
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(),
														new WorkTimeLimitation(), null, null, null,
														new List<IActivityRestriction>());
			var shiftProjectionCache = _mocks.StrictMock<ShiftProjectionCache>();
			var shifts = new List<ShiftProjectionCache> { shiftProjectionCache };
			using (_mocks.Record())
			{
				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlockInfo, _person, _dateOnly, _schedules))
					.Return(null);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teaminfo).Repeat.Any();
				Expect.Call(_teaminfo.GroupMembers).Return(_groupMembers).Repeat.Any();
				Expect.Call(_restrictionAggregator.Aggregate(_schedules, _dateOnly, _person, _teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_sameOpenHoursInTeamBlock.Check(_skillDays, _teamBlockInfo, groupPersonSkillAggregator)).Return(true);
				Expect.Call(_workShiftFilterService.FilterForRoleModel(groupPersonSkillAggregator, _schedules, _dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
																		new WorkShiftFinderResult(_person, _dateOnly), true, false, _skillDays))
					  .Return(shifts);
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(null, _dateOnly, shifts, _skillDays, _teamBlockInfo, _schedulingOptions,TimeZoneGuard.Instance.TimeZone, false, null)).IgnoreArguments()
					  .Return(shiftProjectionCache);
				Expect.Call(_schedules[_person].ScheduledDay(_dateOnly)).Return(_scheduleDay);
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_schedules, _skillDays, _workShiftSelector, _teamBlockInfo, _dateOnly, _person, _schedulingOptions, new EffectiveRestriction(), groupPersonSkillAggregator);

				Assert.That(result, Is.EqualTo(shiftProjectionCache));
			}
		}
	}
}
