using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		private MockRepository _mocks;
		private ITeamBlockSingleDayScheduler _target;
		private ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private IProposedRestrictionAggregator _proposedRestrictionAggregator;
		private IWorkShiftFilterService _workShiftFilterService;
		private ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private IWorkShiftSelector _workShiftSelector;
		private ITeamScheduling _teamScheduling;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private ITeamBlockInfo _teamBlockInfo;
		private IShiftProjectionCache _roleModelShift;
		private ITeamBlockInfo _teamBlockSingleDayInfo;
		private ITeamInfo _teamInfo;
		private IGroupPerson _groupPerson;
		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamBlockSchedulingCompletionChecker = _mocks.StrictMock<ITeamBlockSchedulingCompletionChecker>();
			_proposedRestrictionAggregator = _mocks.StrictMock<IProposedRestrictionAggregator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_skillDayPeriodIntervalDataGenerator = _mocks.StrictMock<ISkillDayPeriodIntervalDataGenerator>();
			_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();
			_teamScheduling = _mocks.StrictMock<ITeamScheduling>();
			_teamBlockSchedulingOptions = _mocks.StrictMock<ITeamBlockSchedulingOptions>();
			_target = new TeamBlockSingleDayScheduler(_teamBlockSchedulingCompletionChecker, _proposedRestrictionAggregator,
			                                          _workShiftFilterService, _skillDayPeriodIntervalDataGenerator,
			                                          _workShiftSelector, _teamScheduling, _teamBlockSchedulingOptions);
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_roleModelShift = _mocks.StrictMock<IShiftProjectionCache>();
			_teamBlockSingleDayInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_groupPerson = _mocks.StrictMock<IGroupPerson>();
		}

		[Test]
		public void ShouldNotTryToScheduleDayThatIsAlreadyScheduled()
		{
			var schedulingOptions = new SchedulingOptions();
			var person1 = PersonFactory.CreatePersonWithGuid("a", "a");
			var person2 = PersonFactory.CreatePersonWithGuid("b", "b");
			var personsInTeam = new List<IPerson> {person1, person2};

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupPerson).Return(_groupPerson);
				Expect.Call(_groupPerson.GroupMembers).Return(personsInTeam);
				Expect.Call(
					_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockSingleDayInfo,
					                                                                                  new DateOnly(),
																									  personsInTeam))
				      .IgnoreArguments()
				      .Return(false);

				Expect.Call(
					_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockSingleDayInfo,
																									  new DateOnly(),
																									  new List<IPerson> { person1 }))
						.IgnoreArguments()
					  .Return(true);

				Expect.Call(
					_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockSingleDayInfo,
																									  new DateOnly(),
																									  new List<IPerson> { person2 }))
						.IgnoreArguments()
					  .Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(schedulingOptions)).Return(true);
				Expect.Call(() => _teamScheduling.DayScheduled += onDayScheduled).IgnoreArguments();
				Expect.Call(
					() =>
					_teamScheduling.ExecutePerDayPerPerson(person2, new DateOnly(), _teamBlockInfo, _roleModelShift,
					                                       new DateOnlyPeriod()));
				Expect.Call(() => _teamScheduling.DayScheduled -= onDayScheduled).IgnoreArguments();

				Expect.Call(
					_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockSingleDayInfo,
																									  new DateOnly(),
																									  personsInTeam))
					  .IgnoreArguments()
					  .Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleSingleDay(_teamBlockInfo, schedulingOptions, personsInTeam, new DateOnly(),
				                                       _roleModelShift, new DateOnlyPeriod());
				Assert.IsTrue(result);
			}
      
      

		}

		private void onDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, e);
			}
		}

	}
}