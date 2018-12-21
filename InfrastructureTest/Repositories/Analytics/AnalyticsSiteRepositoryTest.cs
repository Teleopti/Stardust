using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsSiteRepositoryTest
	{
		public IAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;
		public IAnalyticsSiteRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;

		private int siteId;
		BusinessUnit businessUnit;
		ExistingDatasources datasource;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);
			analyticsDataFactory.Setup(businessUnit);

			analyticsDataFactory.Persist();
		}


		[Test]
		public void ShouldUpdateSiteName()
		{
			siteId = WithAnalyticsUnitOfWork.Get(() => AnalyticsPersonPeriodRepository.GetOrCreateSite(Guid.NewGuid(), "SiteName", businessUnit.BusinessUnitId));
			var site = WithAnalyticsUnitOfWork.Get(() => Target.GetSites().First());

			WithAnalyticsUnitOfWork.Do(() => Target.UpdateName(site.SiteCode, "NewSiteName"));

			var updatedSite = WithAnalyticsUnitOfWork.Get(() => Target.GetSites().First());

			updatedSite.Name.Should().Be.EqualTo("NewSiteName");
		}

		[Test]
		public void ShouldUpdateDataSourceUpdateDate()
		{
			siteId = WithAnalyticsUnitOfWork.Get(() => AnalyticsPersonPeriodRepository.GetOrCreateSite(Guid.NewGuid(), "SiteName", businessUnit.BusinessUnitId));
			var site = WithAnalyticsUnitOfWork.Get(() => Target.GetSites().First());

			WithAnalyticsUnitOfWork.Do(() => Target.UpdateName(site.SiteCode, "NewSiteName"));

			var updatedSite = WithAnalyticsUnitOfWork.Get(() => Target.GetSites().First());

			updatedSite.DataSourceUpdateDate.Should().Not.Be(null);
		}

		[Test]
		public void GetAll_ShouldReturnAllTeams()
		{
			var siteGuid1 = Guid.NewGuid();
			var siteGuid2 = Guid.NewGuid();
			WithAnalyticsUnitOfWork.Do(() =>
			{
				 AnalyticsPersonPeriodRepository.GetOrCreateSite(siteGuid1, "Site name 1",
					businessUnit.BusinessUnitId);
				AnalyticsPersonPeriodRepository.GetOrCreateSite(siteGuid2, "Site name 1",
					businessUnit.BusinessUnitId);
			});


			var sites = WithAnalyticsUnitOfWork.Get(() => Target.GetSites());

			sites.Count.Should().Be.EqualTo(2);
			sites.FirstOrDefault(x => x.SiteCode == siteGuid1).Should().Not.Be.Null();
			sites.FirstOrDefault(x => x.SiteCode == siteGuid2).Should().Not.Be.Null();
		}
	}
}