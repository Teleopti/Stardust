using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockSingleDaySchedulerTest
	{
		private ITeamBlockSingleDayScheduler _target;
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
		private PeriodValueCalculationParameters _periodValueCalculationParameters;
		private IMaxSeatInformationGeneratorBasedOnIntervals _maxSeatInformationGeneratorBasedOnIntervals;
		private IMaxSeatSkillAggregator _maxSeatSkillAggregator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamBlockSchedulingCompletionChecker = _mocks.StrictMock<ITeamBlockSchedulingCompletionChecker>();
			_proposedRestrictionAggregator = _mocks.StrictMock<IProposedRestrictionAggregator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
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
				_workShiftSelector, 
				_teamScheduling, 
				_activityIntervalDataCreator,_maxSeatInformationGeneratorBasedOnIntervals ,_maxSeatSkillAggregator );

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
			_selectedPersons = new List<IPerson> {_person1, _person2};
			_shift = _mocks.StrictMock<IShiftProjectionCache>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_periodValueCalculationParameters = new PeriodValueCalculationParameters(_schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons, MaxSeatsFeatureOptions.DoNotConsiderMaxSeats,false , new Dictionary<DateTime, bool>(),true);
		}

		[Test]
		public void ShouldBeFalseIfNoRoleModel()
		{
			var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, null,
									  _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new EffectiveRestriction(),true);
			Assert.That(result, Is.False);
		}

		[Test]
		public void ShouldBeTrueIfAllSelectedPersonAreScheduled()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
				                                                                                              _dateOnly,
																											  _selectedPersons))
				      .Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
										  _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new EffectiveRestriction(),true);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldScheduleForSameShiftIfSingleAgentTeamAndBlockWithSameShift()
		{
			_schedulingOptions = new SchedulingOptions();
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff;
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.BlockSameShift = true;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person1 }))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person2 }))
					  .Return(false);
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(()=>_teamScheduling.ExecutePerDayPerPerson(_person1, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(()=>_teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
													   _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new EffectiveRestriction(),true );
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldScheduleForOptionsOtherThanSameShift()
		{
			var restriction =  new EffectiveRestriction(new StartTimeLimitation(),
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
			var finderResult = new WorkShiftFinderResult(_person1, _dateOnly);
			var shifts = new List<IShiftProjectionCache> {_shift};
			var activityData = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();
			var skillIntervalDataDic = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			skillIntervalDataDic.Add(_dateOnly, new Dictionary<IActivity, IList<ISkillIntervalData>>());
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson>{_person1}))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson>{_person2}))
					  .Return(false);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person1, _shift))
				      .Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_dateOnly, _person1, _teamBlockInfo, restriction,
				                                           _schedulingOptions, finderResult)).Return(shifts);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person2, _shift))
				      .Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_dateOnly, _person2, _teamBlockInfo, restriction,
				                                           _schedulingOptions, finderResult)).Return(shifts);
				Expect.Call(_activityIntervalDataCreator.CreateFor(_teamBlockInfo, _dateOnly, _schedulingResultStateHolder, false))
					.Return(activityData).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _periodValueCalculationParameters, TimeZoneGuard.Instance.TimeZone)).IgnoreArguments().Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person1, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(true);
				Expect.Call(_maxSeatSkillAggregator.GetAggregatedSkills(_teamBlockInfo.TeamInfo.GroupMembers.ToList(),
					new DateOnlyPeriod(_dateOnly, _dateOnly))).Return(new HashSet<ISkill>( )).Repeat.Twice() ;
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
													   _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new EffectiveRestriction(),false);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldSkipIfNotShiftCanBeAssignedToTeamMember()
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
			var finderResult = new WorkShiftFinderResult(_person1, _dateOnly);
			var shifts = new List<IShiftProjectionCache> { _shift };
			var activityData = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();
			var skillIntervalDataDic = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			skillIntervalDataDic.Add(_dateOnly, new Dictionary<IActivity, IList<ISkillIntervalData>>());
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person1 }))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person2 }))
					  .Return(false);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person1, _shift))
					  .Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_dateOnly, _person1, _teamBlockInfo, restriction,
														   _schedulingOptions, finderResult)).Return(null);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person2, _shift))
					  .Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_dateOnly, _person2, _teamBlockInfo, restriction,
														   _schedulingOptions, finderResult)).Return(shifts);
				Expect.Call(_activityIntervalDataCreator.CreateFor(_teamBlockInfo, _dateOnly, _schedulingResultStateHolder,false))
					.Return(activityData).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _periodValueCalculationParameters, TimeZoneGuard.Instance.TimeZone)).IgnoreArguments().Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(true);
				Expect.Call(_maxSeatSkillAggregator.GetAggregatedSkills(_teamBlockInfo.TeamInfo.GroupMembers.ToList(),
					new DateOnlyPeriod(_dateOnly, _dateOnly))).Return(new HashSet<ISkill>{SkillFactory.CreateSkill("")});
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
													   _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new EffectiveRestriction(),false);
				Assert.That(result, Is.True);
			}
		}


		[Test]
		public void ShouldScheduleWithMaxSeatToggleOn()
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
																		 new EndTimeLimitation(),
																		 new WorkTimeLimitation(), null, null, null,
																		 new List<IActivityRestriction>());
			var finderResult = new WorkShiftFinderResult(_person1, _dateOnly);
			var shifts = new List<IShiftProjectionCache> { _shift };
			var activityData = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();
			var skillIntervalDataDic = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			skillIntervalDataDic.Add(_dateOnly, new Dictionary<IActivity, IList<ISkillIntervalData>>());
			_schedulingOptions.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person1 }))
					  .Return(false);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  new List<IPerson> { _person2 }))
					  .Return(false);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person1, _shift))
						.Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_dateOnly, _person1, _teamBlockInfo, restriction,
																		 _schedulingOptions, finderResult)).Return(shifts);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person2, _shift))
						.Return(restriction);
				Expect.Call(_workShiftFilterService.FilterForTeamMember(_dateOnly, _person2, _teamBlockInfo, restriction,
																		 _schedulingOptions, finderResult)).Return(shifts);
				Expect.Call(_activityIntervalDataCreator.CreateFor(_teamBlockInfo, _dateOnly, _schedulingResultStateHolder, false))
					.Return(activityData).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _periodValueCalculationParameters, TimeZoneGuard.Instance.TimeZone)).IgnoreArguments().Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person1, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(true);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, _dateOnly,
					_schedulingResultStateHolder, TimeZoneGuard.Instance.TimeZone)).Return(new Dictionary<DateTime, bool>()).Repeat.Twice() ;
				Expect.Call(_maxSeatSkillAggregator.GetAggregatedSkills(_teamBlockInfo.TeamInfo.GroupMembers.ToList(),
					new DateOnlyPeriod(_dateOnly, _dateOnly))).Return(new HashSet<ISkill>()).Repeat.Twice();
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, _shift,
														_rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new EffectiveRestriction(),true);
				Assert.That(result, Is.True);
			}
		}

		
	}
}
