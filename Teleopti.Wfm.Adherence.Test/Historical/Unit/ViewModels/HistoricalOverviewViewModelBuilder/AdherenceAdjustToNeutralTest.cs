using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalOverviewViewModelBuilder
{
	[Ignore("Too soon, not done with AdherenceDay yet")]
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class AdherenceAdjustToNeutralTest
	{
		public Adherence.Historical.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldDisplayWithAdjustedToNeutral()
		{
			Now.Is("2019-02-01 08:00");
			var teamId = Guid.NewGuid();
			var person = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent(person)
				.WithAssignment("2019-02-01")
				.WithAssignedActivity("2019-02-01 08:00", "2019-02-01 16:00");
			History
				.StateChanged(person, "2019-02-01 08:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-02-01 10:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-02-01 12:00", "2019-02-01 16:00");
			Now.Is("2019-02-08 08:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Adherence.Should().Be(50);
		}
	}
}