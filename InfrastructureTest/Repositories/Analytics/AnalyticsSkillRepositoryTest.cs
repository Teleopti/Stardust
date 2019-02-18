using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsSkillRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;
		public IAnalyticsSkillRepository Target;

		ExistingDatasources datasource;
		BusinessUnit businessUnit;
		private int skillSetId;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			datasource = new ExistingDatasources(timeZones);
			businessUnit = new BusinessUnit(BusinessUnitUsedInTests.BusinessUnit, datasource);

			var threeSkills = new ThreeSkills(timeZones, businessUnit, datasource);

			analyticsDataFactory.Setup(threeSkills);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Persist();
		}

		private void setUpData()
		{
			skillSetId = Target.AddSkillSet(new AnalyticsSkillSet
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceId = 0,
				DatasourceUpdateDate = DateTime.Now,
				SkillsetCode = "1,2,3",
				SkillsetName = "skill1,skill2,skill3"
			});

			setUpPerson();

			Target.AddAgentSkill(1, 0, true, 1);
			Target.AddAgentSkill(1, 1, false, 1);
			Target.AddAgentSkill(1, 2, true, 1);

			Target.AddAgentSkill(2, 2, true, 1);
		}

		private void setUpPerson()
		{
			var personWithGuid = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			var personPeriodCode1 = Guid.NewGuid();
			var personPeriod1 = new AnalyticsPersonPeriod
			{
				PersonPeriodCode = personPeriodCode1,
				ValidFromDate = new DateTime(2000, 1, 1),
				ValidToDate = new DateTime(2001, 1, 1),
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault(),
				BusinessUnitName = BusinessUnitUsedInTests.BusinessUnit.Name,
				BusinessUnitId = 1,
				ContractCode = Guid.NewGuid(),
				ContractName = "Test contract",
				DatasourceId = 1,
				DatasourceUpdateDate = DateTime.Now,
				Email = personWithGuid.Email,
				EmploymentStartDate = new DateTime(2000, 1, 1),
				EmploymentEndDate = AnalyticsDate.Eternity.DateDate,
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

			AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(personPeriod1);

			// Add another person period
			personPeriod1.PersonPeriodCode = Guid.NewGuid();
			AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(personPeriod1);
		}

		[Test]
		public void ShouldReturnAllSkillsets()
		{
			setUpData();

			var result = Target.SkillSets();
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnSkillset()
		{
			setUpData();

			var list = new List<AnalyticsSkill> {
				new AnalyticsSkill { SkillId = 3 },
				new AnalyticsSkill { SkillId = 1 },
				new AnalyticsSkill { SkillId = 2 }
			};

			var result = Target.SkillSetId(list);
			result.Should().Be.EqualTo(skillSetId);
		}

		[Test]
		public void ShouldReturnSkills()
		{
			setUpData();

			var result = Target.Skills(businessUnit.BusinessUnitId);
			result.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldAddBridgeSkillsetSkill()
		{
			setUpData();

			Target.AddBridgeSkillsetSkill(new AnalyticsBridgeSkillsetSkill
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceId = 1,
				DatasourceUpdateDate = DateTime.Now,
				SkillsetId = skillSetId,
				SkillId = 1
			});
		}

		[Test]
		public void ShouldAddOrUpdateSkill()
		{
			setUpData();

			var analyticsSkill = new AnalyticsSkill
			{
				SkillCode = Guid.NewGuid(),
				BusinessUnitId = businessUnit.BusinessUnitId,
				ForecastMethodCode = Guid.NewGuid(),
				ForecastMethodName = "method1",
				DatasourceUpdateDate = DateTime.Now,
				IsDeleted = false,
				SkillName = "skillName",
				TimeZoneId = 0
			};
			Target.AddOrUpdateSkill(analyticsSkill);

			var result = Target.Skills(businessUnit.BusinessUnitId);
			result.FirstOrDefault(x => x.SkillCode == analyticsSkill.SkillCode).Should().Not.Be.Null();
			analyticsSkill.IsDeleted = true;

			Target.AddOrUpdateSkill(analyticsSkill);

			result = Target.Skills(businessUnit.BusinessUnitId);
			var firstSkill = result.FirstOrDefault(x => x.SkillCode == analyticsSkill.SkillCode);
			(firstSkill != null && firstSkill.IsDeleted).Should().Be.True();
		}
	}
}