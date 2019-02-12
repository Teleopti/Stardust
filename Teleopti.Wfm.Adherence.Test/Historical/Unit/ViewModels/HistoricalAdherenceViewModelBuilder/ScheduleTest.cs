using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	public class ScheduleTest
	{
		public Adherence.Historical.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetSchedule()
		{
			Now.Is("2016-10-11 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name")
				.WithAssignment(person, "2016-10-11")
				.WithActivity(null, "phone", ColorTranslator.FromHtml("#80FF80"))
				.WithAssignedActivity("2016-10-11 09:00", "2016-10-11 17:00");

			var viewModel = Target.Build(person);

			viewModel.Schedules.Single().Name.Should().Be("phone");
			viewModel.Schedules.Single().Color.Should().Be("#80FF80");
			viewModel.Schedules.Single().StartTime.Should().Be("2016-10-11T09:00:00");
			viewModel.Schedules.Single().EndTime.Should().Be("2016-10-11T17:00:00");
		}

		[Test]
		public void ShouldGetSchedules()
		{
			Now.Is("2016-10-11 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name")
				.WithAssignment(person, "2016-10-11")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2016-10-11 09:00", "2016-10-11 12:00")
				.WithActivity(null, "email")
				.WithAssignedActivity("2016-10-11 12:00", "2016-10-11 17:00")
				.WithHistoricalStateChange(person, "2016-10-11 11:00", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-11 11:05", Adherence.Configuration.Adherence.In)
				.WithHistoricalStateChange(person, "2016-10-11 13:30", Adherence.Configuration.Adherence.Out)
				.WithHistoricalStateChange(person, "2016-10-11 17:00", Adherence.Configuration.Adherence.In);

			var viewModel = Target.Build(person);

			viewModel.OutOfAdherences.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldGetScheduleForAgentInChina()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database
				.WithAgent(person, "nicklas", TimeZoneInfoFactory.ChinaTimeZoneInfo())
				.WithAssignment(person, "2016-10-11")
				.WithActivity()
				.WithAssignedActivity("2016-10-11 00:00", "2016-10-11 09:00")
				.WithAssignment(person, "2016-10-12")
				.WithActivity()
				.WithAssignedActivity("2016-10-12 00:00", "2016-10-12 09:00")
				.WithAssignment(person, "2016-10-13")
				.WithActivity()
				.WithAssignedActivity("2016-10-13 00:00", "2016-10-13 09:00");

			var viewModel = Target.Build(person);

			viewModel.Schedules.Single().StartTime.Should().Be("2016-10-12T00:00:00");
		}

		[Test]
		public void ShouldGetFullNightShift()
		{
			Now.Is("2017-04-18 23:00");
			var person = Guid.NewGuid();

			Database
				.WithAgent(person, "nicklas")
				.WithAssignment(person, "2017-04-18")
				.WithActivity()
				.WithAssignedActivity("2017-04-18 23:00", "2017-04-19 01:00")
				.WithActivity()
				.WithAssignedActivity("2017-04-19 01:00", "2017-04-19 02:00")
				;

			var viewModel = Target.Build(person);

			viewModel.Schedules.Last().StartTime.Should().Be("2017-04-19T01:00:00");
		}

		[Test]
		public void ShouldGetOngoingNightShift()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "nicklas")
				.WithAssignment(person, "2017-04-18")
				.WithActivity()
				.WithAssignedActivity("2017-04-18 23:00", "2017-04-19 02:00")
				.WithAssignment(person, "2017-04-19")
				.WithAssignedActivity("2017-04-19 23:00", "2017-04-20 02:00")
				;

			var viewModel = Target.Build(person, "2017-04-18".Date());

			viewModel.Schedules.Single().StartTime.Should().Be("2017-04-18T23:00:00");
		}
	}
}