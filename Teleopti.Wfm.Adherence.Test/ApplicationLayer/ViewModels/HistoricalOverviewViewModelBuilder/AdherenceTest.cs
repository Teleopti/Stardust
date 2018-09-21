using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class AdherenceTest
	{
		public Adherence.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldDisplayEmptyAdherenceForSevenDaysIfNotBeenWorking()
		{
			Now.Is("2018-08-24 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Count(a => a.Adherence == null).Should().Be(7);
		}

		[Test]
		public void ShouldDisplayFullAdherence()
		{
			Now.Is("2018-08-23 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-23")
				.WithAssignedActivity("2018-08-23 08:00", "2018-08-23 17:00")
				.WithHistoricalStateChange("2018-08-23 08:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In);
			Now.Is("2018-08-24 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Last().Adherence.Should().Be("100");
		}

		[Test]
		public void ShouldNotDisplayAdherenceExceptOnLastDay()
		{
			Now.Is("2018-08-23 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-23")
				.WithAssignedActivity("2018-08-23 08:00", "2018-08-23 17:00")
				.WithHistoricalStateChange("2018-08-23 08:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In);
			Now.Is("2018-08-24 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Count(a => a.Adherence == null).Should().Be(6);
		}

		[Test]
		public void ShouldDisplayCalculatedAdherence()
		{
			Now.Is("2018-08-17 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-17")
				.WithAssignedActivity("2018-08-17 10:00", "2018-08-17 20:00")
				.WithHistoricalStateChange("2018-08-17 10:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In)
				.WithHistoricalStateChange("2018-08-17 15:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.Out);
			Now.Is("2018-08-24 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be("50");
		}		
		
		[Test]
		public void ShouldDisplayAdherenceIfOutAllDay()
		{
			Now.Is("2018-08-17 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-17")
				.WithAssignedActivity("2018-08-17 10:00", "2018-08-17 20:00")
				.WithHistoricalStateChange("2018-08-17 10:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.Out);
			Now.Is("2018-08-24 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be("0");
		}		
		
		[Test]
		public void ShouldDisplayEmptyAdherenceIfNeutralAllDay()
		{
			Now.Is("2018-08-17 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-08-17")
				.WithAssignedActivity("2018-08-17 10:00", "2018-08-17 20:00")
				.WithHistoricalStateChange("2018-08-17 10:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.Neutral);
			Now.Is("2018-08-24 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be(null);
		}
		
		[Test]
		public void ShouldDisplayWithNeutralAdherence()
		{
			Now.Is("2018-09-20 08:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithAssignment("2018-09-20")
				.WithAssignedActivity("2018-09-20 10:00", "2018-09-20 18:00")
				.WithHistoricalStateChange("2018-09-20 10:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.Neutral)
				.WithHistoricalStateChange("2018-09-20 16:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.Out)
				.WithHistoricalStateChange("2018-09-20 17:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In);
			Now.Is("2018-09-27 08:00");
		
			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be("50");
		}
	}
}