using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	public class BuildLateForWorkTest
	{
		public Adherence.Historical.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;
		
		[Test]
		public void ShouldNotDisplayLateForWork()
		{
			Now.Is("2018-08-24 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-24 08:00");
			Now.Is("2018-08-25 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().WasLateForWork.Should().Be(false);
		}

		[Test]
		public void ShouldDisplayLateForWork()
		{
			Now.Is("2018-08-27 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithArrivedLateForWork("2018-08-27 10:00", "2018-08-27 11:00");
			Now.Is("2018-08-28 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Last().WasLateForWork.Should().Be(true);
		}

		[Test]
		public void ShouldNotDisplayLateForWorkExceptOnLastDay()
		{
			Now.Is("2018-08-27 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithArrivedLateForWork("2018-08-27 10:00", "2018-08-27 11:00");
			Now.Is("2018-08-28 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Count(a => a.WasLateForWork == false).Should().Be(6);
		}

		[Test]
		public void ShouldDisplayLateForWorkSumZero()
		{
			Now.Is("2018-08-24 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-24 08:00");
			Now.Is("2018-08-25 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().LateForWork.Count.Should().Be(0);
		}

		[Test]
		public void ShouldDisplayLateForWorkSum()
		{
			Now.Is("2018-08-27 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithArrivedLateForWork("2018-08-27 10:00", "2018-08-27 11:00");
			Now.Is("2018-08-28 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().LateForWork.Count.Should().Be(1);
		}

		[Test]
		public void ShouldDisplayLateForkWorkSumInMinutes()
		{
			Now.Is("2018-08-24 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-24 08:00");
			Now.Is("2018-08-25 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().LateForWork.TotalMinutes.Should().Be(0);
		}

		[Test]
		public void ShouldDisplayLateForkWorkSum60Minutes()
		{
			Now.Is("2018-08-27 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithArrivedLateForWork("2018-08-27 10:00", "2018-08-27 11:00");
			Now.Is("2018-08-28 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().LateForWork.TotalMinutes.Should().Be(60);
		}
	}
}