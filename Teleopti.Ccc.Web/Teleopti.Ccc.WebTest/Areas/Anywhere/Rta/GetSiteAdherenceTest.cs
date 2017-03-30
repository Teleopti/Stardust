using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[TestFixture]
	[DomainTest]
	public class GetSiteAdherenceTest : ISetup
	{
		public AgentsInAlarmForSiteViewModelBuilder Target;
		public FakeSiteRepository Sites;
		public FakeSiteInAlarmReader AgentState;
		public FakeNumberOfAgentsInSiteReader AgentsInSite;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeSiteInAlarmReader>().For<ISiteInAlarmReader>();
			system.UseTestDouble<FakeNumberOfAgentsInSiteReader>().For<INumberOfAgentsInSiteReader>();
		}

		[Test]
		public void ShouldReturnOutOfAdherenceCount()
		{
			var site = new Site("s").WithId();
			Sites.Has(site);
			AgentState.Has(new AgentStateReadModel
			{
				SiteId = site.Id.Value,
				IsRuleAlarm = true,
				AlarmStartTime = DateTime.MinValue
			});
			AgentsInSite.Has(site.Id.Value,1);

			var result = Target.Build().Single();

			result.Id.Should().Be(site.Id);
			result.Name.Should().Be("s");
			result.NumberOfAgents.Should().Be(1);
			result.OpenHours.Count().Should().Be(0);
			result.OutOfAdherence.Should().Be(1);
			result.Color.Should().Be("danger");
		}

		[Test]
		public void ShouldReturnSitesWithoutAdherence()
		{
			var site = new Site("s").WithId();
			Sites.Has(site);
			
			var result = Target.Build().Single();

			result.Id.Should().Be(site.Id);
			result.Name.Should().Be("s");
			result.NumberOfAgents.Should().Be(0);
			result.OpenHours.Count().Should().Be(0);
			result.OutOfAdherence.Should().Be(0);
			result.Color.Should().Be(null);
		}

		[Test]
		public void ShouldSotSitesBasedOnName()
		{
			var site1 = new Site("Å").WithId();
			var site2 = new Site("Ä").WithId();
			var site3 = new Site("A").WithId();
			
			Sites.Has(site1);
			Sites.Has(site2);
			Sites.Has(site3);

			var result = Target.Build().Select(x=>x.Id);

			result.Should().Have.SameSequenceAs(new [] {site3.Id.GetValueOrDefault(), site2.Id.GetValueOrDefault() , site1.Id.GetValueOrDefault() });
		}
	}

}
