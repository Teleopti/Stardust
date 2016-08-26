using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[Category("LongRunning")]
	[TestFixture]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsBridgeGroupPagePersonRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsBridgeGroupPagePersonRepository Target;
		public IAnalyticsGroupPageRepository AnalyticsGroupPageRepository;
		private AnalyticsGroup group;
		private AnalyticsGroup groupPage2;
		private Guid personId;
		private Guid personPeriodId;
		private Guid personId2;
		private Guid personPeriodId2;
		private Guid businessUnitId;

		private void createData()
		{
			group = new AnalyticsGroup
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
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(group);
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

			analyticsDataFactory.Setup(new Person(person, datasource, 0, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, businessUnitId,
				false, timeZones.UtcTimeZoneId, personPeriodId));
			analyticsDataFactory.Setup(new Person(person2, datasource, 1, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, businessUnitId,
				false, timeZones.UtcTimeZoneId, personPeriodId2));

			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldGetBridgeGroupPagePerson()
		{
			createData();
			Target.AddBridgeGroupPagePerson(new[] {personId}, group.GroupCode, businessUnitId);

			var result = Target.GetBridgeGroupPagePerson(group.GroupCode, businessUnitId).First();

			result.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldDeleteAllBridgeGroupPagePerson()
		{
			createData();

			Target.AddBridgeGroupPagePerson(new[] { personId }, group.GroupCode, businessUnitId);
			Target.AddBridgeGroupPagePerson(new[] { personId }, groupPage2.GroupCode, businessUnitId);

			Target.DeleteAllBridgeGroupPagePerson(new[] { group.GroupPageCode }, businessUnitId);

			var result = Target.GetBridgeGroupPagePerson(group.GroupCode, businessUnitId);
			result.Should().Be.Empty();
			var result2 = Target.GetBridgeGroupPagePerson(groupPage2.GroupCode, businessUnitId).Single();
			result2.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldDeleteBridgeGroupPagePerson()
		{
			createData();

			Target.AddBridgeGroupPagePerson(new[] { personId }, group.GroupCode, businessUnitId);
			Target.AddBridgeGroupPagePerson(new[] { personId2 }, group.GroupCode, businessUnitId);
			Target.AddBridgeGroupPagePerson(new[] { personId2 }, groupPage2.GroupCode, businessUnitId);

			Target.DeleteBridgeGroupPagePerson(new[] {personId}, group.GroupCode, businessUnitId);

			var result = Target.GetBridgeGroupPagePerson(group.GroupCode, businessUnitId).Single();
			result.Should().Be.EqualTo(personId2);
			var result2 = Target.GetBridgeGroupPagePerson(groupPage2.GroupCode, businessUnitId).Single();
			result2.Should().Be.EqualTo(personId2);
		}

		[Test]
		public void ShouldBeAbleToGetBuildInGroupPagesForPersonPeriod()
		{
			createData();

			Target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new [] { group.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId2, new [] { groupPage2.GroupCode }, businessUnitId);

			var result = Target.GetGroupPagesForPersonPeriod(personPeriodId, businessUnitId).Single();

			result.Should().Be.EqualTo(group.GroupCode);
		}

		[Test]
		public void ShouldBeAbleToDeleteBridgeGroupPagePersonForPersonPeriod()
		{
			createData();

			Target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { group.GroupCode, groupPage2.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId2, new[] { groupPage2.GroupCode }, businessUnitId);

			Target.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { group.GroupCode }, businessUnitId);
			var result = Target.GetGroupPagesForPersonPeriod(personPeriodId, businessUnitId).Single();

			result.Should().Be.EqualTo(groupPage2.GroupCode);
		}

		[Test]
		public void ShouldBeAbleToDeleteBridgeGroupPagePersonRemovedWhenNoPeriodsLeft()
		{
			createData();

			Target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { group.GroupCode, groupPage2.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId2, new[] { groupPage2.GroupCode }, businessUnitId);

			Target.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, new Guid[] { }, businessUnitId);
			var result = Target.GetGroupPagesForPersonPeriod(personPeriodId, businessUnitId);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldBeNotDeleteExcludedPersonPeriods()
		{
			createData();

			Target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { group.GroupCode, groupPage2.GroupCode }, businessUnitId);
			Target.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId2, new[] { groupPage2.GroupCode }, businessUnitId);

			Target.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, new[] { personPeriodId }, businessUnitId);
			var result = Target.GetGroupPagesForPersonPeriod(personPeriodId, businessUnitId).ToList();

			result.Should().Not.Be.Empty();
			result.Should().Have.Count.EqualTo(2);
		}

	}
}