﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class TeamBlockSingleDaySchedulerTest
	{
		private TeamBlockSingleDayScheduler _target;
		private ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private IProposedRestrictionAggregator _proposedRestrictionAggregator;
		private IWorkShiftFilterService _workShiftFilterService;
		private IWorkShiftSelector _workShiftSelector;
		private ITeamScheduling _teamScheduling;
		private MockRepository _mocks;
		private DateOnly _dateOnly;
		private IPerson _person1;
		private IPerson _person2;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private Group _group;
		private DateOnlyPeriod _blockPeriod;
		private TeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private List<IPerson> _selectedPersons;
		private IShiftProjectionCache _shift;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IActivityIntervalDataCreator _activityIntervalDataCreator;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private IMaxSeatInformationGeneratorBasedOnIntervals _maxSeatInformationGeneratorBasedOnIntervals;
		private IMaxSeatSkillAggregator _maxSeatSkillAggregator;
		private IEnumerable<ISkillDay> _skillDays;
		private IScheduleDictionary _schedules;

		public void setup()
		{
			_mocks = new MockRepository();
			_teamBlockSchedulingCompletionChecker = _mocks.StrictMock<ITeamBlockSchedulingCompletionChecker>();
			_proposedRestrictionAggregator = _mocks.StrictMock<IProposedRestrictionAggregator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_skillDays = Enumerable.Empty<ISkillDay>();
			_schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			_schedulingResultStateHolder = _mocks.Stub<ISchedulingResultStateHolder>();
			_schedulingResultStateHolder.Expect(x => x.AllSkillDays()).Return(_skillDays).Repeat.Any();
			_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();
			_teamScheduling = _mocks.StrictMock<ITeamScheduling>();
			_activityIntervalDataCreator = _mocks.StrictMock<IActivityIntervalDataCreator>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_maxSeatInformationGeneratorBasedOnIntervals = _mocks.StrictMock<IMaxSeatInformationGeneratorBasedOnIntervals>();
			_maxSeatSkillAggregator = _mocks.StrictMock<IMaxSeatSkillAggregator>();
			_target = new TeamBlockSingleDayScheduler(
				_teamBlockSchedulingCompletionChecker,
				_proposedRestrictionAggregator,
				_workShiftFilterService,
				_teamScheduling,
				_activityIntervalDataCreator, _maxSeatInformationGeneratorBasedOnIntervals, _maxSeatSkillAggregator, null, new GroupPersonSkillAggregator(new PersonalSkillsProvider()));

			_dateOnly = new DateOnly(2013, 11, 12);
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("ball"), _dateOnly);
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_group = new Group(new List<IPerson> { _person1, _person2 }, "Hej");
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, _scheduleMatrixPro2 };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(_group, groupMatrixes);
			_blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_blockPeriod));
			_schedulingOptions = new SchedulingOptions();
			_selectedPersons = new List<IPerson> { _person1, _person2 };
			_shift = _mocks.StrictMock<IShiftProjectionCache>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
		}

		[Test]
		public void ShouldBeFalseIfNoRoleModel()
		{
			setup();
			var result = _target.ScheduleSingleDay(_workShiftSelector, _teamBlockInfo, _schedulingOptions, _dateOnly, null,
									  _rollbackService, _resourceCalculateDelayer, _skillDays, _schedules, new EffectiveRestriction(), NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null);
			Assert.That(result, Is.False);
		}

		[Test]
		public void ShouldBeTrueIfAllSelectedPersonAreScheduled()
		{
			setup();
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																																			 _dateOnly,
																											  _selectedPersons, _schedulingOptions))
						.Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_workShiftSelector, _teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
										  _rollbackService, _resourceCalculateDelayer, _skillDays, _schedules, new EffectiveRestriction(), NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldScheduleForSameShiftIfSingleAgentTeamAndBlockWithSameShift()
		{
			setup();
			_schedulingOptions = new SchedulingOptions();
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff;
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.BlockSameShift = true;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons, _schedulingOptions))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person1 }, _schedulingOptions))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person2 }, _schedulingOptions))
					  .Return(false);
				
				Expect.Call(_teamScheduling.ExecutePerDayPerPerson(_person1, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer, false, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null, null)).IgnoreArguments().Return(false);
				Expect.Call(_teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer, false, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null, null)).IgnoreArguments().Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons, _schedulingOptions))
					  .Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_workShiftSelector, _teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
														_rollbackService, _resourceCalculateDelayer, _skillDays, _schedules, new EffectiveRestriction(), NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldScheduleForOptionsOtherThanSameShift()
		{
			setup();
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
																		 new EndTimeLimitation(),
																		 new WorkTimeLimitation(), null, null, null,
																		 new List<IActivityRestriction>());
			var finderResult = new WorkShiftFinderResult(_person1, _dateOnly);
			var shifts = new List<IShiftProjectionCache> { _shift };
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons, _schedulingOptions))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person1 }, _schedulingOptions))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person2 }, _schedulingOptions))
					  .Return(false);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedules, _schedulingOptions, _teamBlockInfo, _dateOnly, _person1, _shift))
						.Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_schedules, _dateOnly, _person1, _teamBlockInfo, restriction,
																		 _schedulingOptions, finderResult, false)).Return(shifts);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedules, _schedulingOptions, _teamBlockInfo, _dateOnly, _person2, _shift))
						.Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_schedules, _dateOnly, _person2, _teamBlockInfo, restriction,
																		 _schedulingOptions, finderResult, false)).Return(shifts);
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(null, _dateOnly, shifts, _skillDays, _teamBlockInfo, _schedulingOptions, TimeZoneGuard.Instance.TimeZone, false, null)).IgnoreArguments().Return(shifts[0]).Repeat.AtLeastOnce();
				
				Expect.Call(_teamScheduling.ExecutePerDayPerPerson(_person1, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer, false, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null, null)).IgnoreArguments().Return(false);
				Expect.Call(_teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer, false, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null, null)).IgnoreArguments().Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons, _schedulingOptions))
					  .Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_workShiftSelector, _teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
														_rollbackService, _resourceCalculateDelayer, _skillDays, _schedules, new EffectiveRestriction(), NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldSkipIfNotShiftCanBeAssignedToTeamMember()
		{
			setup();
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(),
														new WorkTimeLimitation(), null, null, null,
														new List<IActivityRestriction>());
			var finderResult = new WorkShiftFinderResult(_person1, _dateOnly);
			var shifts = new List<IShiftProjectionCache> { _shift };
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons, _schedulingOptions))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person1 }, _schedulingOptions))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person2 }, _schedulingOptions))
					  .Return(false);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedules, _schedulingOptions, _teamBlockInfo, _dateOnly, _person1, _shift))
					  .Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_schedules, _dateOnly, _person1, _teamBlockInfo, restriction,
															_schedulingOptions, finderResult, false)).Return(null);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedules, _schedulingOptions, _teamBlockInfo, _dateOnly, _person2, _shift))
					  .Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_schedules, _dateOnly, _person2, _teamBlockInfo, restriction,
															_schedulingOptions, finderResult, false)).Return(shifts);
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(null, _dateOnly, shifts, _skillDays, _teamBlockInfo, _schedulingOptions, TimeZoneGuard.Instance.TimeZone, false, null)).IgnoreArguments().Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(_teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer, false, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null, null)).IgnoreArguments().Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons, _schedulingOptions))
					  .Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_workShiftSelector, _teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
														_rollbackService, _resourceCalculateDelayer, _skillDays, _schedules, new EffectiveRestriction(), NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldScheduleWithConsideringMaxSeat()
		{
			setup();
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
																		 new EndTimeLimitation(),
																		 new WorkTimeLimitation(), null, null, null,
																		 new List<IActivityRestriction>());
			var finderResult = new WorkShiftFinderResult(_person1, _dateOnly);
			var shifts = new List<IShiftProjectionCache> { _shift };
			_schedulingOptions.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons, _schedulingOptions))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person1 }, _schedulingOptions))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person2 }, _schedulingOptions))
					  .Return(false);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedules, _schedulingOptions, _teamBlockInfo, _dateOnly, _person1, _shift))
						.Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_schedules, _dateOnly, _person1, _teamBlockInfo, restriction,
																		 _schedulingOptions, finderResult, false)).Return(shifts);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedules, _schedulingOptions, _teamBlockInfo, _dateOnly, _person2, _shift))
						.Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_schedules, _dateOnly, _person2, _teamBlockInfo, restriction,
																		 _schedulingOptions, finderResult, false)).Return(shifts);
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(null, _dateOnly, shifts, _skillDays, _teamBlockInfo, _schedulingOptions, TimeZoneGuard.Instance.TimeZone, false, null)).IgnoreArguments().Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(_teamScheduling.ExecutePerDayPerPerson(_person1, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer, false, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null, null)).IgnoreArguments().Return(false);
				Expect.Call(_teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer, false, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null, null)).IgnoreArguments().Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons, _schedulingOptions))
					  .Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_workShiftSelector, _teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
														_rollbackService, _resourceCalculateDelayer, _skillDays, _schedules, new EffectiveRestriction(), NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null);
				Assert.That(result, Is.True);
			}
		}
	}
}
