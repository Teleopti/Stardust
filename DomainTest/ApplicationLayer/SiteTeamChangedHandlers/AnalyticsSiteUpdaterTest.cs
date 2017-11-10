using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SiteTeamChangedHandlers
{
	[DomainTest]
	public class AnalyticsSiteUpdaterTest
	{
		public AnalyticsSiteUpdater Target;
		public FakeAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;
		public FakeAnalyticsSiteRepository AnalyticsSiteRepository;

		[Test]
		public void ShouldUpdateSiteName()
		{
			var site = new AnalyticsSite
			{
				SiteCode = Guid.NewGuid(),
				Name = "MySite"
			};
			AnalyticsPersonPeriod personPeriod = new AnalyticsPersonPeriod()
			{
				PersonCode = Guid.NewGuid(),
				SiteCode = site.SiteCode,
				SiteName = site.Name
			};
			AnalyticsSiteRepository.Has(site);
			AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(personPeriod);

			var changeEvent = new SiteNameChangedEvent()
			{
				SiteId = site.SiteCode,
				Name = "NewSiteName"
			};

			Target.Handle(changeEvent);

			AnalyticsSiteRepository.GetSites().First().Name
				.Should().Be.EqualTo(changeEvent.Name);
			AnalyticsPersonPeriodRepository.GetPersonPeriods(personPeriod.PersonCode).First().SiteName
				.Should().Be.EqualTo(changeEvent.Name);
		}
	}
}
