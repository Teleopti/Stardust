using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsBridgeGroupPagePersonRepositoryTest
	{
		ICurrentDataSource currentDataSource;
		AnalyticsGroup @group;
		AnalyticsGroup groupPage2;
		Guid personId;
		Guid personPeriodId;
		Guid personId2;
		Guid personPeriodId2;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			analyticsDataFactory.Setup(businessUnit);
			var person = PersonFactory.CreatePersonWithPersonPeriod(
				PersonFactory.CreatePersonWithGuid("firstname1", "lastname1"), DateOnly.Today, new ISkill[] {},
				new Contract("contract1"), new PartTimePercentage("parttimepercentage1"));
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(
				PersonFactory.CreatePersonWithGuid("firstname2", "lastname2"), DateOnly.Today, new ISkill[] { },
				new Contract("contract2"), new PartTimePercentage("parttimepercentage2"));
			personId = person.Id.GetValueOrDefault();
			personPeriodId = Guid.NewGuid();
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			personId2 = person2.Id.GetValueOrDefault();
			personPeriodId2 = Guid.NewGuid();
			person2.PersonPeriodCollection.First().SetId(personPeriodId2);

			analyticsDataFactory.Setup(new Person(person, datasource, 0, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId, personPeriodId));
			analyticsDataFactory.Setup(new Person(person2, datasource, 1, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId, personPeriodId2));

			analyticsDataFactory.Persist();

			currentDataSource = CurrentDataSource.Make();

			var analyticsGroupPageRepository = new AnalyticsGroupPageRepository(currentDataSource);
			@group = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = false,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};

			groupPage2 = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName2",
				GroupPageNameResourceKey = "GroupPageNameResourceKey2",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName2",
				GroupIsCustom = false,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			analyticsGroupPageRepository.AddGroupPage(@group);
			analyticsGroupPageRepository.AddGroupPage(groupPage2);
		}

		[Test]
		public void ShouldGetBridgeGroupPagePerson()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);
			target.AddBridgeGroupPagePerson(new[] {personId}, @group.GroupCode);

			var result = target.GetBridgeGroupPagePerson(@group.GroupCode).First();

			result.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldDeleteAllBridgeGroupPagePerson()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);
			target.AddBridgeGroupPagePerson(new[] { personId }, @group.GroupCode);
			target.AddBridgeGroupPagePerson(new[] { personId }, groupPage2.GroupCode);

			target.DeleteAllBridgeGroupPagePerson(new[] { @group.GroupPageCode });

			var result = target.GetBridgeGroupPagePerson(@group.GroupCode);
			result.Should().Be.Empty();
			var result2 = target.GetBridgeGroupPagePerson(groupPage2.GroupCode).Single();
			result2.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldDeleteBridgeGroupPagePerson()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);
			target.AddBridgeGroupPagePerson(new[] { personId }, @group.GroupCode);
			target.AddBridgeGroupPagePerson(new[] { personId2 }, @group.GroupCode);
			target.AddBridgeGroupPagePerson(new[] { personId2 }, groupPage2.GroupCode);

			target.DeleteBridgeGroupPagePerson(new[] {personId}, @group.GroupCode);

			var result = target.GetBridgeGroupPagePerson(@group.GroupCode).Single();
			result.Should().Be.EqualTo(personId2);
			var result2 = target.GetBridgeGroupPagePerson(groupPage2.GroupCode).Single();
			result2.Should().Be.EqualTo(personId2);
		}

		[Test]
		public void ShouldBeAbleToGetBuildInGroupPagesForPersonPeriod()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);

			target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new [] { @group.GroupCode });
			target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId2, new [] { groupPage2.GroupCode });

			var result = target.GetBuiltInGroupPagesForPersonPeriod(personPeriodId).Single();

			result.Should().Be.EqualTo(@group.GroupCode);
		}

		[Test]
		public void ShouldBeAbleToDeleteBridgeGroupPagePersonForPersonPeriod()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);

			target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { @group.GroupCode, groupPage2.GroupCode });
			target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId2, new[] { groupPage2.GroupCode });

			target.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { @group.GroupCode });
			var result = target.GetBuiltInGroupPagesForPersonPeriod(personPeriodId).Single();

			result.Should().Be.EqualTo(groupPage2.GroupCode);
		}

		[Test]
		public void ShouldBeAbleToDeleteBridgeGroupPagePersonRemovedWhenNoPeriodsLeft()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);

			target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { @group.GroupCode, groupPage2.GroupCode });
			target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId2, new[] { groupPage2.GroupCode });

			target.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, new Guid[] { });
			var result = target.GetBuiltInGroupPagesForPersonPeriod(personPeriodId);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldBeNotDeleteExcludedPersonPeriods()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);

			target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { @group.GroupCode, groupPage2.GroupCode });
			target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId2, new[] { groupPage2.GroupCode });

			target.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, new[] { personPeriodId });
			var result = target.GetBuiltInGroupPagesForPersonPeriod(personPeriodId).ToList();

			result.Should().Not.Be.Empty();
			result.Should().Have.Count.EqualTo(2);
		}

	}
}