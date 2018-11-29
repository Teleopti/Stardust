using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
	public class ScheduleRangeValidateRulesTest
	{
		private IScheduleRange _scheduleRange;
		private IScenario _scenario;
		private TimeSpan _nightlyRest;
		private IContract _contract;
		private DateTimePeriod _schedulePeriod;
		private IPerson _person;
		private IScheduleDictionary scheduleDic;
		private IPersistableScheduleDataPermissionChecker _permissionChecker;

		[SetUp]
		public void Setup()
		{
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_permissionChecker = new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make());
			_person = PersonFactory.CreatePerson();
			var dic = new Dictionary<IPerson, IScheduleRange>();
			_schedulePeriod = new DateTimePeriod(2007, 8, 1, 2007, 9, 1);
			scheduleDic = new ScheduleDictionaryForTest(_scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2020, 1, 1)),
				dic);
			_scheduleRange = new ScheduleRange(scheduleDic, new ScheduleParameters(_scenario, _person, _schedulePeriod),
				_permissionChecker, new FullPermission());
			dic[_person] = _scheduleRange;
			_nightlyRest = new TimeSpan(8, 0, 0);
			_contract = new Contract("for test")
			{
				WorkTimeDirective = new WorkTimeDirective(new TimeSpan(0, 0, 0), new TimeSpan(40, 0, 0),
					_nightlyRest, new TimeSpan(50, 0, 0))
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void VerifyValidate()
		{
			addPersonAssignmentsToSchedulePart();
			var currentAuthorization = CurrentAuthorization.Make();
			var schedulePart = ExtractedSchedule.CreateScheduleDay(scheduleDic, _person, new DateOnly(2007, 8, 3), currentAuthorization);
			// add another assigment too close to the last one
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, _scenario, new DateTimePeriod(
				new DateTime(2007, 8, 3, 20, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 8, 4, 02, 0, 0, DateTimeKind.Utc)));
			schedulePart.Add(ass);
			ITeam team = new Team();

			_person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
				new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
				team));

			var per = new ScheduleDateTimePeriod(_schedulePeriod);
			var dic = new ScheduleDictionary(_scenario, per, _permissionChecker, currentAuthorization);
			dic.Modify(ScheduleModifier.Scheduler, schedulePart, NewBusinessRuleCollection.Minimum(),
				new DoNothingScheduleDayChangeCallBack(), new ScheduleTagSetter(NullScheduleTag.Instance));

			var scheduleDay = dic.SchedulesForDay(new DateOnly(2007, 8, 1)).First();
			Assert.AreSame(dic, scheduleDay.Owner);
			Assert.AreEqual(0, scheduleDay.BusinessRuleResponseCollection.Count);
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

			((ScheduleRange) _scheduleRange).AddRange(assignments);
		}
	}
}
