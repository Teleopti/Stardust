using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
	public class OverlappingAssignmentRuleTest
	{
		private IContract _contract;
		private TimeSpan _nightlyRest;
		private IPerson _person;
		private IScenario _scenario;
		private DateTimePeriod _schedulePeriod;
		private IScheduleRange _scheduleRange;
		private OverlappingAssignmentRule _target;
		private MockRepository _mocks;

		[Test]
		public void CanCreateRule()
		{
			createRule();
			Assert.IsNotNull(_target);
		}

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_person = PersonFactory.CreatePerson();
			_nightlyRest = new TimeSpan(8, 0, 0);
			_contract = ContractFactory.CreateContract("for test");
			_contract.WorkTimeDirective = new WorkTimeDirective(new TimeSpan(0, 0, 0), new TimeSpan(40, 0, 0),
				_nightlyRest,
				new TimeSpan(50, 0, 0));
			_schedulePeriod = new DateTimePeriod(2007, 8, 1, 2007, 9, 1);
			var dic = _mocks.StrictMock<IScheduleDictionary>();
			using (_mocks.Record())
			{
				Expect.Call(dic.Scenario).Return(_scenario).Repeat.Any();
			}
			_scheduleRange = new ScheduleRange(dic, new ScheduleParameters(_scenario, _person, _schedulePeriod),
				new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission());
		}

		[Test]
		public void VerifyLongestDateTimePeriodForAssignmentReturnsCorrectWhenSeekingAfter()
		{
			_person.AddPersonPeriod(new PersonPeriod(
				new DateOnly(1900, 1, 1),
				new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
				TeamFactory.CreateSimpleTeam()));

			addPersonAssignmentsToScheduleRange();

			createRule();

			var pointInTime = new DateOnly(2007, 8, 5);
			var start = new DateTime(2007, 8, 3, 19, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2007, 9, 1, 0, 0, 0, DateTimeKind.Utc);

			var expected = new DateTimePeriod(start, end);

			var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void VerifyLongestDateTimePeriodForAssignmentReturnsCorrectWhenSeekingBefore()
		{
			_person.AddPersonPeriod(new PersonPeriod(
				new DateOnly(1900, 1, 1),
				new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
				TeamFactory.CreateSimpleTeam()));

			addPersonAssignmentsToScheduleRange();

			createRule();

			var pointInTime = new DateOnly(2007, 7, 31);
			var start = new DateTime(2007, 8, 1, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2007, 8, 3, 10, 0, 0, DateTimeKind.Utc);

			var expected = new DateTimePeriod(start, end);

			var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void VerifyLongestDateTimePeriodForAssignmentReturnsCorrectWhenSeekingOnAssignment()
		{
			_person.AddPersonPeriod(new PersonPeriod(
				new DateOnly(1900, 1, 1),
				new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
				TeamFactory.CreateSimpleTeam()));

			addPersonAssignmentsToScheduleRange();

			createRule();

			var pointInTime = new DateOnly(2007, 8, 1);

			var expected = new DateTimePeriod(new DateTime(2007, 8, 1, 12, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 8, 1, 12, 0, 0, DateTimeKind.Utc));

			var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
			Assert.AreEqual(expected, result);
		}

		private void addPersonAssignmentsToScheduleRange()
		{
			IList<IPersonAssignment> assignments = new List<IPersonAssignment>();
			assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
				_scenario, new DateTimePeriod(new DateTime(2007, 8, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 8, 1, 17, 0, 0, DateTimeKind.Utc))));
			assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
				_scenario, new DateTimePeriod(new DateTime(2007, 8, 2, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 8, 2, 17, 0, 0, DateTimeKind.Utc))));
			assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
				_scenario, new DateTimePeriod(new DateTime(2007, 8, 3, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 8, 3, 19, 0, 0, DateTimeKind.Utc))));

			((Schedule) _scheduleRange).AddRange(assignments);
		}

		private void createRule()
		{
			_target = new OverlappingAssignmentRule();
		}
	}
}
