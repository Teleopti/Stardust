using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[Category("BucketB")]
	[TestFixture]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsBridgeGroupPagePersonRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsBridgeGroupPagePersonRepository Target;
		public IAnalyticsGroupPageRepository AnalyticsGroupPageRepository;
		private AnalyticsGroup groupPage1;
		private AnalyticsGroup groupPage2;
		private Guid personId;
		private Guid personPeriodId;
		private Guid personId2;
		private Guid personPeriodId2;
		private Guid businessUnitId;
		private int analyticsPersonId1;
		private int analyticsPersonId2;

		private void createData()
		{
			groupPage1 = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = false,
				BusinessUnitCode = businessUnitId
			};

			groupPage2 = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName2",
				GroupPageNameResourceKey = "GroupPageNameResourceKey2",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName2",
				GroupIsCustom = false,
				BusinessUnitCode = businessUnitId
			};
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(groupPage1);
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(groupPage2);
		}

		[SetUp]
		public void SetUp()
		{
			businessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault();
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
			analyticsPersonId1 = 0;
			analyticsPersonId2 = 1;
			analyticsDataFactory.Setup(new Person(person, datasource, analyticsPersonId1, new DateTime(2010, 1, 1),
				AnalyticsDate.Eternity.DateDate, 0, -2, 0, businessUnitId,
				false, timeZones.UtcTimeZoneId, personPeriodId));
			analyticsDataFactory.Setup(new Person(person2, datasource, analyticsPersonId2, new DateTime(2010, 1, 1),
				AnalyticsDate.Eternity.DateDate, 0, -2, 0, businessUnitId,
				false, timeZones.UtcTimeZoneId, personPeriodId2));

			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldGetBridgeGroupPagePerson()
		{
			createData();
			Target.AddBridgeGroupPagePerson(new[] {personId}, groupPage1.GroupCode, businessUnitId);

			var result = Target.GetBridgeGroupPagePerson(groupPage1.GroupCode, businessUnitId).First();

			result.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldDeleteAllBridgeGroupPagePerson()
		{
			createData();

			Target.AddBridgeGroupPagePerson(new[] { personId }, groupPage1.GroupCode, businessUnitId);
			Target.AddBridgeGroupPagePerson(new[] { personId }, groupPage2.GroupCode, businessUnitId);

			Target.DeleteAllBridgeGroupPagePerson(new[] { groupPage1.GroupPageCode }, businessUnitId);

			var result = Target.GetBridgeGroupPagePerson(groupPage1.GroupCode, businessUnitId);
			result.Should().Be.Empty();
			var result2 = Target.GetBridgeGroupPagePerson(groupPage2.GroupCode, businessUnitId).Single();
			result2.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldDeleteBridgeGroupPagePerson()
		{
			createData();

			Target.AddBridgeGroupPagePerson(new[] { personId }, groupPage1.GroupCode, businessUnitId);
			Target.AddBridgeGroupPagePerson(new[] { personId2 }, groupPage1.GroupCode, businessUnitId);
			Target.AddBridgeGroupPagePerson(new[] { personId2 }, groupPage2.GroupCode, businessUnitId);

			Target.DeleteBridgeGroupPagePerson(new[] {personId}, groupPage1.GroupCode, businessUnitId);

			var result = Target.GetBridgeGroupPagePerson(groupPage1.GroupCode, businessUnitId).Single();
			result.Should().Be.EqualTo(personId2);
			var result2 = Target.GetBridgeGroupPagePerson(groupPage2.GroupCode, businessUnitId).Single();
			result2.Should().Be.EqualTo(personId2);
		}

		[Test]
		public void ShouldDeleteForGivenPersonAndGroupPage()
		{
			createData();

			Target.AddBridgeGroupPagePerson(new[] { personId }, groupPage1.GroupCode, businessUnitId);
			Target.AddBridgeGroupPagePerson(new[] { personId }, groupPage2.GroupCode, businessUnitId);

			Target.DeleteAllForPerson(groupPage1.GroupPageCode, personId, businessUnitId);

			var result = Target.GetBridgeGroupPagePerson(groupPage1.GroupCode, businessUnitId);
			result.Any().Should().Be.False();
			var result2 = Target.GetBridgeGroupPagePerson(groupPage2.GroupCode, businessUnitId).Single();
			result2.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldBeAbleToGetBuildInGroupPagesForPersonPeriod()
		{
			createData();

			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId1, new [] { groupPage1.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId2, new [] { groupPage2.GroupCode }, businessUnitId);

			var result = Target.GetGroupPagesForPersonPeriod(analyticsPersonId1, businessUnitId).Single();

			result.Should().Be.EqualTo(groupPage1.GroupCode);
		}

		[Test]
		public void ShouldBeAbleToDeleteBridgeGroupPagePersonForPersonPeriod()
		{
			createData();

			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId1, new[] { groupPage1.GroupCode, groupPage2.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId2, new[] { groupPage2.GroupCode }, businessUnitId);

			Target.DeleteBridgeGroupPagePersonForPersonPeriod(analyticsPersonId1, new[] { groupPage1.GroupCode }, businessUnitId);
			var result = Target.GetGroupPagesForPersonPeriod(analyticsPersonId1, businessUnitId).Single();

			result.Should().Be.EqualTo(groupPage2.GroupCode);
		}

		[Test]
		public void ShouldBeAbleToDeleteBridgeGroupPagePersonRemovedWhenNoPeriodsLeft()
		{
			createData();

			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId1, new[] { groupPage1.GroupCode, groupPage2.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId2, new[] { groupPage2.GroupCode }, businessUnitId);

			Target.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, new int[] { });
			var result = Target.GetGroupPagesForPersonPeriod(analyticsPersonId1, businessUnitId);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldBeNotDeleteExcludedPersonPeriods()
		{
			createData();

			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId1, new[] { groupPage1.GroupCode, groupPage2.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId2, new[] { groupPage2.GroupCode }, businessUnitId);

			Target.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, new[] { analyticsPersonId1 });
			var result = Target.GetGroupPagesForPersonPeriod(analyticsPersonId1, businessUnitId).ToList();

			result.Should().Not.Be.Empty();
			result.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldGetBridgeGroupPagePersonExcludingOptionalColumnGroupPages()
		{
			createData();
			var optionalColumnGroupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName3",
				GroupPageNameResourceKey = null,
				GroupCode = Guid.NewGuid(),
				GroupName = "opt",
				GroupIsCustom = false,
				BusinessUnitCode = businessUnitId
			};
			var userDefinedGroupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName4",
				GroupPageNameResourceKey = null,
				GroupCode = Guid.NewGuid(),
				GroupName = "ud",
				GroupIsCustom = true,
				BusinessUnitCode = businessUnitId
			};
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(optionalColumnGroupPage);
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(userDefinedGroupPage);

			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId1, new[] { groupPage1.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId1, new[] { optionalColumnGroupPage.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(analyticsPersonId1, new[] { userDefinedGroupPage.GroupCode }, businessUnitId);

			var result = Target.GetGroupPagesForPersonPeriod(analyticsPersonId1, businessUnitId);
			result.Count().Should().Be.EqualTo(2);
			result.Should().Not.Contain(optionalColumnGroupPage.GroupCode);
		}

	}
}