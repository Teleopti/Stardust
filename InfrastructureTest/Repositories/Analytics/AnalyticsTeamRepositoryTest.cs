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
	public class AnalyticsTeamRepositoryTest
	{
		public IAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;
		public IAnalyticsTeamRepository Target;
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
			businessUnit = new BusinessUnit(BusinessUnitUsedInTests.BusinessUnit, datasource);
			analyticsDataFactory.Setup(businessUnit);

			analyticsDataFactory.Persist();
		}

		[Test]
		public void GetOrCreate_ShouldReturnTeamId()
		{
			siteId = WithAnalyticsUnitOfWork.Get(() => AnalyticsPersonPeriodRepository.GetOrCreateSite(Guid.NewGuid(), "Site name 1", businessUnit.BusinessUnitId));

			var teamId = WithAnalyticsUnitOfWork.Get(() => Target.GetOrCreate(Guid.NewGuid(), siteId, "Team Name", businessUnit.BusinessUnitId));
			teamId.Should().Be.GreaterThan(0);
		}

		[Test]
		public void ShouldUpdateTeamName()
		{
			siteId = WithAnalyticsUnitOfWork.Get(() => AnalyticsPersonPeriodRepository.GetOrCreateSite(Guid.NewGuid(), "Site name 1", businessUnit.BusinessUnitId));

			WithAnalyticsUnitOfWork.Get(() => Target.GetOrCreate(Guid.NewGuid(), siteId, "Team Name", businessUnit.BusinessUnitId));
			var team = WithAnalyticsUnitOfWork.Get(() => Target.GetTeams().First());

			WithAnalyticsUnitOfWork.Do(() => Target.UpdateName(team.TeamCode.Value, "NewTeamName"));

			var updatedTeam = WithAnalyticsUnitOfWork.Get(() => Target.GetTeams().First());

			updatedTeam.Name.Should().Be.EqualTo("NewTeamName");
		}

		[Test]
		public void GetAll_ShouldReturnAllTeams()
		{
			var teamGuid1 = Guid.NewGuid();
			var teamGuid2 = Guid.NewGuid();
			var teamId1 = 0;
			var teamId2 = 0;
			WithAnalyticsUnitOfWork.Do(() =>
			{
				siteId = AnalyticsPersonPeriodRepository.GetOrCreateSite(Guid.NewGuid(), "Site name 1",
					businessUnit.BusinessUnitId);
				teamId1 = Target.GetOrCreate(teamGuid1, siteId, "Team1", businessUnit.BusinessUnitId);
				teamId2 = Target.GetOrCreate(teamGuid2, siteId, "Team2", businessUnit.BusinessUnitId);
			});


			var teams = WithAnalyticsUnitOfWork.Get(() => Target.GetTeams());

			teams.Should().Not.Be.Empty();
			teams.Count.Should().Be.EqualTo(2);
			teams.FirstOrDefault(x => x.TeamCode == teamGuid1 && x.TeamId == teamId1).Should().Not.Be.Null();
			teams.FirstOrDefault(x => x.TeamCode == teamGuid2 && x.TeamId == teamId2).Should().Not.Be.Null();
		}
	}
}