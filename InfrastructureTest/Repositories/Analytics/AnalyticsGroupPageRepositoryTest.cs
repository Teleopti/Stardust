using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using BusinessUnit = Teleopti.Ccc.TestCommon.TestData.Analytics.BusinessUnit;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[Category("BucketB")]
	[TestFixture]
	[AnalyticsDatabaseTest]
	public class AnalyticsGroupPageRepositoryTest
	{
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public IAnalyticsGroupPageRepository Target;
		public IAnalyticsBridgeGroupPagePersonRepository BridgeGroupPagePersonRepository;
		public IAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, datasource);

			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldGetGroupPage()
		{
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetGroupPage(groupPage.GroupPageCode, groupPage.BusinessUnitCode).First());
			result.GroupPageCode.Should().Be.EqualTo(groupPage.GroupPageCode);
			result.GroupPageName.Should().Be.EqualTo(groupPage.GroupPageName);
			result.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.GroupPageNameResourceKey);
			result.GroupCode.Should().Be.EqualTo(groupPage.GroupCode);
			result.GroupName.Should().Be.EqualTo(groupPage.GroupName);
			result.GroupIsCustom.Should().Be.EqualTo(groupPage.GroupIsCustom);
			result.BusinessUnitCode.Should().Be.EqualTo(groupPage.BusinessUnitCode);
		}

		[Test]
		public void ShouldNotAddIfExisting()
		{
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));
			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetGroupPageByGroupCode(groupPage.GroupCode, groupPage.BusinessUnitCode));
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetIfExisting()
		{
			var existingGroupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			var potentialGroupPage = new AnalyticsGroup
			{
				GroupPageCode = existingGroupPage.GroupPageCode,
				GroupPageName = existingGroupPage.GroupPageName,
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				BusinessUnitCode = existingGroupPage.BusinessUnitCode
			};

			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(existingGroupPage));
			var result = WithAnalyticsUnitOfWork.Get(() => Target.AddOrGetGroupPage(potentialGroupPage));
			result.GroupCode.Should().Be(existingGroupPage.GroupCode);
		}

		[Test]
		public void ShouldAddNewIfNotExistingAndReturnNull()
		{
			var newGroupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			var result = WithAnalyticsUnitOfWork.Get(() => Target.AddOrGetGroupPage(newGroupPage));
			result.Should().Be.Null();
		}
		
		[Test]
		public void ShouldGetGroupPageByGroupCode()
		{
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));

			var result = WithAnalyticsUnitOfWork.Get(() =>
			Target.GetGroupPageByGroupCode(groupPage.GroupCode, groupPage.BusinessUnitCode));
			result.GroupPageCode.Should().Be.EqualTo(groupPage.GroupPageCode);
			result.GroupPageName.Should().Be.EqualTo(groupPage.GroupPageName);
			result.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.GroupPageNameResourceKey);
			result.GroupCode.Should().Be.EqualTo(groupPage.GroupCode);
			result.GroupName.Should().Be.EqualTo(groupPage.GroupName);
			result.GroupIsCustom.Should().Be.EqualTo(groupPage.GroupIsCustom);
			result.BusinessUnitCode.Should().Be.EqualTo(groupPage.BusinessUnitCode);
		}

		[Test]
		public void ShouldUpdateGroupPage()
		{
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));

			groupPage.GroupPageName = "GroupPageName2";

			WithAnalyticsUnitOfWork.Do(() => Target.UpdateGroupPage(groupPage));

			var result = WithAnalyticsUnitOfWork.Get(() =>
				Target.GetGroupPage(groupPage.GroupPageCode, groupPage.BusinessUnitCode).First());
			result.GroupPageCode.Should().Be.EqualTo(groupPage.GroupPageCode);
			result.GroupPageName.Should().Be.EqualTo("GroupPageName2");
			result.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.GroupPageNameResourceKey);
			result.GroupCode.Should().Be.EqualTo(groupPage.GroupCode);
			result.GroupName.Should().Be.EqualTo(groupPage.GroupName);
			result.GroupIsCustom.Should().Be.EqualTo(groupPage.GroupIsCustom);
			result.BusinessUnitCode.Should().Be.EqualTo(groupPage.BusinessUnitCode);
		}

		[Test]
		public void ShouldDeleteGroupPage()
		{
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));
			WithAnalyticsUnitOfWork.Do(() => Target.DeleteGroupPages(new[] { groupPage.GroupPageCode }, groupPage.BusinessUnitCode));

			WithAnalyticsUnitOfWork.Do(() => Target.GetGroupPage(groupPage.GroupPageCode, groupPage.BusinessUnitCode).Should().Be.Empty());
		}

		[Test]
		public void ShouldBeAbleToDeleteBasedOnGroupCode()
		{
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));
			var groupPage2 = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName2",
				GroupPageNameResourceKey = "GroupPageNameResourceKey2",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName2",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage2));

			WithAnalyticsUnitOfWork.Do(() =>
				Target.DeleteGroupPagesByGroupCodes(new[] {groupPage.GroupCode}, groupPage.BusinessUnitCode));

			var result = WithAnalyticsUnitOfWork.Get(() =>
				Target.GetGroupPageByGroupCode(groupPage.GroupCode, groupPage.BusinessUnitCode));
			var result2 = WithAnalyticsUnitOfWork.Get(() =>
				Target.GetGroupPageByGroupCode(groupPage2.GroupCode, groupPage2.BusinessUnitCode));
			result.Should().Be.Null();
			result2.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldBeAbleToGetListOfBuildInGroupPages()
		{
			var groupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));
			var groupPage2 = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName2",
				GroupPageNameResourceKey = null,
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName2",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage2));

			var result = WithAnalyticsUnitOfWork.Get(() =>
				Target.GetBuildInGroupPageBase(BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()).FirstOrDefault());

			result.Should().Not.Be.Null();
			result.GroupPageName.Should().Be.EqualTo(groupPage.GroupPageName);
			result.GroupPageCode.Should().Be.EqualTo(groupPage.GroupPageCode);
			result.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.GroupPageNameResourceKey);
		}

		private AnalyticsPersonPeriod CreateAndPersistPerson()
		{
			var personPeriod1 = new AnalyticsPersonPeriod
			{
				PersonPeriodCode = Guid.NewGuid(),
				ValidFromDate = new DateTime(2000, 1, 1),
				ValidToDate = new DateTime(2001, 1, 1),
				BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault(),
				BusinessUnitName = BusinessUnitFactory.BusinessUnitUsedInTest.Name,
				BusinessUnitId = 1,
				ContractCode = Guid.NewGuid(),
				ContractName = "Test contract",
				DatasourceId = 1,
				DatasourceUpdateDate = DateTime.Now,
				Email = string.Empty,
				EmploymentStartDate = new DateTime(2000, 1, 1),
				EmploymentEndDate = AnalyticsDate.Eternity.DateDate,
				EmploymentNumber = "1337",
				EmploymentTypeCode = 0,
				EmploymentTypeName = "",
				FirstName = "John",
				LastName = "Smith",
				IsUser = false,
				Note = string.Empty,
				PersonCode = Guid.NewGuid(),
				PersonName = string.Empty,
				ToBeDeleted = false,
				TeamId = 2,
				TeamCode = Guid.NewGuid(),
				TeamName = string.Empty,
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

			WithAnalyticsUnitOfWork.Do(() => AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(personPeriod1));
			return personPeriod1;
		}
	}
}