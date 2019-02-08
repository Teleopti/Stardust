using System;
using System.Drawing;
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
	public class TimelineTest
	{
		public Adherence.Historical.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

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
		public void ShouldInclude24HourTimelineWhenNoSchedule()
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
		public void ShouldInclude24HourTimelineWhenNoScheduleAndAgentInChina()
		{
			Now.Is("2016-10-11 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "nicklas", TimeZoneInfoFactory.ChinaTimeZoneInfo());

			var viewModel = Target.Build(person);

			viewModel.Timeline.StartTime.Should().Be("2016-10-10T16:00:00");
			viewModel.Timeline.EndTime.Should().Be("2016-10-11T16:00:00");
		}

		[Test]
		public void ShouldInclude24HourTimelineWhenNoScheduleForDate()
		{
			Now.Is("2017-12-14 10:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-14")
				.WithAssignedActivity("2017-12-14 08:00", "2017-12-14 16:00")
				;

			var viewModel = Target.Build(person, "2017-12-10".Date());

			viewModel.Timeline.StartTime.Should().Be("2017-12-10T00:00:00");
			viewModel.Timeline.EndTime.Should().Be("2017-12-11T00:00:00");
		}
	}
}