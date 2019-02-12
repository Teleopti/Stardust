using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
	public class NewNightlyRestRuleTest : IIsolateSystem
	{
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario Scenario;
		public NewNightlyRestRule Target;

		private DateOnly personPeriodStartDate = new DateOnly(2010, 1, 1);

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<NewNightlyRestRule>().For<NewNightlyRestRule>();
		}

		[Test]
		public void VerifyProperties()
		{
			Assert.IsFalse(Target.IsMandatory);
			Assert.IsTrue(Target.HaltModify);
			Target.HaltModify = false;
			Assert.IsFalse(Target.HaltModify);
		}

		[Test]
		public void VerifyNoPersonPeriod()
		{
			var person = setupPerson();
			person.RemoveAllPersonPeriods();

			for (int i = 1; i <= 3; i++)
			{
				var personAssignment = createMainPersonAssignment(person, new DateTimePeriod(2010, 1, i, 8, 2010, 1, i, 17));
				ScheduleStorage.Add(personAssignment);
			}

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(personPeriodStartDate, new DateOnly(2010, 1, 3)), Scenario.Current());
			var scheduleRange = (IScheduleRange)scheduleDictionary[person].Clone();
			var ranges = new Dictionary<IPerson, IScheduleRange> { { person, scheduleRange } };

			var responsList = Target.Validate(ranges, new[] { scheduleRange.ScheduledDay(new DateOnly(2010, 1, 2)) });
			Assert.AreEqual(0, responsList.Count());
		}

		[Test]
		public void VerifyNoSchedules()
		{
			var person = setupPerson();

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(personPeriodStartDate, new DateOnly(2010, 1, 3)), Scenario.Current());
			var scheduleRange = (IScheduleRange)scheduleDictionary[person].Clone();
			var ranges = new Dictionary<IPerson, IScheduleRange> { { person, scheduleRange } };

			var responsList = Target.Validate(ranges, new[] { scheduleRange.ScheduledDay(new DateOnly(2010, 1, 2)) });
			Assert.AreEqual(0, responsList.Count());
		}

		[Test]
		public void VerifyScheduleBreaksRule()
		{
			var person = setupPerson(12);

			for (int i = 1; i <= 3; i++)
			{
				var personAssignment = createMainPersonAssignment(person, new DateTimePeriod(2010, 1, i, 5, 2010, 1, i, 19));
				ScheduleStorage.Add(personAssignment);
			}

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(personPeriodStartDate, new DateOnly(2010, 1, 3)), Scenario.Current());
			var scheduleRange = (IScheduleRange)scheduleDictionary[person].Clone();
			var ranges = new Dictionary<IPerson, IScheduleRange> { { person, scheduleRange } };

			var responsList = Target.Validate(ranges, new[] { scheduleRange.ScheduledDay(new DateOnly(2010, 1, 2)) });

			Assert.AreEqual(4, responsList.Count());
			Assert.IsFalse(responsList.First().Mandatory);
			Assert.AreEqual(Target.HaltModify, responsList.First().Error);

		}

		[Test]
		public void VerifyScheduleOk()
		{
			var person = setupPerson(10);

			for (int i = 1; i <= 3; i++)
			{
				var personAssignment = createMainPersonAssignment(person, new DateTimePeriod(2010, 1, i, 6, 2010, 1, i, 19));
				ScheduleStorage.Add(personAssignment);
			}

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(personPeriodStartDate, new DateOnly(2010, 1, 3)), Scenario.Current());
			var scheduleRange = (IScheduleRange)scheduleDictionary[person].Clone();
			var ranges = new Dictionary<IPerson, IScheduleRange> { { person, scheduleRange } };

			var responsList = Target.Validate(ranges, new[] { scheduleRange.ScheduledDay(new DateOnly(2010, 1, 2)) });

			Assert.AreEqual(0, responsList.Count());
		}

		[Test]
		public void VerifyRuleRemoveRangeBusinessRuleResponse()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(personPeriodStartDate, new List<ISkill>()).WithId();

			var doPeriod = new DateOnlyPeriod(2010, 1, 3, 2010, 1, 3);
			DateTimePeriod period = doPeriod.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			IList<IBusinessRuleResponse> list = new List<IBusinessRuleResponse>();

			doPeriod = new DateOnlyPeriod(2010, 1, 3, 2010, 1, 4);
			IBusinessRuleResponse thisResponse = new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true,
				period, person, doPeriod, "tjillevippen");
			IBusinessRuleResponse anotherResponse = new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true,
				period.MovePeriod(TimeSpan.FromDays(10)), person, doPeriod, "tjillevippen");

			IBusinessRuleResponse thirdResponse = new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true,
				period, PersonFactory.CreatePerson(), doPeriod, "tjillevippen");
			list.Add(thisResponse);
			list.Add(anotherResponse);
			list.Add(thirdResponse);
			var target = new NewNightlyRestRule(new WorkTimeStartEndExtractor());
			target.ClearMyResponses(list, person, new DateOnly(2010, 1, 3));
			Assert.AreEqual(2, list.Count);
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldCalculateOvertime()
		{
			var person = setupPerson(10);

			var activity = new Activity("test") { InWorkTime = true };
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("ot", MultiplicatorType.OBTime);
			for (int i = 1; i <= 3; i++)
			{
				var personAssignment = createMainPersonAssignment(person, new DateTimePeriod(2010, 1, i, 5, 2010, 1, i, 19));
				personAssignment.AddOvertimeActivity(activity, new DateTimePeriod(2010, 1, i, 19, 2010, 1, i, 20), multiplicatorDefinitionSet);
				ScheduleStorage.Add(personAssignment);
			}

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(personPeriodStartDate, new DateOnly(2010, 1, 3)), Scenario.Current());
			var scheduleRange = (IScheduleRange)scheduleDictionary[person].Clone();
			var ranges = new Dictionary<IPerson, IScheduleRange> { { person, scheduleRange } };

			var responsList = Target.Validate(ranges, new[] { scheduleRange.ScheduledDay(new DateOnly(2010, 1, 2)) });

			Assert.AreEqual(4, responsList.Count());
			Assert.IsFalse(responsList.First().Mandatory);
			Assert.AreEqual(Target.HaltModify, responsList.First().Error);

			foreach (var response in responsList)
			{
				Assert.IsTrue(response.FriendlyName.StartsWith("Nattvilan är"));
				Assert.IsTrue(response.Message.StartsWith("Det måste finnas en"));
			}
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldCalculateOvertimeWithDayOff()
		{
			var person = setupPerson(10);

			var activity = new Activity("test") { InWorkTime = true };
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("ot", MultiplicatorType.OBTime);

			var personAssignmentYesterday = createMainPersonAssignment(person, new DateTimePeriod(2010, 1, 1, 5, 2010, 1, 1, 19));
			ScheduleStorage.Add(personAssignmentYesterday);

			var personAssignmentToday = createMainPersonAssignmenDayoff(person, new DateOnly(2010, 1, 2));
			personAssignmentToday.AddOvertimeActivity(activity, new DateTimePeriod(2010, 1, 2, 0, 2010, 1, 2, 1), multiplicatorDefinitionSet);
			ScheduleStorage.Add(personAssignmentToday);

			var personAssignmentTomorrow = createMainPersonAssignmenDayoff(person, new DateOnly(2010, 1, 3));
			ScheduleStorage.Add(personAssignmentTomorrow);

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(personPeriodStartDate, new DateOnly(2010, 1, 3)), Scenario.Current());
			var scheduleRange = (IScheduleRange)scheduleDictionary[person].Clone();
			var ranges = new Dictionary<IPerson, IScheduleRange> { { person, scheduleRange } };

			var responsList = Target.Validate(ranges, new[] { scheduleRange.ScheduledDay(new DateOnly(2010, 1, 2)) });

			Assert.AreEqual(2, responsList.Count());
			Assert.IsFalse(responsList.First().Mandatory);
			Assert.AreEqual(Target.HaltModify, responsList.First().Error);

			foreach (var response in responsList)
			{
				Assert.IsTrue(response.FriendlyName.StartsWith("Nattvilan är"));
				Assert.IsTrue(response.Message.StartsWith("Det måste finnas en"));
			}
		}

		[Test]
		public void ShouldNotCalculateAbsence()
		{
			var person = setupPerson(10);

			var activity = new Activity("test") { InWorkTime = true };
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("ot", MultiplicatorType.OBTime);

			var personAssignmentYesterday = createMainPersonAssignment(person, new DateTimePeriod(2010, 1, 1, 5, 2010, 1, 1, 19));
			personAssignmentYesterday.AddOvertimeActivity(activity, new DateTimePeriod(2010, 1, 1, 19, 2010, 1, 1, 20), multiplicatorDefinitionSet);
			ScheduleStorage.Add(personAssignmentYesterday);

			ScheduleStorage.Add(new PersonAbsence(person, Scenario.Current(), new AbsenceLayer(new Absence(), new DateTimePeriod(2010, 1, 2, 0, 2010, 1, 3, 0))));

			var personAssignmentTomorrow = createMainPersonAssignment(person, new DateTimePeriod(2010, 1, 3, 5, 2010, 1, 3, 19));
			personAssignmentTomorrow.AddOvertimeActivity(activity, new DateTimePeriod(2010, 1, 3, 19, 2010, 1, 3, 20), multiplicatorDefinitionSet);
			ScheduleStorage.Add(personAssignmentTomorrow);

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(personPeriodStartDate, new DateOnly(2010, 1, 3)), Scenario.Current());
			var scheduleRange = (IScheduleRange)scheduleDictionary[person].Clone();
			var ranges = new Dictionary<IPerson, IScheduleRange> { { person, scheduleRange } };

			var responsList = Target.Validate(ranges, new[] { scheduleRange.ScheduledDay(new DateOnly(2010, 1, 2)) });

			Assert.AreEqual(0, responsList.Count());
		}

		private IPersonAssignment createMainPersonAssignment(IPerson person, DateTimePeriod period)
		{
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			return PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, Scenario.Current(), main, period, shiftCategory).WithId();
		}
		private IPersonAssignment createMainPersonAssignmenDayoff(IPerson person, DateOnly day)
		{
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			return PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(), day, dayOffTemplate);
		}

		private IPerson setupPerson(int nightlyRestHours = 6)
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(personPeriodStartDate, new List<ISkill>()).WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personPeriod = person.PersonPeriods(personPeriodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(24), TimeSpan.FromHours(nightlyRestHours), TimeSpan.FromHours(10));
			return person;
		}
	}
}
