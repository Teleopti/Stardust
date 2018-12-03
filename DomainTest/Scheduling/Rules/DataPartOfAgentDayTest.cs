using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture]
	public class DataPartOfAgentDayTest
	{
		 [Test]
		 public void ShouldResultInNoBusinessRuleRespone()
		 {
			 var scheduleOk = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));

			 var pa = new PersonAssignment(scheduleOk.Person, scheduleOk.Scenario, new DateOnly(2000, 1, 1));
			 var start = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			 var end = new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			 pa.AddActivity(new Activity("d"), new DateTimePeriod(start, end));
			 scheduleOk.Add(pa);

			 new DataPartOfAgentDay()
					.Validate(null, new[] {scheduleOk})
					.Should().Be.Empty();
		 }

		[Test]
		public void ShouldReturnCorrectResponseIfStartsTooEarly()
		{
			var start = new DateTime(1999, 12, 31, 15, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);
			var scheduleDataTooEarly = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2000, 1, 1);

			var expected = new BusinessRuleResponse(typeof (DataPartOfAgentDay), string.Empty, true, true, assignmentPeriod,
					                         scheduleDataTooEarly.Person, new DateOnlyPeriod(dateOnly, dateOnly), "tjillevippen");

			var pa = new PersonAssignment(scheduleDataTooEarly.Person, scheduleDataTooEarly.Scenario, dateOnly);
			pa.AddActivity(new Activity("d"), assignmentPeriod);
			scheduleDataTooEarly.Add(pa);

			new DataPartOfAgentDay()
				.Validate(null, new[] {scheduleDataTooEarly})
				.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldReturnCorrectResponseIfStartsTooLate()
		{
			var start = new DateTime(2000, 1, 2, 15, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 18, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);
			var scheduleDataTooEarly = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2000, 1, 1);

			var expected =  new BusinessRuleResponse(typeof (DataPartOfAgentDay), string.Empty, true, true, assignmentPeriod,
					                         scheduleDataTooEarly.Person, new DateOnlyPeriod(dateOnly, dateOnly), "tjillevippen");

			var pa = new PersonAssignment(scheduleDataTooEarly.Person, scheduleDataTooEarly.Scenario, dateOnly);
			pa.AddActivity(new Activity("d"), assignmentPeriod);
			scheduleDataTooEarly.Add(pa);

			new DataPartOfAgentDay()
				.Validate(null, new[] { scheduleDataTooEarly })
				.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldBeOkWithNoAssignment()
		{
			var scheduleOk = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));

			new DataPartOfAgentDay()
				 .Validate(null, new[] { scheduleOk })
				 .Should().Be.Empty();
		}

		[Test]
		public void ShouldResultInTwoResponses()
		{
			var start = new DateTime(2000, 1, 2, 15, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 18, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);
			var scheduleDataTooEarly = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var dateOnly = new DateOnly(2000, 1, 1);

			var expected = new BusinessRuleResponse(typeof (DataPartOfAgentDay), string.Empty, true, true, assignmentPeriod,
				                         scheduleDataTooEarly.Person, new DateOnlyPeriod(dateOnly, dateOnly), "tjillevippen");

			var pa = new PersonAssignment(scheduleDataTooEarly.Person, scheduleDataTooEarly.Scenario, dateOnly);
			pa.AddActivity(new Activity("d"), assignmentPeriod);
			var pa2 = pa.EntityClone();
			scheduleDataTooEarly.Add(pa2);

			new DataPartOfAgentDay()
				.Validate(null, new[] { scheduleDataTooEarly })
				.Should().Have.SameValuesAs(expected, expected);
		}

		[Test]
		public void ShouldBeOkWithPersonalActivityOnOtherDay()
		{
			var start = new DateTime(2000, 1, 2, 2, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);
			var scheduleData = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var pa = new PersonAssignment(scheduleData.Person, scheduleData.Scenario, new DateOnly(2000, 1, 1));
			pa.SetDayOff(new DayOffTemplate());
			pa.AddPersonalActivity(new Activity(), assignmentPeriod);

			scheduleData.Add(pa);

			new DataPartOfAgentDay().Validate(null, new[] { scheduleData }).Should().Be.Empty();
		}
	}
}