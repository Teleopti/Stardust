using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceDate
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class DateTest
	{
		public Adherence.Historical.HistoricalAdherenceDate Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldReturnADate()
		{
			Now.Is("2017-12-12 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithActivity(null, "phone")
				.WithAssignment(person, "2017-12-12")
				.WithAssignedActivity("2017-12-12 08:00", "2017-12-12 16:00");

			var result = Target.MostRecentShiftDate(person);

			result.Should().Be("2017-12-12".Date());
		}

		[Test]
		public void ShouldReturnDateForYesterdaysShift()
		{
			Now.Is("2017-12-12 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithActivity(null, "phone")
				.WithAssignment(person, "2017-12-11")
				.WithAssignedActivity("2017-12-11 08:00", "2017-12-11 16:00");

			var result = Target.MostRecentShiftDate(person);

			result.Should().Be("2017-12-11".Date());
		}

		[Test]
		public void ShouldReturnDateForYesterdaysShiftWhenNoActivityToday()
		{
			Now.Is("2017-12-12 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithActivity(null, "phone")
				.WithAssignment(person, "2017-12-09")
				.WithAssignment(person, "2017-12-10")
				.WithAssignment(person, "2017-12-11")
				.WithAssignedActivity("2017-12-11 08:00", "2017-12-11 16:00")
				.WithAssignment(person, "2017-12-12")
				;

			var result = Target.MostRecentShiftDate(person);

			result.Should().Be("2017-12-11".Date());
		}

		[Test]
		public void ShouldReturnDateFor7DaysAgo()
		{
			Now.Is("2017-12-12 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithActivity(null, "phone")
				.WithAssignment(person, "2017-12-06")
				.WithAssignedActivity("2017-12-06 08:00", "2017-12-06 16:00")
				.WithAssignment(person, "2017-12-07")
				.WithAssignment(person, "2017-12-08")
				.WithAssignment(person, "2017-12-09")
				.WithAssignment(person, "2017-12-10")
				.WithAssignment(person, "2017-12-11")
				.WithAssignment(person, "2017-12-12")
				;

			var result = Target.MostRecentShiftDate(person);

			result.Should().Be("2017-12-06".Date());
		}

		[Test]
		public void ShouldReturnTodaysDateFor8DaysAgo()
		{
			Now.Is("2017-12-12 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithActivity(null, "phone")
				.WithAssignment(person, "2017-12-05")
				.WithAssignedActivity("2017-12-05 08:00", "2017-12-05 16:00")
				.WithAssignment(person, "2017-12-06")
				.WithAssignment(person, "2017-12-07")
				.WithAssignment(person, "2017-12-08")
				.WithAssignment(person, "2017-12-09")
				.WithAssignment(person, "2017-12-10")
				.WithAssignment(person, "2017-12-11")
				.WithAssignment(person, "2017-12-12")
				;

			var result = Target.MostRecentShiftDate(person);

			result.Should().Be("2017-12-12".Date());
		}

		[Test]
		public void ShouldReturnYesterdaysShiftWhenTodaysShiftHasNotStarted()
		{
			Now.Is("2017-12-12 07:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithActivity(null, "phone")
				.WithAssignment(person, "2017-12-10")
				.WithAssignedActivity("2017-12-10 08:00", "2017-12-10 16:00")
				.WithAssignment(person, "2017-12-11")
				.WithAssignedActivity("2017-12-11 08:00", "2017-12-11 16:00")
				.WithAssignment(person, "2017-12-12")
				.WithAssignedActivity("2017-12-12 08:00", "2017-12-12 16:00")
				;

			var result = Target.MostRecentShiftDate(person);

			result.Should().Be("2017-12-11".Date());
		}

		[Test]
		public void ShouldReturnTodaysShiftWhenSecondLayerIsStartingActivity()
		{
			Now.Is("2017-12-12 12:00");
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithActivity(phone, "phone")
				.WithActivity(admin, "break")
				.WithAssignment(person, "2017-12-11")
				.WithAssignedActivity("2017-12-11 08:00", "2017-12-11 16:00")
				.WithAssignment(person, "2017-12-12")
				.WithAssignedActivity(admin, "2017-12-12 13:00", "2017-12-12 16:00")
				.WithAssignedActivity(phone, "2017-12-12 08:00", "2017-12-12 16:00")
				;

			var result = Target.MostRecentShiftDate(person);

			result.Should().Be("2017-12-12".Date());
		}
		
		[Test]
		public void ShouldReturnDateForShiftOfAgentInChina()
		{
			Now.Is("2017-12-12 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "nån", TimeZoneInfoFactory.ChinaTimeZoneInfo())
				.WithActivity(null, "phone")
				.WithAssignment(person, "2017-12-13")
				.WithAssignedActivity("2017-12-12 18:00", "2017-12-12 20:00")
				;

			var result = Target.MostRecentShiftDate(person);

			result.Should().Be("2017-12-13".Date());
		}
		
		[Test]
		public void ShouldReturnDateForShiftBeforeFullDayAbsence()
		{
			Now.Is("2017-12-12 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithActivity(null, "phone")
				.WithAssignment(person, "2017-12-10")
				.WithAssignedActivity("2017-12-10 08:00", "2017-12-10 16:00")
				.WithAssignment(person, "2017-12-12")
				.WithPersonAbsence("2017-12-12 08:00", "2017-12-12 16:00")
				;

			var result = Target.MostRecentShiftDate(person);

			result.Should().Be("2017-12-10".Date());
		}
	}
}