using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsPersonPeriodRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsPersonPeriodRepository Target;
		AnalyticsPersonPeriod personPeriod1;
		Guid personId;
		Guid personId2;
		ExistingDatasources datasource;
		BusinessUnit businessUnit;
		private Guid personPeriodCode1;
		private IPerson _personWithGuid;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			analyticsDataFactory.Setup(businessUnit);
			_personWithGuid = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			var personWithGuid2 = PersonFactory.CreatePersonWithGuid("firstName2", "lastName2");
			personId = _personWithGuid.Id.GetValueOrDefault();
			personId2 = personWithGuid2.Id.GetValueOrDefault();

			analyticsDataFactory.Setup(new Person(_personWithGuid, datasource, 0, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId));
			analyticsDataFactory.Setup(new Person(personWithGuid2, datasource, 1, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId));

			analyticsDataFactory.Persist();

			personPeriodCode1 = Guid.NewGuid();
		}

		[Test]
		public void ShouldReturnOnePersonPeriods()
		{
			SetUpData();

			var result = Target.GetPersonPeriods(personId2);
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnTwoPersonPeriods()
		{
			SetUpData();

			var result = Target.GetPersonPeriods(personId);
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void DeleteOnePersionPeriod_ShouldReturnPersonPeriodDeleted()
		{
			SetUpData();

			Target.DeletePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonPeriodCode = personPeriodCode1
			});
			var result = Target.GetPersonPeriods(personId);
			result.Count(a => a.ToBeDeleted).Should().Be.EqualTo(1);
			result.Count(a => !a.ToBeDeleted).Should().Be.EqualTo(1);
		}

		private void SetUpData()
		{
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
				Email = _personWithGuid.Email,
				EmploymentStartDate = new DateTime(2000, 1, 1),
				EmploymentEndDate = new DateTime(2059, 12, 31),
				EmploymentNumber = "",
				EmploymentTypeCode = 0,
				EmploymentTypeName = "",
				FirstName = _personWithGuid.Name.FirstName,
				LastName = _personWithGuid.Name.LastName,
				IsAgent = true,
				IsUser = false,
				Note = _personWithGuid.Note,
				PersonCode = _personWithGuid.Id.GetValueOrDefault(),
				PersonName = _personWithGuid.Name.ToString(),
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

			Target.AddPersonPeriod(personPeriod1);
		}
	}
}