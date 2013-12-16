using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
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
		private ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private ITeamScheduling _teamScheduling;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
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

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamBlockSchedulingOptions = _mocks.StrictMock<ITeamBlockSchedulingOptions>();
			_teamBlockSchedulingCompletionChecker = _mocks.StrictMock<ITeamBlockSchedulingCompletionChecker>();
			_proposedRestrictionAggregator = _mocks.StrictMock<IProposedRestrictionAggregator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_skillDayPeriodIntervalDataGenerator = _mocks.StrictMock<ISkillDayPeriodIntervalDataGenerator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();
			_teamScheduling = _mocks.StrictMock<ITeamScheduling>();
			_teamBlockSchedulingOptions = _mocks.StrictMock<ITeamBlockSchedulingOptions>();
			_target = new TeamBlockSingleDayScheduler(_teamBlockSchedulingCompletionChecker, _proposedRestrictionAggregator,
			                                          _workShiftFilterService, _skillDayPeriodIntervalDataGenerator,
			                                          _workShiftSelector, _teamScheduling, _teamBlockSchedulingOptions);

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
		}

		[Test]
		public void ShouldBeFalseIfNoRoleModel()
		{
			var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _selectedPersons, _dateOnly, null,
									  _blockPeriod);
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
				var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _selectedPersons, _dateOnly, _shift,
				                          _blockPeriod);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldScheduleForSameShift()
		{
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
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(true).Repeat.Twice();
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(()=>_teamScheduling.ExecutePerDayPerPerson(_person1, _dateOnly, _teamBlockInfo, _shift, _blockPeriod));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(()=>_teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _blockPeriod));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _selectedPersons, _dateOnly, _shift,
				                                       _blockPeriod);
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
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
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
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false).Repeat.Twice();
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false).Repeat.Twice();
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person1, _shift))
				      .Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _person1, _teamBlockInfo, restriction,
				                                           _schedulingOptions, finderResult)).Return(shifts);
				Expect.Call(_proposedRestrictionAggregator.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person2, _shift))
				      .Return(restriction);
				Expect.Call(_workShiftFilterService.Filter(_dateOnly, _person2, _teamBlockInfo, restriction,
				                                           _schedulingOptions, finderResult)).Return(shifts);
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo)).Return(activityData).Repeat.AtLeastOnce();
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons)).Return(shifts[0]).Repeat.AtLeastOnce();
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person1, _dateOnly, _teamBlockInfo, _shift, _blockPeriod));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.DayScheduled += _target.OnDayScheduled);
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person2, _dateOnly, _teamBlockInfo, _shift, _blockPeriod));
				Expect.Call(() => _teamScheduling.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
																											  _dateOnly,
																											  _selectedPersons))
					  .Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _selectedPersons, _dateOnly, _shift,
													   _blockPeriod);
				Assert.That(result, Is.True);
			}
		}
	}
}
