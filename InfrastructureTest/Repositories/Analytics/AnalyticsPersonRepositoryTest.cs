using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsPersonRepositoryTest
	{
		AnalyticsPersonPeriod personPeriod1;
		Guid personId;
		Guid personId2;
		ExistingDatasources datasource;
		BusinessUnit businessUnit;
		private int siteId;
		private Guid personPeriodCode1;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			analyticsDataFactory.Setup(businessUnit);
			//var person = new TestDataFactory().Person("Ashley Andeen").Person;
			var personWithGuid = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			var personWithGuid2 = PersonFactory.CreatePersonWithGuid("firstName2", "lastName2");
			personId = personWithGuid.Id.GetValueOrDefault();
			personId2 = personWithGuid2.Id.GetValueOrDefault();

			analyticsDataFactory.Setup(new Person(personWithGuid, datasource, 0, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId));
			analyticsDataFactory.Setup(new Person(personWithGuid2, datasource, 1, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId));

			analyticsDataFactory.Persist();

			var analyticsPersonRepository = new AnalyticsPersonRepository();
			personPeriodCode1 = Guid.NewGuid();
			personPeriod1 = new AnalyticsPersonPeriod
			{
				PersonPeriodCode = personPeriodCode1,
				ValidFromDate = new DateTime(2000, 1, 1),
				ValidToDate = new DateTime(2001, 1, 1),
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				BusinessUnitName = BusinessUnitFactory.BusinessUnitUsedInTest.Name,
				BusinessUnitId = 1,
				ContractCode = Guid.NewGuid(),
				ContractName = "Test contract",
				DatasourceId = 1,
				DatasourceUpdateDate = DateTime.Now,
				Email = personWithGuid.Email,
				EmploymentStartDate = new DateTime(2000, 1, 1),
				EmploymentEndDate = new DateTime(2059, 12, 31),
				EmploymentNumber = "",
				EmploymentTypeCode = 0,
				EmploymentTypeName = "",
				FirstName = personWithGuid.Name.FirstName,
				LastName = personWithGuid.Name.LastName,
				IsAgent = true,
				IsUser = false,
				Note = personWithGuid.Note,
				PersonCode = personWithGuid.Id.GetValueOrDefault(),
				PersonName = personWithGuid.Name.ToString(),
				ToBeDeleted = false,
				TeamId = 1,
				TeamCode = Guid.NewGuid(),
				TeamName = "team",
				SiteId = 1,
				SiteCode = Guid.NewGuid(),
				SiteName = "site",
				SkillsetId = null,
				WindowsDomain = "domain\\user",
				WindowsUsername = "user",
				ValidToDateIdMaxDate = 1,
				ValidToIntervalIdMaxDate = 1,
				ValidFromDateIdLocal = 1,
				ValidToDateIdLocal = 1,
				ValidFromDateLocal = DateTime.Now,
				ValidToDateLocal = DateTime.Now.AddYears(1),
				ValidToIntervalId = 1,
				ParttimeCode = Guid.NewGuid(),
				ParttimePercentage = "100%",
				TimeZoneId = 1,
				ValidFromDateId = 1,
				ValidFromIntervalId = 1,
				ValidToDateId = 1
			};

			analyticsPersonRepository.AddPersonPeriod(personPeriod1);
			siteId = analyticsPersonRepository.SiteId(Guid.NewGuid(), "Site name 1", businessUnit.BusinessUnitId);
		}

		[Test]
		public void ShouldReturnOnePersonPeriods()
		{
			var target = new AnalyticsPersonRepository();
			var result = target.GetPersonPeriods(personId2);
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnTwoPersonPeriods()
		{
			var target = new AnalyticsPersonRepository();
			var result = target.GetPersonPeriods(personId);
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void GetTeam_ShouldReturnTeam()
		{
			var target = new AnalyticsPersonRepository();
			var team = target.TeamId(Guid.NewGuid(), siteId, "Team Name", businessUnit.BusinessUnitId);
			team.Should().Be.GreaterThan(0);
		}

		[Test]
		public void ShouldReturnBusinessUnit()
		{
			var target = new AnalyticsPersonRepository();
			var response = target.BusinessUnitId(BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault());
			response.Should().Be.EqualTo(businessUnit.BusinessUnitId);
		}

		[Test]
		public void DeleteOnePersionPeriod_ShouldReturnPersonPeriodDeleted()
		{
			var target = new AnalyticsPersonRepository();
			target.DeletePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonPeriodCode = personPeriodCode1
			});
			var result = target.GetPersonPeriods(personId);
			result.Count(a => a.ToBeDeleted).Should().Be.EqualTo(1);
			result.Count(a => !a.ToBeDeleted).Should().Be.EqualTo(1);
		}
	}
}