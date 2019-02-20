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
		public IAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;

		[SetUp]
		public void SetUp()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var timeZones = new UtcAndCetTimeZones();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(BusinessUnitUsedInTests.BusinessUnit, datasource);

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
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
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
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage));
			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetGroupPageByGroupCode(groupPage.GroupCode, groupPage.BusinessUnitCode));
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetWhenTryingToAddAlreadyExisting()
		{
			var existingGroupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
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
			var result = WithAnalyticsUnitOfWork.Get(() => Target.AddAndGetGroupPage(potentialGroupPage));
			result.GroupId.Should().Be.GreaterThan(-1);
			result.GroupCode.Should().Be(existingGroupPage.GroupCode);
		}

		[Test]
		public void ShouldAddNewIfNotExistingAndReturnNew()
		{
			var newGroupPage = new AnalyticsGroup
			{
				GroupPageCode = Guid.NewGuid(),
				GroupPageName = "GroupPageName1",
				GroupPageNameResourceKey = "GroupPageNameResourceKey1",
				GroupCode = Guid.NewGuid(),
				GroupName = "GroupName1",
				GroupIsCustom = true,
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			};
			var result = WithAnalyticsUnitOfWork.Get(() => Target.AddAndGetGroupPage(newGroupPage));
			result.GroupId.Should().Be.GreaterThan(-1);
			result.GroupCode.Should().Be(newGroupPage.GroupCode);
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
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
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
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
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
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
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
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
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
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
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
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
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
				BusinessUnitCode = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddGroupPageIfNotExisting(groupPage2));

			var result = WithAnalyticsUnitOfWork.Get(() =>
				Target.GetBuildInGroupPageBase(BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()).FirstOrDefault());

			result.Should().Not.Be.Null();
			result.GroupPageName.Should().Be.EqualTo(groupPage.GroupPageName);
			result.GroupPageCode.Should().Be.EqualTo(groupPage.GroupPageCode);
			result.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.GroupPageNameResourceKey);
		}
	}
}