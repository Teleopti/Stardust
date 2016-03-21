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
		AnalyticsGroupPage groupPage;
		AnalyticsGroupPage groupPage2;
		Guid personId;
		Guid personId2;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			analyticsDataFactory.Setup(businessUnit);
			//var person = new TestDataFactory().Person("Ashley Andeen").Person;
			var personWithGuid = PersonFactory.CreatePersonWithGuid("firstName","lastName");
			var personWithGuid2 = PersonFactory.CreatePersonWithGuid("firstName2","lastName2");
			personId = personWithGuid.Id.GetValueOrDefault();
			personId2 = personWithGuid2.Id.GetValueOrDefault();
			analyticsDataFactory.Setup(new Person(personWithGuid, datasource, 0, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId));
			analyticsDataFactory.Setup(new Person(personWithGuid2, datasource, 1, new DateTime(2010, 1, 1),
				new DateTime(2059, 12, 31), 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				false, timeZones.UtcTimeZoneId));

			analyticsDataFactory.Persist();

			currentDataSource = CurrentDataSource.Make();

			var analyticsGroupPageRepository = new AnalyticsGroupPageRepository(currentDataSource);
			groupPage = new AnalyticsGroupPage
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};

			groupPage2 = new AnalyticsGroupPage
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName2",
				GroupPageNameResourceKey = "GroupPageNameResourceKey2",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName2",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			analyticsGroupPageRepository.AddGroupPage(groupPage);
			analyticsGroupPageRepository.AddGroupPage(groupPage2);
		}

		[Test]
		public void ShouldGetBridgeGroupPagePerson()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);
			target.AddBridgeGroupPagePerson(new[] {personId}, groupPage.GroupCode);

			var result = target.GetBridgeGroupPagePerson(groupPage.GroupCode).First();

			result.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldDeleteAllBridgeGroupPagePerson()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);
			target.AddBridgeGroupPagePerson(new[] { personId }, groupPage.GroupCode);
			target.AddBridgeGroupPagePerson(new[] { personId }, groupPage2.GroupCode);

			target.DeleteAllBridgeGroupPagePerson(new[] { groupPage.GroupPageCode });

			var result = target.GetBridgeGroupPagePerson(groupPage.GroupCode);
			result.Should().Be.Empty();
			var result2 = target.GetBridgeGroupPagePerson(groupPage2.GroupCode).Single();
			result2.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldDeleteBridgeGroupPagePerson()
		{
			var target = new AnalyticsBridgeGroupPagePersonRepository(currentDataSource);
			target.AddBridgeGroupPagePerson(new[] { personId }, groupPage.GroupCode);
			target.AddBridgeGroupPagePerson(new[] { personId2 }, groupPage.GroupCode);
			target.AddBridgeGroupPagePerson(new[] { personId2 }, groupPage2.GroupCode);

			target.DeleteBridgeGroupPagePerson(new[] {personId}, groupPage.GroupCode);

			var result = target.GetBridgeGroupPagePerson(groupPage.GroupCode).Single();
			result.Should().Be.EqualTo(personId2);
			var result2 = target.GetBridgeGroupPagePerson(groupPage2.GroupCode).Single();
			result2.Should().Be.EqualTo(personId2);
		}
	}
}