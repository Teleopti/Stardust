using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	public class ScheduleTest
	{
		public Domain.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeHistoricalAdherenceReadModelPersister ReadModel;
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
				;
			ReadModel.AddOut(person, "2016-10-11 11:00".Utc());
			ReadModel.AddIn(person, "2016-10-11 11:05".Utc());
			ReadModel.AddOut(person, "2016-10-11 13:30".Utc());
			ReadModel.AddIn(person, "2016-10-11 17:00".Utc());

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
			Now.Is("2017-04-19 01:00");
			var person = Guid.NewGuid();

			Database
				.WithAgent(person, "nicklas")
				.WithAssignment(person, "2017-04-18")
				.WithActivity()
				.WithAssignedActivity("2017-04-18 23:00", "2017-04-19 02:00")
				.WithAssignment(person, "2017-04-19")
				.WithAssignedActivity("2017-04-19 23:00", "2017-04-20 02:00")
				;

			var viewModel = Target.Build(person);

			viewModel.Schedules.Single().StartTime.Should().Be("2017-04-18T23:00:00");
		}

		[Test]
		public void ShouldIncludeTimelineEndpoints()
		{
			Now.Is("2016-10-11 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name")
				.WithAssignment(person, "2016-10-11")
				.WithActivity(null, "phone", ColorTranslator.FromHtml("#80FF80"))
				.WithAssignedActivity("2016-10-11 09:00", "2016-10-11 17:00");

			var viewModel = Target.Build(person);

			viewModel.Timeline.StartTime.Should().Be("2016-10-11T08:00:00");
			viewModel.Timeline.EndTime.Should().Be("2016-10-11T18:00:00");
		}

		[Test]
		public void ShouldIncludeTimelineEndpointsForAgentInChina()
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

			viewModel.Timeline.StartTime.Should().Be("2016-10-11T23:00:00");
			viewModel.Timeline.EndTime.Should().Be("2016-10-12T10:00:00");
		}

		[Test]
		public void ShouldIncludeTimelineEndpointsWithoutSchedule()
		{
			Now.Is("2016-10-11 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name");

			var viewModel = Target.Build(person);

			viewModel.Timeline.StartTime.Should().Be("2016-10-11T00:00:00");
			viewModel.Timeline.EndTime.Should().Be("2016-10-12T00:00:00");
		}

		[Test]
		public void ShouldIncludeTimelineEndpointsWithoutScheduleWhenAgentInChina()
		{
			Now.Is("2016-10-11 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "nicklas", TimeZoneInfoFactory.ChinaTimeZoneInfo());

			var viewModel = Target.Build(person);

			viewModel.Timeline.StartTime.Should().Be("2016-10-10T16:00:00");
			viewModel.Timeline.EndTime.Should().Be("2016-10-11T16:00:00");
		}

	}
}