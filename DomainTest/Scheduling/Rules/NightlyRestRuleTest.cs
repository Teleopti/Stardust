using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
	public class NightlyRestRuleTest
	{
		private IContract _contract;
		private TimeSpan _nightlyRest;
		private IPerson _person;
		private IScenario _scenario;
		private DateTimePeriod _schedulePeriod;
		private IScheduleRange _scheduleRange;
		private NightlyRestRule _target;
		private IPersistableScheduleDataPermissionChecker _permissionChecker;

		[SetUp]
		public void Setup()
		{
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_permissionChecker = new PersistableScheduleDataPermissionChecker(new FullPermission());

			_person = PersonFactory.CreatePerson();
			_nightlyRest = new TimeSpan(8, 0, 0);
			_contract = ContractFactory.CreateContract("for test");
			_contract.WorkTimeDirective = new WorkTimeDirective(new TimeSpan(0, 0, 0), new TimeSpan(40, 0, 0),
				_nightlyRest,
				new TimeSpan(50, 0, 0));
			_schedulePeriod = new DateTimePeriod(2007, 8, 1, 2007, 9, 1);
			var dic = new ScheduleDictionaryForTest(_scenario, new ScheduleDateTimePeriod(_schedulePeriod),
				new Dictionary<IPerson, IScheduleRange>());
			_scheduleRange = new ScheduleRange(dic, new ScheduleParameters(_scenario, _person, _schedulePeriod),
				_permissionChecker,new FullPermission());
			_target = new NightlyRestRule();
		}

		[Test]
		public void CanCreateRule()
		{
			Assert.IsNotNull(_target);
		}

		[Test]
		public void VerifyCanFindLongestDateTimePeriodForAssignment()
		{
			addPersonAssignmentsForLongest();
			_person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
				new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
				TeamFactory.CreateSimpleTeam()));

			var pointInTime = new DateOnly(2007, 8, 2);
			var start = new DateTime(2007, 8, 2, 1, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2007, 8, 3, 2, 0, 0, DateTimeKind.Utc);

			var expected = new DateTimePeriod(start, end);

			var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void VerifyCanFindLongestDateTimePeriodForAssignmentUtcPlus()
		{
			addPersonAssignmentsForLongest();
			_person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
				new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
				TeamFactory.CreateSimpleTeam()));
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			var pointInTime = new DateOnly(2007, 8, 2);
			var start = new DateTime(2007, 8, 2, 1, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2007, 8, 3, 2, 0, 0, DateTimeKind.Utc);

			var expected = new DateTimePeriod(start, end);

			var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void VerifyCanFindLongestDateTimePeriodForAssignmentUtcMinus()
		{
			addPersonAssignmentsForLongest();
			_person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
				new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
				TeamFactory.CreateSimpleTeam()));
			_person.PermissionInformation.SetDefaultTimeZone(
				(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")));

			var pointInTime = new DateOnly(2007, 8, 2);
			var start = new DateTime(2007, 8, 2, 1, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2007, 8, 3, 2, 0, 0, DateTimeKind.Utc);

			var expected = new DateTimePeriod(start, end);

			var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void VerifyFindLongestWhenNoPersonAssignmentsReturnsStartAndEndOfPeriod()
		{
			_person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
				new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
				TeamFactory.CreateSimpleTeam()));

			var pointInTime = new DateOnly(2007, 8, 7);
			var start = new DateTime(2007, 8, 1, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2007, 9, 1, 0, 0, 0, DateTimeKind.Utc);

			var expected = new DateTimePeriod(start, end);

			var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void VerifyFindLongestWhenNoPersonPeriodReturnsCorrect()
		{
			addPersonAssignmentsToSchedulePart();


			var pointInTime = new DateOnly(2007, 8, 7);
			var start = new DateTime(2007, 8, 7, 12, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2007, 8, 7, 12, 0, 0, DateTimeKind.Utc);

			var expected = new DateTimePeriod(start, end);

			var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void VerifyLongestWhenAlreadyScheduledAtTwelveOnTheSameDay()
		{
			_person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
				new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
				TeamFactory.CreateSimpleTeam()));
			addPersonAssignmentsToSchedulePart();

			var pointInTime = new DateOnly(2007, 8, 2);
			var start = new DateTime(2007, 8, 2, 12, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2007, 8, 2, 12, 0, 0, DateTimeKind.Utc);

			var expected = new DateTimePeriod(start, end);

			var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
			Assert.AreEqual(expected, result);
		}

		private void addPersonAssignmentsToSchedulePart()
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

		private void addPersonAssignmentsForLongest()
		{
			IList<IPersonAssignment> assignments = new List<IPersonAssignment>();
			assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
				_scenario, new DateTimePeriod(new DateTime(2007, 8, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 8, 1, 17, 0, 0, DateTimeKind.Utc))));
			assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
				_scenario, new DateTimePeriod(new DateTime(2007, 8, 3, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2007, 8, 3, 19, 0, 0, DateTimeKind.Utc))));

			((Schedule) _scheduleRange).AddRange(assignments);
		}
	}
}
