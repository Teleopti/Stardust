using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
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
	}
}