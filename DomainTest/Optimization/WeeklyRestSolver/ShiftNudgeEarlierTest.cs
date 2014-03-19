using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
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
	    private ISchedulingOptions _schedulingOptions;
	    private IResourceCalculateDelayer _resourceCalculateDelayer;
	    private ITeamBlockInfo _teamBlockInfo;
	    private IPersonAssignment _personAssignment;
	    private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IList<IPerson> _selectedPersons;
		
		[SetUp]
        public void SetUp()
        {
            _mocks = new MockRepository();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_teamBlockScheduler = _mocks.StrictMock<ITeamBlockScheduler>();
			_teamBlockRestrictionAggregator = _mocks.StrictMock<ITeamBlockRestrictionAggregator>();
			_target = new ShiftNudgeEarlier(_teamBlockClearer, _teamBlockRestrictionAggregator, _teamBlockScheduler);
	        _scheduleDay = _mocks.StrictMock<IScheduleDay>();
	        _rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingOptions = new SchedulingOptions();
	        _resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
	        _teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
	        var period = new DateTimePeriod(new DateTime(2014, 3, 19, 8, 0, 0, DateTimeKind.Utc),
		        new DateTime(2014, 3, 19, 16, 0, 0, DateTimeKind.Utc));
			_personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(), period);
	        _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_selectedPersons = new List<IPerson> {_personAssignment.Person};

        }

        [Test]
        public void ShouldReturnTrueIfNudgeSuccess()
        {
	        var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
		        new WorkTimeLimitation(), null, new DayOffTemplate(), null,
		        new List<IActivityRestriction>());

			var adjustedEffectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
				new WorkTimeLimitation(), null, new DayOffTemplate(), null,
				new List<IActivityRestriction>());

			using (_mocks.Record())
			{
				commonMocks(effectiveRestriction);

				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _personAssignment.Date, _schedulingOptions,
					new DateOnlyPeriod(), _selectedPersons, _rollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, adjustedEffectiveRestriction)).IgnoreArguments().Return(true);
			}

	        using (_mocks.Playback())
			{
				bool result = _target.Nudge(_scheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer,
					_teamBlockInfo, _schedulingResultStateHolder, new DateOnlyPeriod(), _selectedPersons);
				Assert.IsTrue(result);
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
				bool result = _target.Nudge(_scheduleDay, _rollbackService, _schedulingOptions, _resourceCalculateDelayer,
					_teamBlockInfo, _schedulingResultStateHolder, new DateOnlyPeriod(), _selectedPersons);
				Assert.IsFalse(result);
			}
		}

	    private void commonMocks(EffectiveRestriction effectiveRestriction)
	    {
		    Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
		    Expect.Call(_scheduleDay.TimeZone).Return(TimeZoneInfo.Utc);
		    Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
		    Expect.Call(_teamBlockRestrictionAggregator.Aggregate(_personAssignment.Date, _personAssignment.Person,
			    _teamBlockInfo, _schedulingOptions)).Return(effectiveRestriction);
	    }
    }
}
