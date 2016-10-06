using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsTeamRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;
		public IAnalyticsTeamRepository Target;

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
		public void GetOrCreate_ShouldReturnTeamId()
		{
			siteId = AnalyticsPersonPeriodRepository.GetOrCreateSite(Guid.NewGuid(), "Site name 1", businessUnit.BusinessUnitId);

			var teamId = Target.GetOrCreate(Guid.NewGuid(), siteId, "Team Name", businessUnit.BusinessUnitId);
			teamId.Should().Be.GreaterThan(0);
		}

		[Test]
		public void GetAll_ShouldReturnAllTeams()
		{
			siteId = AnalyticsPersonPeriodRepository.GetOrCreateSite(Guid.NewGuid(), "Site name 1", businessUnit.BusinessUnitId);

			var teamGuid1 = Guid.NewGuid();
			var teamGuid2 = Guid.NewGuid();
			var teamName1 = "Team1";
			var teamName2 = "Team2";
			var teamId1 = Target.GetOrCreate(teamGuid1, siteId, teamName1, businessUnit.BusinessUnitId);
			var teamId2 = Target.GetOrCreate(teamGuid2, siteId, teamName2, businessUnit.BusinessUnitId);
			var teams = Target.GetTeams();

			teams.Should().Not.Be.Empty();
			var team1 = teams.First();
			team1.TeamCode.Should().Be.EqualTo(teamGuid1);
			team1.TeamId.Should().Be.EqualTo(teamId1);
			var team2 = teams.Last();
			team2.TeamCode.Should().Be.EqualTo(teamGuid2);
			team2.TeamId.Should().Be.EqualTo(teamId2);
		}
	}
}