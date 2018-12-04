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
	[DefaultData]
	[TestFixture]
	public class AgentDaysTest
	{
		public Adherence.Historical.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;
		
		[Test]
		public void ShouldDisplaySevenDays()
		{
			Now.Is("2018-08-22 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-22 14:00");
			Now.Is("2018-08-23 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Count().Should().Be(7);
		}

		[Test]
		public void ShouldGetADateForEachOfTheSevenDays()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");
			Now.Is("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Count(x => x.Date != null).Should().Be(7);
		}

		[Test]
		public void ShouldGetCorrectDateForFirstDay()
		{
			Now.Is("2018-08-22 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-22 14:00");
			Now.Is("2018-08-23 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.First().Date.Should().Be("20180816");
		}

		[Test]
		public void ShouldGetSevenDaysSequentially()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");
			Now.Is("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Select(x => x.Date).Should().Have.SameValuesAs(new[] {"20180817", "20180818", "20180819", "20180820", "20180821", "20180822", "20180823"});
		}

		[Test]
		public void ShouldDisplayDateForSevenDaysSequentially()
		{
			Now.Is("2018-08-23 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-08-23 14:00");
			Now.Is("2018-08-24 14:00");

			var data = Target.Build(null, new[] {teamId}).First();

			data.DisplayDays.Should().Have.SameValuesAs(new[] {"08/17", "08/18", "08/19", "08/20", "08/21", "08/22", "08/23"});
		}

	}
}