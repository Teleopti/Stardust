using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsPersonPeriodRepositoryTest
	{
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public IAnalyticsPersonPeriodRepository Target;
		private AnalyticsPersonPeriod personPeriod1;
		private Guid personId;
		private Guid personId2;
		private ExistingDatasources datasource;
		private BusinessUnit businessUnit;
		private Guid personPeriodCode1;
		private IPerson _personWithGuid;
		private AnalyticTeam _analyticTeam;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			analyticsDataFactory.Setup(businessUnit);
			var validFrom = new DateTime(2010, 1, 1);

			_personWithGuid = makePerson(validFrom);
			var personWithGuid2 = makePerson(validFrom);

			personId = _personWithGuid.Id.GetValueOrDefault();
			personId2 = personWithGuid2.Id.GetValueOrDefault();
			_analyticTeam = new AnalyticTeam
			{
				TeamCode = Guid.NewGuid(),
				Name = "team",
				TeamId = 1
			};

			analyticsDataFactory.Setup(new Person(_personWithGuid, datasource, 0, validFrom,
				AnalyticsDate.Eternity.DateDate, 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId));
			analyticsDataFactory.Setup(new Person(personWithGuid2, datasource, 1, validFrom,
				AnalyticsDate.Eternity.DateDate, 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId));

			analyticsDataFactory.Persist();

			personPeriodCode1 = Guid.NewGuid();
		}

		private static IPerson makePerson(DateTime validFrom)
		{
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(validFrom));
			personPeriod.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			person.AddPersonPeriod(personPeriod);
			return person;
		}

		[Test]
		public void ShouldReturnOnePersonPeriods()
		{
			setUpData();

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetPersonPeriods(personId2));
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnTwoPersonPeriods()
		{
			setUpData();

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetPersonPeriods(personId));
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void DeleteOnePersionPeriod_ShouldReturnPersonPeriodDeleted()
		{
			setUpData();

			WithAnalyticsUnitOfWork.Do(() => Target.DeletePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonPeriodCode = personPeriodCode1
			}));
			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetPersonPeriods(personId));
			result.Count(a => a.ToBeDeleted).Should().Be.EqualTo(1);
			result.Count(a => !a.ToBeDeleted).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUpdatePersonPeriod()
		{
			setUpData();
			
			var personPeriod = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personPeriodCode1));
			personPeriod.Email = "fun@stuff.com";
			WithAnalyticsUnitOfWork.Do(() => Target.AddOrUpdatePersonPeriod(personPeriod));

			var result = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personPeriodCode1));
			result.Should().Not.Be.Null();
			result.Email.Should().Be.EqualTo(personPeriod.Email);
		}

		[Test]
		public void ShouldUpdateTeamName()
		{
			setUpData();
			var personToUpdate = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personPeriodCode1));
			var teamCodeToUpdate = _analyticTeam.TeamCode.GetValueOrDefault();
			personPeriodCode1 = Guid.NewGuid();
			_analyticTeam.TeamCode = Guid.NewGuid();
			setUpData();
			
			WithAnalyticsUnitOfWork.Do(() => Target.UpdateTeamName(teamCodeToUpdate, "NewTeamName"));

			var personAfterUpdate = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personToUpdate.PersonPeriodCode));
			var personWithNoUpdate = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personPeriodCode1));
			personAfterUpdate.TeamName.Should().Be.EqualTo("NewTeamName");
			personWithNoUpdate.TeamName.Should().Be.EqualTo(personToUpdate.TeamName);
		}

		[Test]
		public void ShouldUpdateSiteName()
		{
			setUpData();
			var personToUpdate = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personPeriodCode1));
			var siteCodeToUpdate = personToUpdate.SiteCode;

			WithAnalyticsUnitOfWork.Do(() => Target.UpdateSiteName(siteCodeToUpdate, "NewSiteName"));

			var personAfterUpdate = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personToUpdate.PersonPeriodCode));
			personAfterUpdate.SiteName.Should().Be.EqualTo("NewSiteName");
		}

		[Test, TestCaseSource(typeof(CommonNameDescriptionSettingsTestData), nameof(CommonNameDescriptionSettingsTestData.TestCasesIntegration))]
		public void UpdateNamesTest(string commonNameDescriptionSetting)
		{
			setUpData();
			var commonNameDescription = new CommonNameDescriptionSetting(commonNameDescriptionSetting);
			WithAnalyticsUnitOfWork.Do(() => Target.UpdatePersonNames(commonNameDescription, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()));
			var correctName = commonNameDescription.BuildFor(personPeriod1.FirstName, personPeriod1.LastName, personPeriod1.EmploymentNumber);
			var updatedName = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personPeriodCode1).PersonName);
			updatedName.Should().Be.EqualTo(correctName);
		}

		[Test]
		public void UpdateNamesTestShouldNotUpdateOtherBusinessUnit()
		{
			setUpData();
			var commonNameDescription = new CommonNameDescriptionSetting("#123 {FirstName}");
			WithAnalyticsUnitOfWork.Do(() => Target.UpdatePersonNames(commonNameDescription, Guid.NewGuid()));
			var newName = commonNameDescription.BuildFor(personPeriod1.FirstName, personPeriod1.LastName, personPeriod1.EmploymentNumber);
			var personName = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personPeriodCode1).PersonName);
			personName.Should().Not.Be.EqualTo(newName);
		}

		private void setUpData()
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
				EmploymentEndDate = AnalyticsDate.Eternity.DateDate,
				EmploymentNumber = "1337",
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
				TeamId = _analyticTeam.TeamId,
				TeamCode = _analyticTeam.TeamCode.GetValueOrDefault(),
				TeamName = _analyticTeam.Name,
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

			WithAnalyticsUnitOfWork.Do(() => Target.AddOrUpdatePersonPeriod(personPeriod1));
		}

		[Test]
		public void ShouldLoadPerson()
		{
			setUpData();

			var expected = WithAnalyticsUnitOfWork.Get(() => Target.PersonPeriod(personPeriodCode1));
			var pers = WithAnalyticsUnitOfWork.Get(() => Target.PersonAndBusinessUnit(personPeriodCode1));
			pers.Should().Not.Be.Null();
			pers.PersonId.Should().Be.EqualTo(expected.PersonId);
			pers.BusinessUnitId.Should().Be.EqualTo(expected.BusinessUnitId);
		}
	}
}