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
	[RealPermissions]
	public class PermissionTest
	{
		public Adherence.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldCalculateAdherenceWithoutPermissions()
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
	}
}