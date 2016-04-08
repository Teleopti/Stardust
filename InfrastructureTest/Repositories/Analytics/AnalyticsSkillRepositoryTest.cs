using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsSkillRepositoryTest
	{
		ExistingDatasources datasource;
		BusinessUnit businessUnit;
		private int skillSetId;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			var threeSkills = new ThreeSkills(timeZones, businessUnit, datasource);

			analyticsDataFactory.Setup(threeSkills);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Persist();

			var analyticsSkillRepository = new AnalyticsSkillRepository();
			skillSetId = analyticsSkillRepository.AddSkillSet(new AnalyticsSkillSet
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceId = 0,
				DatasourceUpdateDate = DateTime.Now,
				SkillsetCode = "1,2,3",
				SkillsetName = "skill1,skill2,skill3"
			});

			SetUpPerson();

			analyticsSkillRepository.AddAgentSkill(1, 0, true, 1);
			analyticsSkillRepository.AddAgentSkill(1, 1, false, 1);
			analyticsSkillRepository.AddAgentSkill(1, 2, true, 1);

			analyticsSkillRepository.AddAgentSkill(2, 2, true, 1);
		}

		private void SetUpPerson()
		{
			var personWithGuid = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			var analyticsPersonRepository = new AnalyticsPersonPeriodRepository();
			var personPeriodCode1 = Guid.NewGuid();
			var personPeriod1 = new AnalyticsPersonPeriod
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

			// Add another person period
			personPeriod1.PersonPeriodCode = Guid.NewGuid();
			analyticsPersonRepository.AddPersonPeriod(personPeriod1);
		}

		[Test]
		public void ShouldReturnAllSkillsets()
		{
			var target = new AnalyticsSkillRepository();
			var result = target.SkillSets();
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnSkillset()
		{
			var target = new AnalyticsSkillRepository();
			var list = new List<AnalyticsSkill> {
				new AnalyticsSkill { SkillId = 3 },
				new AnalyticsSkill { SkillId = 1 },
				new AnalyticsSkill { SkillId = 2 }
			};

			var result = target.SkillSetId(list);
			result.Should().Be.EqualTo(skillSetId);
		}

		[Test]
		public void ShouldReturnSkills()
		{
			var target = new AnalyticsSkillRepository();
			var result = target.Skills(businessUnit.BusinessUnitId);
			result.Count.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldReturnFactAgentSkills()
		{
			var target = new AnalyticsSkillRepository();
			var result = target.GetFactAgentSkillsForPerson(1);
			result.Count.Should().Be.EqualTo(3);
			result.Count(a => a.Active).Should().Be.EqualTo(2);
			result.Count(a => !a.Active).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDeleteAgentSkillsForOnePerson()
		{
			var target = new AnalyticsSkillRepository();
			target.DeleteAgentSkillForPersonId(1);
			var result = target.GetFactAgentSkillsForPerson(1);
			result.Count.Should().Be.EqualTo(0);

			var result2 = target.GetFactAgentSkillsForPerson(2);
			result2.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAddBridgeSkillsetSkill()
		{
			var target = new AnalyticsSkillRepository();
			target.AddBridgeSkillsetSkill(new AnalyticsBridgeSkillsetSkill
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceId = 1,
				DatasourceUpdateDate = DateTime.Now,
				SkillsetId = skillSetId,
				SkillId = 1
			});
		}
	}
}