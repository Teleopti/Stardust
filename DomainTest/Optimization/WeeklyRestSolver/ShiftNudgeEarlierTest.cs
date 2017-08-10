using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class ShiftNudgeEarlierTest
    {
        private ShiftNudgeEarlier _target;
        private MockRepository _mocks;
        private IScheduleDay _scheduleDay;
		private ITeamBlockClearer _teamBlockClearer;
		private ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private ITeamBlockScheduler _teamBlockScheduler;
	    private ISchedulePartModifyAndRollbackService _rollbackService;
	    private SchedulingOptions _schedulingOptions;
	    private ITeamBlockInfo _teamBlockInfo;
	    private IPersonAssignment _personAssignment;
	    private ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private IBlockInfo _blockInfo;
		private ITeamInfo _teamInfo;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDayPro _scheduleDayPro;
	    private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly GroupPersonSkillAggregator groupPersonSkillAggregator = new GroupPersonSkillAggregator(new PersonalSkillsProvider());

		[SetUp]
        public void SetUp()
        {
            _mocks = new MockRepository();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_teamBlockScheduler = _mocks.StrictMock<ITeamBlockScheduler>();
			_teamBlockRestrictionAggregator = _mocks.StrictMock<ITeamBlockRestrictionAggregator>();
		    _mainShiftOptimizeActivitySpecificationSetter = _mocks.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();
			_target = new ShiftNudgeEarlier(_teamBlockClearer, _teamBlockRestrictionAggregator, _teamBlockScheduler, _mainShiftOptimizeActivitySpecificationSetter, null, groupPersonSkillAggregator);
	        _scheduleDay = _mocks.StrictMock<IScheduleDay>();
	        _rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingOptions = new SchedulingOptions();
	        _teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
	        var period = new DateTimePeriod(new DateTime(2014, 3, 19, 8, 0, 0, DateTimeKind.Utc),
		        new DateTime(2014, 3, 19, 16, 0, 0, DateTimeKind.Utc));
			_personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(), period);
	        _schedulingResultStateHolder = _mocks.Stub<ISchedulingResultStateHolder>();
			_blockInfo = _mocks.StrictMock<IBlockInfo>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
        }

        [Test]
        public void ShouldReturnTrueIfNudgeSuccess()
        {
	        var effectiveRestriction = new EffectiveRestriction();
			var adjustedEffectiveRestriction = new EffectiveRestriction();
			var matrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var unlocked = new [] { _scheduleDayPro };

			using (_mocks.Record())
			{
				commonMocks(effectiveRestriction);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_personAssignment.Date)).Return(matrixes);
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(unlocked);
				Expect.Call(_scheduleDayPro.Day).Return(_personAssignment.Date);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(null, _teamBlockInfo, _personAssignment.Date, _schedulingOptions,
					_rollbackService, null,
					null, null,
					new ShiftNudgeDirective(adjustedEffectiveRestriction, ShiftNudgeDirective.NudgeDirection.Left), NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator))
					.IgnoreArguments()
					.Return(true);
			}

	        using (_mocks.Playback())
			{
				bool result = _target.Nudge(_scheduleDay, _rollbackService, _schedulingOptions, 
					_teamBlockInfo, _schedulingResultStateHolder, null, true);
				Assert.IsTrue(result);
			}
        }

		[Test]
		public void ShouldReturnFalseIfNotNudgeSuccess()
		{
			var effectiveRestriction = new EffectiveRestriction();
			var adjustedEffectiveRestriction = new EffectiveRestriction();
			var matrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var unlocked = new [] { _scheduleDayPro };

			using (_mocks.Record())
			{
				commonMocks(effectiveRestriction);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_personAssignment.Date)).Return(matrixes);
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(unlocked);
				Expect.Call(_scheduleDayPro.Day).Return(_personAssignment.Date);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(null, _teamBlockInfo, _personAssignment.Date, _schedulingOptions,
					_rollbackService, null,
					null, null,
					new ShiftNudgeDirective(adjustedEffectiveRestriction, ShiftNudgeDirective.NudgeDirection.Left), NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator))
					.IgnoreArguments()
					.Return(false);
				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(_personAssignment.Date, _personAssignment.Date));
			}

			using (_mocks.Playback())
			{
				bool result = _target.Nudge(_scheduleDay, _rollbackService, _schedulingOptions, 
					_teamBlockInfo, _schedulingResultStateHolder, null, true);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfInvalidEndTimeLimitation()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(TimeSpan.FromHours(16), null),
				new WorkTimeLimitation(), null, new DayOffTemplate(), null,
				new List<IActivityRestriction>());

			using (_mocks.Record())
			{
				commonMocks(effectiveRestriction);
			}

			using (_mocks.Playback())
			{
				bool result = _target.Nudge(_scheduleDay, _rollbackService, _schedulingOptions, 
					_teamBlockInfo, _schedulingResultStateHolder, null, true);
				Assert.IsFalse(result);
			}
		}

		[Test]
	    public void ShouldReturnFalseIfInvalidEndTimeLimitationWhenShiftEndAtMidnigh()
	    {
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), 
																new EndTimeLimitation(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(50)), 
																null),
																new WorkTimeLimitation(), 
																null, new DayOffTemplate(), 
																null,
																new List<IActivityRestriction>());

			var period = new DateTimePeriod(new DateTime(2014, 3, 19, 16, 0, 0, DateTimeKind.Utc), new DateTime(2014, 3, 20, 0, 0, 0, DateTimeKind.Utc));
			_personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(), period);

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_scheduleDay.TimeZone).Return(TimeZoneInfo.Utc);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockRestrictionAggregator.Aggregate(null, _personAssignment.Date, _personAssignment.Person, _teamBlockInfo, _schedulingOptions)).Return(effectiveRestriction);
			}

			using (_mocks.Playback())
			{
				var result = _target.Nudge(_scheduleDay, _rollbackService, _schedulingOptions, _teamBlockInfo, _schedulingResultStateHolder, null, true);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfLocked()
		{
			var effectiveRestriction = new EffectiveRestriction();
			var matrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var unlocked = new IScheduleDayPro[0];

			using (_mocks.Record())
			{
				commonMocks(effectiveRestriction);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_personAssignment.Date)).Return(matrixes);
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(unlocked);
			}

			using (_mocks.Playback())
			{
				var result = _target.Nudge(_scheduleDay, _rollbackService, _schedulingOptions, _teamBlockInfo, _schedulingResultStateHolder, null, true);
				Assert.IsFalse(result);
			}
		}

	    private void commonMocks(EffectiveRestriction effectiveRestriction)
	    {
		    Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
		    Expect.Call(_scheduleDay.TimeZone).Return(TimeZoneInfo.Utc);
			Expect.Call(() => _rollbackService.ClearModificationCollection());
		    Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
		    Expect.Call(_teamBlockRestrictionAggregator.Aggregate(null, _personAssignment.Date, _personAssignment.Person,
			    _teamBlockInfo, _schedulingOptions)).Return(effectiveRestriction);
	    }
    }
}
